using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using CargoWise.eHub.Products.ACAS.BR.UInterchange2UShipment;
using Microsoft.XLANGs.BaseTypes;

namespace CargoWise.eHub.Products.ACAS.Tests
{
  [TestClass]
  public class UInterchange2UShipment_ACAS_BR_Test
  {
    const string filePath = "UInterchange2UShipment.CCT.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUInterchange2UShipment()
    {
      AssertMapping<UI2UShipment_FHL_CCT>("Test1_input_FHL.xml", "Test1_output_FHL.xml");
      AssertMapping<UI2UShipment_FZB_CCT>("Test2_input_FZB.xml", "Test2_output_FZB.xml");
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
