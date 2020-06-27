using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static CsProjEditor.XmlUtils;

namespace CsProjEditor
{
    public partial class Project
    {
        #region node operation

        /// <summary>
        /// Get group's node values
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string[] GetNodes(string group)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return GetNodes(Root, group);
        }
        public string[] GetNodes(XElement root, string group)
        {
            var ns = root.Name.Namespace;
            var elementsBase = root.Elements(ns + group);
            return elementsBase
                .SelectMany(xs => xs.Elements().Select(y => y.Name.LocalName))
                .ToArray();
        }
        /// <summary>
        /// Get group's node values
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string[] GetNodes(string group, int index)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return GetNodes(Root, group, index);
        }
        public string[] GetNodes(XElement root, string group, int index)
        {
            var ns = root.Name.Namespace;
            var elementsBase = root.Elements(ns + group);
            return elementsBase
                .Skip(index).FirstOrDefault() // get index element
                ?.Elements()
                .Select(y => y.Name.LocalName)
                .ToArray();
        }
        /// <summary>
        /// Get node's value
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string[] GetNode(string group, string node)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return GetNode(Root, group, node);
        }
        public string[] GetNode(XElement root, string group, string node)
        {
            var ns = root.Name.Namespace;
            var elementsBase = root.Elements(ns + group).Elements(ns + node);
            return elementsBase.Select(x => x.Name.LocalName).ToArray();
        }

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
            var elementsBase = root.Elements(ns + group).Elements(ns + node).ToArray();

            // no validation for inserting node
            //if (elementsBase.Any()) return;

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

        #endregion
    }
}
