using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.EDIFACT.Schemas.D99B;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2IFTMBF_INTTRA;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversalShipment2IFTMBF_INTTRA_Tests
  {
    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMBF_INTTRA()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test1_input.xml", "Test1_output_MultiPickupDropOff.xml", multiPickupDelivery: "TRUE");
      AssertMapping("Test2_input.xml", "Test2_output.xml", payableElseWhere: "A");
      AssertMapping("Test3_input.xml", "Test3_output.xml", isDirect: true);
      AssertMapping("Test4_input.xml", "Test4_output.xml", isDirect: true);
      AssertMapping("Test5_CLD_input.xml", "Test5_CLD_output.xml");
      AssertMapping("Test6_LCL_CLD_input.xml", "Test6_LCL_CLD_output.xml");
      AssertMapping("Test7_input.xml", "Test7_output.xml", "TRUE");
      AssertMapping("Test8_input.xml", "Test8_output.xml");
      AssertMapping("Test9_input.xml", "Test9_output.xml", "TRUE");
      AssertMapping("Test10_input_TransportModes.xml", "Test10_output_TransportModes.xml");
      AssertMapping("Test11_input.xml", "Test11_output.xml");
      AssertMapping("Test12_input.xml", "Test12_output.xml");
      AssertMapping("Test13_IsOutOfGauge_input.xml", "Test13_IsOutOfGauge_output.xml");
      AssertMapping("Test14_IsOutOfGaugeBeFalse_input.xml", "Test14_IsOutOfGaugeBeFalse_output.xml");
      AssertMapping("Test15_SHP_GroupingMethod_input.xml", "Test15_SHP_GroupingMethod_output.xml");
      AssertMapping("Test16_FlatContainerQuality_input.xml", "Test16_FlatContainerQuality_output.xml");
      AssertMapping("Test17_DNG_GroupingMethod_NoShipmentsAttached_input.xml", "Test17_DNG_GroupingMethod_NoShipmentsAttached_output.xml");
      AssertMapping("Test18_DNG_GroupingMethod_input.xml", "Test18_DNG_GroupingMethod_output.xml");
    }

    public void AssertMapping(string sourceFile, string expectedFile, string isSummary = "FALSE", string multiPickupDelivery = "FALSE", string payableElseWhere = "", bool isDirect = false)
    {
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.INTTRA.UNH1", "@maxlength", "14")).Return("29");
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "48K8")).Return("48T8").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "45R1")).Return("45R1").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "22G0")).Return("22G0").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "")).Return("").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BNE", "INTTRA")).Return("CGWS");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTID", "INTTRA", "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER__1", "29", "C00001007"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER__1", "C00001007", "AMD", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER__1", "C00001007", "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER__1", "C00001007", "CLD", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER__1", "C00001007", "", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "29"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER__1", "C00001007", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER__1", "C00001007", "3.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER__1", "C00001007", "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER__1", "C00001007", "Booking Request", "DocumentName")).Repeat.Any();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("INTTRA_BK1");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CARGOWISE"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "INTTRANG2"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "29"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "29"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTMBF_CGWS_29"));

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_BK1", "BLAA")).Return("BLAH").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_BK1", "MSCU")).Return("MSCU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_BK1", "")).Return("BLAH").Repeat.Any();

      // handle 3 cases for package code:
      // package type
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "P_I")).Return("P_O").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "PKG")).Return("PKG").Repeat.Any();
      // ISO
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "I_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "I_I")).Return("I_O").Repeat.Any();
      // default
      mockCodeMapper.Expect(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Defaults", "Package Type")).Return("D_O").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "TESTSENDER__1", "INTTRA")).Return(isSummary).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", "INTTRA", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "INTTRA", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", "INTTRA", "XXX")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "INTTRA", "XXX")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "QualifierCode", "INTTRA", "GEN")).Return("SSR").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Container Quality Code", "Carrier Code", "INTTRA", "GEN")).Return("FGE").Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsMultiPickup(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(multiPickupDelivery).Repeat.Any();
      mockOCMHelper.Expect(x => x.IsMultiDropOff(Arg<string>.Is.Anything, Arg<string>.Is.Anything)).Return(multiPickupDelivery).Repeat.Any();

      mockOCMHelper.Expect(x => x.GetServiceProvider("INTTRA_BK1")).Return("INTTRA").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode("INTTRA")).Return(payableElseWhere).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", "INTTRA")).Return(isDirect.ToString()).Repeat.Any();
      if (isDirect)
      {
        mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "INTTRA", "C00001007", "C00001007")).Repeat.Any();
      }
      else
      {
        mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "INTTRA", "29", "C00001007")).Repeat.Any();
      }
      

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      sourceFile = "CarrierUniversal2IFTMBF_INTTRA.TestFiles." + sourceFile;
      expectedFile = "CarrierUniversal2IFTMBF_INTTRA.TestFiles." + expectedFile;

      mapTester.Execute<CarrierUniversal2IFTMBF_INTTRA>(sourceFile, expectedFile);

      var schemaValidator = new SchemaValidator();
      schemaValidator.ValidateSchema<EFACT_D99B_IFTMBF>(expectedFile, ErrorWhileList);
    }

    List<string> ErrorWhileList
    {
      get
      {
        return new List<string>()
        {
          "datatype 'String' - The actual length is less than the MinLength value.",
          "C21501",  //SealParty
          "DGS05",
          "C524",
          "EQDLoop1",
          "C53601"
        };
      }
    }
  }
}
