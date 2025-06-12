using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.BE.CPOINT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;
using System.Collections.Generic;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Tests
{
  [TestClass]
  public class UShipment2CertifiedPickup_Tests
  {
    const string filePath = "CPOINT.UShipment2CertifiedPickup.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2CertifiedPickup()
    {
      AssertMapping("Test1_input_Accept_BTW.xml", "Test1_output.xml", "ORG", true, "");
      AssertMapping("Test2_input_Decline.xml", "Test2_output.xml", "ORG", true, "");
      AssertMapping("Test3_input_Revoke_Transporter.xml", "Test3_output.xml", "WTH", false);
      AssertMapping("Test4_input_Transfer_Forwarder.xml", "Test4_output.xml", "ORG", false, "");
      AssertMapping("Test5_input_Accept_PSN.xml", "Test5_output.xml", "ORG", true, "");
      AssertMapping("Test6_input_Accept_EOR.xml", "Test6_output.xml", "ORG", true, "");
      AssertMapping("Test7_input_Accept_DUN.xml", "Test7_output.xml", "ORG", true, "");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string purpose, bool acceptDecline, string previousJobNumber = "CPO0000000099")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      var serviceProvider = "CPOINT";
      var destinationParty = serviceProvider + "_CPu1";
      var serviceProviderMSGID = serviceProvider.Substring(0, 3) + "MSG";
      var serviceProviderID = serviceProvider.Substring(0, 3) + "ID";
      var serviceProviderPrefix = serviceProvider.Substring(0, 3);
      var formattedInterchangeNumber = serviceProviderPrefix + "0000000099";

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("TrackingID_0000001");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "CPU_TESTSENDER_1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "FPM_" + serviceProvider + "_BEANR"));

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.BE.NXPORT.Interchange", "@maxlength", "50")).Return("1");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", destinationParty)).Return(serviceProvider).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", destinationParty)).Return(serviceProviderMSGID);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", destinationParty)).Return(serviceProviderID);

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER", "HAM", serviceProvider)).Return("CPOINT001");
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CPOINT", "@recipientId", "TESTSENDER", "@ST_ID", serviceProviderMSGID, "@value", "C00001280_MSCU2104261", "@referenceType", "CPU")).Return(previousJobNumber).Repeat.Any();

      if (string.IsNullOrEmpty(previousJobNumber))
      {
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.BE.NXPORT", "@maxlength", "50")).Return("99");
        mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "SubscriptionPrefix", destinationParty)).Return(serviceProviderPrefix);
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "C00001280_MSCU2104261", "CPU"));
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", "C00001280_MSCU2104261", formattedInterchangeNumber, "CPU"));

      }

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "ForwardingConsol", "ForwardingType"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, purpose, "Purpose"));

      if (acceptDecline)
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER",
          formattedInterchangeNumber, "Certified Pickup - Accept/Decline", "DocumentName"));
      }
      else
      {
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER",
          formattedInterchangeNumber, "Certified Pickup - Transfer", "DocumentName"));
      }

      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "BEANR", "OperationPort"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderID, serviceProvider, "TESTSENDER", "CPOINT001"));
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", "TrackingID_0000001", "1"));

      var extensionObjects = new Dictionary<string, object>()
        {
            { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
            { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
            { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
        };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
      mapTester.ExecuteCompiled<UShipment2CertifiedPickup>(input, expectedOutput);

      mockContextAccessor.VerifyAllExpectations();
      mockDataModelAccessor.VerifyAllExpectations();
      mockCodeMapper.VerifyAllExpectations();
    }
  }
}