using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace CsProjEditor
{
    internal static class XmlUtils
    {
        private static readonly int baseSpaceNum = 2;

        public static XDeclaration GetDeclarationFromPath(string path)
        {
            var doc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
            return doc.Declaration;
        }

        public static XDeclaration GetDeclarationFromXml(string xml)
        {
            var doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);
            return doc.Declaration;
        }

        public static string GetNameSpace(XElement root, XNamespace ns)
        {
            var namespaceString = root.LastAttribute.ToString();
            var nsString = namespaceString.Contains(ns.ToString())
                ? $" {namespaceString}"
                : $" {root.LastAttribute.Name}=\"{ns}\"";
            return nsString;
        }
        public static string GetIndentSpace(XElement root, string element, string[] insideElement, string eol)
        {
            var entries = root.ToString().Split(new[] { eol }, StringSplitOptions.RemoveEmptyEntries);
            return GetIndentSpace(entries, element, insideElement, eol);
        }

        public static string GetIndentSpace(string path, string element, string[] insideElement, string eol)
        {
            var entries = File.ReadAllLines(path);
            return GetIndentSpace(entries, element, insideElement, eol);
        }

        private static string GetIndentSpace(string[] entries, string element, string[] insideElement, string eol)
        {
            var elementSpace = entries
                .Where(x => x.Contains(element))
                .Select(x => x?.IndexOf("<"))
                .FirstOrDefault() ?? baseSpaceNum;
            var insideElementSpace = insideElement != null && insideElement.Any()
                ? CalculateIndent(entries, insideElement, eol)
                : 0;
            var diff = insideElementSpace - elementSpace;
            var space = diff >= 0 ? new string(' ', diff) : new string(' ', baseSpaceNum);
            return space;
        }

        private static int CalculateIndent(string[] entries, string[] insideElement, string eol)
        {
            var matchElements = insideElement.Where(x => !x.Contains(eol)).ToArray();
            if (matchElements.Any())
            {
                var result = matchElements.SelectMany(y => entries.Where(x => x.Contains(y)).Select(x => x?.IndexOf(y.First()) ?? baseSpaceNum)).Min();
                return result;
            }
            else
            {
                return baseSpaceNum;
            }
        }
    }
}
