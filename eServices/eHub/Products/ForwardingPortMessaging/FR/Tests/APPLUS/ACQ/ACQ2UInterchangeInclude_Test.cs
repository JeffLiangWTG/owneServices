using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using CargoWise.eHub.Core.Transforms.Helper;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.FR.Tests
{
  [TestClass]
  public class ACQ2UInterchangeInclude_Test
  {
    const string filePath = "APPLUS.ACQ.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestACQ2UInterchangeInclude()
    {
      AssertMapping("Test1_DOS_ack_ok_input.xml", "Test1_DOS_ack_ok_output.xml", "DOS", "ORG", "V", "C0000100099_2_2");
      AssertMapping("Test2_DOS_ack_ok_input.xml", "Test2_DOS_ack_ok_output.xml", "DOS", "ORG", "R");
      AssertMapping("Test3_AMQ_ack_ok_input.xml", "Test3_AMQ_ack_ok_output.xml", "AMQ", "ORG", "V");
      AssertMapping("Test4_CAED_ack_ok_input.xml", "Test4_CAED_ack_ok_output.xml", "CAED", "ORG", "V");
      AssertMapping("Test5_CDM_ack_ok_input.xml", "Test5_CDM_ack_ok_output.xml", "CDM", "ORG", "V");
      AssertMapping("Test6_CRESA_ack_ok_input.xml", "Test6_CRESA_ack_ok_output.xml", "CRESA", "ORG", "V");
      AssertMapping("Test7_LDE_ack_ok_input.xml", "Test7_LDE_ack_ok_output.xml", "LDE", "ORG", "V");
      AssertMapping("Test8_LPD_ack_ok_input.xml", "Test8_LPD_ack_ok_output.xml", "LPD", "ORG", "V");
      AssertMapping("Test9_noSubscribedJobNo_input.xml", "Test9_noSubscribedJobNo_output.xml", "DOS", "ORG", "R", "");
      AssertMapping("Test10_DOS_ack_ok_WTH_input.xml", "Test10_DOS_ack_ok_WTH_output.xml", "DOS", "WTH", "V");
      AssertMapping("Test11_DOS_ack_ok_input_without_sic.xml", "Test11_DOS_ack_ok_output_without_sic.xml", "DOS", "ORG", "V");
      AssertMapping("Test12_CRESA_ack_ok_input_without_ref.xml", "Test12_CRESA_ack_ok_output_without_ref.xml", "CRESA", "ORG", "V");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string messageType, string purpose, string responseStatus, string subscribeJobNumber = "C0000100099")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var senderID = "APPLUS";
      var recipientID = "DHL";
      var expectedDocumentName = GetDocumentName(messageType);

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID).Repeat.AtLeastOnce();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MGISIC", "MGI", "DHL", messageType + "_REF_0001", "AMQ_MSGREF_0000000001")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MGISIC", "MGI", "DHL", messageType + "_ACQ_REF_00001", "AMQ_MSGREF_0000000001")).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "SubscriptionPrefix", "APPLUS")).Return("MGI");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", "APPLUS")).Return("MGI");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", "APPLUS")).Return("MGIMSG").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MGI", "MGI", "MGI System Configuration", "Acknowledgment Status", "Output Code", "V")).Return("PRO").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MGI", "MGI", "MGI System Configuration", "Acknowledgment Status", "Output Code", "R")).Return("XXX").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MGI", "MGI", "MGI System Configuration", "Acknowledgment Status", "Output Code", "")).Return("").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MGIMSG", "@value", "AMQ_MSGREF_0000000001", "@referenceType", "JobNumber")).Return(subscribeJobNumber).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MGIMSG", "@value", "AMQ_MSGREF_0000000001", "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MGIMSG", "@value", "AMQ_MSGREF_0000000001", "@referenceType", "DocumentName")).Return(expectedDocumentName).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MGIMSG", "@value", "AMQ_MSGREF_0000000001", "@referenceType", "ContainerNumber")).Return("CONT1111111").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MGIMSG", "@value", "AMQ_MSGREF_0000000001", "@referenceType", "OperationPort")).Return("FRDKK").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MGIMSG", "@value", "AMQ_MSGREF_0000000001", "@referenceType", "Purpose")).Return(purpose);
      if (responseStatus == "V")
      {
        if (purpose == "ORG")
        {
          mockCodeMapper.Expect(x => x.GetRecipientCode("MGI", "MGI", "MGI System Configuration", "ACQ Event Type", "Event Type", responseStatus, purpose)).Return("MAA").Repeat.Any();
        }
        if (purpose == "WTH")
        {
          mockCodeMapper.Expect(x => x.GetRecipientCode("MGI", "MGI", "MGI System Configuration", "ACQ Event Type", "Event Type", responseStatus, purpose)).Return("MWA").Repeat.Any();
        }
      }
      if (responseStatus == "R")
      {
        mockCodeMapper.Expect(x => x.GetRecipientCode("MGI", "MGI", "MGI System Configuration", "ACQ Event Type", "Event Type", responseStatus, purpose)).Return("MRJ").Repeat.Any();
      }

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<ACQ2UInterchangeInclude>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }

    string GetDocumentName(string code)
    {

      switch (code)
      {
        case "DOS":
          return "File Creation Request";
        case "AMQ":
          return "Container Booking Notification";
        case "CAED":
          return "Check Pre-Customs Declaration";
        case "CDM":
          return "Mass Unpack Report";
        case "CRESA":
          return "Goods Received (CRESA)";
        case "LDE":
          return "Final Packing List";
        case "LPD":
          return "Provisional Unpacking List";
        default:
          return "";
      }
    }
  }

  [TestClass]
  public class ACQ2TRC_Test
  {
    const string filePath = "APPLUS.ACQ.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestACQ2TRC()
    {
      AssertMapping("Test2_DOS_ack_ok_input.xml", "TestX_TRC_EmptyBody_output.xml", "DOS");
      AssertMapping("Test6_CRESA_ack_ok_input.xml", "Test6_CRESA_TRC_Output.xml", "CRESA", "");
      AssertMapping("Test8_LPD_ack_ok_input.xml", "Test8_LPD_TRC_Output.xml", "LPD", "");
      AssertMapping("Test8_LPD_ack_ok_input.xml", "TestX_TRC_EmptyBody_output.xml", "LPD");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string messageType, string subscribedPortRef = "C0000100099")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var senderID = "APPLUS";
      var recipientID = "DHL";
      var subscribeJobNumber = "C0000100099";
      var PortRef = "REF_0001";
      var expectedDocumentName = GetDocumentName(messageType);

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "TRC_HYEBNEUAT_1")).Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "FPM_MGI_APPLUS")).Repeat.Any();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "HYEBNEUAT")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MGISIC", "MGI", "DHL", messageType + "_REF_0001", "AMQ_MSGREF_0000000001")).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "SubscriptionPrefix", "APPLUS")).Return("MGI");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", "APPLUS")).Return("MGI");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", "APPLUS")).Return("MGIMSG").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MGI", "MGI", "MGI System Configuration", "Acknowledgment Status", "Output Code", "V")).Return("PRO").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MGI", "MGI", "MGI System Configuration", "Acknowledgment Status", "Output Code", "R")).Return("XXX").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("MGI", "MGI", "MGI System Configuration", "Acknowledgment Status", "Output Code", "")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "SenderID", "MGI", "APPLUS")).Return("MGI_SenderID").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "RecipientID", "MGI", "APPLUS")).Return("MGI_RecipientID").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "DestinationUser", "MGI", "APPLUS")).Return("MGI_DUser").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "DestinationParty", "MGI", "APPLUS")).Return("MGI_DParty").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MGIMSG", "@value", "AMQ_MSGREF_0000000001", "@referenceType", "JobNumber")).Return(subscribeJobNumber).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MGIMSG", "@value", "AMQ_MSGREF_0000000001", "@referenceType", "ForwardingType")).Return("ForwardingConsol").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MGIMSG", "@value", "AMQ_MSGREF_0000000001", "@referenceType", "DocumentName")).Return(expectedDocumentName).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MGIMSG", "@value", "AMQ_MSGREF_0000000001", "@referenceType", "ContainerNumber")).Return("CONT1111111").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MGIMSG", "@value", "AMQ_MSGREF_0000000001", "@referenceType", "OperationPort")).Return("APPLUS").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MGIMSG", "@value", "AMQ_MSGREF_0000000001", "@referenceType", "Purpose")).Return("APP").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", "MGIMSG", "@value", PortRef, "@referenceType", "PortRef")).Return(subscribedPortRef).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderID, "@ST_ID", "MGIMSG", "@value", "AMQ_MSGREF_0000000001")).Return("HYEBNEUAT").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MGIMSG", "MGI", "HYEBNEUAT", "MGI0000000099", subscribeJobNumber, "JobNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MGIMSG", "MGI", "HYEBNEUAT", "C0000100099", "MGI0000000099", "MGI0000000099")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MGIMSG", "MGI", "HYEBNEUAT", "MGI0000000099", "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MGIMSG", "MGI", "HYEBNEUAT", "MGI0000000099", expectedDocumentName, "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MGIMSG", "MGI", "HYEBNEUAT", "MGI0000000099", "CONT1111111", "ContainerNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MGIMSG", "MGI", "HYEBNEUAT", "MGI0000000099", "APPLUS", "OperationPort")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MGIMSG", "MGI", "HYEBNEUAT", "MGI0000000099", "APP", "Purpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("MGIMSG", "MGI", "HYEBNEUAT", PortRef, subscribeJobNumber, "PortRef")).Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS.Interchange", "@maxlength", "14")).Return("1").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS", "@maxlength", "14")).Return("99").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<ACQ2TRC>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }

    string GetDocumentName(string code)
    {

      switch (code)
      {
        case "DOS":
          return "File Creation Request";
        case "AMQ":
          return "Container Booking Notification";
        case "CAED":
          return "Check Pre-Customs Declaration";
        case "CDM":
          return "Mass Unpack Report";
        case "CRESA":
          return "Unannounced Goods Report";
        case "LDE":
          return "Final Packing List";
        case "LPD":
          return "Provisional Unpacking List";
        default:
          return "";
      }
    }
  }
}
