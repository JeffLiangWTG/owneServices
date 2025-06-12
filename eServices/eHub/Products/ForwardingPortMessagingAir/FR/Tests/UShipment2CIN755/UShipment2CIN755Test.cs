using System.Collections.Generic;
using System.Reflection;
using CargoWise.BizTalk.UnitTestFX;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.CIN;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.Tests
{
	[TestClass]
	public class UShipment2CIN755Test
	{
		const string filePath = "UShipment2CIN755.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void TestUShipment2CIN755()
		{
			AssertMapping("Test1_ORG_input.xml", "Test1_ORG_output.xml", "ORG");
			AssertMapping("Test2_AMD_input.xml", "Test2_AMD_output.xml", "AMD");
			AssertMapping("Test3_WTH_input.xml", "Test3_WTH_output.xml", "WTH");
		}

		void AssertMapping(string inputFile, string expectedOutputFile, string purposeCode)
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var serviceProvider = "CIN";
			var serviceProviderMSGID = serviceProvider + "MSG";

			var senderID = "TESTSENDER";
			var recipientID = serviceProvider + "_755";

			var internalTrackingID = "CCF9920A-765D-4334-B657-1656729A44FE";
			var interchangeCounter = "2235";
			var interchangeId = "0000000000" + interchangeCounter;
			var msgCounter = "1";
			var messageReferenceNumber = "0000000000" + msgCounter;
			var shipmentID = "SAPAR00000016";

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDateMapper = MockRepository.GenerateStrictMock<DateMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderID);
			mockContextAccessor.Expect(x => x.GetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(recipientID);
			mockContextAccessor.Expect(x => x.GetContextProperty("InternalTrackingID", "http://cargowise.com/ehub/tracking/2010/06")).Return(internalTrackingID);

			mockContextAccessor.Expect(x => x.SetContextProperty("OverrideFilename", "http://cargowise.com/ehub/processing/2010/06", $"CIN755_{senderID}_{interchangeId}")).Repeat.Any();

			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE_AIR", "FORWARDING_PORT_MESSAGE_AIR", "FPMA System Configuration", "Port Settings", "MSGID", recipientID)).Return(serviceProviderMSGID).Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE_AIR", "FORWARDING_PORT_MESSAGE_AIR", "FPMA System Configuration", "Port Settings", "Name", recipientID)).Return(serviceProvider).Repeat.Any();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.CIN.UNH", "@maxlength", "14")).Return(interchangeCounter).Repeat.Any();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.CIN.MRN", "@maxlength", "11")).Return(msgCounter).Repeat.Any();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", serviceProvider, "@recipientId", senderID, "@ST_ID", serviceProviderMSGID, "@value", shipmentID, "@referenceType", "JobNumber")).Return(messageReferenceNumber).Repeat.Any();

			mockDataModelAccessor.Expect(x => x.GetClientRegistrationCode("TESTSENDER", "SYD", "CIN")).Return("SND").Repeat.Any();

			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, internalTrackingID, messageReferenceNumber)).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, "23FR000170248524E5", messageReferenceNumber, "MRN")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, "23FRC02300316235E4", messageReferenceNumber, "MRN")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReferenceNumber, shipmentID, "JobNumber")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, shipmentID, messageReferenceNumber, "JobNumber")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReferenceNumber, "ForwardingShipment", "ForwardingType")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReferenceNumber, "Export Notification (755)", "DocumentName")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReferenceNumber, purposeCode, "ActionPurpose")).Repeat.Any();
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue(serviceProviderMSGID, serviceProvider, senderID, messageReferenceNumber, "AAAAA", "OperationPort")).Repeat.Any();

			mockDateMapper.Expect(x => x.CurrentDateTimeUTC("O")).Return("2023-08-31T23:59:58.6464725Z");

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/DateMapper", mockDateMapper },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor }
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<UShipment2CIN755>(input, expectedOutput);
		}
	}
}
