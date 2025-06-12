using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.EDIFACT.Schemas.D99B;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2IFTMIN_CGS;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CU2CUniveralISO8859;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2IFTMIN_CGS_Tests
  {
    const string filePath = "CarrierUniversal2IFTMIN_CGS.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMIN_CGS()
    {
      AssertMapping("Test1_input.xml", "Test1_cleanup.xml", "Test1_output.xml", "BLAH");
      AssertMapping("Test2_input.xml", "Test2_cleanup.xml", "Test2_output.xml", "BLAH", includeRegulatingCountry: true);
      AssertMapping("Test3_input.xml", "Test3_cleanup.xml", "Test3_output.xml", "OOLU", payableElseWhere: "A", payableElseWhereDescription: "ELSEWHERE");
      AssertMapping("Test4_input.xml", "Test4_cleanup.xml", "Test4_output.xml", "OOLU");
      AssertMapping("Test5_input.xml", "Test5_cleanup.xml", "Test5_output.xml", "BLAH");
      AssertMapping("Test6_input.xml", "Test6_cleanup.xml", "Test6_output.xml", "BLAH");
      AssertMapping("Test7_input.xml", "Test7_cleanup.xml", "Test7_output.xml", "BLAH");
      AssertMapping("Test8_input.xml", "Test8_cleanup.xml", "Test8_output.xml", "BLAH");
      AssertMapping("Test9_input.xml", "Test9_cleanup.xml", "Test9_output.xml", "BLAH");
      AssertMapping("TestWrapper01_input.xml", "TestWrapper01_cleanup.xml", "TestWrapper01_output.xml", "BLAH");
      AssertMapping("TestWrapper02_input.xml", "TestWrapper02_cleanup.xml", "TestWrapper02_output.xml", "BLAH");
      AssertMapping("TestWrapper03_input.xml", "TestWrapper03_cleanup.xml", "TestWrapper03_output.xml", "BLAH");
      AssertMapping("TestWrapper04_input.xml", "TestWrapper04_cleanup.xml", "TestWrapper04_output.xml", "BLAH");
      AssertMapping("Test10_input.xml", "Test10_cleanup.xml", "Test10_output.xml", "BLAH", isSummary: "TRUE");
      AssertMapping("Test11_input.xml", "Test11_cleanup.xml", "Test11_output.xml", "BLAH");
      AssertMapping("Test12_input.xml", "Test12_cleanup.xml", "Test12_output.xml", "BLAH", isSummary: "TRUE");
      AssertMapping("Test13_input.xml", "Test13_cleanup.xml", "Test13_output.xml", "BLAH");
      AssertMapping("Test13_input.xml", "Test13_cleanup.xml", "Test13_output_RFFGN1.xml", "BLAH", numberOfSegment: "1");
      AssertMapping("Test13_input.xml", "Test13_cleanup.xml", "Test13_output_RFFGN4.xml", "BLAH", numberOfSegment: "4");
      AssertMapping("Test14_input.xml", "Test14_cleanup.xml", "Test14_output.xml", "BLAH");
      AssertMapping("Test15_Brazil_summary_input.xml", "Test15_Brazil_summary_cleanup.xml", "Test15_Brazil_summary_output.xml", "OOLU", isSummary: "TRUE");
      AssertMapping("Test16_Brazil_input.xml", "Test16_Brazil_cleanup.xml", "Test16_Brazil_output.xml", "OOLU");
      AssertMapping("Test17_SHP_GroupingMethod_input.xml", "Test17_SHP_GroupingMethod_cleanup.xml", "Test17_SHP_GroupingMethod_output.xml", "MSCU");
      AssertMapping("Test18_input.xml", "Test18_cleanup.xml", "Test18_output.xml", "BLAH");
      AssertMapping("Test19_DNG_GroupingMethod_input.xml", "Test19_DNG_GroupingMethod_cleanup.xml", "Test19_DNG_GroupingMethod_output.xml", "MSCU");
      AssertMapping("Test20_ICS2_Carrier_Not_DRT_input.xml", "Test20_ICS2_Carrier_Not_DRT_cleanup.xml", "Test20_ICS2_Carrier_Not_DRT_output.xml", "BLAH");
      AssertMapping("Test21_ICS2_Carrier_Is_DRT_input.xml", "Test21_ICS2_Carrier_Is_DRT_cleanup.xml", "Test21_ICS2_Carrier_Is_DRT_output.xml", "BLAH");
      AssertMapping("Test22_ICS2_Carrier_Not_DRT_Count_gt1_input.xml", "Test22_ICS2_Carrier_Not_DRT_Count_gt1_cleanup.xml", "Test22_ICS2_Carrier_Not_DRT_Count_gt1_output.xml", "BLAH");
      AssertMapping("Test23_ICS2_Carrier_Not_DRT_Group_Count_gt1_input.xml", "Test23_ICS2_Carrier_Not_DRT_Group_Count_gt1_cleanup.xml", "Test23_ICS2_Carrier_Not_DRT_Group_Count_gt1_output.xml", "BLAH");
      AssertMapping("Test24_ICS2_Declarant_Not_DRT_input.xml", "Test24_ICS2_Declarant_Not_DRT_cleanup.xml", "Test24_ICS2_Declarant_Not_DRT_output.xml", "BLAH");
      AssertMapping("Test25_ICS2_Declarant_Is_DRT_input.xml", "Test25_ICS2_Declarant_Is_DRT_cleanup.xml", "Test25_ICS2_Declarant_Is_DRT_output.xml", "BLAH");
      AssertMapping("Test26_ICS2_Carrier_Is_Not_DRT_input.xml", "Test26_ICS2_Carrier_Is_Not_DRT_cleanup.xml", "Test26_ICS2_Carrier_Is_Not_DRT_output.xml", "BLAH", isSummary: "TRUE");
      AssertMapping("Test27_Non_ICS2_Required_input.xml", "Test27_Non_ICS2_Required_cleanup.xml", "Test27_Non_ICS2_Required_output.xml", "BLAH", isSummary: "TRUE", requireICS2: "FALSE");
      AssertMapping("Test28_ICS2_Carrier_DRT_Count_gt1_input.xml", "Test28_ICS2_Carrier_DRT_Count_gt1_cleanup.xml", "Test28_ICS2_Carrier_DRT_Count_gt1_output.xml", "BLAH");
      AssertMapping("Test29_ICS2_Carrier_Not_DRT_SingleSub_NoConsigneeAddressInSub_cleanup.xml", "Test29_ICS2_Carrier_Not_DRT_SingleSub_NoConsigneeAddressInSub_input.xml", "Test29_ICS2_Carrier_Not_DRT_SingleSub_NoConsigneeAddressInSub_output.xml", "BLAH");
    }

    void AssertMapping(string inputFile, string cleanupFile, string expectedOutputFile, string scac, string isSummary = "FALSE", string numberOfSegment = "2", bool includeRegulatingCountry = false, string payableElseWhere = "", string payableElseWhereDescription = "", string requireICS2 = "TRUE")
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

      var incRegulatingCountry = includeRegulatingCountry ? "true" : "false";
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("CARGOSMART_SI1");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CARGOWISE"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CARGOSMART"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CargoSmart.UNH1", "@maxlength", "14")).Return("1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "ContainerTypeToISOCode", "CargoSmart Code", "22G0")).Return("CT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "ContainerTypeToISOCode", "CargoSmart Code", "")).Return("CT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "ContainerTypeToISOCode", "CargoSmart Code", "42G0")).Return("CT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "ContainerTypeToISOCode", "CargoSmart Code", "42R0")).Return("CT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "ContainerTypeToISOCode", "CargoSmart Code", "40G0")).Return("CT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "ContainerTypeToISOCode", "CargoSmart Code", "45R1")).Return("CT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "Gov Reference Format", "NumberOfSegment", "CARGOSMART_SI1")).Return(numberOfSegment).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "Gov Reference Format", "Inc.RegulatingCountry", "CARGOSMART_SI1")).Return(incRegulatingCountry).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "ICS2 Requirement", "Supported", "CARGOSMART_SI1")).Return(requireICS2).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CARGOSMART", "@recipientId", "TESTSENDER__1", "@ST_ID", "CGSMSG", "@value", "C00001007")).Return("");
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BN1", "CARGOSMART")).Return("CGWS");
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTMIN_CGWS_1"));
      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BNE", "CARGOSMART")).Return("CGWS");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSID", "CARGOSMART", "TESTSENDER__1", "CGWS"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "1", "C00001007"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "CGS0000000001", "C00001007"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "C00001007", "CGS0000000001"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSBRS", "CARGOSMART", "TESTSENDER__1", "C00001007", "APP"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "1"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "C00001007", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "C00001007", "3.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "C00001007", "ORG", "ActionPurpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "C00001007", "AGT", "ShipmentType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "C00001007", "DRT", "ShipmentType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "C00001007", "Shipping Instruction", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "C00001007", "ForwardingConsol", "ForwardingType"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "UNOC"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_2", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));

      switch (scac)
      {
        case "BLAH":
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CARGOSMART_SI1", "BLAA")).Return("BLAH").Repeat.Any();
          break;
        case "MSCU":
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CARGOSMART_SI1", "MSCU")).Return("MSCU").Repeat.Any();
          break;
        case "OOLU":
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CARGOSMART_SI1", "OOLA")).Return("OOLU").Repeat.Any();
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CARGOSMART_SI1", "OOLU")).Return("OOLU").Repeat.Any();
          mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "CARGOSMART_SI1", "ANLA")).Return("ANLC").Repeat.Any();
          break;
      }

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "TESTSENDER__1", "CARGOSMART")).Return(isSummary).Repeat.Any();

      // handle 3 cases for package code:
      // package type
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "PackageTypeToX12", "X12 Code", "P_I")).Return("P_O").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "PackageTypeToX12", "X12 Code", "PLT")).Return("P_O").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "PackageTypeToX12", "X12 Code", "PKG")).Return("P_O").Repeat.Any();
      // ISO
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "PackageTypeToX12", "X12 Code", "I_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "I_I")).Return("I_O").Repeat.Any();
      // default
      mockCodeMapper.Expect(x => x.GetRecipientCode("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "PackageTypeToX12", "X12 Code", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCodeUnkeyed("CARGOSMART_SI", "CARGOSMART_SI", "Shipping Instruction IFTMIN to CargoSmart SI", "Defaults", "Package Type")).Return("D_O").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CargoSmart.BGM", "@maxlength", "14")).Return("88");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "CGS0000000088", "C00001007")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "C00001007", "CGS0000000088")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "CGS0000000088", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CGSMSG", "CARGOSMART", "TESTSENDER__1", "CGS0000000088", "3.0.0", "FormVersion")).Repeat.Any();

      mockOCMHelper.Expect(x => x.GetServiceProvider("CARGOSMART_SI1")).Return("CARGOSMART").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode("CARGOSMART")).Return(payableElseWhere).Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereDescription("CARGOSMART")).Return(payableElseWhereDescription).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "CGWS", "CARGOSMART", "CGS0000000088", "C00001007")).Repeat.Any();

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
      mapTester2.ExecuteCompiled<CarrierUniversal2IFTMIN_CGS>(cleanup, expectedOutput);

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
          "C21501",  //SealParty
          "DGS05",
          "DTM_9",
          //"FTX"
        };
      }
    }
  }
}
