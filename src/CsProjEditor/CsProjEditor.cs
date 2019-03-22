using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CsProjEditor
{
    public class CsProjEditor
    {
        public enum EolType { CRLF, LF, }

        private static readonly Func<EolType, string> EolString = eol => eol == EolType.CRLF ? "\r\n" : "\n";
        private readonly int baseSpaceNum = 2;

        private readonly string csproj;

        public EolType Eol { get; private set; }
        public Encoding Encoding { get; private set; }
        public XElement Root { get; private set; }
        public bool Initialized { get; private set; }

        private CsProjEditor(string path) => this.csproj = path;

        /// <summary>
        /// Load csproj from path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
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
            var eol = EolString(Eol);
            return ToXmlString(Root, eol);
        }

        private string ToXmlString(XElement root, string eol)
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
            return xml;
        }

        #region File Operation

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
            var xml = ToXmlString(root, eol);

            // write
            var bytes = encoding.GetBytes(xml);
            File.WriteAllBytes(path, bytes);
        }

        #endregion

        #region node operation

        public void InsertNode(string name, string key, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertNode(Root, name, key, value, EolString(Eol));
        }
        public void InsertNode(XElement root, string name, string key, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertNode(root, name, key, value, EolString(Eol));
        }
        public void InsertNode(XElement root, string name, string key, string value, string eol)
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
            var space = GetIntentSpace(root, $"<{name}>", elements.ToArray(), eol);

            // insert node
            root.Element(ns + name).Add(space, new XElement(ns + key, value), "\n", space);
        }

        public void ReplaceNode(string name, string key, string replacement, RegexOptions option = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            ReplaceNode(Root, name, key, key, replacement, option);
        }
        public void ReplaceNode(string name, string key, string pattern, string replacement, RegexOptions option = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            ReplaceNode(Root, name, key, pattern, replacement, option);
        }
        public void ReplaceNode(XElement root, string name, string key, string pattern, string replacement, RegexOptions option = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementsBase = root.Elements(ns + name).Elements(ns + key).ToArray();
            if (!elementsBase.Any()) return;

            // replace node.
            var origin = root.Element(ns + name).Element(ns + key);
            var replaced = Regex.Replace(origin.Name.LocalName, pattern, replacement, option);
            if (origin.Name.LocalName != replaced)
            {
                origin.Name = ns + replaced;
            }
        }

        public void RemoveNode(string name, string key, bool leaveBrankLine = false)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            RemoveNode(Root, name, key, EolString(Eol), leaveBrankLine);
        }
        public void RemoveNode(XElement root, string name, string key, bool leaveBrankLine = false)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            RemoveNode(root, name, key, EolString(Eol), leaveBrankLine);
        }
        public void RemoveNode(XElement root, string name, string key, string eol, bool leaveBrankLine)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementsBase = root.Elements(ns + name).Elements(ns + key).ToArray();
            if (!elementsBase.Any()) return;

            if (leaveBrankLine)
            {
                // remove node, this leave node as brank line.
                root.Element(ns + name).Element(ns + key).Remove();
            }
            else
            {
                // get space
                var elements = elementsBase.Select(x => x?.ToString()).Where(x => x != null);
                if (ns != null)
                {
                    var nsString = GetNameSpace(root, ns);
                    elements = elements.Select(x => x.Replace(nsString, ""));
                }
                var space = GetIntentSpace(root, $"<{name}>", elements.ToArray(), eol);

                // remove node and do not leave brank line.
                // ReplaceAll Element to be nothing + Add removed elements
                var parent = root.Element(ns + name).Element(ns + key);
                var before = parent.ElementsBeforeSelf();
                var after = parent.ElementsAfterSelf();
                var removed = before.Concat(after).ToArray();
                root.Element(ns + name).ReplaceAll(eol, space);
                foreach (var item in removed)
                {
                    root.Element(ns + name).Add(space, item, eol, space);
                }
            }
        }

        #endregion

        #region Value Operation

        public void ReplaceValue(string name, string key, string pattern, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            ReplaceValue(Root, name, key, pattern, replacement, option);
        }
        public void ReplaceValue(XElement root, string name, string key, string pattern, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementBase = root.Elements(ns + name).Elements(ns + key);
            if (!elementBase.Any()) return;

            // replace value
            var origin = root.Element(ns + name).Element(ns + key);
            var replaced = Regex.Replace(origin.Value, pattern, replacement, option);
            if (replaced != origin.Value)
            {
                origin.Value = replaced;
            }
        }

        public void RemoveValue(string name, string key)
        {
            RemoveValue(Root, name, key);
        }
        public void RemoveValue(XElement root, string name, string key)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementBase = root.Elements(ns + name).Elements(ns + key);
            if (!elementBase.Any()) return;

            // remove value
            root.Element(ns + name).Element(ns + key).ReplaceAll();
        }

        public void AppendValue(string name, string key, string value)
        {
            AppendValue(Root, name, key, value);
        }
        public void AppendValue(XElement root, string name, string key, string value)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementBase = root.Elements(ns + name).Elements(ns + key).ToArray();
            if (!elementBase.Any()) return;

            // append value
            foreach (var item in elementBase)
            {
                item.Value += value;
            }
        }
        public void PrependValue(string name, string key, string value)
        {
            PrependValue(Root, name, key, value);
        }
        public void PrependValue(XElement root, string name, string key, string value)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementBase = root.Elements(ns + name).Elements(ns + key).ToArray();
            if (!elementBase.Any()) return;

            // prepend value
            foreach (var item in elementBase)
            {
                item.Value = value + item.Value;
            }
        }
        public void SetValue(string name, string key, string value)
        {
            SetValue(Root, name, key, value);
        }
        public void SetValue(XElement root, string name, string key, string value)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementBase = root.Elements(ns + name).Elements(ns + key).ToArray();
            if (!elementBase.Any()) return;

            // set value
            foreach (var item in elementBase)
            {
                item.Value = value;
            }
        }

        #endregion

        #region Attirbute Operation

        public void InsertAttribute(string name, string key, string attribute, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertAttribute(Root, name, key, attribute, value, EolString(Eol));
        }
        public void InsertAttribute(XElement root, string name, string key, string attribute, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertAttribute(root, name, key, attribute, value, EolString(Eol));
        }
        public void InsertAttribute(XElement root, string name, string key, string attribute, string value, string eol)
        {
            var ns = root.Name.Namespace;
            // validation
            var element = root.Elements(ns + name).Elements(ns + key).Where(x => x.FirstAttribute?.ToString() == $"{attribute}=\"{value}\"").Any();
            if (element) return;

            // get space
            var elements = root.Element(ns + name).Elements().Select(x => x?.ToString()).Where(x => x != null);
            if (ns != null)
            {
                var nsString = GetNameSpace(root, ns);
                elements = elements.Select(x => x.Replace(nsString, ""));
            }
            var space = GetIntentSpace(root, $"<{name}>", elements.ToArray(), eol);

            // insert attribute
            root.Element(ns + name).Add(space, new XElement(ns + key, new XAttribute(attribute, value)), eol, space);
        }

        public void RemoveAttribute(string name, string key, string attribute, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            RemoveAttribute(Root, name, key, attribute, value);
        }
        public void RemoveAttribute(XElement root, string name, string key, string attribute, string value)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = root.Elements(ns + name).Elements(ns + key).Where(x => x.FirstAttribute?.ToString() == $"{attribute}=\"{value}\"").ToArray();
            if (!elements.Any()) return;

            // remove attribute
            foreach (var item in elements)
            {
                item.FirstAttribute.Remove();
            }
        }

        #endregion

        #region utils
        private XDeclaration GetDeclaration(string path)
        {
            var doc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
            return doc.Declaration;
        }

        private string GetNameSpace(XElement root, XNamespace ns)
        {
            var namespaceString = root.LastAttribute.ToString();
            var nsString = namespaceString.Contains(ns.ToString())
                ? $" {namespaceString}"
                : $" {root.LastAttribute.Name}=\"{ns}\"";
            return nsString;
        }

        private string GetIntentSpace(XElement root, string element, string[] insideElement, string eol)
        {
            var entries = root.ToString().Split(new[] { eol }, StringSplitOptions.RemoveEmptyEntries);
            var elementSpace = entries.Where(x => x.Contains(element)).Select(x => x?.IndexOf("<")).FirstOrDefault() ?? baseSpaceNum;
            var insideElementSpace = insideElement != null && insideElement.Any()
                ? insideElement.Where(x => !x.Contains(eol)).SelectMany(y => entries.Where(x => x.Contains(y)).Select(x => x?.IndexOf(y.First()) ?? baseSpaceNum)).Min()
                : 0;
            var diff = insideElementSpace - elementSpace;
            var space = diff >= 0 ? new string(' ', diff) : new string(' ', baseSpaceNum);
            return space;
        }

        private string GetIntentSpace(string path, string element, string[] insideElement, string eol)
        {
            var entries = File.ReadAllLines(path);
            var elementSpace = entries.Where(x => x.Contains(element)).Select(x => x?.IndexOf("<")).FirstOrDefault() ?? baseSpaceNum;
            var insideElementSpace = insideElement != null && insideElement.Any()
                ? insideElement.Where(x => !x.Contains(eol)).SelectMany(y => entries.Where(x => x.Contains(y)).Select(x => x?.IndexOf(y.First()) ?? baseSpaceNum)).Min()
                : 0;
            var diff = insideElementSpace - elementSpace;
            var space = diff >= 0 ? new string(' ', diff) : new string(' ', baseSpaceNum);
            return space;
        }

        #endregion
    }
}
