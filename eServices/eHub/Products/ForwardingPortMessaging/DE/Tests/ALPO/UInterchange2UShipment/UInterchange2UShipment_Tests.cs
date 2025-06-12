using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.ForwardingPortMessaging.DE.ALPO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.DE.Tests
{
  [TestClass]
  public class UInterchange2UShipment_Tests
  {
    const string filePath = "ALPO.UInterchange2UShipment.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUInterchange2UShipment()
    {
      AssertMapping("Test1_input_AUFTRAG.xml", "Test1_output_AUFTRAG.xml");
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