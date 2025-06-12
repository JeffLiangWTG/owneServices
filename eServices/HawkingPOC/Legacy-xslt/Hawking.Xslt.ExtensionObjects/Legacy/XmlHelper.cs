using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.XPath;
using Hawking.Xslt.ExtensionObjects.Interfaces;

namespace Hawking.Xslt.ExtensionObjects.Legacy
{
    public class XmlHelper : IXmlHelper
    {
        public XPathNodeIterator GetWithOverrides(XPathNodeIterator nodes)
        {
            if (nodes.Count == 0)
                return nodes;

            List<string> xpaths = new List<string>();

            while (nodes.MoveNext())
            {
                XPathNavigator node = nodes.Current;

                string overpath = (node.NodeType == XPathNodeType.Attribute)
                          ? "../../*[local-name()='DocumentaryOverride']/" + GetQualName(node.SelectSingleNode("..")) + "/@" + GetQualName(node)
                          : "../*[local-name()='DocumentaryOverride']/" + GetQualName(node);

                xpaths.Add(GetXPath(node.SelectSingleNode(overpath) ?? node));
            }

            return nodes.Current.Select(String.Join(" | ", xpaths));
        }

        public XPathNodeIterator GetWithOverrides(XPathNodeIterator nodes, string nodeName)
        {
            if (nodes.Count == 0)
                return nodes;

            var xpaths = new List<string>();

            while (nodes.MoveNext())
            {
                var overpath = string.Empty;
                var node = nodes.Current;

                var names = nodeName.Split(new[] { "/@" }, StringSplitOptions.None);
                var xpath = string.Empty;

                xpath = names.Length > 1 ? string.Format(CultureInfo.InvariantCulture, "./*[local-name()='DocumentaryOverride']/{0}/@*[local-name()='{1}']", GetQualName(node, names[0]), names[1]) :
                    string.Format(CultureInfo.InvariantCulture, "./*[local-name()='DocumentaryOverride']/{0}", GetQualName(node, names[0]));

                if (node.SelectSingleNode(xpath) == null)
                {
                    xpath = names.Length > 1 ? string.Format(CultureInfo.InvariantCulture, "./{0}/@*[local-name()='{1}']", GetQualName(node, names[0]), names[1])
                        : string.Format(CultureInfo.InvariantCulture, "./{0}", GetQualName(node, names[0]));
                }
                if (node.SelectSingleNode(xpath) != null)
                {
                    xpaths.Add(GetXPath(node.SelectSingleNode(xpath)));
                }
            }
            if (xpaths.Count > 0)
            {
                return nodes.Current.Select(String.Join(" | ", xpaths));
            }
            return nodes.Current.Select("./*[local-name()='NotExistingElementDAF6853F-0A19-4423-8264-F536A76C2F46']");
        }

        static string GetXPath(XPathNavigator node)
        {
            string attr = "";

            if (node.NodeType == XPathNodeType.Attribute)
            {
                attr = "/@" + GetQualName(node);
                node.MoveToParent();
            }

            string name = node.Name;
            string xpath = "/" + GetQualName(node);
            int pos = 1;

            while (node.MoveToPrevious())
                if (node.Name == name)
                    pos++;

            xpath += "[" + pos + "]" + attr;

            if (node.MoveToParent() && node.NodeType == XPathNodeType.Element)
                return GetXPath(node) + xpath;
            else
                return xpath;
        }

        static string GetQualName(XPathNavigator node)
        {
            return GetQualName(node, node.LocalName);
        }

        static string GetQualName(XPathNavigator node, string nodeName)
        {
            return "*[local-name()='" + nodeName + "' and namespace-uri()='" + node.NamespaceURI + "']";
        }
    }
}
