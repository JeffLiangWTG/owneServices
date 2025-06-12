using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.ForwardingPortMessaging.BE.CPOINT;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Tests
{
  [TestClass]
  public class UInterchange2UShipment_Tests
  {
    const string filePath = "CPOINT.UInterchange2UShipment.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUS2UI()
    {
      AssertMapping("Test1_input_EBADEC_UInterchange.xml", "Test1_output_EBADEC_UShipment.xml");
      AssertMapping("Test2_input_CertifiedPickup_UInterchange.xml", "Test2_output_CertifiedPickup_UInterchange.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mapTester = new MapTester(Assembly.GetExecutingAssembly());
      mapTester.ExecuteCompiled<UInterchange2UShipment>(input, expectedOutput);
    }
  }
}