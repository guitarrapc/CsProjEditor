using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static CsProjEditor.XmlUtils;

namespace CsProjEditor
{
    public partial class Project
    {
        #region group operation

        /// <summary>
        /// Get groups
        /// </summary>
        /// <returns></returns>
        public string[] GetGroups()
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return GetGroups(Root);
        }
        public string[] GetGroups(XElement root)
        {
            var ns = root.Name.Namespace;
            var elementsBase = root.Elements();
            return elementsBase.Select(x => x.Name.LocalName).ToArray();
        }

        /// <summary>
        /// Get group
        /// </summary>
        /// <returns></returns>
        public string[] GetGroup(string group)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return GetGroup(Root, group);
        }
        public string[] GetGroup(XElement root, string group)
        {
            var ns = root.Name.Namespace;
            var elementsBase = root.Elements(ns + group);
            return elementsBase.Select(x => x.Name.LocalName).ToArray();
        }

        /// <summary>
        /// Check group is exists or not
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool ExistsGroup(string group)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            return ExistsGroup(Root, group);
        }
        public bool ExistsGroup(XElement root, string group)
        {
            var ns = root.Name.Namespace;
            var elementsBase = root.Elements(ns + group);
            return elementsBase.Any();
        }

        /// <summary>
        /// Insert node
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="value"></param>
        public void InsertGroup(string group)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertGroup(Root, group, Eol.ToEolString());
        }
        public void InsertGroup(XElement root, string group)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            InsertGroup(root, group, Eol.ToEolString());
        }
        public void InsertGroup(XElement root, string group, string eol)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementsBase = root.Elements().ToArray();

            // get space
            var elements = elementsBase.Select(x => x?.ToString()).Where(x => x != null);
            if (ns != null)
            {
                var nsString = GetNameSpace(root, ns);
                elements = elements.Select(x => x.Replace(nsString, ""));
            }
            var space = GetIndentSpace(root, $"<{group}>", elements.ToArray(), eol);

            // insert group
            root.Add(space, new XElement(ns + group, eol, space), eol);
        }

        /// <summary>
        /// Remove group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="leaveBrankLine"></param>
        public void RemoveGroup(string group, bool leaveBrankLine = false)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            RemoveGroup(Root, group, Eol.ToEolString(), leaveBrankLine);
        }
        public void RemoveNode(XElement root, string group, bool leaveBrankLine = false)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            RemoveGroup(root, group, Eol.ToEolString(), leaveBrankLine);
        }
        public void RemoveGroup(XElement root, string group, string eol, bool leaveBrankLine)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementsBase = root.Elements(ns + group).ToArray();
            if (!elementsBase.Any()) return;

            if (leaveBrankLine)
            {
                // remove node, this leave node as brank line.
                root.Element(ns + group).Remove();
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

                // remove group and do not leave brank line.
                // ReplaceAll Element to be nothing + Add removed elements
                var parent = root.Element(ns + group);
                var before = parent.ElementsBeforeSelf();
                var after = parent.ElementsAfterSelf();
                var removed = before.Concat(after).ToArray();
                root.ReplaceAll(eol, space);
                foreach (var item in removed)
                {
                    root.Add(space, item, eol);
                }
            }
        }

        /// <summary>
        /// Replace group
        /// </summary>
        /// <param name="group"></param>
        /// <param name="node"></param>
        /// <param name="replacement"></param>
        /// <param name="option"></param>
        public void ReplaceGroup(string group, string replacement, RegexOptions option = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            ReplaceGroup(Root, group, group, replacement, option);
        }
        public void ReplaceGroup(string group, string pattern, string replacement, RegexOptions option = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            if (!Initialized) throw new Exception("Detected not yet initialized, please run Load() first.");
            ReplaceGroup(Root, group, pattern, replacement, option);
        }
        public void ReplaceGroup(XElement root, string group, string pattern, string replacement, RegexOptions option = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)
        {
            var ns = root.Name.Namespace;
            // validation
            var elementsBase = root.Elements(ns + group).ToArray();
            if (!elementsBase.Any()) return;

            // replace node.
            var origin = root.Element(ns + group);
            var replaced = Regex.Replace(origin.Name.LocalName, pattern, replacement, option);
            if (origin.Name.LocalName != replaced)
            {
                origin.Name = ns + replaced;
            }
        }

        #endregion
    }
}
