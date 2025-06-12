using System.Xml.XPath;

namespace Hawking.Xslt.ExtensionObjects.Interfaces
{
    public interface IXmlHelper
    {
        XPathNodeIterator GetWithOverrides(XPathNodeIterator nodes);
        XPathNodeIterator GetWithOverrides(XPathNodeIterator nodes, string nodeName);
    }
}
