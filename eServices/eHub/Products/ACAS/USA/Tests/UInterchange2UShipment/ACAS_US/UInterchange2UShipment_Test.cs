using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.ACAS.US.Transforms.UInterchange2UShipment.ACAS_US;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Products.ACAS.US.Tests
{
  [TestClass]
  public class UInterchange2UShipment_ACAS_US_Test
  {
    const string filePath = "UInterchange2UShipment.ACAS_US.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUInterchange2UShipment()
    {
      AssertMapping<UInterchange2UShipment_FRI>("Test1_input_AirCargoAdvanceScreening.xml", "Test1_output_AirCargoAdvanceScreening.xml");
      AssertMapping<UInterchange2UShipment_ASN>("Test2_input_AcknowledgementOfHold.xml", "Test2_output_AcknowledgementOfHold.xml");
      AssertMapping<UInterchange2UShipment_FHL>("Test3_input_HouseCheckList.xml", "Test3_output_HouseCheckList.xml");
    }

    void AssertMapping<T>(string inputFile, string expectedOutputFile) where T : TransformBase
    {
      var sourceFile = filePath + inputFile;
      var expectedFile = filePath + expectedOutputFile;

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly());
      mapTester.ExecuteCompiled<T>(sourceFile, expectedFile);
    }
  }
}
