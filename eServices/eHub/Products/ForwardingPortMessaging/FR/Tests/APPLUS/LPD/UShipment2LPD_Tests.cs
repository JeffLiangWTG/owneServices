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
  public class UShipment2LPD_Tests
  {
    const string filePath = "APPLUS.LPD.TestFiles.";

    [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
    public void TestUShipment2LPD()
    {
      AssertMapping("Test1_input.xml", "Test1_SOGET_output.xml", "SOGET", operationalPort: "REPOS");
      AssertMapping("Test1_input.xml", "Test1_MGI_output.xml", "MGI", operationalPort: "REPOS");
      AssertMapping("Test1_input.xml", "Test1_SOGET_previousMessageIdentifier_output.xml", "SOGET", operationalPort: "REPOS", previousMessageIdentifier: "SOG0000001817");
      AssertMapping("Test1_input.xml", "Test1_MGI_previousMessageIdentifier_output.xml", "MGI", operationalPort: "REPOS", previousMessageIdentifier: "SOG0000001817");
      AssertMapping("Test1_input.xml", "Test1_MGI_output.xml", "MGI", routingPartyValue: "SOGET", operationalPort: "REPOS");
      AssertMapping("Test2_input.xml", "Test2_output.xml", "MGI");
    }

    void AssertMapping(string inputFile, string expectedOutputFile, string serviceProvider, string routingPartyValue = "", string operationalPort = "FRMRS", string previousMessageIdentifier = "")
    {
      var input = filePath + inputFile;
      var expectedOutput = filePath + expectedOutputFile;

      var destinationParty = serviceProvider + "_AMQ1";
      var serviceProviderMSGID = serviceProvider.Substring(0, 3) + "MSG";
      var serviceProviderID = serviceProvider.Substring(0, 3) + "ID";

			var serviceProviderPrefix = serviceProvider.Substring(0, 3);
			var formattedInterchangeNumber = serviceProviderPrefix + "0000000099";

			var messageIdentifier = !(string.IsNullOrEmpty(previousMessageIdentifier)) ? previousMessageIdentifier : formattedInterchangeNumber;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
      var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
      var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

      mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER");
      mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);
      mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("TrackingID_0000001");
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "LPD_TESTSENDER_1"));
      mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "FPM_" + serviceProvider + "_" + operationalPort));

      var routingParty = routingPartyValue != "" ? routingPartyValue : serviceProvider;
      if (routingParty != "" && serviceProvider != routingParty)
      {
        mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", routingParty + "_" + serviceProvider + "_1"));
      }

      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS", "@maxlength", "14")).Return("99").Repeat.Any();
      mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS.Interchange", "@maxlength", "14")).Return("1").Repeat.Any();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "TESTSENDER", "@ST_ID", serviceProviderMSGID, "@value", "C00001266_CONT1111111_LPD", "@referenceType", "MessageReference")).Return(previousMessageIdentifier).Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "TESTSENDER", "@ST_ID", serviceProviderMSGID, "@value", "C00001266_CONT2222222_LPD", "@referenceType", "MessageReference")).Return(previousMessageIdentifier).Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider + "_AMQ1")).Return(serviceProvider).Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", destinationParty)).Return(serviceProviderMSGID);
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", destinationParty)).Return(serviceProviderID);
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "SubscriptionPrefix", destinationParty)).Return(serviceProviderPrefix);

			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "SenderID", serviceProvider, operationalPort, routingParty)).Return(serviceProvider + "_01");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "RecipientID", serviceProvider, operationalPort, routingParty)).Return(serviceProvider + "_02");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "DestinationUser", serviceProvider, operationalPort, routingParty)).Return(serviceProvider + "_03");
      mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "DestinationParty", serviceProvider, operationalPort, routingParty)).Return(serviceProvider + "_04");

      mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER", operationalPort, serviceProvider)).Return("APPLUS001,DHLLTR");
      mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderID, serviceProvider, "TESTSENDER", "APPLUS001,DHLLTR"));

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, "C00001266", "JobNumber")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", "C00001266", messageIdentifier, messageIdentifier)).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, "ForwardingConsol", "ForwardingType")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, "ORG", "Purpose")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, "WTH", "Purpose")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, operationalPort, "OperationPort")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, "Provisional Unpacking List (LPD)", "DocumentName")).Repeat.Any();

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", "TrackingID_0000001", "1_" + messageIdentifier)).Repeat.Any();

			string[] containerNumbers = { "CONT1111111", "CONT2222222" };
			foreach (var containerNumber in containerNumbers)
			{
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, containerNumber, "RequestID")).Repeat.Any();
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", messageIdentifier, containerNumber, "ContainerNumber")).Repeat.Any();

				if (string.IsNullOrEmpty(previousMessageIdentifier))
				{
					mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", string.Concat("C00001266_", containerNumber, "_LPD"), messageIdentifier, "MessageReference")).Repeat.Any();
				}
			}


			mockDataModelAccessor.Expect(x => x.GetClientRegistrationAttri1AsString("TESTSENDER", operationalPort, serviceProvider)).Return(routingParty).Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "ContainerTypeToISOCode", serviceProvider + " Code", "45G1")).Return("45G1").Repeat.Any();
      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "ContainerTypeToISOCode", serviceProvider + " Code", "45G2")).Return("45G2").Repeat.Any();

      mockCodeMapper.Expect(x => x.GetRecipientCode(serviceProvider, serviceProvider, serviceProvider + " System Configuration", "Package Type ISO", serviceProvider + " Code", "PKG")).Return("PKG").Repeat.Any();

      var extensionObjects = new Dictionary<string, object>()
      {
        { "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
        { "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
      };

      var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);

      mapTester.ExecuteCompiled<UShipment2LPD>(input, expectedOutput);
    }
  }
}
