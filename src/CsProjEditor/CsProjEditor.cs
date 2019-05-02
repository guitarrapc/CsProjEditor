using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static CsProjEditor.XmlUtils;

namespace CsProjEditor
{
    public class CsProjEditor
    {
        private readonly string _path;
        private readonly string _xml;

        public EolType Eol { get; private set; }
        public Encoding Encoding { get; private set; }
        public XElement Root { get; private set; }
        public bool Initialized { get; private set; }

        private CsProjEditor(string path, string xml) => (_path, _xml) = (path, xml);

        /// <summary>
        /// Load csproj from path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static CsProjEditor Load(string path, LoadOptions options = LoadOptions.PreserveWhitespace)
        {
            var editor = new CsProjEditor(path, File.ReadAllText(path));
            editor.Root = XElement.Load(path, options);
            editor.Encoding = CsProjEditor.GetUtf8Encoding(path);
            editor.Eol = CsProjEditor.GetEndOfLine(path);
            editor.Initialized = true;
            return editor;
        }

        /// <summary>
        /// Load csproj from stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static CsProjEditor Load(Stream stream, LoadOptions options = LoadOptions.PreserveWhitespace)
        {
            StreamReader streamReader = null;
            BinaryReader binaryReader = null;
            try
            {
                string xml = "";
                // no `using statement` to re-use stream
                // manual read stream to get string
                streamReader = new StreamReader(stream);
                xml = streamReader.ReadToEnd();
                if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

                // manual read stream as binary to get bom
                byte[] bytes = new byte[3];
                binaryReader = new BinaryReader(stream);
                binaryReader.Read(bytes, 0, 3);
                if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);

                var editor = new CsProjEditor("", xml);
                editor.Root = XElement.Parse(xml, options);
                editor.Encoding = GetUtf8Encoding(bytes);
                editor.Eol = CsProjEditor.GetEndOfLineFromXml(xml);
                editor.Initialized = true;
                return editor;
            }
            finally
            {
                streamReader?.Close();
                binaryReader?.Close();
            }
        }

        public override string ToString()
        {
            var eol = Eol.ToEolString();
            return ToXmlString(Root, eol);
        }

        private string ToXmlString(XElement root, string eol)
        {
            var declare = GetDeclarationFromXml(_xml);

            // gen xml
            string xml;
            if (declare == null)
            {
                xml = root.ToString();
                xml = xml.Replace(EolType.CRLF.ToEolString(), eol);
            }
            else
            {
                xml = declare.ToString();
                xml += eol;
                xml += root.ToString();
                xml = xml.Replace(EolType.CRLF.ToEolString(), eol);
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
            using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var bytes = new byte[3];
                reader.Read(bytes, 0, 3);
                var isBom = bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
                return new System.Text.UTF8Encoding(isBom);
            }
        }
        public static Encoding GetUtf8Encoding(byte[] bytes)
        {
            var isBom = bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
            return new System.Text.UTF8Encoding(isBom);
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
            using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read))
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
        public static EolType GetEndOfLine(Stream stream)
        {
            var bits = new byte[1024];
            stream.Read(bits, 0, 1024);
            var isCRLF = bits.Where((e, i) => i < bits.Length - 1)
                .Select((e, i) => (left: e, right: bits[i + 1]))
                .Where(x => x.left == 13 && x.right == 10)
                .Any();
            return isCRLF ? EolType.CRLF : EolType.LF;
        }
        public static EolType GetEndOfLineFromXml(string xml)
        {
            var bits = xml.Take(1024).Select(x => Convert.ToByte(x)).ToArray();
            var isCRLF = bits.Where((e, i) => i < bits.Length - 1)
                .Select((e, i) => (left: e, right: bits[i + 1]))
                .Where(x => x.left == 13 && x.right == 10)
                .Any();
            return isCRLF ? EolType.CRLF : EolType.LF;
        }

        /// <summary>
        /// Save to destination path
        /// </summary>
        /// <param name="path"></param>
        public void Save(string path)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            Save(Root, path, Eol.ToEolString(), Encoding);
        }
        public void Save(XElement root, string path)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            Save(root, path, Eol.ToEolString(), Encoding);
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

        /// <summary>
        /// Check node is exists or not
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Insert node
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        public void InsertNode(string group, string node, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertNode(Root, group, node, value, Eol.ToEolString());
        }
        public void InsertNode(XElement root, string group, string node, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertNode(root, group, node, value, Eol.ToEolString());
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
            var space = GetIndentSpace(root, $"<{group}>", elements.ToArray(), eol);

            // insert node
            root.Element(ns + group).Add(space, new XElement(ns + node, value), eol, space);
        }

        /// <summary>
        /// Replace node
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="replacement"></param>
        /// <param name="option"></param>
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

        /// <summary>
        /// Remove node
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="leaveBrankLine"></param>
        public void RemoveNode(string group, string node, bool leaveBrankLine = false)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            RemoveNode(Root, group, node, Eol.ToEolString(), leaveBrankLine);
        }
        public void RemoveNode(XElement root, string group, string node, bool leaveBrankLine = false)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            RemoveNode(root, group, node, Eol.ToEolString(), leaveBrankLine);
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
                var space = GetIndentSpace(root, $"<{group}>", elements.ToArray(), eol);

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

        /// <summary>
        /// Check node's value is desired or not
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Replace node's value
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        /// <param name="replacement"></param>
        /// <param name="option"></param>
        public void ReplaceNodeValue(string group, string node, string value, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            ReplaceNodeValue(Root, group, node, value, value, replacement, option);
        }
        public void ReplaceNodeValue(string group, string node, string value, string pattern, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            ReplaceNodeValue(Root, group, node, value, pattern, replacement, option);
        }
        public void ReplaceNodeValue(XElement root, string group, string node, string value, string pattern, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
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

        /// <summary>
        /// Remove node's value
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        public void RemoveNodeValue(string group, string node)
        {
            RemoveNodeValue(Root, group, node);
        }
        public void RemoveNodeValue(XElement root, string group, string node)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementBase = root.Elements(ns + group).Elements(ns + node);
            if (!elementBase.Any()) return;

            // remove value
            root.Element(ns + group).Element(ns + node).ReplaceAll();
        }

        /// <summary>
        /// Append node's value
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        public void AppendNodeValue(string group, string node, string value)
        {
            AppendNodeValue(Root, group, node, value);
        }
        public void AppendNodeValue(XElement root, string group, string node, string value)
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
        /// <summary>
        /// Pretend node's value
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        public void PrependNodeValue(string group, string node, string value)
        {
            PrependNodeValue(Root, group, node, value);
        }
        public void PrependNodeValue(XElement root, string group, string node, string value)
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
        /// <summary>
        /// Set node's value
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        public void SetNodeValue(string group, string node, string value)
        {
            SetNodeValue(Root, group, node, value);
        }
        public void SetNodeValue(XElement root, string group, string node, string value)
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

        /// <summary>
        /// Check attirbute is exists or not
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ExistsAttribute(string group, string node, string attribute, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return ExistsAttribute(Root, group, node, attribute, value);
        }
        public bool ExistsAttribute(XElement root, string group, string node, string attribute, string value)
        {
            var ns = root.Name.Namespace;
            // validation
            var element = root.Elements(ns + group).Elements(ns + node).Where(x => x?.FirstAttribute?.Name == attribute && x?.FirstAttribute?.Value == value);
            return element.Any();
        }

        /// <summary>
        /// Insert attribute
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <param name="filterElement"></param>
        public void InsertAttribute(string group, string node, string attribute, string value, Func<XElement, bool> filterElement)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertAttribute(Root, group, node, attribute, value, Eol.ToEolString(), filterElement);
        }
        public void InsertAttribute(XElement root, string group, string node, string attribute, string value, Func<XElement, bool> filterElement)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertAttribute(root, group, node, attribute, value, Eol.ToEolString(), filterElement);
        }
        public void InsertAttribute(XElement root, string group, string node, string attribute, string value, string eol, Func<XElement, bool> filterElement)
        {
            var ns = root.Name.Namespace;
            // validation
            var element = root.Elements(ns + group).Elements(ns + node).Where(x => x?.FirstAttribute?.Name == attribute && x?.FirstAttribute?.Value == value).Any();
            if (element) return;

            // get space
            var elements = root.Elements(ns + group).Where(filterElement).FirstOrDefault()?.Elements().Select(x => x?.ToString()).Where(x => x != null);
            if (ns != null)
            {
                var nsString = GetNameSpace(root, ns);
                elements = elements.Select(x => x.Replace(nsString, ""));
            }
            var space = GetIndentSpace(root, $"<{group}>", elements.ToArray(), eol);

            // insert attribute
            root.Element(ns + group).Add(space, new XElement(ns + node, new XAttribute(attribute, value)), eol, space);
        }

        /// <summary>
        /// Remove attirbute
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        public void RemoveAttribute(string group, string node, string attribute, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            RemoveAttribute(Root, group, node, attribute, value);
        }
        public void RemoveAttribute(XElement root, string group, string node, string attribute, string value)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = root.Elements(ns + group).Elements(ns + node).Where(x => x?.FirstAttribute?.Name == attribute && x?.FirstAttribute?.Value == value).ToArray();
            if (!elements.Any()) return;

            // remove attribute
            foreach (var item in elements)
            {
                item.FirstAttribute.Remove();
            }
        }

        /// <summary>
        /// Replace attirbute
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <param name="replacement"></param>
        /// <param name="option"></param>
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
            var elements = root.Elements(ns + group).Elements(ns + node).Where(x => x?.FirstAttribute?.Name == attribute && x?.FirstAttribute?.Value == value).ToArray();
            if (!elements.Any()) return;

            // replace attribute
            foreach (var item in elements)
            {
                var replaced = Regex.Replace(item.FirstAttribute.Value, pattern, replacement);
                item.SetAttributeValue(attribute, replaced);
            }
        }

        #endregion
    }
}
