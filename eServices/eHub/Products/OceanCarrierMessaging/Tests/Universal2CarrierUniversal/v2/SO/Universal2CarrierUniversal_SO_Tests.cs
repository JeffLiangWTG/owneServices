using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v2;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class Universal2CarrierUniversal_SO_Tests
  {
    const string filePath = "Universal2CarrierUniversal.v2.SO.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal_v2_SO()
    {
      AssertMapping("Test1_input_UInterchange.xml", "Test1_input_UShipment.xml", "Test1_output_CarrierUniversal.xml");
      AssertMapping("Test2_input_UInterchange.xml", "Test2_input_UShipment.xml", "Test2_output_CarrierUniversal.xml");
      AssertMapping("Test3_input_UInterchange_LCL.xml", "Test3_input_UShipment_LCL.xml", "Test3_output_CarrierUniversal_LCL.xml");
      AssertMapping("Test4_input_UInterchange_Commodity.xml", "Test4_input_UShipment_Commodity.xml", "Test4_output_CarrierUniversal_Commodity.xml");
      AssertMapping("Test5_input_UInterchange_GroupingMethod.xml", "Test5_input_UShipment_GroupingMethod.xml", "Test5_output_CarrierUniversal_GroupingMethod.xml");
    }

    void AssertMapping(string universalInterchangeFile, string universalShipmentFile, string carrierUniversalFile)
    {
      var universalInterchange = filePath + universalInterchangeFile;
      var universalShipment = filePath + universalShipmentFile;
      var carrierUniversal = filePath + carrierUniversalFile;

      var mapTester = new MapTester(Assembly.GetExecutingAssembly());
      mapTester.Execute<UniversalInterchange2UniversalShipment_SO>(universalInterchange, universalShipment);

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("TRUE");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "DRT")).Return("FALSE");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "AGT")).Return("FALSE");

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper }
      };

      var mapTester2 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester2.Execute<Universal2CarrierUniversal_SO>(universalShipment, carrierUniversal);

      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
