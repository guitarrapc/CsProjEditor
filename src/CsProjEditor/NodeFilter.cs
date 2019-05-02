using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CsProjEditor
{
    internal readonly struct NodeFilter
    {
        public readonly string GroupName;
        public readonly string NodeName;
        public readonly string NodeValue;
        public readonly IEnumerable<string> AttributeNames;
        public readonly IEnumerable<string> AttributeValues;

        public NodeFilter(string groupName, string nodeName, string nodeValue, IEnumerable<string> attributeNames, IEnumerable<string> attributeValues)
            => (GroupName, NodeName, NodeValue, AttributeNames, AttributeValues) = (groupName, nodeName, nodeValue, attributeNames, attributeValues);
    }

    internal static class NodeFilterExtensions
    {
        public static IEnumerable<XElement> Filter(XElement root, Func<NodeFilter, bool> predicate)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            foreach (var group in root.Elements())
            {
                foreach (var node in group.Elements())
                {
                    var arg = new NodeFilter(
                        group.Name.LocalName,
                        node.Name.LocalName,
                        node.Value,
                        node?.Attributes().Select(x => x?.Name?.LocalName),
                        node?.Attributes().Select(x => x?.Value)
                    );
                    if (!predicate(arg)) continue;
                    yield return node;
                }
            }
        }
    }
}
