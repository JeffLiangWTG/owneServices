using System.IO;
using System.Xml;
using CargoWise.eHub.Products.ACAS.Schemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;
using Winterdom.BizTalk.PipelineTesting.Simple;

namespace CargoWise.eHub.Products.ACAS.US.Tests
{
  [TestClass]
  public class Schemas_Test : BaseSchemaTests
  {
    const string filePath = "Schemas.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestACAS_US_Envelope_Schema()
    {
      AssertSchemaOutputXML<ACAS_US_Envelope>("ACAS_US_Envelop_PER_Input.txt", "ACAS_US_Envelop_PER_Output.xml");
      AssertSchemaOutputXML<ACAS_US_Envelope>("ACAS_US_Envelop_PSN_Input.txt", "ACAS_US_Envelop_PSN_Output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestACAS_US_PER_Schema()
    {
      AssertSchemaOutputXML<ACAS_US_PER>("ACAS_US_PER_1_Input.txt", "ACAS_US_PER_1_Output.xml");
      AssertSchemaOutputXML<ACAS_US_PER>("ACAS_US_PER_2_Input.txt", "ACAS_US_PER_2_Output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestACAS_US_PSN_Schema()
    {
      AssertSchemaOutputXML<ACAS_US_PSN>("ACAS_US_PSN_1_Input.txt", "ACAS_US_PSN_1_Output.xml");
      AssertSchemaOutputXML<ACAS_US_PSN>("ACAS_US_PSN_2_Input.txt", "ACAS_US_PSN_2_Output.xml");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestACAS_US_ASN_Schema()
    {
      AssertSchemaOutputFlatFile<ACAS_US_ASN>("ACAS_US_ASN_Input.xml", "ACAS_US_ASN_Output.txt");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestACAS_US_FHL_Schema()
    {
      AssertSchemaOutputFlatFile<ACAS_US_FHL>("ACAS_US_FHL_Input.xml", "ACAS_US_FHL_Output.txt");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestACAS_US_FRI_Schema()
    {
      AssertSchemaOutputFlatFile<ACAS_US_FRI>("ACAS_US_FRI_1_Input.xml", "ACAS_US_FRI_1_Output.txt");
      AssertSchemaOutputFlatFile<ACAS_US_FRI>("ACAS_US_FRI_2_Input.xml", "ACAS_US_FRI_2_Output.txt");
    }

    void AssertSchemaOutputXML<T>(string inputFile, string outputFile) where T : SchemaBase
    {
      string inputXMLResourceName = filePath + inputFile;
      string expectedOutputFlatFileResourceName = filePath + outputFile;

      using (var inputMsg = GetEmbeddedResource(inputXMLResourceName))
      using (var outputMsg = SchemaTester<T>.ParseFF(inputMsg))
      using (var sr = new StreamReader(outputMsg))
      {
        var expectedResult = GetResourceAsString(expectedOutputFlatFileResourceName);
        var actualResult = sr.ReadToEnd();

        XmlDocument xmlDocumentExpected = new XmlDocument();
        XmlDocument xmlDocumentActual = new XmlDocument();
        var expectedResultInString = expectedResult.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
        var actualResultInString = actualResult.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
        xmlDocumentExpected.LoadXml(expectedResultInString);
        xmlDocumentActual.LoadXml(actualResultInString);

        Assert.AreEqual(xmlDocumentExpected.InnerXml, xmlDocumentActual.InnerXml);
      }
    }

    void AssertSchemaOutputFlatFile<T>(string inputFile, string outputFile) where T : SchemaBase
    {
      string inputXMLResourceName = filePath + inputFile;
      string expectedOutputFlatFileResourceName = filePath + outputFile;

      using (var inputMsg = GetEmbeddedResource(inputXMLResourceName))
      using (var outputMsg = SchemaTester<T>.AssembleFF(inputMsg))
      using (var sr = new StreamReader(outputMsg))
      {
        Assert.AreEqual(GetResourceAsString(expectedOutputFlatFileResourceName), sr.ReadToEnd());
      }
    }
  }
}
