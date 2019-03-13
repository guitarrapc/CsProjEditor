using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CsProjEditor
{
    public class CsProjEditor
    {
        public enum EolType
        {
            CRLF,
            LF,
        }
        private static Func<EolType, string> EolString = eol => eol == EolType.CRLF ? "\r\n" : "\n";
        private readonly int baseSpaceNum = 2;

        private readonly string csproj;
        public EolType Eol { get; private set; }
        public Encoding Encoding { get; private set; }

        public XElement Root { get; private set; }
        public bool Initialized { get; private set; }

        private CsProjEditor(string path)
        {
            this.csproj = path;
        }

        public static CsProjEditor Load(string path, LoadOptions options = LoadOptions.PreserveWhitespace)
        {
            var editor = new CsProjEditor(path);
            editor.Root = XElement.Load(path, options);
            editor.Encoding = CsProjEditor.GetUtf8Encoding(path);
            editor.Eol = CsProjEditor.GetEndOfLine(path);
            editor.Initialized = true;
            return editor;
        }

        public override string ToString()
        {
            var declare = GetDeclaration(csproj);

            // gen xml
            string xml;
            if (declare == null)
            {
                xml = Root.ToString();
            }
            else
            {
                xml = declare.ToString();
                xml += Eol;
                xml += Root.ToString();
            }
            return xml;
        }

        /// <summary>
        /// determine utf8 contains bom from first 3 bytes of file.
        /// </summary>
        /// <remarks>
        /// UTF8 BOM == EFBBBF == 239, 187, 191
        /// </remarks>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Encoding GetUtf8Encoding(string path)
        {
            using (var reader = new FileStream(path, FileMode.Open))
            {
                var bits = new byte[3];
                reader.Read(bits, 0, 3);
                var isBom = bits[0] == 0xEF && bits[1] == 0xBB && bits[2] == 0xBF;
                return new System.Text.UTF8Encoding(isBom);
            }
        }

        /// <summary>
        /// check file contains crlf or not.
        /// </summary>
        /// <remarks>
        /// lf == 0A == 10
        /// crlf == 0D0A == 1310
        /// </remarks>
        /// <param name="path"></param>
        /// <returns></returns>
        public static EolType GetEndOfLine(string path)
        {
            using (var reader = new FileStream(path, FileMode.Open))
            {
                var bits = new byte[1024];
                reader.Read(bits, 0, 1024);
                var isCRLF = bits.Where((e, i) => i < bits.Length - 1)
                    .Select((e, i) => (left: e, right: bits[i + 1]))
                    .Where(x => x.left == 13 && x.right == 10)
                    .Any();
                return isCRLF ? EolType.CRLF : EolType.LF;
            }
        }

        private XDeclaration GetDeclaration(string path)
        {
            var doc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
            return doc.Declaration;
        }

        public void Save(string path)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            Save(Root, path, EolString(Eol), Encoding);
        }
        public void Save(XElement root, string path)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            Save(root, path, EolString(Eol), Encoding);
        }
        public void Save(XElement root, string path, string eol, Encoding encoding)
        {
            var declare = GetDeclaration(csproj);

            // gen xml
            string xml;
            if (declare == null)
            {
                xml = root.ToString();
            }
            else
            {
                xml = declare.ToString();
                xml += eol;
                xml += root.ToString();
            }
            // add line end
            xml += eol;

            // write
            var bytes = encoding.GetBytes(xml);
            File.WriteAllBytes(path, bytes);
        }

        public void Replace(string name, string key, string value)
        {
            Replace(Root, name, key, value);
        }
        public void Replace(XElement root, string name, string key, string value)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementBase = root.Elements(ns + name).Elements(ns + key).ToArray();
            if (!elementBase.Any()) return;

            // replace
            foreach (var item in elementBase)
            {
                item.Value = value;
            }
        }

        public void Insert(string name, string key, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            Insert(Root, name, key, value, EolString(Eol));
        }
        public void Insert(XElement root, string name, string key, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            Insert(root, name, key, value, EolString(Eol));
        }
        public void Insert(XElement root, string name, string key, string value, string eol)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementsBase = root.Elements(ns + name).Elements(ns + key).ToArray();
            if (elementsBase.Any()) return;

            // get space
            var elements = elementsBase.Select(x => x?.ToString()).Where(x => x != null);
            if (ns != null)
            {
                var nsString = GetNameSpace(root, ns);
                elements = elements.Select(x => x.Replace(nsString, ""));
            }
            var space = GetIntentSpace($"<{name}>", elements.ToArray(), eol);

            // insert element
            root.Element(ns + name).Add(space, new XElement(ns + key, value), "\n", space);
        }

        public void InsertAttribute(string name, string attribute, string key, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertAttribute(Root, name, attribute, key, value, EolString(Eol));
        }
        public void InsertAttribute(XElement root, string name, string attribute, string key, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertAttribute(root, name, attribute, key, value, EolString(Eol));
        }
        public void InsertAttribute(XElement root, string name, string attribute, string key, string value, string eol)
        {
            var ns = root.Name.Namespace;
            // validation
            var element = root.Elements(ns + name).Elements(ns + attribute).Where(x => x.FirstAttribute?.ToString() == value).Any();
            if (element) return;

            // get space
            var elements = root.Element(ns + name).Elements().Select(x => x?.ToString()).Where(x => x != null);
            if (ns != null)
            {
                var nsString = GetNameSpace(root, ns);
                elements = elements.Select(x => x.Replace(nsString, ""));
            }
            var space = GetIntentSpace($"<{name}>", elements.ToArray(), eol);

            // insert element
            root.Element(ns + name).Add(space, new XElement(ns + attribute, new XAttribute(key, value)), eol, space);
        }

        private string GetNameSpace(XElement root, XNamespace ns)
        {
            var namespaceString = root.LastAttribute.ToString();
            var nsString = namespaceString.Contains(ns.ToString())
                ? $" {namespaceString}"
                : $" {root.LastAttribute.Name}=\"{ns}\"";
            return nsString;
        }

        private string GetIntentSpace(string element, string[] insideElement, string Eol)
        {
            var entries = Root.ToString().Split(new[] { Eol }, StringSplitOptions.RemoveEmptyEntries);
            var elementSpace = entries.Where(x => x.Contains(element)).Select(x => x?.IndexOf("<")).FirstOrDefault() ?? baseSpaceNum;
            var insideElementSpace = insideElement != null && insideElement.Any()
                ? insideElement.Where(x => !x.Contains(Eol)).SelectMany(y => entries.Where(x => x.Contains(y)).Select(x => x?.IndexOf(y.First()) ?? baseSpaceNum)).Min()
                : 0;
            var diff = insideElementSpace - elementSpace;
            var space = diff >= 0 ? new string(' ', diff) : new string(' ', baseSpaceNum);
            return space;
        }

        private string GetIntentSpace(string path, string element, string[] insideElement, string Eol)
        {
            var entries = File.ReadAllLines(path);
            var elementSpace = entries.Where(x => x.Contains(element)).Select(x => x?.IndexOf("<")).FirstOrDefault() ?? baseSpaceNum;
            var insideElementSpace = insideElement != null && insideElement.Any()
                ? insideElement.Where(x => !x.Contains(Eol)).SelectMany(y => entries.Where(x => x.Contains(y)).Select(x => x?.IndexOf(y.First()) ?? baseSpaceNum)).Min()
                : 0;
            var diff = insideElementSpace - elementSpace;
            var space = diff >= 0 ? new string(' ', diff) : new string(' ', baseSpaceNum);
            return space;
        }
    }
}
