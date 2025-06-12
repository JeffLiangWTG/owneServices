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
  public class UShipment2EBADEC_Tests
  {
    const string filePath = "CPOINT.UShipment2EBADEC.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2EBADEC()
    {
        AssertMapping("Test1_input_containers_ORG_DUNS.xml", "Test1_output_containers_ORG_DUNS.xml", "ORG", "");
        AssertMapping("Test2_input_containers_AMD_PSN.xml", "Test2_output_containers_AMD_PSN.xml", "AMD");
        AssertMapping("Test3_input_containers_WTH_EOR.xml", "Test3_output_containers_WTH_EOR.xml", "WTH");
        AssertMapping("Test4_input_vehicles_ORG_DUNS.xml", "Test4_output_vehicles_ORG_DUNS.xml", "ORG");
        AssertMapping("Test5_input_vehicles_AMD_PSN.xml", "Test5_output_vehicles_AMD_PSN.xml", "AMD");
        AssertMapping("Test6_input_vehicles_WTH_EOR.xml", "Test6_output_vehicles_WTH_EOR.xml", "WTH");
        AssertMapping("Test7_input_containers_FWSubShip.xml", "Test7_output_containers_FWSubShip.xml", "ORG", "");
        AssertMapping("Test8_input_vehicles_FWSubShip.xml", "Test8_output_vehicles_FWSubShip.xml", "ORG", "");
        AssertMapping("Test9_input_ferry_containers_ORG_DUNS.xml", "Test9_output_ferry_containers_ORG_DUNS.xml", "ORG", "");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string purpose, string previousJobNumber = "CPO0000000099")
    {
        var input = filePath + inputFile;
        var expectedOutput = filePath + expectedOutputFile;

        var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
        var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
        var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

        var serviceProvider = "CPOINT";
        var destinationParty = serviceProvider + "_EBADEC1";
        var serviceProviderMSGID = serviceProvider.Substring(0, 3) + "MSG";
        var serviceProviderID = serviceProvider.Substring(0, 3) + "ID";
        var serviceProviderPrefix = serviceProvider.Substring(0, 3);
        var newInterchangeNumber = (purpose == "ORG" || purpose == "AMD") ? "0000000100" : "0000000099";
        var formattedInterchangeNumber = serviceProviderPrefix + newInterchangeNumber;

        mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER");
        mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);
        mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("TrackingID_0000001");
        mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "EBADEC_TESTSENDER_1"));
        mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "FPM_" + serviceProvider + "_BEANR"));

        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.BE.CPOINT.Interchange", "@maxlength", "50")).Return("1");
        mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider + "_EBADEC1")).Return(serviceProvider).Repeat.Any();
        mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", destinationParty)).Return(serviceProviderMSGID);
        mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", destinationParty)).Return(serviceProviderID);
        mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "RecipientID", serviceProvider, "BEANR", "")).Return(serviceProvider + "_Recipient_ID");

        mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER", "SHA", serviceProvider)).Return("CPOINT001");
        mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", "CPOINT", "@recipientId", "TESTSENDER", "@ST_ID", serviceProviderMSGID, "@value", "C2001246030", "@referenceType", "EBADEC")).Return(previousJobNumber).Repeat.Any();

        if (string.IsNullOrEmpty(previousJobNumber) || purpose != "WTH")
        {
            mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.BE.CPOINT", "@maxlength", "50")).Return("100");
            mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "SubscriptionPrefix", destinationParty)).Return(serviceProviderPrefix);
            mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "C2001246030", "EBADEC"));
            mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", "C2001246030", formattedInterchangeNumber, "EBADEC"));
        }

        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "ForwardingConsol", "ForwardingType"));
        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, purpose, "Purpose"));

        mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "Notification of Export Consignment", "DocumentName"));
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
        mapTester.ExecuteCompiled<UShipment2EBADEC>(input, expectedOutput);

        mockContextAccessor.VerifyAllExpectations();
        mockDataModelAccessor.VerifyAllExpectations();
        mockCodeMapper.VerifyAllExpectations();
    }
  }
}