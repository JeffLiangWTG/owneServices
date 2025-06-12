using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UInterchange2UShipment_Tests
  {
    const string filePath = "Universal2CarrierUniversal.UInterchange2UShipment.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUInterchange2UShipment()
    {
      AssertMapping("Test01_input_UniversalInterchange_BK.xml", "Test01_output_UniversalShipment_BK.xml");
      AssertMapping("Test02_input_UniversalInterchange_SI.xml", "Test02_output_UniversalShipment_SI.xml");
      AssertMapping("Test03_input_UniversalInterchange_SO.xml", "Test03_output_UniversalShipment_SO.xml");
      AssertMapping("Test04_input_UInterchange_VGM.xml", "Test04_output_UShipment_VGM.xml");
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
