using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;
using System.Collections.Generic;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v3;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class Universal2CarrierUniversal_V3_SO_Tests
  {
    const string filePath = "Universal2CarrierUniversal.v3.SO.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal_v3_SO()
    {
      AssertMapping("Test2_input_UShipment.xml", "Test2_output_CarrierUniversal.xml");
    }

    void AssertMapping(string universalShipmentFile, string carrierUniversalFile)
    {
      var universalShipment = filePath + universalShipmentFile;
      var carrierUniversal = filePath + carrierUniversalFile;

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
