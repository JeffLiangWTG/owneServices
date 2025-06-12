using System.Xml.Linq;
using CargoWise.eHub.Products.RailInc.Schemas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting.Simple;
using System;

namespace CargoWise.eHub.Products.RailInc.Tests
{
  [TestClass]
  public class CarLocationExceptionSchemaTests
  {
    public void AssertTestCase(String testcase)
    {
      var ff_source = "CarLocationMessageY2UniversalInterchange.TestFiles.Text." + testcase + "_FF.txt";
      var xml_source = "CarLocationMessageY2UniversalInterchange.TestFiles.Text." + testcase + "_Input.xml";
      var generated_xml = XDocument.Load(SchemaTester<CLEDetail>.ParseFF(TestHelper.GetEmbeddedResource(ff_source))).ToString(); // XDocument is used for formatting
      var formatted_xml = XDocument.Parse(TestHelper.GetResourceAsString(xml_source)).ToString(); // XDocument is used for formatting
      Assert.AreEqual(formatted_xml, generated_xml);
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarLocationExceptionSchema()
    {
      AssertTestCase("CLE");
    }
  }
}
