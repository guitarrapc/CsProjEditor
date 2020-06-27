using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static CsProjEditor.XmlUtils;

namespace CsProjEditor
{
    public partial class Project
    {
        #region node value operation

        /// <summary>
        /// Get node's value
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string[] GetNodeValue(string group, string node)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return GetNodeValue(Root, group, node);
        }
        public string[] GetNodeValue(XElement root, string group, string node)
        {
            var ns = root.Name.Namespace;
            var elementsBase = root.Elements(ns + group).Elements(ns + node);
            return elementsBase.Select(x => x.Value).ToArray();
        }

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
        /// Set nodes value without specify which node by value match
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
        /// <summary>
        /// Set node's value
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        public void SetNodeValue(string group, string node, string value, string newValue)
        {
            SetNodeValue(Root, group, node, value, newValue);
        }
        public void SetNodeValue(XElement root, string group, string node, string value, string newValue)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementBase = root.Elements(ns + group).Elements(ns + node).Where(x => x.Value == value).ToArray();
            if (!elementBase.Any()) return;

            // set value
            foreach (var item in elementBase)
            {
                item.Value = newValue;
            }
        }

        /// <summary>
        /// Append node's value
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        public void AppendNodeValue(string group, string node, string value, string append)
        {
            AppendNodeValue(Root, group, node, value, append);
        }
        public void AppendNodeValue(XElement root, string group, string node, string value, string append)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementBase = root.Elements(ns + group).Elements(ns + node).Where(x => x.Value == value).ToArray();
            if (!elementBase.Any()) return;

            // append value
            foreach (var item in elementBase)
            {
                item.Value += append;
            }
        }
        /// <summary>
        /// Prepend node's value
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        public void PrependNodeValue(string group, string node, string value, string prepend)
        {
            PrependNodeValue(Root, group, node, value, prepend);
        }
        public void PrependNodeValue(XElement root, string group, string node, string value, string prepend)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementBase = root.Elements(ns + group).Elements(ns + node).Where(x => x.Value == value).ToArray();
            if (!elementBase.Any()) return;

            // prepend value
            foreach (var item in elementBase)
            {
                item.Value = prepend + item.Value;
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

        #endregion
    }
}
