using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v3;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  public partial class Universal2CarrierUniversal_V3_Tests
  {
    [TestClass]
    public class Universal2CarrierUniversal_V3_BK_Tests
    {
      const string filePath = "Universal2CarrierUniversal.v3.BK.TestFiles.";

      [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
      public void TestUniversal2CarrierUniversal_v3_BK()
      {
        AssertMapping("Test01_UniversalShipment_BK_input.xml", "Test01_CarrierUniversal_BK_Output.xml");
        AssertMapping("Test12_U2CU_BK_MultipleVehicles_input.xml", "Test12_U2CU_BK_MultipleVehicles_output.xml");
				AssertMapping("Test16_U2CU_BK_NoMainLeg_Input.xml", "Test16_U2CU_BK_NoMainLeg_Output.xml");
        //genset is false
        AssertMapping("Test17_U2CU_BK_PreallocatedUNDGCollection_Input.xml", "Test17_U2CU_BK_PreallocatedUNDGCollection_Output.xml");
        //genset is true
        AssertMapping("Test18_U2CU_BK_WithoutPreallocatedUNDGCollection_Input.xml", "Test18_U2CU_BK_WithoutPreallocatedUNDGCollection_Output.xml");
      }

      public void AssertMapping(string inputFile, string expectedOutputFile)
      {
        var input = filePath + inputFile;
        var expectedOutput = filePath + expectedOutputFile;
        var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

        mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "DRT")).Return("FALSE");
        var extensionObjects = new Dictionary<string, object>()
        {
          {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper}
        };

        var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
        mapTester.Execute<Universal2CarrierUniversal_BK>(input, expectedOutput);

        mockCodeMapper.VerifyAllExpectations();
      }
    }
  }
}
