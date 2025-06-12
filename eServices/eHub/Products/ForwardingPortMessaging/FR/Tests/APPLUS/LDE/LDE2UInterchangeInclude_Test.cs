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
  public class LDE2UInterchangeInclude_Test
  {
    const string filePath = "APPLUS.LDE.TestFiles_LDE_NOTIF.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestLDE2UInterchangeInclude()
    {
      AssertMapping("Test1_input.xml", "Test1_output.xml", false);
      AssertMapping("Test2_input.xml", "Test2_output.xml", true);
      AssertMapping("Test3_input.xml", "Test3_output.xml", false, eHubID: "DHL");
      AssertMapping("Test4_input.xml", "Test4_output.xml", false, eHubID: "DHL");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, bool emptyKey, string eHubID = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var recipientID = "DHL";
      var senderID = "MGI";
      var serviceProvider = "MGI";
      var serviceProviderPrefix = serviceProvider.Substring(0, 3);
      var serviceProviderID = serviceProviderPrefix + "ID";
      var serviceProviderMSGID = serviceProviderPrefix + "MSG";
      var serviceProviderSICID = serviceProviderPrefix + "SIC";

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", recipientID)).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GeteHubIDByCode("B915,B915", serviceProvider)).Return(eHubID).Repeat.Any();
      if (string.IsNullOrEmpty(eHubID))
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", serviceProvider, "@ST_ID", serviceProviderID, "@value", "B915,B915")).Return("DHL").Repeat.Once();
      }

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", serviceProvider)).Return(serviceProviderID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", serviceProvider)).Return(serviceProviderMSGID).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider)).Return(serviceProvider).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "SubscriptionPrefix", serviceProvider)).Return(serviceProviderPrefix).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "SCAC (Inbound)", "SCAC", serviceProvider, "SEATRACI")).Return("SEATRACI").Repeat.Any();

      if (emptyKey)
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", serviceProviderSICID, "@value", "LDE_LDE60757037")).Return("").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", "", "@referenceType", "JobNumber")).Return("").Repeat.Any();
      }
      else
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", serviceProviderSICID, "@value", "LDE_LDE60757037")).Return("MGI0000001").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", "MGI0000001", "@referenceType", "JobNumber")).Return("C00001234").Repeat.Any();
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderID, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", "MGI0000001", "@referenceType", "DocumentName")).Return("LDE Notification").Repeat.Any();
      }

      mockCodeMapper.Expect(x => x.GetRecipientCode(senderID, senderID, serviceProvider + " System Configuration", "Acknowledgment Status", "Output Code", "OK")).Return("OK").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(senderID, senderID, serviceProvider + " System Configuration", "Acknowledgment Status", "Output Code", "VAL")).Return("OK").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(senderID, senderID, serviceProvider + " System Configuration", "ContainerTypeToISOCode", "MGI Code", "42G1")).Return("ISO42G1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(senderID, senderID, serviceProvider + " System Configuration", "ContainerTypeToISOCode", "MGI Code", "4410")).Return("ISO4410").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "SCAC (Inbound)", "SCAC", senderID, "CONE")).Return("SCACCONE").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "SCAC (Inbound)", "SCAC", senderID, "CSEH")).Return("SCACCSEH").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<LDE2UInterchangeInclude>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}
