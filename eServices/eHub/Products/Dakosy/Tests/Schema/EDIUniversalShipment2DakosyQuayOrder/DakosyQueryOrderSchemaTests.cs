using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.Dakosy.BT.Schemas;
using Winterdom.BizTalk.PipelineTesting.Simple;
using System.Xml;

namespace CargoWise.eHub.Products.Dakosy.BT.Tests.Schema
{
  [TestClass]
  public class DakosyQueryOrderSchemaTests : BaseSchemaTests
  {
    const string filePath = "Schema.EDIUniversalShipment2DakosyQuayOrder.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void EDIUniversalShipment2DakosyQuayOrder_Schema()
    {
      AssertSchema("Test1_input_HDS_Senator.txt", "Test1_output_HDS_Senator.xml");
      AssertSchema("Test2_input_HDS_Consol_Container.txt", "Test2_output_HDS_Consol_Container.xml");
      AssertSchema("Test3_input_HDSwith1MRNand2Chassis_unwrapped.txt", "Test3_output_HDSwith1MRNand2Chassis_unwrapped.xml");
      AssertSchema("Test4_input_AE1.txt", "Test4_output_AE1.xml");
    }

    void AssertSchema(string inputFile, string outputFile)
    {
      var sourceFile = filePath + inputFile;
      var expectedOutputFile = filePath + outputFile;

      using (var inputMsg = GetEmbeddedResource(sourceFile))
      using (var outputMsg = SchemaTester<DakosyQuayOrder>.ParseFF(inputMsg))
      using (var sr = new StreamReader(outputMsg))
      {
        var expectedResult = GetResourceAsString(expectedOutputFile);
        var actualResult = sr.ReadToEnd();
        XmlDocument xmlDocumentExpected = new XmlDocument();
        XmlDocument xmlDocumentActual = new XmlDocument();
        xmlDocumentExpected.LoadXml(expectedResult);
        xmlDocumentActual.LoadXml(actualResult);
        Assert.AreEqual(xmlDocumentExpected.InnerXml, xmlDocumentActual.InnerXml);
      }
    }
  }
}
