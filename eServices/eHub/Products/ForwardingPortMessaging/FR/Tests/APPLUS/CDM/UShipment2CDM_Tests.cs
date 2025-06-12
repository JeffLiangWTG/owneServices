using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS;
using CargoWise.eHub.Products.ForwardingPortMessaging.FR.Schemas.APPLUS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.FR.Tests
{
  [TestClass]
  public class UShipment2CDM_Tests
  {
    const string filePath = "APPLUS.CDM.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2CDM()
    {
      AssertMapping("Test1_input.xml", "Test1_SOGET_output.xml", "SOGET");
      AssertMapping("Test1_input.xml", "Test1_MGI_output.xml", "MGI");
      AssertMapping("Test1_input.xml", "Test1_SOGET_previousMessageIdentifier_output.xml", "SOGET", previousMessageIdentifier: "SOG0000001817");
      AssertMapping("Test1_input.xml", "Test1_MGI_previousMessageIdentifier_output.xml", "MGI", previousMessageIdentifier: "SOG0000001817");
      AssertMapping("Test1_input.xml", "Test1_MGI_output.xml", "MGI", routingPartyValue: "SOGET");
      AssertMapping("Test2_input.xml", "Test2_SOGET_output.xml", "SOGET");
      AssertMapping("Test2_input.xml", "Test2_MGI_output.xml", "MGI");

      AssertSchema("Test1_SOGET_output.xml");
      AssertSchema("Test1_MGI_output.xml");
      AssertSchema("Test1_MGI_output.xml");
      AssertSchema("Test2_SOGET_output.xml");
      AssertSchema("Test2_MGI_output.xml");
    }

    public void AssertSchema(string expectedOutputFile)
    {
      var expectedOutput = filePath + expectedOutputFile;
      var schemaValidator = XMLValidator.Validate<cdm>(expectedOutput);
      Assert.AreEqual(string.Empty, schemaValidator, "UShipment2CDM is not valid: \r\n " + schemaValidator);
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string serviceProvider, string routingPartyValue = "", string previousMessageIdentifier = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      var destinationParty = serviceProvider + "_CDM1";
      var serviceProviderMSGID = serviceProvider.Substring(0, 3) + "MSG";
      var serviceProviderID = serviceProvider.Substring(0, 3) + "ID";
      var serviceProviderPrefix = serviceProvider.Substring(0, 3);
      var formattedInterchangeNumber = serviceProviderPrefix + "0000000099";

      var messageIdentifier = !(string.IsNullOrEmpty(previousMessageIdentifier)) ? previousMessageIdentifier : formattedInterchangeNumber;

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("TrackingID_0000001");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "CDM_TESTSENDER_1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "FPM_" + serviceProvider + "_FRMRS"));

      var routingParty = routingPartyValue != "" ? routingPartyValue : serviceProvider;
      if (routingParty != "" && serviceProvider != routingParty)
      {
        mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", routingParty + "_" + serviceProvider + "_1"));
      }

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS.Interchange", "@maxlength", "14")).Return("1");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS", "@maxlength", "14")).Return("99").Repeat.Any();

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "TESTSENDER", "@ST_ID", serviceProviderMSGID, "@value", "C00001266_SUDU6959917_CDM", "@referenceType", "MessageReference")).Return(previousMessageIdentifier).Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "TESTSENDER", "@ST_ID", serviceProviderMSGID, "@value", "C00001266_SUDU6959918_CDM", "@referenceType", "MessageReference")).Return(previousMessageIdentifier).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider + "_CDM1")).Return(serviceProvider).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", destinationParty)).Return(serviceProviderMSGID);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", destinationParty)).Return(serviceProviderID);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "SubscriptionPrefix", destinationParty)).Return(serviceProviderPrefix);

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "SenderID", serviceProvider, "FRMRS", routingParty)).Return(serviceProvider + "_SENDER_ID");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "RecipientID", serviceProvider, "FRMRS", routingParty)).Return(serviceProvider + "_Recipient_ID");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "DestinationUser", serviceProvider, "FRMRS", routingParty)).Return(serviceProvider + "_U").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "DestinationParty", serviceProvider, "FRMRS", routingParty)).Return(serviceProvider + "_P").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Container Mode", "Containerized", "FCL")).Return("true").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Container Mode", "Containerized", "LCL")).Return("false").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Package Type ISO", serviceProvider + " Code", "PKG")).Return("PKG").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Package Type ISO", serviceProvider + " Code", "BAG")).Return("").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Package Type ISO", "Output Code", "BAG")).Return("BAG").Repeat.Any();

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER", "FRMRS", serviceProvider)).Return("APPLUS001,DHLLTR");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderID, serviceProvider, "TESTSENDER", "APPLUS001,DHLLTR"));

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, "C00001266", "JobNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", "C00001266", messageIdentifier, messageIdentifier)).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, "ForwardingConsol", "ForwardingType")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, "ORG", "Purpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, "AMD", "Purpose")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, "Outturn Report (CDM)", "DocumentName")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, "FRMRS", "OperationPort")).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", "TrackingID_0000001", "1_" + messageIdentifier)).Repeat.Any();

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, "LPD000003434", "RequestID")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, "LPD000003434", "ContainerNumber")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, "LPD000003435", "RequestID")).Repeat.Any();
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, "LPD000003435", "ContainerNumber")).Repeat.Any();

      string[] containerNumbers = { "SUDU6959917", "SUDU6959918" };
      foreach (var containerNumber in containerNumbers)
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, containerNumber, "RequestID")).Repeat.Any();
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, containerNumber, "ContainerNumber")).Repeat.Any();

        if (string.IsNullOrEmpty(previousMessageIdentifier))
        {
          mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", string.Concat("C00001266_", containerNumber, "_CDM"), messageIdentifier, "MessageReference")).Repeat.Any();
        }
      }

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationAttri1AsString("TESTSENDER", "FRMRS", serviceProvider)).Return(routingParty).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
          { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
          { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
          { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<UShipment2CDM>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}
