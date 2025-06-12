using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.EDIFACT.Schemas.D99B;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2IFTMIN_INTTRA;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CU2CUniveralISO8859;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2IFTMIN_INTTRA_Tests
  {
    const string filePath = "CarrierUniversal2IFTMIN_INTTRA.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void CarrierUniversal2IFTMIN_INTTRA()
    {
      AssertMapping("Test1_input.xml", "Test1_cleanup.xml", "Test1_output.xml");
      AssertMapping("Test2_US_Domestic_input.xml", "Test2_US_Domestic_cleanup.xml", "Test2_US_Domestic_output.xml", payableElseWhere: "A", payableElseWhereDescription: "ELSEWHERE");
      AssertMapping("Test3_input.xml", "Test3_cleanup.xml", "Test3_output.xml", isDirect: true);
      AssertMapping("Test4_input.xml", "Test4_cleanup.xml", "Test4_output.xml", isDirect: true);
      AssertMapping("Test4_CLD_input.xml", "Test4_CLD_cleanup.xml", "Test4_CLD_output.xml");
      AssertMapping("Test5_GCL_input.xml", "Test5_GCL_cleanup.xml", "Test5_GCL_output.xml");
      AssertMapping("Test5_input.xml", "Test5_cleanup.xml", "Test5_output.xml");
      AssertMapping("Test6_input.xml", "Test6_cleanup.xml", "Test6_output.xml");
      AssertMapping("Test7_input.xml", "Test7_cleanup.xml", "Test7_output.xml");
      AssertMapping("Test8_input.xml", "Test8_cleanup.xml", "Test8_output.xml", "TRUE");
      AssertMapping("Test9_input.xml", "Test9_cleanup.xml", "Test9_output.xml", "TRUE");
      AssertMapping("Test10_input.xml", "Test10_cleanup.xml", "Test10_output.xml");
      AssertMapping("Test11_input.xml", "Test11_cleanup.xml", "Test11_output.xml");
      AssertMapping("Test11_input.xml", "Test11_cleanup.xml", "Test11_output_RFFGN1.xml", numberOfSegment: "1");
      AssertMapping("Test11_input.xml", "Test11_cleanup.xml", "Test11_output_RFFGN4.xml", numberOfSegment: "4");
      AssertMapping("Test12_Brazil_summary_input.xml", "Test12_Brazil_summary_cleanup.xml", "Test12_Brazil_summary_output.xml", isSummary: "TRUE");
      AssertMapping("Test13_Brazil_input.xml", "Test13_Brazil_cleanup.xml", "Test13_Brazil_output.xml");
      AssertMapping("Test14_SHP_GroupingMethod_input.xml", "Test14_SHP_GroupingMethod_cleanup.xml", "Test14_SHP_GroupingMethod_output.xml");
      AssertMapping("Test15_input.xml", "Test15_cleanup.xml", "Test15_output.xml");
      AssertMapping("Test21_DNG_GroupingMethod_input.xml", "Test21_DNG_GroupingMethod_cleanup.xml", "Test21_DNG_GroupingMethod_output.xml");

      AssertMapping("Test16_input_ICS2_Carrier_SingleSub.xml", "Test16_cleanup_ICS2_Carrier_SingleSub.xml", "Test16_output_ICS2_Carrier_SingleSub.xml", isSummary: "TRUE");
      AssertMapping("Test17_input_ICS2_Carrier_DRT_SingleSub.xml", "Test17_cleanup_ICS2_Carrier_DRT_SingleSub.xml", "Test17_output_ICS2_Carrier_DRT_SingleSub.xml", isSummary: "TRUE");
      AssertMapping("Test18_input_ICS2_Declarant_SingleSub.xml", "Test18_cleanup_ICS2_Declarant_SingleSub.xml", "Test18_output_ICS2_Declarant_SingleSub.xml", isSummary: "TRUE");
      AssertMapping("Test19_input_ICS2_Carrier_MultiSub.xml", "Test19_cleanup_ICS2_Carrier_MultiSub.xml", "Test19_output_ICS2_Carrier_MultiSub.xml", isSummary: "TRUE");
      AssertMapping("Test20_input_ICS2_Carrier_MultiSub_GroupingMethod.xml", "Test20_cleanup_ICS2_Carrier_MultiSub_GroupingMethod.xml", "Test20_output_ICS2_Carrier_MultiSub_GroupingMethod.xml");
      AssertMapping("Test22_input_Non_ICS2_Required.xml", "Test22_cleanup_Non_ICS2_Required.xml", "Test22_output_Non_ICS2_Required.xml", requireICS2: "false");
      AssertMapping("Test23_input_ICS2_Declarant_DRT_SingleSub.xml", "Test23_cleanup_ICS2_Declarant_DRT_SingleSub.xml", "Test23_output_ICS2_Declarant_DRT_SingleSub.xml", isSummary: "TRUE");
    }

    void AssertMapping(string inputFile, string cleanupFile, string expectedOutputFile, string isSummary = "FALSE", string numberOfSegment = "2", string payableElseWhere = "", string payableElseWhereDescription = "", bool isDirect = false, string requireICS2 = "true")
    {
      var input = filePath + inputFile;
      var cleanup = filePath + cleanupFile;
      var expectedOutput = filePath + expectedOutputFile;
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER_INTTRA");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("INTTRA_SI1");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CARGOWISE"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "INTTRA"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "11"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "11"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "INFTMIN_CGWS_11"));

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTID", "INTTRA", "TESTSENDER_INTTRA", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER_INTTRA", "11", "C00001007"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER_INTTRA", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "11"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER_INTTRA", "C00001007", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER_INTTRA", "C00001007", "3.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(Arg<string>.Is.Equal("INTMSG"), Arg<string>.Is.Equal("INTTRA"), Arg<string>.Is.Equal("TESTSENDER_INTTRA"), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal("ActionPurpose")));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(Arg<string>.Is.Equal("INTMSG"), Arg<string>.Is.Equal("INTTRA"), Arg<string>.Is.Equal("TESTSENDER_INTTRA"), Arg<string>.Is.Anything, Arg<string>.Is.Anything, Arg<string>.Is.Equal("ShipmentType")));
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER_INTTRA", "BNE", "INTTRA")).Return("CGWS").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER_INTTRA", "BN1", "INTTRA")).Return("CGWS").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER_INTTRA", "SHA", "INTTRA")).Return("CGWS").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER_INTTRA", "C00001007", "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER_INTTRA", "C00001007", "Booking Request", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER_INTTRA", "C00001007", "Shipping Instruction", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("INTMSG", "INTTRA", "TESTSENDER_INTTRA", "C00001007", "Shipping Instruction WIP", "DocumentName")).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.INTTRA.UNH1", "@maxlength", "14")).Return("11");
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "42G0")).Return("42G0");
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "40G0")).Return("40G0");
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "45R1")).Return("45R1");

      // handle 3 cases for package code:
      // package type
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "P_I")).Return("P_O").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "PLT")).Return("P_O").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "PKG")).Return("P_O").Repeat.Any();
      // ISO
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "I_I")).Return("").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "I_I")).Return("I_O").Repeat.Any();
      // default
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCodeUnkeyed("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Defaults", "Package Type")).Return("D_O").Repeat.Any();
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_SI1", "SUDU")).Return("asda");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_SI1", "us2A")).Return("us20");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_SI1", "ca1A")).Return("ca18");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_SI1", "CMDA")).Return("CMDU");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_SI1", "ANLA")).Return("ANLC");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_SI1", "INTT")).Return("INTT");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_SI1", "CHHK")).Return("CHHK");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_SI1", "MSCU")).Return("MSCU");
      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "INTTRA_SI1", "")).Return("");
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "Package Type", "INTTRA Code", "PCS")).Return("asdad");
      mockCodeMapper.Stub(x => x.GetRecipientCode("INTTRA_SI", "INTTRA_SI", "Shipping Instruction IFTMIN to INTTRA", "ContainerTypeToISOCode", "INTTRA Code", "45G0")).Return("asd");
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "Gov Reference Format", "NumberOfSegment", "INTTRA_SI1")).Return(numberOfSegment).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "Gov Reference Format", "Inc.RegulatingCountry", "INTTRA_SI1")).Return("true").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "ICS2 Requirement", "Supported", "INTTRA_SI1")).Return(requireICS2).Repeat.Any();

      mockCodeMapper.Stub(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Reference Label Type", "Use Long Reference", "INTTRA_SI1")).Return("false").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "TESTSENDER_INTTRA", "INTTRA")).Return(isSummary).Repeat.Any();

      mockOCMHelper.Expect(x => x.IsCoLoad("")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("AGT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("DRT")).Return("FALSE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("CLD")).Return("TRUE").Repeat.Any();
      mockOCMHelper.Expect(x => x.IsCoLoad("GCL")).Return("TRUE").Repeat.Any();

      mockOCMHelper.Expect(x => x.GetServiceProvider("INTTRA_SI1")).Return("INTTRA").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode("INTTRA")).Return(payableElseWhere).Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereDescription("INTTRA")).Return(payableElseWhereDescription).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "IsDirect", "INTTRA")).Return(isDirect.ToString()).Repeat.Any();
      if (isDirect)
      {
        mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER_INTTRA", "CGWS", "INTTRA", "C00001007", "C00001007")).Repeat.Any();
      }
      else
      {
        mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER_INTTRA", "CGWS", "INTTRA", "11", "C00001007")).Repeat.Any();
      }

      var extensionObjects = new Dictionary<string, object>() {
            { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
                { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
            };

      MapTester mapTester1 = new MapTester(Assembly.GetExecutingAssembly());
      MapTester mapTester2 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester1.Execute<CU2CUniveralISO8859>(input, cleanup);
      mapTester2.Execute<CarrierUniversal2IFTMIN_INTTRA>(cleanup, expectedOutput);

      mockDateMapper.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();

      var schemaValidator = new SchemaValidator();
      schemaValidator.ValidateSchema<EFACT_D99B_IFTMIN>(expectedOutput, ErrorWhileList);
    }

    List<string> ErrorWhileList
    {
      get
      {
        return new List<string>()
            {
                "datatype 'String' - The actual length is less than the MinLength value.",
                "C21501"  //SealParty
            };
      }
    }
  }
}
