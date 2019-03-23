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

        public bool ExistsNode(string group, string node)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return ExistsNode(Root, group, node);
        }
        public bool ExistsNode(XElement root, string group, string node)
        {
            var ns = root.Name.Namespace;
            var elementsBase = root.Elements(ns + group).Elements(ns + node);
            return elementsBase.Any();
        }

        public void InsertNode(string group, string node, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertNode(Root, group, node, value, EolString(Eol));
        }
        public void InsertNode(XElement root, string group, string node, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertNode(root, group, node, value, EolString(Eol));
        }
        public void InsertNode(XElement root, string group, string node, string value, string eol)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementsBase = root.Elements(ns + group).Elements(ns + node).ToArray();
            if (elementsBase.Any()) return;

            // get space
            var elements = elementsBase.Select(x => x?.ToString()).Where(x => x != null);
            if (ns != null)
            {
                var nsString = GetNameSpace(root, ns);
                elements = elements.Select(x => x.Replace(nsString, ""));
            }
            var space = GetIntentSpace(root, $"<{group}>", elements.ToArray(), eol);

            // insert node
            root.Element(ns + group).Add(space, new XElement(ns + node, value), eol, space);
        }

        public void ReplaceNode(string group, string node, string replacement, RegexOptions option = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            ReplaceNode(Root, group, node, node, replacement, option);
        }
        public void ReplaceNode(string group, string node, string pattern, string replacement, RegexOptions option = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            ReplaceNode(Root, group, node, pattern, replacement, option);
        }
        public void ReplaceNode(XElement root, string group, string node, string pattern, string replacement, RegexOptions option = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementsBase = root.Elements(ns + group).Elements(ns + node).ToArray();
            if (!elementsBase.Any()) return;

            // replace node.
            var origin = root.Element(ns + group).Element(ns + node);
            var replaced = Regex.Replace(origin.Name.LocalName, pattern, replacement, option);
            if (origin.Name.LocalName != replaced)
            {
                origin.Name = ns + replaced;
            }
        }

        public void RemoveNode(string group, string node, bool leaveBrankLine = false)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            RemoveNode(Root, group, node, EolString(Eol), leaveBrankLine);
        }
        public void RemoveNode(XElement root, string group, string node, bool leaveBrankLine = false)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            RemoveNode(root, group, node, EolString(Eol), leaveBrankLine);
        }
        public void RemoveNode(XElement root, string group, string node, string eol, bool leaveBrankLine)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementsBase = root.Elements(ns + group).Elements(ns + node).ToArray();
            if (!elementsBase.Any()) return;

            if (leaveBrankLine)
            {
                // remove node, this leave node as brank line.
                root.Element(ns + group).Element(ns + node).Remove();
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
                var space = GetIntentSpace(root, $"<{group}>", elements.ToArray(), eol);

                // remove node and do not leave brank line.
                // ReplaceAll Element to be nothing + Add removed elements
                var parent = root.Element(ns + group).Element(ns + node);
                var before = parent.ElementsBeforeSelf();
                var after = parent.ElementsAfterSelf();
                var removed = before.Concat(after).ToArray();
                root.Element(ns + group).ReplaceAll(eol, space);
                foreach (var item in removed)
                {
                    root.Element(ns + group).Add(space, item, eol, space);
                }
            }
        }

        #endregion

        #region node value operation

        public bool ExistsNodeValue(string group, string node, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return ExistsNodeValue(Root, group, node, value);
        }
        public bool ExistsNodeValue(XElement root, string group, string node, string value)
        {
            var ns = root.Name.Namespace;
            var elementsBase = root.Elements(ns + group).Elements(ns + node).Where(x => x.Value == value);
            return elementsBase.Any();
        }

        public void ReplaceValue(string group, string node, string value, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            ReplaceValue(Root, group, node, value, value, replacement, option);
        }
        public void ReplaceValue(string group, string node, string value, string pattern, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            ReplaceValue(Root, group, node, value, pattern, replacement, option);
        }
        public void ReplaceValue(XElement root, string group, string node, string value, string pattern, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementBase = root.Elements(ns + group).Elements(ns + node).Where(x => x.Value == value);
            if (!elementBase.Any()) return;

            // replace value
            foreach (var item in elementBase)
            {
                var replaced = Regex.Replace(item.Value, pattern, replacement, option);
                item.Value = replaced;
            }
        }

        public void RemoveValue(string group, string node)
        {
            RemoveValue(Root, group, node);
        }
        public void RemoveValue(XElement root, string group, string node)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementBase = root.Elements(ns + group).Elements(ns + node);
            if (!elementBase.Any()) return;

            // remove value
            root.Element(ns + group).Element(ns + node).ReplaceAll();
        }

        public void AppendValue(string group, string node, string value)
        {
            AppendValue(Root, group, node, value);
        }
        public void AppendValue(XElement root, string group, string node, string value)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementBase = root.Elements(ns + group).Elements(ns + node).ToArray();
            if (!elementBase.Any()) return;

            // append value
            foreach (var item in elementBase)
            {
                item.Value += value;
            }
        }
        public void PrependValue(string group, string node, string value)
        {
            PrependValue(Root, group, node, value);
        }
        public void PrependValue(XElement root, string group, string node, string value)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementBase = root.Elements(ns + group).Elements(ns + node).ToArray();
            if (!elementBase.Any()) return;

            // prepend value
            foreach (var item in elementBase)
            {
                item.Value = value + item.Value;
            }
        }
        public void SetValue(string group, string node, string value)
        {
            SetValue(Root, group, node, value);
        }
        public void SetValue(XElement root, string group, string node, string value)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementBase = root.Elements(ns + group).Elements(ns + node).ToArray();
            if (!elementBase.Any()) return;

            // set value
            foreach (var item in elementBase)
            {
                item.Value = value;
            }
        }

        #endregion

        #region attirbute operation

        public void InsertAttribute(string group, string node, string attribute, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertAttribute(Root, group, node, attribute, value, EolString(Eol));
        }
        public void InsertAttribute(XElement root, string group, string node, string attribute, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertAttribute(root, group, node, attribute, value, EolString(Eol));
        }
        public void InsertAttribute(XElement root, string group, string node, string attribute, string value, string eol)
        {
            var ns = root.Name.Namespace;
            // validation
            var element = root.Elements(ns + group).Elements(ns + node).Where(x => x.FirstAttribute?.ToString() == $"{attribute}=\"{value}\"").Any();
            if (element) return;

            // get space
            var elements = root.Element(ns + group).Elements().Select(x => x?.ToString()).Where(x => x != null);
            if (ns != null)
            {
                var nsString = GetNameSpace(root, ns);
                elements = elements.Select(x => x.Replace(nsString, ""));
            }
            var space = GetIntentSpace(root, $"<{group}>", elements.ToArray(), eol);

            // insert attribute
            root.Element(ns + group).Add(space, new XElement(ns + node, new XAttribute(attribute, value)), eol, space);
        }

        public void RemoveAttribute(string group, string node, string attribute, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            RemoveAttribute(Root, group, node, attribute, value);
        }
        public void RemoveAttribute(XElement root, string group, string node, string attribute, string value)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = root.Elements(ns + group).Elements(ns + node).Where(x => x.FirstAttribute?.ToString() == $"{attribute}=\"{value}\"").ToArray();
            if (!elements.Any()) return;

            // remove attribute
            foreach (var item in elements)
            {
                item.FirstAttribute.Remove();
            }
        }

        public void ReplaceAttribute(string group, string node, string attribute, string value, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            ReplaceAttribute(Root, group, node, attribute, value, value, replacement, option);
        }
        public void ReplaceAttribute(string group, string node, string attribute, string value, string pattern, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            ReplaceAttribute(Root, group, node, attribute, value, pattern, replacement, option);
        }
        public void ReplaceAttribute(XElement root, string group, string node, string attribute, string value, string pattern, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = root.Elements(ns + group).Elements(ns + node).Where(x => x.FirstAttribute?.ToString() == $"{attribute}=\"{value}\"").ToArray();
            if (!elements.Any()) return;

            // replace attribute
            foreach (var item in elements)
            {
                var replaced = Regex.Replace(item.FirstAttribute.Value, pattern, replacement);
                item.SetAttributeValue(attribute, replaced);
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
