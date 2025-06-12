//2023-10-02 VPL
//not using this at the moment, but might use it for future refactoring
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper
{
  public class UndgHelper
  {
    public void AddUNDG(string imoClass, string flashPoint, string properShippingName, string undgCode, string techicalName, string packingGroup, string packQty, string weight, string marinePollutantCode)
    {
      var undg = new Undg();

      undg.IMOClass = imoClass;
      undg.FlashPoint = flashPoint;
      undg.ProperShippingName = properShippingName;
      undg.UNDGCode = undgCode;
      undg.TechicalName = techicalName;
      undg.PackingGroup = packingGroup;
      undg.PackQty = packQty;
      undg.Weight = weight;
      undg.MarinePollutantCode = marinePollutantCode;

      undgs.Add(undg);
    }

    public List<Undg> NewUNDGs()
    {
      return undgs;
    }

    List<Undg> undgs = new List<Undg>();

    public void Reset()
    {
      undgs = new List<Undg>();
    }

    public int Count()
    {
      return undgs.Count;
    }

    public XPathNodeIterator NewUndgCollection()
    {
      XmlDocument doc = new XmlDocument();
      var undgCollection = doc.CreateElement("UNDGCollection");
      doc.AppendChild(undgCollection);

      foreach (var undg in undgs)
      {
        var undgElement = doc.CreateElement("UNDG");

        var imoClassNode = doc.CreateElement(nameof(undg.IMOClass));
        imoClassNode.InnerText = undg.IMOClass.ToString();
        undgElement.AppendChild(imoClassNode);

        var flashPointNode = doc.CreateElement(nameof(undg.FlashPoint));
        flashPointNode.InnerText = undg.FlashPoint.ToString();
        undgElement.AppendChild(flashPointNode);

        var properShippingNameNode = doc.CreateElement(nameof(undg.ProperShippingName));
        properShippingNameNode.InnerText = undg.ProperShippingName.ToString();
        undgElement.AppendChild(properShippingNameNode);

        var undgCodeNode = doc.CreateElement(nameof(undg.UNDGCode));
        undgCodeNode.InnerText = undg.UNDGCode;
        undgElement.AppendChild(undgCodeNode);

        var techicalNameNode = doc.CreateElement(nameof(undg.TechicalName));
        techicalNameNode.InnerText = undg.TechicalName;
        undgElement.AppendChild(techicalNameNode);

        var packingGroupNode = doc.CreateElement(nameof(undg.PackingGroup));
        packingGroupNode.InnerText = undg.PackingGroup;
        undgElement.AppendChild(packingGroupNode);

        var packQtyNode = doc.CreateElement(nameof(undg.PackQty));
        packQtyNode.InnerText = undg.PackQty;
        undgElement.AppendChild(packQtyNode);

        var weightNode = doc.CreateElement(nameof(undg.Weight));
        weightNode.InnerText = undg.Weight;
        undgElement.AppendChild(weightNode);

        var marinePollutantCodeNode = doc.CreateElement(nameof(undg.MarinePollutantCode));
        marinePollutantCodeNode.InnerText = undg.MarinePollutantCode;
        undgElement.AppendChild(marinePollutantCodeNode);

        undgCollection.AppendChild(undgElement);
      }

      return doc.DocumentElement?.CreateNavigator().Select("UNDG");
    }
  }
}
