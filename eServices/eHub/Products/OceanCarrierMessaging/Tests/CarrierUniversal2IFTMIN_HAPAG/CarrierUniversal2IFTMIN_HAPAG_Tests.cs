using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2IFTMIN_HAPAG;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2IFTMIN_HAPAG_Tests
  {
    const string filePath = "CarrierUniversal2IFTMIN_HAPAG.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMIN_HAPAG()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml");
      AssertMapping("Test2_input.xml", "Test2_output.xml", payableElseWhere: "A", payableElseWhereDescription: "ELSEWHERE");
      AssertMapping("Test3_input_USExport.xml", "Test3_output_USExport.xml");
      AssertMapping("Test4_input_Brazil.xml", "Test4_output_Brazil.xml");
      AssertMapping("Test5_input.xml", "Test5_output.xml");
      AssertMapping("Test6_input.xml", "Test6_output.xml", isSummary: "TRUE");
      AssertMapping("Test7_input.xml", "Test7_output.xml", isSummary: "TRUE");
      AssertMapping("Test8_input.xml", "Test8_output.xml");
      AssertMapping("Test8_input.xml", "Test8_output_RFFGN1.xml", numberOfSegment: "1");
      AssertMapping("Test8_input.xml", "Test8_output_RFFGN4.xml", numberOfSegment: "4");
      AssertMapping("Test9_input.xml", "Test9_output.xml");
      AssertMapping("Test10_Brazil_summary_input.xml", "Test10_Brazil_summary_output.xml", isSummary: "TRUE");
      AssertMapping("Test11_Brazil_input.xml", "Test11_Brazil_output.xml");
      AssertMapping("Test12_SHP_GroupingMethod_input.xml", "Test12_SHP_GroupingMethod_output.xml");
      AssertMapping("Test13_input.xml", "Test13_output.xml");
      AssertMapping("Test14_input.xml", "Test14_output.xml");
      AssertMapping("Test15_DNG_GroupingMethod_input.xml", "Test15_DNG_GroupingMethod_output.xml");
      AssertMapping("Test16_SGP_GroupingMethod_input.xml", "Test16_SGP_GroupingMethod_output.xml");
      AssertMapping("Test17_ICS2_Carrier_DRT_input.xml", "Test17_ICS2_Carrier_DRT_output.xml", isICS2RequirementSupported: "TRUE");
      AssertMapping("Test18_ICS2_Carrier_AGT_input.xml", "Test18_ICS2_Carrier_AGT_output.xml", isICS2RequirementSupported: "TRUE");
      AssertMapping("Test19_ICS2_Declarant_AGT_input.xml", "Test19_ICS2_Declarant_AGT_output.xml", isICS2RequirementSupported: "TRUE");
      AssertMapping("Test20_ICS2_Carrier_AGT_Summary_input.xml", "Test20_ICS2_Carrier_AGT_Summary_output.xml", isICS2RequirementSupported: "TRUE", isSummary: "TRUE");
      AssertMapping("Test21_ICS2_Carrier_AGT_GroupingMethod_input.xml", "Test21_ICS2_Carrier_AGT_GroupingMethod_output.xml", isICS2RequirementSupported: "TRUE");
      AssertMapping("Test22_ICS2_Carrier_AGT_Summary_input.xml", "Test22_ICS2_Carrier_AGT_Summary_output.xml", isICS2RequirementSupported: "TRUE", isSummary: "TRUE");
      AssertMapping("Test23_ICS2_Carrier_DRT_BuyerDocumentaryAddress_input.xml", "Test23_ICS2_Carrier_DRT_BuyerDocumentaryAddress_output.xml", isICS2RequirementSupported: "TRUE");
      AssertMapping("Test24_Non_ICS2_Required_input.xml", "Test24_Non_ICS2_Required_output.xml");
      AssertMapping("Test25_ICS2_Carrier_AGT_GroupingMethod_input.xml", "Test25_ICS2_Carrier_AGT_GroupingMethod_output.xml", isICS2RequirementSupported: "TRUE");
      AssertMapping("Test26_ICS2_Carrier_AGT_Summary_input.xml", "Test26_ICS2_Carrier_AGT_Summary_output.xml", isICS2RequirementSupported: "TRUE");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMIN_HAPAG_CannotFindUNB3()
    {
      var exceptionMessage = "Could not found matching PartyReceiverID in the UNB3 Lookup code mapping.(Sender: * - Multiple senders, Recipient: SHIPPING_INSTRUCTION, Interface: OCM System Configuration, Code Set: UNB3, Input: [CarrierID:HAPAG_LLOYD_SI1], [SCAC:OOLU])";
      var input = filePath + "Test1_input.xml";
      var extensionObjects = SetupMappingExtensions(string.Empty);

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteAssertException<CarrierUniversal2IFTMIN_HAPAG>(input, exceptionMessage);
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string destinationPartyReceiverIdentifier = "HLCU", string isSummary = "FALSE", string numberOfSegment = "2", string payableElseWhere = "", string payableElseWhereDescription = "", string isICS2RequirementSupported = "FALSE")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var extensionObjects = SetupMappingExtensions(destinationPartyReceiverIdentifier, isSummary, numberOfSegment, payableElseWhere, payableElseWhereDescription, isICS2RequirementSupported);

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<CarrierUniversal2IFTMIN_HAPAG>(input, expectedOutput);
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/CodeMapper"].VerifyAllExpectations();
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/DateMapper"].VerifyAllExpectations();
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"].VerifyAllExpectations();
    }

    Dictionary<string, object> SetupMappingExtensions(string destinationPartyReceiverIdentifier = "HLCU", string isSummary = "FALSE", string numberOfSegment = "2", string payableElseWhere = "", string payableElseWhereDescription = "", string isICS2RequirementSupported = "FALSE")
    {
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockStringMapper = MockRepository.GenerateStrictMock<StringMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER__1");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("HAPAG_LLOYD_SI1");

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "CARGOWISE"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", destinationPartyReceiverIdentifier));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.HAPAG_LLOYD.UNH1", "@maxlength", "14")).Return("3");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "HAPAG_LLOYD", "@recipientId", "TESTSENDER__1", "@ST_ID", "HAPMSG", "@value", "C00678677")).Return("HYEDAUTST");

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BN1", "HAPAG_LLOYD")).Return("HAPL").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER__1", "BNE", "HAPAG_LLOYD")).Return("HAPL").Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "UNOC"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_2", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTMIN_HAPL_3"));

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPID", "HAPAG_LLOYD", "TESTSENDER__1", "HAPL"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "3", "C00678677"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "C00678677", "AMD", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "C00678677", "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "C00678677", "WTH", "ActionPurpose")).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "HAPAG_LLOYD_SI1", "OOLA")).Return("OOLU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "HAPAG_LLOYD_SI1", "HLCU")).Return("HLCU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "HAPAG_LLOYD_SI1", "MSCU")).Return("HLCU").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "HAPAG_LLOYD_SI1", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", "HAPAG_LLOYD_SI1", "OOLU")).Return(destinationPartyReceiverIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", "HAPAG_LLOYD_SI1", "HLCU")).Return(destinationPartyReceiverIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "ContainerTypeToISOCode", "Output Code", "40R0")).Return("40RT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "ContainerTypeToISOCode", "Output Code", "22R0")).Return("CT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "ContainerTypeToISOCode", "Output Code", "22G0")).Return("22G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "ContainerTypeToISOCode", "Output Code", "42G0")).Return("42G0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "ContainerTypeToISOCode", "Output Code", "22R1")).Return("22R1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "ContainerTypeToISOCode", "Output Code", "45R0")).Return("45R0").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "ContainerTypeToISOCode", "Output Code", "45R1")).Return("45R1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "Package Type", "Output Code", "D_I")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "Package Type", "Output Code", "P_I")).Return("PI").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "Package Type", "Output Code", "I_I")).Return("II").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "Package Type", "Output Code", "PLT")).Return("PLT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "Package Type", "Output Code", "PKG")).Return("PKG").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "Package Type", "Output Code", "BAG")).Return("BAG").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("HAPAG_LLOYD", "HAPAG_LLOYD", "HAPAG LLOYD Provider Configuration", "Package Type", "Output Code", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "D_I")).Return("DI").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Package Type ISO", "Package Type", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "Gov Reference Format", "NumberOfSegment", "HAPAG_LLOYD_SI1")).Return(numberOfSegment).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "Gov Reference Format", "Inc.RegulatingCountry", "HAPAG_LLOYD_SI1")).Return("true").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "ICS2 Requirement", "Supported", "HAPAG_LLOYD_SI1")).Return(isICS2RequirementSupported).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "eBL Provider", "Output Code", "HAPAG_LLOYD_SI1", "OOLU", "Cargo X")).Return("CX").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPID", "HAPAG_LLOYD", "TESTSENDER__1", "C00678677", "AGT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPID", "HAPAG_LLOYD", "TESTSENDER__1", "C00678677", "DRT", "ShipmentType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "3", "Shipping Instruction", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "3", "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("TrackingID12345678").Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "TrackingID12345678", "3")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "HYEDAUTST", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("HAPMSG", "HAPAG_LLOYD", "TESTSENDER__1", "HYEDAUTST", "3.0.0", "FormVersion")).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "TESTSENDER__1", "HAPAG_LLOYD")).Return(isSummary).Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB4_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "160311"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB4_2", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "0851"));
      mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyMMddHHmm")).Return("1603110851").Repeat.Any();

      mockOCMHelper.Expect(x => x.GetServiceProvider("HAPAG_LLOYD_SI1")).Return("HAPAG_LLOYD").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode("HAPAG_LLOYD")).Return(payableElseWhere).Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereDescription("HAPAG_LLOYD")).Return(payableElseWhereDescription).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TESTSENDER__1", "HAPL", "HAPAG_LLOYD", "HYEDAUTST", "C00678677")).Repeat.Any();

      return new Dictionary<string, object>()
            {
                { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/StringMapper", mockStringMapper },
                { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
                { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
                { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
            };
    }
  }
}
