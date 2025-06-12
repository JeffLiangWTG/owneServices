using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Clients.EDI.EDIFACT.Schemas.D99B;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.CarrierUniversal2IFTMIN_MAERSK;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class CarrierUniversal2IFTMIN_MAERSK_Tests
  {
    const string filePath = "CarrierUniversal2IFTMIN_MAERSK.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMIN_MAERSK()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", "MAEU", "CARGOWISE", "TST_MAEU");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "MCCQ", "CARGOWISE1", "TST_MCCQ", payableElseWhere: "A", payableElseWhereDescription: "ELSEWHERE");
      AssertMapping("Test3_input.xml", "Test3_output.xml", "SAFM", "CARGOWISE2", "TST_SAFM");
      AssertMapping("Test4_input.xml", "Test4_output.xml", "SEAU", "CARGOWISE3", "SEAU_TST");
      AssertMapping("Test5_input.xml", "Test5_output.xml", "SEJJ", "CARGOWISE4", "TST_SEJJ");
      AssertMapping("Test6_input.xml", "Test6_output.xml", "MAEU", "CARGOWISE5", "MAEU_TST");
      AssertMapping("Test7_input.xml", "Test7_output.xml", "SEAU", "CARGOWISE6", "TST_SEAU");
      AssertMapping("Test8_input.xml", "Test8_output.xml", "MAEU", "CARGOWISE", "TST_MAEU", isSummary: "TRUE");
      AssertMapping("Test9_input.xml", "Test9_output.xml", "MAEU", "CARGOWISE", "TST_MAEU", isSummary: "TRUE");
      AssertMapping("Test10_input.xml", "Test10_output.xml", "MAEU", "CARGOWISE", "TST_MAEU");
      AssertMapping("Test10_input.xml", "Test10_output_RFFGN1.xml", "MAEU", "CARGOWISE", "TST_MAEU", numberOfSegment: "1");
      AssertMapping("Test10_input.xml", "Test10_output_RFFGN4.xml", "MAEU", "CARGOWISE", "TST_MAEU", numberOfSegment: "4");
      AssertMapping("Test11_input.xml", "Test11_output.xml", "MAEU", "CARGOWISE", "TST_MAEU");
      AssertMapping("Test12_Brazil_input.xml", "Test12_Brazil_output.xml", "SAFM", "CARGOWISE2", "TST_SAFM");
      AssertMapping("Test13_input.xml", "Test13_output.xml", "MAEU", "CARGOWISE", "TST_MAEU");
      AssertMapping("Test14_input.xml", "Test14_output.xml", "MAEU", "CARGOWISE", "TST_MAEU");
      AssertMapping("Test15_input.xml", "Test15_output.xml", "MAEU", "CARGOWISE", "TST_MAEU");
      AssertMapping("Test16_SHP_GroupingMethod_input.xml", "Test16_SHP_GroupingMethod_output.xml", "MSCU", "CARGOWISE", "TST_MAEU");
      AssertMapping("Test17_No_RFF_TN_input.xml", "Test17_No_RFF_TN_output.xml", "MAEU", "CARGOWISE", "TST_MAEU");
      AssertMapping("Test18_DNG_GroupingMethod_input.xml", "Test18_DNG_GroupingMethod_output.xml", "MSCU", "CARGOWISE", "TST_MAEU");
    }

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMIN_MAERSK_ICS2()
    {
      AssertMapping("ICS2.Test1_input_Carrier_SingleSub.xml", "ICS2.Test1_output_Carrier_SingleSub_NonICS2Required.xml", "SAFM", "CARGOWISE2", "TST_SAFM", requireICS2: "FALSE");
      AssertMapping("ICS2.Test1_input_Carrier_SingleSub.xml", "ICS2.Test1_output_Carrier_SingleSub.xml", "SAFM", "CARGOWISE2", "TST_SAFM");
      AssertMapping("ICS2.Test2_input_Carrier_SingleSub_NonICS2Port.xml", "ICS2.Test2_output_Carrier_SingleSub_NonICS2Port.xml", "SAFM", "CARGOWISE2", "TST_SAFM");
      AssertMapping("ICS2.Test3_input_Carrier_DRT_SingleSub.xml", "ICS2.Test3_output_Carrier_DRT_SingleSub.xml", "SAFM", "CARGOWISE2", "TST_SAFM");
      AssertMapping("ICS2.Test4_input_Carrier_DRT_SingleSub_NonICS2Port.xml", "ICS2.Test4_output_Carrier_DRT_SingleSub_NonICS2Port.xml", "SAFM", "CARGOWISE2", "TST_SAFM");
      AssertMapping("ICS2.Test5_input_Declarant_SingleSub.xml", "ICS2.Test5_output_Declarant_SingleSub.xml", "SAFM", "CARGOWISE2", "TST_SAFM");
      AssertMapping("ICS2.Test6_input_Declarant_SingleSub_NonICS2Port.xml", "ICS2.Test6_output_Declarant_SingleSub_NonICS2Port.xml", "SAFM", "CARGOWISE2", "TST_SAFM");
      AssertMapping("ICS2.Test7_input_Carrier_MultiSub.xml", "ICS2.Test7_output_Carrier_MultiSub.xml", "MAEU", "CARGOWISE", "TST_MAEU");
      AssertMapping("ICS2.Test8_input_Carrier_MultiSub_IsSummary.xml", "ICS2.Test8_output_Carrier_MultiSub_IsSummary.xml", "MAEU", "CARGOWISE", "TST_MAEU", isSummary: "TRUE");
      AssertMapping("ICS2.Test9_input_Carrier_MultiSub_GroupingMethod.xml", "ICS2.Test9_output_Carrier_MultiSub_GroupingMethod.xml", "MSCU", "CARGOWISE", "TST_MAEU");
    }

    [TestMethod]
    [TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMIN_MAERSK_CannotFindUNB3_PartyReceiverID()
    {
      var exceptionMessage = "Could not found matching PartyReceiverID in the UNB3 Lookup code mapping.(Sender: * - Multiple senders, Recipient: SHIPPING_INSTRUCTION, Interface: OCM System Configuration, Code Set: UNB3, Input: [CarrierID:MAERSK_SI], [SCAC:MAEU])";
      AssertException("Test1_input.xml", exceptionMessage, "MAEU", "MAEU", string.Empty);
    }

    [TestMethod]
    [TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCarrierUniversal2IFTMIN_MAERSK_CannotFindUNB3_PartySenderID()
    {
      var exceptionMessage = "Could not found matching PartySenderID in the UNB3 Lookup code mapping.(Sender: * - Multiple senders, Recipient: SHIPPING_INSTRUCTION, Interface: OCM System Configuration, Code Set: UNB3, Input: [CarrierID:MAERSK_SI], [SCAC:MAEU])";
      AssertException("Test1_input.xml", exceptionMessage, "MAEU", string.Empty, "MAEU");
    }

    void AssertException(string inputFile, string exceptionMessage, string SCAC, string partySenderIdentifier, string partyReceiverIdentifier, string isSummary = "FALSE")
    {
      var input = filePath + inputFile;

      var extensionObjects = SetupMappingExtensions(SCAC, partySenderIdentifier, partyReceiverIdentifier, isSummary);

      MapTester mapTester1 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester1.ExecuteAssertException<CarrierUniversal2IFTMIN_MAERSK>(input, exceptionMessage);
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string SCAC, string partySenderIdentifier, string partyReceiverIdentifier,
                       string isSummary = "FALSE", string numberOfSegment = "2",
                       string payableElseWhere = "", string payableElseWhereDescription = "", string requireICS2 = "TRUE")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var extensionObjects = SetupMappingExtensions(SCAC, partySenderIdentifier, partyReceiverIdentifier, isSummary, numberOfSegment, payableElseWhere, payableElseWhereDescription, requireICS2);

      MapTester mapTester1 = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester1.Execute<CarrierUniversal2IFTMIN_MAERSK>(input, expectedOutput);

      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/DateMapper"].VerifyAllExpectations();
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/CodeMapper"].VerifyAllExpectations();
      extensionObjects["http://schemas.microsoft.com/BizTalk/2003/ContextAccessor"].VerifyAllExpectations();

      var schemaValidator = new SchemaValidator();
      schemaValidator.ValidateSchema<EFACT_D99B_IFTMIN>(expectedOutput, ErrorWhiteList);
    }

    List<string> ErrorWhiteList
    {
      get
      {
        return new List<string>()
        {
          "datatype 'String' - The actual length is less than the MinLength value.",
          "The value '' is invalid according to its datatype",
          "C21501"  //SealParty
        };
      }
    }

    Dictionary<string, object> SetupMappingExtensions(string SCAC, string partySenderIdentifier, string partyReceiverIdentifier, string isSummary = "FALSE", string numberOfSegment = "2", string payableElseWhere = "", string payableElseWhereDescription = "",
      string requireICS2 = "TRUE")
    {
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockOCMHelper = MockRepository.GenerateStrictMock<OCMHelper>();
      var mockICS2Helper = MockRepository.GenerateStrictMock<ICS2Helper>();
      var mockSubscriptionHelper = MockRepository.GenerateStrictMock<SubscriptionHelper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TestSender");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("MAERSK_SI");
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("36ACCF3C-893F-40CE-BA01-1D8CBE2DF678");

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", partySenderIdentifier));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", partyReceiverIdentifier));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TestSender", "BN1", "MAERSK")).Return("HYEBNEBN1").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.MAERSK.UNH1", "@maxlength", "14")).Return("23").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.MAERSK.BGM", "@maxlength", "14")).Return("32").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEID", "MAERSK", "TestSender", "HYEBNEBN1"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "23", "CEBS0000682392"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "MAE0000000032", "CEBS0000682392", "IFTMIN"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "CEBS0000682392", "MAE0000000032", "IFTMIN"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "MAE0000000032", "23", "InterchangeNum"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "CEBS0000682392", "23", "InterchangeNum"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "23", "Shipping Instruction", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "23", "ForwardingConsol", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "36ACCF3C-893F-40CE-BA01-1D8CBE2DF678", "23"));

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEBRS", "MAERSK", "TestSender", "CEBS0000682392", "ORG"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEBRS", "MAERSK", "TestSender", "CEBS0000682392", ""));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEID", "MAERSK", "TestSender", "CEBS0000682392", "AGT", "ShipmentType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEID", "MAERSK", "TestSender", "CEBS0000682392", "DRT", "ShipmentType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "CEBS0000682392", "1.0.0", "FormVersion")).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", "MAERSK_SI", SCAC)).Return(SCAC).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartySenderID", "MAERSK_SI", SCAC)).Return(partySenderIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", "MAERSK_SI", SCAC)).Return(partyReceiverIdentifier).Repeat.Any();

      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "23"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "23"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));

      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "IFTMIN_HYEBNEBN1_23"));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "MAERSK", "@recipientId", "TestSender", "@ST_ID", "MAEMSG", "@value", "CEBS0000682392", "@referenceType", "IFTMIN")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45G0")).Return("45G8").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45R0")).Return("45R7").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "22G0")).Return("22G9").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "45R1")).Return("45R1").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "Package Type", "MAERSK Code", "PCE")).Return("PCE").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "Package Type", "MAERSK Code", "PLT")).Return("PLT").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "Package Type", "MAERSK Code", "PKG")).Return("PKG").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "ContainerTypeToISOCode", "MAERSK Code", "45G0")).Return("45GMK").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "ContainerTypeToISOCode", "MAERSK Code", "45R0")).Return("45RMK").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "ContainerTypeToISOCode", "MAERSK Code", "22G0")).Return("22GMK").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MAERSK", "MAERSK", "MAERSK System Configuration", "ContainerTypeToISOCode", "MAERSK Code", "45R1")).Return("45R1").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Cargo Details Format", "Is Summary", "TestSender", "MAERSK_SI1")).Return(isSummary).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "Gov Reference Format", "NumberOfSegment", "MAERSK_SI")).Return(numberOfSegment).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "Gov Reference Format", "Inc.RegulatingCountry", "MAERSK_SI")).Return("true").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("OCMIFTMIN", "OCMIFTMIN", "OCM IFTMIN Configuration", "ICS2 Requirement", "Supported", "MAERSK_SI")).Return(requireICS2).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "MAE0000000032", "1.0.0", "FormVersion")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MAEMSG", "MAERSK", "TestSender", "MAE0000000032", "3.0.0", "FormVersion")).Repeat.Any();

      mockOCMHelper.Expect(x => x.GetServiceProvider("MAERSK_SI")).Return("MAERSK").Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereOutputCode("MAERSK")).Return(payableElseWhere).Repeat.Any();
      mockOCMHelper.Expect(x => x.GetPayableElseWhereDescription("MAERSK")).Return(payableElseWhereDescription).Repeat.Any();

      mockICS2Helper.Expect(x => x.IsICS2Port("DEBRE")).Return(true).Repeat.Any();
      mockICS2Helper.Expect(x => x.IsICS2Port("NLRTM")).Return(true).Repeat.Any();
      mockICS2Helper.Expect(x => x.IsICS2Port("AUMEL")).Return(false).Repeat.Any();
      mockICS2Helper.Expect(x => x.IsICS2Port("INBOM")).Return(false).Repeat.Any();
      mockICS2Helper.Expect(x => x.IsICS2Port("BRMAO")).Return(false).Repeat.Any();
      mockICS2Helper.Expect(x => x.IsICS2Port("CNTXG")).Return(false).Repeat.Any();
      mockICS2Helper.Expect(x => x.IsICS2Port("")).Return(false).Repeat.Any();

      mockSubscriptionHelper.Expect(x => x.InsertBoleroSubscription("TestSender", "HYEBNEBN1", "MAERSK", "MAE0000000032", "CEBS0000682392")).Repeat.Any();

      return new Dictionary<string, object>() {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/OCMHelper", mockOCMHelper },
        { "http://schemas.microsoft.com/BizTalk/2003/ICS2Helper", mockICS2Helper },
        { "http://schemas.microsoft.com/BizTalk/2003/SubscriptionHelper", mockSubscriptionHelper }
      };
    }
  }
}
