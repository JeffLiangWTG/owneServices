using System.Xml.XPath;

namespace Hawking.Xslt.ExtensionObjects.Interfaces
{
    public interface ICACustomsGroupingHelper
    {
        XPathNavigator GroupByPackingLine(XPathNodeIterator commercialInvoiceLineCollection, XPathNodeIterator packingLineCollection);
    }
}
