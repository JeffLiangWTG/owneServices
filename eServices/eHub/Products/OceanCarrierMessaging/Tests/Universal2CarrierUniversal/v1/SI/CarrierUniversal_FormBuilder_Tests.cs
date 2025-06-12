using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Universal2CarrierUniversal.v1;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  public partial class Universal2CarrierUniversal_V1_Tests
  {
    [TestClass]
    public class Universal2CarrierUniversal_SI_Tests
    {
      const string filePath = "Universal2CarrierUniversal.v1.SI.TestFiles.";

      [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
      public void TestCarrierUniversal_FormBuilder_v1_SI_UniversalInterchange()
      {
        AssertMapping_UniversalInterchange("Test01_UniversalInterchange_SI.xml", "Test01_UniversalShipment_SI.xml", "Test01_CarrierUniversal_SI_Output.xml");
        AssertMapping_UniversalInterchange("Test17_UniversalInterchange_GroupingMethod_SI.xml", "Test17_UniversalShipment_GroupingMethod_SI.xml", "Test17_CarrierUniversal_GroupingMethod_SI_Output.xml");
      }

      void AssertMapping_UniversalInterchange(string uInterchange, string uShipment, string cUniversal) 
      {
        var universalInterchange = filePath + uInterchange;
        var universalShipment = filePath + uShipment;
        var carrierUniversal = filePath + cUniversal;

        var mapTester = new MapTester(Assembly.GetExecutingAssembly());
        mapTester.Execute<UniversalInterchange2UniversalShipment_SI>(universalInterchange, universalShipment);

        var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
        var mockContextAccessor = MockRepository.GeneratePartialMock<ContextAccessor>();

        mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("TRUE");
        mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "DRT")).Return("FALSE");
        mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "AGT")).Return("FALSE");

        mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("INTTRA");
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Reference Label Type", "Use Long Reference", "INTTRA")).Return("false").Repeat.Any();

        mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Anything, Arg<string>.Is.Anything,
          Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal("Registration Type"),
          Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return("").Repeat.Any();

        var extensionObjects = new Dictionary<string, object>()
        {
          {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
          {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor}
        };

        var mapTester2 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
        mapTester2.Execute<Universal2CarrierUniversal_SI>(universalShipment, carrierUniversal);
      }

      [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
      public void TestCarrierUniversal_FormBuilder_v1_SI()
      {
        AssertMapping("Test02_U2CU_SI_PickupDeliveryAddress_input.xml", "Test02_U2CU_SI_PickupDeliveryAddress_output.xml");
        AssertMapping("Test03_U2CU_SI_SubShipmentCollection_input.xml", "Test03_U2CU_SI_SubShipmentCollection_output.xml");
        AssertMapping("Test04_U2CU_SI_FindITNUsingMinLegOrder_input.xml", "Test04_U2CU_SI_FindITNUsingMinLegOrder_output.xml");
        AssertMapping("Test05_U2CU_SI_FindPOLandPOD_input.xml", "Test05_U2CU_SI_FindPOLandPOD_output.xml");
        AssertMapping("Test06_U2CU_SI_LegType_input.xml", "Test06_U2CU_SI_LegType_output.xml");
        AssertMapping("Test07_U2CU_SI_NoOrgCode_input.xml", "Test07_U2CU_SI_NoOrgCode_output.xml");
        AssertMapping("Test08_U2CU_SI_Without_SubShipmentCollection_input.xml", "Test08_U2CU_SI_Without_SubShipmentCollection_output.xml");

        AssertMapping("Test09_U2CU_SI_VehicleCollection_input.xml", "Test09_U2CU_SI_VehicleCollection_output.xml");
        AssertMapping("Test10_U2CU_SI_VehicleCollection_3PT_input.xml", "Test10_U2CU_SI_VehicleCollection_3PT_output.xml");

        //FormVersion = 1.0.0
        AssertMapping("Test11_NewFormMessage_input.xml", "Test11_NewFormMessage_ShortReference_output.xml");
        AssertMapping("Test11_NewFormMessage_input.xml", "Test11_NewFormMessage_LongReference_output.xml", "CMACGM");

        AssertMapping("Test12_U2CU_SI_tempControl_packingLine_input.xml", "Test12_U2CU_SI_tempControl_packingLine_output.xml");

        AssertMapping("Test14_U2CU_SI_MultipleITN_input.xml", "Test14_U2CU_SI_MultipleITN_output.xml");

        //LCL CLD
        AssertMapping("Test13_U2CU_SI_LCL_input.xml", "Test13_U2CU_SI_LCL_output.xml");
        AssertMapping("Test15_U2CU_SI_DummyContainer_input.xml", "Test15_U2CU_SI_DummyContainer_output.xml");

        AssertMapping("Test16_U2CU_SI_Brazil_input.xml", "Test16_U2CU_SI_Brazil_output.xml");

        AssertMapping("Test18_UniversalShipment_GroupingMethod_SI.xml", "Test18_CarrierUniversal_GroupingMethod_SI_Output.xml");
        AssertMapping("Test19_input_UniversalShipment_GroupingMethod.xml", "Test19_output_CarrierUniversal_GroupingMethod_SI.xml");
      }

      void AssertMapping(string inputFile, string expectedOutputFile, string destinationParty = "INTTRA")
      {
        var input = filePath + inputFile;
        var expectedOutput = filePath + expectedOutputFile;
        var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
        var mockContextAccessor = MockRepository.GeneratePartialMock<ContextAccessor>();

        mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);

        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "CLD")).Return("TRUE").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "AGT")).Return("FALSE").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "STD")).Return("FALSE").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "NVOCC", "Is Co-load", "DRT")).Return("FALSE").Repeat.Any();

        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "AU", "", "1")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "", "1")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "US", "1")).Return("XXX").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "US", "2")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "AU", "US", "1")).Return("YYY").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "AU", "US", "2")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "AU", "1")).Return("ZZZ").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "AU", "2")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "AU", "AU", "1")).Return("AAA").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "AU", "AU", "2")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "AU", "CN", "1")).Return("ACN").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "AU", "CN", "2")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "CN", "1")).Return("...").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "CN", "2")).Return("UCN").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "CN", "3")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "ZA", "1")).Return("...").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "ZA", "2")).Return("UCN").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "US", "ZA", "3")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "SA", "ZA", "1")).Return("SSS").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "SA", "ZA", "2")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "SA", "ZA", "3")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Registration Type", "SA", "", "1")).Return("").Repeat.Any();

        mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal("NZ"), Arg<string>.Is.Anything)).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal("NZ"), Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal("CA"), Arg<string>.Is.Anything)).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode(Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal("CA"), Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return("").Repeat.Any();

        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Reference Label Type", "Use Long Reference", "INTTRA")).Return("false").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Reference Label Type", "Use Long Reference", "CMACGM")).Return("true").Repeat.Any();

        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label", "US", "US", "1")).Return("USXXXXX").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label", "US", "AU", "1")).Return("ZZZZZZZ").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label", "US", "CN", "2")).Return("UCNNNNN").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "US", "US", "1")).Return("USXXXXX_LONG").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "US", "AU", "1")).Return("ZZZZZZZ_LONG").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Gov Reference Number", "Reference Label Long", "US", "CN", "2")).Return("UCNNNNN_LONG").Repeat.Any();

        var extensionObjects = new Dictionary<string, object>()
        {
          {"http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper},
          {"http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor}
        };

        var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
        mapTester.Execute<Universal2CarrierUniversal_SI>(input, expectedOutput);

        mockCodeMapper.VerifyAllExpectations();
        mockContextAccessor.VerifyAllExpectations();

      }
    }
  }
}
