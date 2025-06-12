using System.Collections.Generic;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests2.Tests.XMLHelperTest
{
  [TestClass]
  public class XMLHelperTest
  {
    [TestMethod]
    public void TestConvertStringIntoXMLNodes()
    {
      var testData = "ITN11 ; ITN22 ; ITN33 ; ITN44 ; ";

      var xmlHelper = new XMLHelper();
      var xmlNodes = xmlHelper.ConvertToXMLNodes(testData, "Number", ";");

      Assert.AreEqual(4, xmlNodes.Count);


      var actualValues = new List<string>();
      while (xmlNodes.MoveNext())
      {
        actualValues.Add(xmlNodes.Current.Value);
      }
      CollectionAssert.AreEqual(new[] { "ITN11", "ITN22", "ITN33", "ITN44" }, actualValues);
    }
  }
}