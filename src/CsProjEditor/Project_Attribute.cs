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
        /// Get attirbute Value for Group
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public CsProjAttribute[] GetAttribute(string group, bool onlyAttributed = false)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return GetAttribute(Root, group, onlyAttributed);
        }
        /// <summary>
        /// Get attirbute Value for Group
        /// </summary>
        /// <param name="root"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public CsProjAttribute[] GetAttribute(XElement root, string group, bool onlyAttributed = false)
        {
            var ns = root.Name.Namespace;
            // validation
            var element = onlyAttributed
                ? root.Elements(ns + group).Where(x => x.HasAttributes)
                : root.Elements(ns + group);
            return element.Select(x => x?.FirstAttribute == null ? null : new CsProjAttribute(x.FirstAttribute.Name.ToString(), x.FirstAttribute.Value?.ToString())).ToArray();
        }
        /// <summary>
        /// Get attirbute Value for Node
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public CsProjAttribute[] GetAttribute(string group, string node, bool onlyAttributed = false)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return GetAttribute(Root, group, node, onlyAttributed);
        }
        /// <summary>
        /// Get attirbute Value for Node
        /// </summary>
        /// <param name="root"></param>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public CsProjAttribute[] GetAttribute(XElement root, string group, string node, bool onlyAttributed = false)
        {
            var ns = root.Name.Namespace;
            // validation
            var element = onlyAttributed
                ? root.Elements(ns + group).Elements(ns + node).Where(x => x.HasAttributes)
                : root.Elements(ns + group).Elements(ns + node);
            return element.Select(x => new CsProjAttribute(x?.FirstAttribute?.Name?.ToString(), x?.FirstAttribute?.Value?.ToString())).ToArray();
        }

        /// <summary>
        /// Check attirbute is exists or not on Group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public bool ExistsAttribute(string group, CsProjAttribute attribute)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return ExistsAttribute(Root, group, attribute);
        }
        /// <summary>
        /// Check attirbute is exists or not on Group
        /// </summary>
        /// <param name="root"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public bool ExistsAttribute(XElement root, string group, CsProjAttribute attribute)
        {
            var ns = root.Name.Namespace;
            var attributeElements = root.Elements(ns + group).Where(x => x.HasAttributes);
            // validation
            var element = string.IsNullOrEmpty(attribute.Value)
                ? attributeElements.Where(x => x.Attributes().Any(xs => xs.Name == attribute.Name))
                : attributeElements.Where(x => x.Attributes().Any(xs => xs.Name == attribute.Name && xs.Value == attribute.Value));
            return element.Any();
        }
        /// <summary>
        /// Check attirbute is exists or not on Node
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public bool ExistsAttribute(string group, string node, CsProjAttribute attribute)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return ExistsAttribute(Root, group, node, attribute);
        }
        /// <summary>
        /// Check attirbute is exists or not on Node
        /// </summary>
        /// <param name="root"></param>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public bool ExistsAttribute(XElement root, string group, string node, CsProjAttribute attribute)
        {
            var ns = root.Name.Namespace;
            var attributeElements = root.Elements(ns + group).Elements(ns + node).Where(x => x.HasAttributes);
            // validation
            var element = string.IsNullOrEmpty(attribute.Value)
                ? attributeElements.Where(x => x.Attributes().Any(xs => xs.Name == attribute.Name))
                : attributeElements.Where(x => x.Attributes().Any(xs => xs.Name == attribute.Name && xs.Value == attribute.Value));
            return element.Any();
        }

        /// <summary>
        /// Insert attribute on Group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="attributes"></param>
        /// <param name="filterElement"></param>
        public void InsertAttribute(string group, CsProjAttribute attributes, Func<XElement, bool> filterElement, bool allowDuplicate = false)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertAttribute(Root, group, new[] { attributes }, Eol.ToEolString(), filterElement, allowDuplicate);
        }
        /// <summary>
        /// Insert attribute on Group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="attributes"></param>
        /// <param name="filterElement"></param>
        public void InsertAttribute(string group, CsProjAttribute[] attributes, Func<XElement, bool> filterElement, bool allowDuplicate = false)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertAttribute(Root, group, attributes, Eol.ToEolString(), filterElement, allowDuplicate);
        }
        /// <summary>
        /// Insert attribute on Group
        /// </summary>
        /// <param name="root"></param>
        /// <param name="group"></param>
        /// <param name="attributes"></param>
        /// <param name="eol"></param>
        /// <param name="filterElement"></param>
        public void InsertAttribute(XElement root, string group, CsProjAttribute[] attributes, string eol, Func<XElement, bool> filterElement, bool allowDuplicate = false)
        {
            var ns = root.Name.Namespace;
            // validation
            var checkedAttributes = allowDuplicate
                ? attributes
                : attributes.Where(attribute => !root.Elements(ns + group)
                    .Where(x => x.HasAttributes)
                    .Where(x => x.Attributes().Any(xs => xs.Name == attribute.Name && xs.Value == attribute.Value))
                    .Any())
                .ToArray();
            if (!checkedAttributes.Any()) return;

            // get space
            var x = root.Elements(ns + group).ToArray();
            var elements = root.Elements(ns + group).Where(filterElement).FirstOrDefault()?.Elements().Select(x => x?.ToString()).Where(x => x != null);
            if (ns != null && elements != null)
            {
                var nsString = GetNameSpace(root, ns);
                elements = elements.Select(x => x.Replace(nsString, ""));
            }
            var space = GetIndentSpace(root, $"<{group}>", elements?.ToArray(), eol);

            // insert attribute
            var xAttributes = checkedAttributes.Select(x => new XAttribute(x.Name, x.Value)).ToArray();
            root.Add(space, new XElement(ns + group, xAttributes), eol, space);
        }
        /// <summary>
        /// Insert attribute on Node
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attributes"></param>
        /// <param name="filterElement"></param>
        public void InsertAttribute(string group, string node, CsProjAttribute attributes, Func<XElement, bool> filterElement, bool allowDuplicate = false)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertAttribute(Root, group, node, new[] { attributes }, Eol.ToEolString(), filterElement, allowDuplicate);
        }
        /// <summary>
        /// Insert attribute on Node
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attributes"></param>
        /// <param name="filterElement"></param>
        public void InsertAttribute(string group, string node, CsProjAttribute[] attributes, Func<XElement, bool> filterElement, bool allowDuplicate = false)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertAttribute(Root, group, node, attributes, Eol.ToEolString(), filterElement, allowDuplicate);
        }
        /// <summary>
        /// Insert attribute on Node
        /// </summary>
        /// <param name="root"></param>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attributes"></param>
        /// <param name="eol"></param>
        /// <param name="filterElement"></param>
        public void InsertAttribute(XElement root, string group, string node, CsProjAttribute[] attributes, string eol, Func<XElement, bool> filterElement, bool allowDuplicate = false)
        {
            var ns = root.Name.Namespace;
            // validation
            var checkedAttributes = allowDuplicate
                ? attributes
                : attributes.Where(attribute => !root.Elements(ns + group).Elements(ns + node)
                    .Where(x => x.HasAttributes)
                    .Where(x => x.Attributes().Any(xs => xs.Name == attribute.Name && xs.Value == attribute.Value))
                    .Any())
                .ToArray();
            if (!checkedAttributes.Any()) return;

            // get space
            var elements = root.Elements(ns + group).Where(filterElement).FirstOrDefault()?.Elements().Select(x => x?.ToString()).Where(x => x != null);
            if (ns != null && elements != null)
            {
                var nsString = GetNameSpace(root, ns);
                elements = elements.Select(x => x.Replace(nsString, ""));
            }
            var space = GetIndentSpace(root, $"<{group}>", elements.ToArray(), eol);

            // insert attribute
            var xAttributes = checkedAttributes.Select(x => new XAttribute(x.Name, x.Value)).ToArray();
            root.Element(ns + group).Add(space, new XElement(ns + node, xAttributes), eol, space);
        }

        /// <summary>
        /// Set attribute on Group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="attribute"></param>
        /// <param name="filterElement"></param>
        public void SetAttribute(string group, CsProjAttribute attribute, Func<XElement, bool> filterElement)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            SetAttribute(Root, group, attribute, filterElement);
        }
        /// <summary>
        /// Set attribute on Group
        /// </summary>
        /// <param name="root"></param>
        /// <param name="group"></param>
        /// <param name="attribute"></param>
        /// <param name="filterElement"></param>
        public void SetAttribute(XElement root, string group, CsProjAttribute attribute, Func<XElement, bool> filterElement)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = root.Elements(ns + group).Where(filterElement);
            if (!elements.Any()) return;

            // set attribute
            foreach (var target in elements)
            {
                target.SetAttributeValue(attribute.Name, attribute.Value);
            }
        }
        /// <summary>
        /// Set attribute on Node
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="filterElement"></param>
        public void SetAttribute(string group, string node, CsProjAttribute attribute, Func<XElement, bool> filterElement)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            SetAttribute(Root, group, node, attribute, filterElement);
        }
        /// <summary>
        /// Set attribute on Node
        /// </summary>
        /// <param name="root"></param>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="filterElement"></param>
        public void SetAttribute(XElement root, string group, string node, CsProjAttribute attribute, Func<XElement, bool> filterElement)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = root.Elements(ns + group).Elements(ns + node).Where(filterElement);
            if (!elements.Any()) return;

            // set attribute
            foreach (var target in elements)
            {
                target.SetAttributeValue(attribute.Name, attribute.Value);
            }
        }

        /// <summary>
        /// Remove attirbute on Group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="attribute"></param>
        public void RemoveAttribute(string group, CsProjAttribute attribute)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            RemoveAttribute(Root, group, attribute);
        }
        /// <summary>
        /// Remove attirbute on Group
        /// </summary>
        /// <param name="root"></param>
        /// <param name="group"></param>
        /// <param name="attribute"></param>
        public void RemoveAttribute(XElement root, string group, CsProjAttribute attribute)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = string.IsNullOrEmpty(attribute.Value)
                ? root.Elements(ns + group)
                    .Where(x => x.HasAttributes)
                    .Where(x => x.Attributes().Any(xs => xs.Name == attribute.Name))
                    .ToArray()
                : root.Elements(ns + group)
                    .Where(x => x.HasAttributes)
                    .Where(x => x.Attributes().Any(xs => xs.Name == attribute.Name && xs.Value == attribute.Value))
                    .ToArray();
            if (!elements.Any()) return;

            // remove attribute
            foreach (var item in elements)
            {
                item.FirstAttribute.Remove();
            }
        }
        /// <summary>
        /// Remove attirbute on Node
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        public void RemoveAttribute(string group, string node, CsProjAttribute attribute)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            RemoveAttribute(Root, group, node, attribute);
        }
        /// <summary>
        /// Remove attirbute on Node
        /// </summary>
        /// <param name="root"></param>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        public void RemoveAttribute(XElement root, string group, string node, CsProjAttribute attribute)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = string.IsNullOrEmpty(attribute.Value)
                ? root.Elements(ns + group).Elements(ns + node)
                    .Where(x => x.HasAttributes)
                    .Where(x => x.Attributes().Any(xs => xs.Name == attribute.Name))
                    .ToArray()
                : root.Elements(ns + group).Elements(ns + node)
                    .Where(x => x.HasAttributes)
                    .Where(x => x.Attributes().Any(xs => xs.Name == attribute.Name && xs.Value == attribute.Value))
                    .ToArray();
            if (!elements.Any()) return;

            // remove attribute
            foreach (var item in elements)
            {
                item.FirstAttribute.Remove();
            }
        }

        /// <summary>
        /// Replace attirbute on Group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="attribute"></param>
        /// <param name="replacement"></param>
        /// <param name="option"></param>
        public void ReplaceAttribute(string group, CsProjAttribute attribute, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            ReplaceAttribute(Root, group, attribute, attribute.Name, replacement, option);
        }
        /// <summary>
        /// Replace attirbute on Group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="attribute"></param>
        /// <param name="pattern"></param>
        /// <param name="replacement"></param>
        /// <param name="option"></param>
        public void ReplaceAttribute(string group, CsProjAttribute attribute, string pattern, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            ReplaceAttribute(Root, group, attribute, pattern, replacement, option);
        }
        /// <summary>
        /// Replace attirbute on Group
        /// </summary>
        /// <param name="root"></param>
        /// <param name="group"></param>
        /// <param name="attribute"></param>
        /// <param name="pattern"></param>
        /// <param name="replacement"></param>
        /// <param name="option"></param>
        public void ReplaceAttribute(XElement root, string group, CsProjAttribute attribute, string pattern, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = root.Elements(ns + group)
                .Where(x => x.HasAttributes)
                .Where(x => x.Attributes().Any(xs => xs.Name == attribute.Name && xs.Value == attribute.Value))
                .ToArray();
            if (!elements.Any()) return;

            // replace attribute
            foreach (var item in elements)
            {
                var replaced = Regex.Replace(item.FirstAttribute.Name.LocalName, pattern, replacement, option);
                item.ReplaceAttributes(new XAttribute(replaced, item.FirstAttribute.Value));
            }
        }
        /// <summary>
        /// Replace attirbute on Node
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="replacement"></param>
        /// <param name="option"></param>
        public void ReplaceAttribute(string group, string node, CsProjAttribute attribute, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            ReplaceAttribute(Root, group, node, attribute, attribute.Name, replacement, option);
        }
        /// <summary>
        /// Replace attirbute on Node
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="pattern"></param>
        /// <param name="replacement"></param>
        /// <param name="option"></param>
        public void ReplaceAttribute(string group, string node, CsProjAttribute attribute, string pattern, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            ReplaceAttribute(Root, group, node, attribute, pattern, replacement, option);
        }
        /// <summary>
        /// Replace attirbute on Node
        /// </summary>
        /// <param name="root"></param>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="attribute"></param>
        /// <param name="pattern"></param>
        /// <param name="replacement"></param>
        /// <param name="option"></param>
        public void ReplaceAttribute(XElement root, string group, string node, CsProjAttribute attribute, string pattern, string replacement, RegexOptions option = RegexOptions.Multiline | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            var ns = root.Name.Namespace;
            // validation
            var elements = root.Elements(ns + group).Elements(ns + node)
                .Where(x => x.HasAttributes)
                .Where(x => x.Attributes().Any(xs => xs.Name == attribute.Name && xs.Value == attribute.Value))
                .ToArray();
            if (!elements.Any()) return;

            // replace attribute
            foreach (var item in elements)
            {
                var replaced = Regex.Replace(item.FirstAttribute.Name.LocalName, pattern, replacement, option);
                item.ReplaceAttributes(new XAttribute(replaced, item.FirstAttribute.Value));
            }
        }

        #endregion
    }
}
