using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CsProjEditor
{
    public class CsProjEditor
    {
        private readonly int baseSpaceNum = 2;

        private readonly string csproj;
        private readonly string pfx;
        private readonly string thumbprint;

        public CsProjEditor(string csproj, string pfx, string thumbprint)
        {
            // 0: path_to_csproj 
            // 1: pfx_file_name
            // 2: pfx_thumbprint
            this.csproj = csproj;
            this.pfx = pfx;
            this.thumbprint = thumbprint;
        }

        // MEMO: APP pfx and Package.StoreAssociation.xml should prepare/exists in advanced. (with manual relation on Visual Studio)
        /// <summary>
        /// TODO: Edit to make UWP Store associated csproj.
        /// </summary>
        public void Edit()
        {
            if (!File.Exists(csproj)) return;

            // prepare
            var encoding = GetUtf8Encoding(csproj);
            var eol = GetEndOfLine(csproj);
            var root = XElement.Load(csproj, LoadOptions.PreserveWhitespace);
            var declare = GetDeclaration(csproj);

            // edit
            Replace(root, "PropertyGroup", "PackageCertificateKeyFile", pfx);
            Insert(root, "PropertyGroup", "PackageCertificateThumbprint", thumbprint);
            Insert(root, "PropertyGroup", "GenerateAppInstallerFile", "False");
            Insert(root, "PropertyGroup", "AppxAutoIncrementPackageRevision", "True");
            Insert(root, "PropertyGroup", "AppxSymbolPackageEnabled", "False");
            Insert(root, "PropertyGroup", "AppxBundle", "Always");
            Insert(root, "PropertyGroup", "AppxBundlePlatforms", "x86");
            Insert(root, "PropertyGroup", "AppInstallerUpdateFrequency", "1");
            Insert(root, "PropertyGroup", "AppInstallerCheckForUpdateFrequency", "OnApplicationRun");
            InsertAttribute(root, "ItemGroup", "None", "Include", pfx);
            InsertAttribute(root, "ItemGroup", "None", "Include", "Package.StoreAssociation.xml");

            // write
            Write(root, csproj, declare, eol.letter, encoding);
        }

        /// <summary>
        /// determine utf8 contains bom from first 3 bytes of file.
        /// </summary>
        /// <remarks>
        /// UTF8 BOM == EFBBBF == 239, 187, 191
        /// </remarks>
        /// <param name="path"></param>
        /// <returns></returns>
        private static Encoding GetUtf8Encoding(string path)
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
        private static (string letter, bool isCRLF) GetEndOfLine(string path)
        {
            using (var reader = new FileStream(path, FileMode.Open))
            {
                var bits = new byte[1024];
                reader.Read(bits, 0, 1024);
                var isCRLF = bits.Where((e, i) => i < bits.Length - 1)
                    .Select((e, i) => (left: e, right: bits[i + 1]))
                    .Where(x => x.left == 13 && x.right == 10)
                    .Any();

                return (isCRLF ? "\r\n" : "\n", isCRLF);
            }
        }

        private static XDeclaration GetDeclaration(string path)
        {
            var doc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
            return doc.Declaration;
        }

        public void Write(XElement root, string path, XDeclaration declare, string eol, Encoding encoding)
        {
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

        public void Insert(XElement root, string name, string key, string value)
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
            var space = GetIntentSpace(csproj, $"<{name}>", elements.ToArray());

            // insert element
            root.Element(ns + name).Add(space, new XElement(ns + key, value), "\n", space);
        }

        public void InsertAttribute(XElement root, string name, string attribute, string key, string value)
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
            var space = GetIntentSpace(csproj, $"<{name}>", elements.ToArray());

            // insert element
            root.Element(ns + name).Add(space, new XElement(ns + attribute, new XAttribute(key, value)), "\n", space);
        }

        private string GetNameSpace(XElement root, XNamespace ns)
        {
            var namespaceString = root.LastAttribute.ToString();
            var nsString = namespaceString.Contains(ns.ToString())
                ? $" {namespaceString}"
                : $" {root.LastAttribute.Name}=\"{ns}\"";
            return nsString;
        }

        private string GetIntentSpace(string path, string element, string[] insideElement)
        {
            var file = File.ReadAllLines(path);
            var elementSpace = file.Where(x => x.Contains(element)).Select(x => x?.IndexOf("<")).FirstOrDefault() ?? baseSpaceNum;
            var insideElementSpace = insideElement != null && insideElement.Any()
                ? insideElement.Where(x => !x.Contains("\n")).SelectMany(y => file.Where(x => x.Contains(y)).Select(x => x?.IndexOf(y.First()) ?? baseSpaceNum)).Min()
                : 0;
            var diff = insideElementSpace - elementSpace;
            var space = diff >= 0 ? new string(' ', diff) : new string(' ', baseSpaceNum);
            return space;
        }
    }
}
