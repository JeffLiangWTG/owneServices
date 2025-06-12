using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UInterchange2UShipment.V1;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UInterchange2UShipment_BLData_Tests
  {
    const string filePath = "UInterchange2UShipment.v1.BLData.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUS2UI_BLData()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
    }

    void AssertMapping(string inputFile, string expectedOutputFile)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mapTester = new MapTester(Assembly.GetExecutingAssembly());
      mapTester.ExecuteCompiled<UInterchange2UShipment_BLData>(input, expectedOutput);
    }
  }
}

