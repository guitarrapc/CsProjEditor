using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static CsProjEditor.XmlUtils;

namespace CsProjEditor
{
    public partial class Project
    {
        #region attirbute value operation

        /// <summary>
        /// Get attirbute Value
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public string[] GetAttributeValue(string group, string node, string attribute)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return GetAttributeValue(Root, group, node, attribute);
        }
        public string[] GetAttributeValue(XElement root, string group, string node, string attribute)
        {
            var ns = root.Name.Namespace;
            // validation
            var element = root.Elements(ns + group).Elements(ns + node).Where(x => x?.FirstAttribute?.Name == attribute);
            return element.Select(x => x?.FirstAttribute?.Value).ToArray();
        }

        /// <summary>
        /// Check attirbute and value is exists or not
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ExistsAttributeValue(string group, string node, string attribute, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return ExistsAttributeValue(Root, group, node, attribute, value);
        }
        public bool ExistsAttributeValue(XElement root, string group, string node, string attribute, string value)
        {
            var ns = root.Name.Namespace;
            // validation
            var element = root.Elements(ns + group).Elements(ns + node).Where(x => x?.FirstAttribute?.Name == attribute && x?.FirstAttribute?.Value == value);
            return element.Any();
        }

        /// <summary>
        /// Set attribute's value without specify which attribute by value match
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        public void SetAttributeValue(string group, string node, string attribute, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            SetAttributeValue(Root, group, node, attribute, value);
        }
        public void SetAttributeValue(XElement root, string group, string node, string attribute, string value)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = root.Elements(ns + group).Elements(ns + node).Where(x => x?.FirstAttribute?.Name == attribute).ToArray();
            if (!elements.Any()) return;

            // set attribute value
            foreach (var item in elements)
            {
                item.FirstAttribute.Value = value;
            }
        }
        /// <summary>
        /// Set attribute
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        public void SetAttributeValue(string group, string node, string attribute, string value, string newValue)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            SetAttributeValue(Root, group, node, attribute, value, newValue);
        }
        public void SetAttributeValue(XElement root, string group, string node, string attribute, string value, string newValue)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = root.Elements(ns + group).Elements(ns + node).Where(x => x?.FirstAttribute?.Name == attribute && x?.FirstAttribute?.Value == value).ToArray();
            if (!elements.Any()) return;

            // set attribute value
            foreach (var item in elements)
            {
                item.FirstAttribute.Value = newValue;
            }
        }

        /// <summary>
        /// Append attribute
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        public void AppendAttributeValue(string group, string node, string attribute, string value, string append)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            AppendAttributeValue(Root, group, node, attribute, value, append);
        }
        public void AppendAttributeValue(XElement root, string group, string node, string attribute, string value, string append)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = root.Elements(ns + group).Elements(ns + node).Where(x => x?.FirstAttribute?.Name == attribute && x?.FirstAttribute?.Value == value).ToArray();
            if (!elements.Any()) return;

            // set attribute value
            foreach (var item in elements)
            {
                item.FirstAttribute.Value += append;
            }
        }

        /// <summary>
        /// Prepend attribute
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        public void PrependAttributeValue(string group, string node, string attribute, string value, string prepend)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            PrependAttributeValue(Root, group, node, attribute, value, prepend);
        }
        public void PrependAttributeValue(XElement root, string group, string node, string attribute, string value, string prepend)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = root.Elements(ns + group).Elements(ns + node).Where(x => x?.FirstAttribute?.Name == attribute && x?.FirstAttribute?.Value == value).ToArray();
            if (!elements.Any()) return;

            // set attribute value
            foreach (var item in elements)
            {
                item.FirstAttribute.Value = prepend + item.FirstAttribute.Value;
            }
        }

        /// <summary>
        /// Remove attirbute value
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        public void RemoveAttributeValue(string group, string node, string attribute, string value)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            RemoveAttributeValue(Root, group, node, attribute, value);
        }
        public void RemoveAttributeValue(XElement root, string group, string node, string attribute, string value)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = root.Elements(ns + group).Elements(ns + node).Where(x => x?.FirstAttribute?.Name == attribute && x?.FirstAttribute?.Value == value).ToArray();
            if (!elements.Any()) return;

            // remove attribute
            foreach (var item in elements)
            {
                item.FirstAttribute.Value = "";
            }
        }

        /// <summary>
        /// Replace attirbute value
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <param name="replacement"></param>
        /// <param name="option"></param>
        public void ReplaceAttributeValue(string group, string node, string attribute, string value, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            ReplaceAttributeValue(Root, group, node, attribute, value, value, replacement, option);
        }
        public void ReplaceAttributeValue(string group, string node, string attribute, string value, string pattern, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            ReplaceAttributeValue(Root, group, node, attribute, value, pattern, replacement, option);
        }
        public void ReplaceAttributeValue(XElement root, string group, string node, string attribute, string value, string pattern, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
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
