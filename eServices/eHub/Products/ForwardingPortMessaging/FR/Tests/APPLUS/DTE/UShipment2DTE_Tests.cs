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
  public class UShipment2DTE_Tests
  {
    const string filePath = "APPLUS.DTE.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2DTE()
    {
      AssertMapping("Test1_input.xml", "Test1_SOGET_output.xml", "SOGET");
      AssertMapping("Test1_input.xml", "Test1_MGI_output.xml", "MGI");
      AssertMapping("Test1_input.xml", "Test1_MGI_output.xml", "MGI", routingPartyValue: "SOGET");

      AssertSchema("Test1_SOGET_output.xml");
      AssertSchema("Test1_MGI_output.xml");
    }

    public void AssertSchema(string expectedOutputFile)
    {
      var expectedOutput = filePath + expectedOutputFile;
      var schemaValidator = XMLValidator.Validate<dte>(expectedOutput);
      Assert.AreEqual(string.Empty, schemaValidator, "UShipment2DTE is not valid: \r\n " + schemaValidator);
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string serviceProvider, string routingPartyValue = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      var destinationParty = serviceProvider + "_DTE1";
      var serviceProviderMSGID = serviceProvider.Substring(0, 3) + "MSG";
      var serviceProviderID = serviceProvider.Substring(0, 3) + "ID";
      var serviceProviderPrefix = serviceProvider.Substring(0, 3);
      var formattedInterchangeNumber = serviceProviderPrefix + "0000000099";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("TrackingID_0000001");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "DTE_TESTSENDER_1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "FPM_" + serviceProvider + "_FRMRS"));

      var routingParty = routingPartyValue != "" ? routingPartyValue : serviceProvider;
      if (routingParty != "" && serviceProvider != routingParty)
      {
        mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", routingParty + "_" + serviceProvider + "_1"));
      }

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS", "@maxlength", "14")).Return("99");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS.Interchange", "@maxlength", "14")).Return("1");

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider + "_DTE1")).Return(serviceProvider).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", destinationParty)).Return(serviceProviderMSGID);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", destinationParty)).Return(serviceProviderID);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "SubscriptionPrefix", destinationParty)).Return(serviceProviderPrefix);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Container Mode", "Containerized", "FCL")).Return("true").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "SenderID", serviceProvider, "FRMRS", routingParty)).Return(serviceProvider + "_01");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "RecipientID", serviceProvider, "FRMRS", routingParty)).Return(serviceProvider + "_02");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "DestinationUser", serviceProvider, "FRMRS", routingParty)).Return(serviceProvider + "_03");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "DestinationParty", serviceProvider, "FRMRS", routingParty)).Return(serviceProvider + "_04");

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER", "FRMRS", serviceProvider)).Return("APPLUS001,DHLLTR");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "C00001266", "JobNumber"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", "C00001266", formattedInterchangeNumber, formattedInterchangeNumber));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "ForwardingConsol", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "ORG", "Purpose"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "Underbond Movement Request - Export (DTE)", "DocumentName"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "FRMRS", "OperationPort"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "C00001266", "RequestID"));

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderID, serviceProvider, "TESTSENDER", "APPLUS001,DHLLTR"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", "TrackingID_0000001", "1_"+ formattedInterchangeNumber));

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationAttri1AsString("TESTSENDER", "FRMRS", serviceProvider)).Return(routingParty).Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
          { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
          { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
          { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<UShipment2DTE>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}