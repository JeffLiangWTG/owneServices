using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.NL.Portbase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.NL.Tests
{
  [TestClass]
  public class UShipment2M2400_Tests
  {
    const string filePath = "Portbase.UShipment2M2400.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2M2400()
    {
      AssertMapping("Test1_input_ContainerTerminal_ORG.xml", "Test1_output_ContainerTerminal_ORG.xml", "ORG", false, "");
      AssertMapping("Test2_input_ContainerTerminal_AMD.xml", "Test2_output_ContainerTerminal_AMD.xml", "AMD", false);
      AssertMapping("Test3_input_FerryTerminal_ORG.xml", "Test3_output_FerryTerminal_ORG.xml", "ORG", true, "");
      AssertMapping("Test4_input_FerryTerminal_AMD.xml", "Test4_output_FerryTerminal_AMD.xml", "AMD", true);

      //TODO: AMD-WTH
    }

    void AssertMapping(string sourceFile, string expectedFile, string purpose, bool isFerryTerminal, string previousJobNumber = "PBS0000000098I")
    {
      var input = filePath + sourceFile;
      var expectedOutput = filePath + expectedFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      var serviceProvider = "PORTBASE";
      var serviceProviderMSGID = serviceProvider.Substring(0, 3) + "MSG";
      var serviceProviderID = serviceProvider.Substring(0, 3) + "ID";
      var msgPrefix = "PBS";

      var senderID = "TESTSENDER";
      var recipientID = serviceProvider + "_M2400";

      var fileSenderID = "PORTBASE001";
      var interchangeNumber = "99";
      var messageIdentifier = msgPrefix + "00000000" + interchangeNumber + "I";
      var consolAndMRN = "C00679603_17DE345355864324E8";
      if (isFerryTerminal)
      {
        consolAndMRN = "C00679603_17DE810357630693E6";
      }

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID);
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("TrackingID_0000001");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "M2400_TESTSENDER_" + interchangeNumber));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "FPM_" + serviceProvider + "_NLRTM"));

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", recipientID)).Return(serviceProvider).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", recipientID)).Return(serviceProviderMSGID);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", recipientID)).Return(serviceProviderID);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "SubscriptionPrefix", recipientID)).Return(msgPrefix);

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode(senderID, "AMS", serviceProvider)).Return(fileSenderID);

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.MRN", "@maxlength", "14")).Return(interchangeNumber);
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.Portbase.MRN", "@maxlength", "50")).Return(interchangeNumber);

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", senderID, "@ST_ID", serviceProviderMSGID, "@value", consolAndMRN, "@referenceType", "M2400")).Return(previousJobNumber).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageIdentifier, consolAndMRN, "M2400"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, consolAndMRN, messageIdentifier, "M2400"));

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageIdentifier, "ForwardingConsol", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageIdentifier, purpose, "Purpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageIdentifier, "Import Notification", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageIdentifier, "NLRTM", "OperationPort"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderID, serviceProvider, senderID, fileSenderID));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, "TrackingID_0000001", interchangeNumber));

      if (isFerryTerminal)
      {
        if (previousJobNumber == "")
        {
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", senderID, "@ST_ID", serviceProviderMSGID, "@value", "STNL0012345678001_17DE810357630693E6", "@referenceType", "ShipmentId")).Return("").Repeat.Any();
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", senderID, "@ST_ID", serviceProviderMSGID, "@value", "STNL0012345678001", "@referenceType", "ShipmentIdCounter")).Return("").Repeat.Any();
          mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, "STNL0012345678001", "1", "ShipmentIdCounter"));
          mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, "STNL0012345678001_17DE810357630693E6", "1", "ShipmentId"));
        }
        else
        {
          mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", senderID, "@ST_ID", serviceProviderMSGID, "@value", "STNL0012345678001_17DE810357630693E6", "@referenceType", "ShipmentId")).Return("2").Repeat.Any();
        }
      }

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
      };

      MapTester mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.Execute<UShipment2M2400>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
    }
  }
}