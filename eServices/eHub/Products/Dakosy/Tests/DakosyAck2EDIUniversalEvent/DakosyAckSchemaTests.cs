using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.Dakosy.BT.Schemas;
using Winterdom.BizTalk.PipelineTesting.Simple;
using System.Xml;

namespace CargoWise.eHub.Products.Dakosy.BT.Tests.DakosyAck2EDIUniversalEvent
{
  [TestClass]
  public class DakosyAckSchemaTests : BaseSchemaTests
  {
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestDakosyAck()
    {
      using (var inputMsg = GetEmbeddedResource("DakosyAck2EDIUniversalEvent.TestFiles.DakosyAck_input.txt"))
      using (var outputMsg = SchemaTester<DakosyAck>.ParseFF(inputMsg))
      using (var sr = new StreamReader(outputMsg))
      {
        var expectedResult = GetResourceAsString("DakosyAck2EDIUniversalEvent.TestFiles.DakosyAck_output.xml");
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
