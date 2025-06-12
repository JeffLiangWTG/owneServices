using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper
{
  public class MultipleRFFListHelper
  {
    public void AddRFF(string value, string regulatingCountry, string countryOfIssue, string addressType)
    {
      AddRFF(value, regulatingCountry, countryOfIssue, addressType, "", true);
    }

    public void AddRFF(string value, string regulatingCountry, string countryOfIssue, string addressType, string typeCode)
    {
      AddRFF(value, regulatingCountry, countryOfIssue, addressType, typeCode, true);
    }

    public void AddRFF(string value, string regulatingCountry, string countryOfIssue, string addressType, string typeCode, bool shouldAddPrefixForCountryOfIssue)
    {
      if(!string.IsNullOrEmpty(value))
      {
        var list = new RFF();
        list.Value = value;
        list.RegulatingCountry = regulatingCountry;
        list.CountryOfIssue = countryOfIssue;
        list.AddressType = addressType;
        list.TypeCode = typeCode;
        list.Order = countryOfIssue == regulatingCountry ? 1 : 2;
        list.ShouldAddPrefixForCountryOfIssue = shouldAddPrefixForCountryOfIssue;

        rffList.Add(list);
      }
    }

    public List<RFF> GNList()
    {
      return rffGNList;
    }

    public List<RFF> RestList()
    {
      return rffRestList;
    }

    List<RFF> rffList = new List<RFF>();
    List<RFF> rffGNList = new List<RFF>();
    List<RFF> rffRestList = new List<RFF>();

    public void Reset()
    {
      rffList = new List<RFF>();
      rffGNList = new List<RFF>();
      rffRestList = new List<RFF>();
    }

    public int Count()
    {
      return rffList.Count;
    }

    public int RFFGNCount()
    {
      return rffGNList.Count;
    }

    public int RFFRestCount()
    {
      return rffRestList.Count;
    }

    public void CalculateRFFList(int segment, string includeRegulatingCountry)
    {
      CalculateIncludeRegulatingCountry(includeRegulatingCountry);
      CalculateRFFList(segment);
      FormatRestValue(rffRestList);
    }

    void CalculateRFFList(int segment)
    {
      var notifyParty = rffList.Where(x => x.AddressType == "NI").OrderBy(x => x.Order).ToList();
      var notifyParty1 = rffList.Where(x => x.AddressType == "N1").OrderBy(x => x.Order).ToList();
      var consignee = rffList.Where(x => x.AddressType == "CN").OrderBy(x => x.Order).ToList();
      var consignor = rffList.Where(x => x.AddressType == "CZ").OrderBy(x => x.Order).ToList();
      var consignor1 = rffList.Where(x => x.AddressType == "SH").OrderBy(x => x.Order).ToList();

      FormatRFFList(notifyParty, segment);
      FormatRFFList(notifyParty1, segment);
      FormatRFFList(consignee, segment);
      FormatRFFList(consignor, segment);
      FormatRFFList(consignor1, segment);
    }

    void FormatRFFList(List<RFF> list, int segment)
    {

      var restCount = list.Count() - segment;
      if (restCount <= 0)
      {
        rffGNList.AddRange(list);
      }
      else
      {
        rffGNList.AddRange(list.GetRange(0, segment));
        rffRestList.AddRange(list.GetRange(segment, restCount));
      }
    }

    string GetType(string code)
    {
      switch (code)
      {
        case "NI":
        case "N1":
          return "Main Notify Party";
        case "CN":
          return "Consignee";
        case "CZ":
        case "SH":
          return "Consignor";
        default:
          return "";
      }
    }

    void FormatRestValue(List<RFF> restList)
    {
      foreach(var rest in restList)
      {
        var regulatingCountry = rest.RegulatingCountry;
        var numberAndCountry = string.IsNullOrEmpty(regulatingCountry) ? rest.Value : (rest.Value + ":" + regulatingCountry);
        if (rest.ShouldAddPrefixForCountryOfIssue && !string.IsNullOrEmpty(rest.TypeCode))
        {
          numberAndCountry = rest.TypeCode + ":" + numberAndCountry;
        }
        rest.Value = string.Format("{0} {1}", GetType(rest.AddressType), numberAndCountry);
      }
    }

    void CalculateIncludeRegulatingCountry(string includeRegulatingCountry)
    {
      if(includeRegulatingCountry == "false")
      {
        rffList.ForEach(x => x.RegulatingCountry = "");
      }
    }

    XPathNodeIterator NewRFFCollection(List<RFF> list)
    {
      if (list.Count == 0)
      {
        return null;
      }

      XmlDocument doc = new XmlDocument();
      var rffCollection = doc.CreateElement("RFFCollection");
      doc.AppendChild(rffCollection);

      foreach (var rffDetail in list)
      {
        var rff = doc.CreateElement("RFF");


        var valueNode = doc.CreateElement(nameof(rffDetail.Value));
        valueNode.InnerText = rffDetail.Value.ToString();
        rff.AppendChild(valueNode);

        var regulatingCountryNode = doc.CreateElement(nameof(rffDetail.RegulatingCountry));
        regulatingCountryNode.InnerText = rffDetail.RegulatingCountry.ToString();
        rff.AppendChild(regulatingCountryNode);

        var countryOfIssuealueNode = doc.CreateElement(nameof(rffDetail.CountryOfIssue));
        countryOfIssuealueNode.InnerText = rffDetail.CountryOfIssue.ToString();
        rff.AppendChild(countryOfIssuealueNode);

        var addressTypeNode = doc.CreateElement(nameof(rffDetail.AddressType));
        addressTypeNode.InnerText = rffDetail.AddressType.ToString();
        rff.AppendChild(addressTypeNode);

        var typeCodeNode = doc.CreateElement(nameof(rffDetail.TypeCode));
        typeCodeNode.InnerText = rffDetail.TypeCode.ToString();
        rff.AppendChild(typeCodeNode);

        rffCollection.AppendChild(rff);
      }

      return doc.DocumentElement?.CreateNavigator().Select("RFF");
    }

    public XPathNodeIterator RFFGNCollection()
    {
      return NewRFFCollection(rffGNList);
    }

    public XPathNodeIterator RFFRestCollection()
    {
      return NewRFFCollection(rffRestList);
    }
  }

  public class RFF
  {
    public string Value { get; set; }
    public string RegulatingCountry { get; set; }
    public string CountryOfIssue { get; set; }
    public string AddressType { get; set; }
    public string TypeCode { get; set; }
    public int Order { get; set; }
    public bool ShouldAddPrefixForCountryOfIssue { get; set; }
  }
}
