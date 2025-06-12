using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.UniversalShipment2VERMAS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.Tests
{
  [TestClass]
  public class UniversalShipment2VERMAS_Tests
  {
    const string filePath = "UniversalShipment2VERMAS.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUniversalShipment2VERMAS()
    {
      //Test: ContainerTypeISOCode Fallback
      AssertMapping("Test1_input.xml", "Test1_output.xml", "NaN", "containerTypeMapping", "", "WTG");
      AssertMapping("Test1_input.xml", "Test2_output.xml", "NaN", "ISOCode", "");
      AssertMapping("Test1_input.xml", "Test3_output.xml", "NaN", "", "");

      //Test: DataSourceTypeIsNotForwardingConsol_NoSubscriptionNoConsolID
      AssertMapping("Test4_input.xml", "Test4_output.xml", "NaN", "", "", "WTG");

      //Test: DocumentOverridePurpose
      AssertMapping("Test5_input.xml", "Test5_output.xml", "NaN", "", "");
      AssertMapping("Test6_input.xml", "Test6_output.xml", "NaN", "", "");

      //Test : SealType Fallback
      AssertMapping("Test7_input.xml", "Test7_output.xml", "NaN", "", "SealCode");
      AssertMapping("Test8_input.xml", "Test8_output.xml", "NaN", "", "SealCode");

      AssertMapping("Test10_input.xml", "Test10_output.xml", "NaN", "containerTypeMappingV1", "SealCode", "MAERSK", "MAERSK_BK1", "TRUE");

      //CallInsertSubscriptionValue, Fallback to PreviousSubReference : VERMAS
      AssertMapping("Test1_input.xml", "Test9_output.xml", "previousConsolReference", "containerTypeMappingV1", "SealCode");

      //Test: DestinationPartySenderIdentifier Fallback
      AssertMapping("Test1_input.xml", "Test3_output.xml", "NaN", "", "", partySenderIdentifier: "");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string previousConsolReference, string expectedISOCode, string sealCode, string defaultInterfaceCarrierName = "", string recipientID = "RecipientID", string sendAgentReference = "FALSE", string partySenderIdentifier = "CW1")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var index = recipientID.IndexOf("_");
      var serviceProvider = index >= 0
        ? recipientID.Substring(0, index)
        : recipientID;

      var partySenderIdentifierForContextProperty = string.IsNullOrEmpty(partySenderIdentifier) ? "CARGOWISE" : partySenderIdentifier;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("SenderID").Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID).Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("InboxID").Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEDIHeader", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "true"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", partySenderIdentifierForContextProperty));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartySenderQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverIdentifier", "http://schemas.microsoft.com/Edi/PropertySchema", "PartyReceiverID"));
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationPartyReceiverQualifier", "http://schemas.microsoft.com/Edi/PropertySchema", "ZZZ"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB5", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "InterchangeNum"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "UNOC"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB1_2", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "3"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNH1", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", "InterchangeNum"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "VERMAS_CarrierID_InterchangeNum"));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB9", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("UNB11", "http://schemas.microsoft.com/BizTalk/2006/edi-properties", ""));
      mockContextAccessor.Expect(x => x.SetContextProperty("EarlyTerminateEdifactUnb", "http://cargowise.com/ehub/processing/2010/06", "true"));

      var carrierName = defaultInterfaceCarrierName.Length > 0 ? defaultInterfaceCarrierName : "CarrierName";
      if (defaultInterfaceCarrierName.Length > 0)
      {
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Default Interface Name", "Interface Name", recipientID)).Return(carrierName + " Provider Configuration").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode(carrierName, carrierName, carrierName + " Provider Configuration", "ContainerTypeToISOCode", carrierName + " Code", "22G0")).Return(expectedISOCode).Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode(carrierName, carrierName, carrierName + " Provider Configuration", "ContainerTypeToISOCode", carrierName + " Code", "22G1")).Return(expectedISOCode).Repeat.Any();
      }
      else
      {
        mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Default Interface Name", "Interface Name", recipientID)).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("RecipientID", "RecipientID", "VGM Per Container VERMAS to CarrierName", "ContainerTypeToISOCode", "CarrierName Code", "22G0")).Return(expectedISOCode).Repeat.Any();
        //mockCodeMapper.Expect(x => x.GetRecipientCode("RecipientID", "RecipientID", "VGM Per Container VERMAS to CarrierName", "ContainerTypeToISOCode", "CarrierName Code", "22G1")).Return(expectedISOCode).Repeat.Any();
      }

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "SCAC", "Output Code", recipientID, "MAEU")).Return("carrierSCAC").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartyReceiverID", recipientID, "carrierSCAC")).Return("PartyReceiverID").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "UNB3", "PartySenderID", recipientID, "carrierSCAC")).Return(partySenderIdentifier).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "CarrierName", serviceProvider)).Return(carrierName).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "ID", serviceProvider)).Return("IDID").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "MSGID", serviceProvider)).Return("MSGID").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "CarrierSettings", "SubscriptionPrefix", serviceProvider)).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "22G0")).Return(expectedISOCode).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "ContainerTypeToISOCode", "Carrier Code", "22G1")).Return(expectedISOCode).Repeat.Any();

      mockCodeMapper.Stub(x => x.GetRecipientCode("OCMVERMAS", "OCMVERMAS", "OCM VERMAS Configuration", "Defaults", "Send Agent Reference", recipientID)).Return(sendAgentReference);

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode(Arg<string>.Is.Equal("SenderID"), Arg.Is("A01"), Arg<string>.Is.Equal(carrierName))).Return("CarrierID").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.OceanCarrierMessaging.Transforms." + carrierName + ".UNH1", "@maxlength", "14")).Return("InterchangeNum").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", carrierName, "@recipientId", "SenderID", "@ST_ID", "MSGID", "@value", "consolID_ContainerNo")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", carrierName, "@recipientId", "SenderID", "@ST_ID", "MSGID", "@value", "consolID_ContainerNo", "@referenceType", "VERMAS")).Return(previousConsolReference).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", carrierName, "@recipientId", "SenderID", "@ST_ID", "MSGID", "@value", "_ContainerNo")).Return(previousConsolReference).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MSGID", carrierName, "SenderID", "NaN", "InterchangeNum", "InterchangeNum")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MSGID", carrierName, "SenderID", "previousConsolReference", "InterchangeNum", "InterchangeNum")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MSGID", carrierName, "SenderID", "InboxID", "InterchangeNum")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("IDID", carrierName, "SenderID", "CarrierID")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MSGID", carrierName, "SenderID", "InterchangeNum", "consolID_ContainerNo")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MSGID", carrierName, "SenderID", "InterchangeNum", "_ContainerNo")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MSGID", carrierName, "SenderID", "InterchangeNum", "Verified Gross Container Weight", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MSGID", carrierName, "SenderID", "InterchangeNum", "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MSGID", carrierName, "SenderID", "InterchangeNum", "Container", "SubMessageType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MSGID", carrierName, "SenderID", "InterchangeNum", "ORG", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MSGID", carrierName, "SenderID", "InterchangeNum", "AMD", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MSGID", carrierName, "SenderID", "InterchangeNum", "WTH", "ActionPurpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MSGID", carrierName, "SenderID", "InterchangeNum", "", "ShipmentType")).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("SHIPPING_INSTRUCTION", "SHIPPING_INSTRUCTION", "OCM System Configuration", "Seal Party Type", "Carrier Code", recipientID, "CAR")).Return(sealCode).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor }
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<UniversalShipment2VERMAS>(input, expectedOutput);

      mockDataModelAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockContextAccessor.VerifyAllExpectations();
    }
  }
}
