using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.BE.CPOINT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Tests
{
  [TestClass]
  public class CertifiedPickup2UInterchangeInclude_Tests
  {
    const string filePath = "CPOINT.CertifiedPickup2UInterchangeInclude.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestCertifiedPickup2UInterchangeInclude()
    {
      AssertMapping("Test1_input_Error_Message.xml", "Test1_output.xml", false, false);
      AssertMapping("Test2_input_Notification_Message.xml", "Test2_output.xml", true, false);
      AssertMapping("Test3_input_Error_Message.xml", "Test3_output.xml", false, true);
      AssertMapping("Test4_input_Notification_Message.xml", "Test4_output.xml", true, true);
      AssertMapping("Test5_input_Notification_Message_ATH_ATW.xml", "Test5_output.xml", true, true, "ATH");
      AssertMapping("Test6_input_Error_Message.xml", "Test6_output.xml", false, true, includeSubscribedJobNumber: false);
      AssertMapping("Test7_input_Notification_Message.xml", "Test7_output.xml", true, true, includeSubscribedJobNumber: false);
    }

    void AssertMapping(string inputFile, string expectedOutputFile, bool isNotificationMessage, bool externalReferenceFilled, string eventType = "MPP", bool includeSubscribedJobNumber = true)
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
      var dataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

      var serviceProvider = "CPOINT";
      var serviceProviderMSGID = serviceProvider.Substring(0, 3) + "MSG";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(serviceProvider).Repeat.AtLeastOnce();
      mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", "HYEBNEUAT")).Repeat.Any();
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("123-456-789").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", serviceProvider, "@ST_ID", serviceProvider, "@value", "NXT20000051292")).Return("HYEBNEUAT").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", serviceProvider)).Return(serviceProvider);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider)).Return(serviceProvider);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", serviceProvider)).Return(serviceProviderMSGID);

      if (externalReferenceFilled)
      {
        if (includeSubscribedJobNumber)
        {
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", "ext123", "@referenceType", "CPU")).Return("ID123_456");
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", "ext123", "@referenceType", "ForwardingType")).Return("FWType123");
        }
        else
        {
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", "ext123", "@referenceType", "CPU")).Return(string.Empty);
        }

        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", "ext123", "@referenceType", "OperationPort")).Return("BEZEE");
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", "ext123", "@referenceType", "DocumentName")).Return("Certified Pickup - DocumentName");

        if (!isNotificationMessage)
        {
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "", "@ST_ID", serviceProviderMSGID, "@value", "ext123", "@referenceType", "Purpose")).Return("Purpose123");
        }
      }

      if (isNotificationMessage)
      {
        mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Notification Message", "Event Type", "Transfer", "Transferred", "ReleaseRight", "TRUE")).Return(eventType).Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Notification Message", "Event Type", "Transfer", "Transferred", "ReleaseRight", "FALSE")).Return(eventType).Repeat.Any();
      }

      dataModelAccessor.Expect(x => x.InsertSubscriptionValue("NXPTRC", "HYEBNEUAT", "CPOINT", "123-456-789", "Certified Pickup - ReleaseRight___TCLU1036476", "MRJ")).Repeat.Any();
      dataModelAccessor.Expect(x => x.InsertSubscriptionValue("NXPTRC", "HYEBNEUAT", "CPOINT", "123-456-789", "Certified Pickup - ReleaseRight__BL2104261_MSCU2104261", "MPP")).Repeat.Any();
      dataModelAccessor.Expect(x => x.InsertSubscriptionValue("NXPTRC", "HYEBNEUAT", "CPOINT", "123-456-789", "Certified Pickup - DocumentName___TCLU1036476", "MRJ")).Repeat.Any();
      dataModelAccessor.Expect(x => x.InsertSubscriptionValue("NXPTRC", "HYEBNEUAT", "CPOINT", "123-456-789", "Certified Pickup - DocumentName__BL2104261_MSCU2104261", "MPP")).Repeat.Any();
      dataModelAccessor.Expect(x => x.InsertSubscriptionValue("NXPTRC", "HYEBNEUAT", "CPOINT", "123-456-789", "Certified Pickup - DocumentName_REL210426_1_BL2104261_MSCU2104261", "ATH")).Repeat.Any();


      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", dataModelAccessor}
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<CertifiedPickup2UInterchangeInclude>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
