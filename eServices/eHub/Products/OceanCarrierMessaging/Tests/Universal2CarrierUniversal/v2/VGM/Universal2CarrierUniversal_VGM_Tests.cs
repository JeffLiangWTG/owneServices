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
  public class Universal2CarrierUniversal_VGM_Tests
  {
    const string filePath = "Universal2CarrierUniversal.v2.VGM.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal_v2_VGM()
    {
      AssertMapping("Test1_AGT_input_UInterchange.xml", "Test1_AGT_input_UShipment.xml", "Test1_AGT_output_CarrierUniversal.xml");
      AssertMapping("Test2_CLD_input_UInterchange.xml", "Test2_CLD_input_UShipment.xml", "Test2_CLD_output_CarrierUniversal.xml");
    }

    void AssertMapping(string universalInterchangeFile, string universalShipmentFile, string carrierUniversalFile)
    {
      var universalInterchange = filePath + universalInterchangeFile;
      var universalShipment = filePath + universalShipmentFile;
      var carrierUniversal = filePath + carrierUniversalFile;

      var mapTester = new MapTester(Assembly.GetExecutingAssembly());
      mapTester.Execute<UniversalInterchange2UniversalShipment_VGM>(universalInterchange, universalShipment);

      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "AGT")).Return("FALSE");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("TRUE");

      var extensionObjects = new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper }
      };

      var mapTester2 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester2.Execute<Universal2CarrierUniversal_VGM>(universalShipment, carrierUniversal);

      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
