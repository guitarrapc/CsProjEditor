using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static CsProjEditor.XmlUtils;

namespace CsProjEditor
{
    public partial class Project
    {
        #region attirbute operation

        /// <summary>
        /// Get attirbute Value
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public string[] GetAttribute(string group, string node)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return GetAttribute(Root, group, node);
        }
        public string[] GetAttribute(XElement root, string group, string node)
        {
            var ns = root.Name.Namespace;
            // validation
            var element = root.Elements(ns + group).Elements(ns + node);
            return element.Select(x => x?.FirstAttribute?.Name?.ToString()).ToArray();
        }

        /// <summary>
        /// Check attirbute is exists or not
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public bool ExistsAttribute(string group, string node, string attribute)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return ExistsAttribute(Root, group, node, attribute);
        }
        public bool ExistsAttribute(XElement root, string group, string node, string attribute)
        {
            var ns = root.Name.Namespace;
            // validation
            var element = root.Elements(ns + group).Elements(ns + node).Where(x => x?.FirstAttribute?.Name == attribute);
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
        /// Set attribute
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <param name="filterElement"></param>
        public void SetAttribute(string group, string node, string attribute, string value, Func<XElement, bool> filterElement)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            SetAttribute(Root, group, node, attribute, value, filterElement);
        }
        public void SetAttribute(XElement root, string group, string node, string attribute, string value, Func<XElement, bool> filterElement)
        {
            var ns = root.Name.Namespace;
            // validation
            var element = root.Elements(ns + group).Elements(ns + node).Where(x => x?.FirstAttribute?.Name != attribute);
            if (!element.Any()) return;

            // set attribute
            foreach (var target in element)
            {
                target.SetAttributeValue(attribute, value);
            }
        }

        /// <summary>
        /// Remove attirbute
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        public void RemoveAttribute(string group, string node, string attribute)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            RemoveAttribute(Root, group, node, attribute);
        }
        public void RemoveAttribute(XElement root, string group, string node, string attribute)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = root.Elements(ns + group).Elements(ns + node).Where(x => x?.FirstAttribute?.Name == attribute).ToArray();
            if (!elements.Any()) return;

            // remove attribute
            foreach (var item in elements)
            {
                item.FirstAttribute.Remove();
            }
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
        /// <param name="replacement"></param>
        /// <param name="option"></param>
        public void ReplaceAttribute(string group, string node, string attribute, string value, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            ReplaceAttribute(Root, group, node, attribute, value, attribute, replacement, option);
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
                var replaced = Regex.Replace(item.FirstAttribute.Name.LocalName, pattern, replacement);
                item.ReplaceAttributes(new XAttribute(replaced, item.FirstAttribute.Value));
            }
        }

        #endregion
    }
}
