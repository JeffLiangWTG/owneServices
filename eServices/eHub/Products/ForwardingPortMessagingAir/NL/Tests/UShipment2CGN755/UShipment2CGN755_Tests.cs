using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.CARGONAUT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.Tests
{
	[TestClass]
	public class UShipment2CGN755_Tests
	{
		const string filePath = "UShipment2CGN755.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_UShipment2CGN755()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml");
			AssertMapping("Test2_input.xml", "Test2_output.xml");
			AssertMapping("Test3_input.xml", "Test3_output.xml", "ForwardingConsol", "AMD", string.Empty);
			AssertMapping("Test4_input.xml", "Test4_output.xml", "ForwardingConsol");
		}

		void AssertMapping(string inputFile, string expectedOutputFile, string forwardingType = "ForwardingShipment", string actionPurpose = "ORG", string operationPort = "NLAMS")
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var serviceProvider = "CARGONAUT";
			var serviceProviderMSGID = "CGNMSG";
			var senderID = "TESTSENDER";
			var recipientID = "CARGONAUT_755";

			var internalTrackingID = "CCF9920A-765D-4334-B657-1656729A44FE";
			var interchangeCounter = "57622";
			var interchangeId = "000000000" + interchangeCounter;
			var msgCounter = "100006920";
			var messageReferenceNumber = "00" + msgCounter;
			var shipmentID = "SAPAR00000016";

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID);
			mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return(internalTrackingID);

			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", $"CGN755_{senderID}_{interchangeId}")).Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE_AIR", "FORWARDING_PORT_MESSAGE_AIR", "FPMA System Configuration", "Port Settings", "MSGID", recipientID)).Return(serviceProviderMSGID).Repeat.Once();
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE_AIR", "FORWARDING_PORT_MESSAGE_AIR", "FPMA System Configuration", "Port Settings", "Name", recipientID)).Return(serviceProvider).Repeat.Once();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.CARGONAUT.UNH", "@maxlength", "14")).Return(interchangeCounter).Repeat.Once();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", senderID, "@ST_ID", serviceProviderMSGID, "@value", shipmentID, "@referenceType", "JobNumber")).Return(messageReferenceNumber).Repeat.Once();

			mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER", "AMS", serviceProvider)).Return("XXX").Repeat.Once();

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, internalTrackingID, messageReferenceNumber)).Repeat.Once();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, "MRN00000011", messageReferenceNumber, "MRN")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, "MRN00000022", messageReferenceNumber, "MRN")).Repeat.Any();

			if (actionPurpose == "ORG")
			{
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.CARGONAUT.MRN", "@maxlength", "11")).Return(msgCounter).Repeat.Once();
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReferenceNumber, shipmentID, "JobNumber")).Repeat.Once();
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, shipmentID, messageReferenceNumber, "JobNumber")).Repeat.Once();
			}
			else
			{
				mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessagingAir.NL.CARGONAUT.MRN", "@maxlength", "11")).Return(msgCounter).Repeat.Never();
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReferenceNumber, shipmentID, "JobNumber")).Repeat.Never();
				mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, shipmentID, messageReferenceNumber, "JobNumber")).Repeat.Never();
			}

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReferenceNumber, forwardingType, "ForwardingType")).Repeat.Once();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReferenceNumber, "Export Notification Cargonaut (NL)", "DocumentName")).Repeat.Once();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReferenceNumber, actionPurpose, "ActionPurpose")).Repeat.Once();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReferenceNumber, operationPort, "OperationPort")).Repeat.Once();

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("yyyyMMddHHmm")).Return("202407031615").Repeat.Any();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.Execute<UShipment2CGN755>(input, expectedOutput);

			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
		}
	}
}
