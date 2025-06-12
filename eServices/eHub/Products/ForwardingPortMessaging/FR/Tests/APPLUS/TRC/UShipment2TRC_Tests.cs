using System.Collections.Generic;
using System.Reflection;
using System.Security;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS;
using CargoWise.eHub.Products.ForwardingPortMessaging.FR.Schemas.APPLUS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.FR.Tests
{
	[TestClass]
	public class UShipment2TRC_Tests
	{
		const string filePath = "APPLUS.TRC.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2TRC()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml", "SOGET", "ORG", "Import", "SUDU6959917;SUDU6958096");
			AssertMapping("Test1b_input.xml", "Test1b_output.xml", "SOGET", "AMD", "Import", "SUDU6959917;SUDU6958096");
			AssertMapping("Test2_input.xml", "Test2_output.xml", "SOGET", "WTH", "Export");
			AssertMapping("Test3_input.xml", "Test3_SOGET_output.xml", "SOGET", "ORG");
			AssertMapping("Test3_input.xml", "Test3_MGI_output.xml", "MGI", "ORG");
			AssertMapping("Test3_input.xml", "Test3_MGI_output.xml", "MGI", "ORG", routingPartyValue: "SOGET");
			AssertMapping("Test4_input.xml", "Test4_SOGET_output.xml", "SOGET", "ORG");
			AssertMapping("Test4_input.xml", "Test4_MGI_output.xml", "MGI", "ORG");
		}

		void AssertMapping(string inputFile, string expectedOutputFile, string serviceProvider, string purpose, string documentName = "", string containers = "", string routingPartyValue = "")
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			var destinationParty = serviceProvider + "_TRC1";
			var serviceProviderMSGID = serviceProvider.Substring(0, 3) + "MSG";
			var serviceProviderID = serviceProvider.Substring(0, 3) + "ID";
			var serviceProviderPrefix = serviceProvider.Substring(0, 3);
			var formattedInterchangeNumber = serviceProviderPrefix + "0000000099";

			var numberOfCountainers = containers.Split(';').Length;

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return("TESTSENDER");
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(destinationParty);
			mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return("TrackingID_0000001");
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", "TRC_TESTSENDER_1"));
			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideEmailSubject", "http://cargowise.com/ehub/processing/2010/06", "FPM_" + serviceProvider + "_FRMRS"));

			var routingParty = routingPartyValue != "" ? routingPartyValue : serviceProvider;
			mockDataModelAccessor.Expect(x => x.GetClientRegistrationAttri1AsString("TESTSENDER", "FRMRS", serviceProvider)).Return(routingParty);
			if (routingParty != "" && serviceProvider != routingParty)
			{
				mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", routingParty + "_" + serviceProvider + "_1"));
			}

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS.Interchange", "@maxlength", "14")).Return("1").Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessaging.FR.APPLUS", "@maxlength", "14")).Return("99").Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "Name", serviceProvider + "_TRC1")).Return(serviceProvider);
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "MSGID", destinationParty)).Return(serviceProviderMSGID);
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "ID", destinationParty)).Return(serviceProviderID);
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Port Settings", "SubscriptionPrefix", destinationParty)).Return(serviceProviderPrefix);
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "SenderID", serviceProvider, "FRMRS", routingParty)).Return(serviceProvider + "_SENDER_ID");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "RecipientID", serviceProvider, "FRMRS", routingParty)).Return(serviceProvider + "_Recipient_ID");
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "DestinationUser", serviceProvider, "FRMRS", routingParty)).Return(serviceProvider + "_U").Repeat.Times(numberOfCountainers);
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE", "FORWARDING_PORT_MESSAGE", "FPM System Configuration", "Service Provider Settings", "DestinationParty", serviceProvider, "FRMRS", routingParty)).Return(serviceProvider + "_P").Repeat.Times(numberOfCountainers);

			mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER", "FRMRS", serviceProvider)).Return("APPLUS001,DHLLTR");
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderID, serviceProvider, "TESTSENDER", "APPLUS001,DHLLTR"));
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", "TrackingID_0000001", "1_" + formattedInterchangeNumber)).Repeat.Times(numberOfCountainers);

			foreach (string container in containers.Split(';'))
			{
				var previousConsolReference = ("AMD;WTH").Contains(purpose) ? formattedInterchangeNumber : string.Empty;
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", "TESTSENDER", "@ST_ID", serviceProviderMSGID, "@value", string.Concat("C00001266_", container, "_TRC"), "@referenceType", "JobNumber")).Return(previousConsolReference);
			}

			if (purpose == "ORG")
			{
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "ForwardingConsol", "ForwardingType")).Repeat.Times(numberOfCountainers);
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, string.Concat("Tracing Request (TRC)", documentName != string.Empty ? string.Concat(" - ", documentName) : string.Empty), "DocumentName")).Repeat.Times(numberOfCountainers);
				

				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, formattedInterchangeNumber, "RequestID")).Repeat.Times(numberOfCountainers);

				foreach (string container in containers.Split(';'))
				{
					mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, string.Concat("C00001266_", container, "_TRC"), "JobNumber"));
					mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", string.Concat("C00001266_", container, "_TRC"), formattedInterchangeNumber, "JobNumber"));

					mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, container, "ContainerNumber"));
				}
			}

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, purpose, "Purpose")).Repeat.Times(numberOfCountainers);
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "CARRIERBOOKINGREF", "BookingConfirmationReference")).Repeat.Times(numberOfCountainers);
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "BOL123456789", "WayBillNumber")).Repeat.Times(numberOfCountainers);
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "MSCCCCUS", "Carrier")).Repeat.Times(numberOfCountainers);
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, "TESTSENDER", formattedInterchangeNumber, "FRMRS", "OperationPort")).Repeat.Times(numberOfCountainers);

			var extensionObjects = new Dictionary<string, object>()
	  {
		{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
		{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
		{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
	  };

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2TRC>(input, expectedOutput);

			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();

			AssertSchema(expectedOutput);
		}

		public void AssertSchema(string expectedOutputFile)
		{
			var schemaValidator = XMLValidator.Validate<trc>(expectedOutputFile);
			Assert.AreEqual(string.Empty, schemaValidator, expectedOutputFile + "\r\nUShipment2TRC is not valid: \r\n " + schemaValidator);
		}
	}
}
