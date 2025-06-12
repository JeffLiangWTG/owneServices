using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper
{
  public class XMLHelper
  {
    public XPathNodeIterator ConvertToXMLNodes(string inputText, string elementName, string delimiter)
    {
      if (string.IsNullOrEmpty(inputText))
      {
        return null;
      }

      XmlDocument doc = new XmlDocument();
      var elementCollection = doc.CreateElement(elementName + "Collection");
      doc.AppendChild(elementCollection);

      var valueList = delimiter != "" ? inputText.Split(delimiter.ToCharArray()[0]) : new [] { inputText };

      if (valueList.Length > 0)
      {
        foreach (var item in valueList)
        {
          var value = item.Trim(' ');
          if (!string.IsNullOrEmpty(value))
          {
            var node = doc.CreateElement(elementName);
            node.InnerText = value;
            elementCollection.AppendChild(node);
          }
        }

      }
      return doc.DocumentElement?.CreateNavigator().Select(elementName);
    }
  }
}
