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
	public class XML757_2UInterchangeInclude_Tests
	{
		const string filePath = "XML757_2UInterchangeInclude.TestFiles.";

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_XML757_2UInterchangeInclude()
		{
			AssertMapping("Test1_input.xml", "Test1_output.xml", "12FRD0619B19477001", "331_R");
			AssertMapping("Test2_input.xml", "Test2_output.xml", "23FR000170248524E5", "MAN_REC");
			AssertMapping("Test3_input.xml", "Test3_output.xml", "MRN_Num_000001", "XXXXX", "");
		}

		void AssertMapping(string inputFile, string expectedOutputFile, string mrn, string lastStatus, string expectedEventCode = "STU")
		{
			var input = filePath + inputFile;
			var expectedOutput = filePath + expectedOutputFile;

			var recipientID = "recipientId111";
			var senderId = "CIN";
			var serviceProviderMsgId = senderId.Substring(0, 3) + "MSG";
			var subscribedDocumentIdentifier = "subDocID_123";

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderId).Repeat.Once();

			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE_AIR", "FORWARDING_PORT_MESSAGE_AIR", "FPMA System Configuration", "Port Settings", "MSGID", senderId)).Return(serviceProviderMsgId).Repeat.Once();
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE_AIR", "FORWARDING_PORT_MESSAGE_AIR", "FPMA System Configuration", "Port Settings", "Name", senderId)).Return(senderId).Repeat.Once();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", mrn, "@referenceType", "MRN")).Return(subscribedDocumentIdentifier).Repeat.Once();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", subscribedDocumentIdentifier, "@referenceType", "DocumentName")).Return("Export Notification (755)").Repeat.Once();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", subscribedDocumentIdentifier, "@referenceType", "JobNumber")).Return("SAPAR00000016").Repeat.Once();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", subscribedDocumentIdentifier, "@referenceType", "ForwardingType")).Return("ForwardingShipment").Repeat.Once();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", subscribedDocumentIdentifier, "@referenceType", "OperationPort")).Return("FRCDG").Repeat.Once();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderId, "@ST_ID", serviceProviderMsgId, "@value", subscribedDocumentIdentifier)).Return(recipientID).Repeat.Any();
			mockContextAccessor.Expect(x => x.SetContextProperty("DestinationParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties", recipientID));

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("GetCounterValue", "@value", "@name", "CargoWise.eHub.Products.ForwardingPortMessagingAir.FR.CIN.757", "@maxlength", "14")).Return("1").Repeat.Any();

			mockCodeMapper.Stub(x => x.GetRecipientCode("CIN", "CIN", "CIN Provider Configuration", "Event Type", "EventType", lastStatus)).Return(expectedEventCode);
			if (!string.IsNullOrEmpty(expectedEventCode))
			{
				mockCodeMapper.Stub(x => x.GetRecipientCode("CIN", "CIN", "CIN Provider Configuration", "Event Type", "EventParameters", lastStatus)).Return("Department=Terminal|MessageType=.|EquipmentReferenceNumber=.|ReferenceNumber=.|CustomsReferenceNumber=.|Location=.|Type=FFM and departure message received by CIN|Reason=.");
			}
			mockDataModelAccessor.Expect(x => x.InsertSubscriptionValue("CINTRC", recipientID, senderId,  subscribedDocumentIdentifier + "_" + mrn + "_1", lastStatus, "StatusUpdate")).Repeat.Once();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
			};

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteCompiled<XML757_2UInterchangeInclude>(input, expectedOutput);

			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
			mockCodeMapper.VerifyAllExpectations();
		}

		[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
		public void Test_XML757_2UInterchangeInclude_ThrowException()
		{
			var input = filePath + "Test4_ThrowException_input.xml";

			var senderId = "CIN";
			var serviceProviderMsgId = senderId.Substring(0, 3) + "MSG";
			var recipientID = "";
			var subscribedDocumentIdentifier = "";
			var mrn = "MRN_Num_000001";

			var mockContextAccessor = MockRepository.GenerateStrictMock<ContextAccessor>();
			var mockCodeMapper = MockRepository.GenerateStrictMock<CodeMapper>();
			var mockDataModelAccessor = MockRepository.GenerateStrictMock<DataModelAccessor>();

			var extensionObjects = new Dictionary<string, object>()
			{
				{ "http://schemas.microsoft.com/BizTalk/2003/ContextAccessor", mockContextAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/DataModelAccessor", mockDataModelAccessor },
				{ "http://schemas.microsoft.com/BizTalk/2003/CodeMapper", mockCodeMapper },
			};

			mockContextAccessor.Expect(x => x.GetContextProperty("SourceParty", "http://schemas.microsoft.com/BizTalk/2003/system-properties")).Return(senderId).Repeat.Once();

			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedReference", "@reference", "@senderId", senderId, "@recipientId", "", "@ST_ID", serviceProviderMsgId, "@value", mrn, "@referenceType", "MRN")).Return(subscribedDocumentIdentifier).Repeat.Once();
			mockCodeMapper.Expect(x => x.CallActionProcedureHelper("SelectSubscribedClients", "", "@senderId", senderId, "@ST_ID", serviceProviderMsgId, "@value", subscribedDocumentIdentifier)).Return(recipientID).Repeat.Any();
			mockCodeMapper.Expect(x => x.GetRecipientCode("FORWARDING_PORT_MESSAGE_AIR", "FORWARDING_PORT_MESSAGE_AIR", "FPMA System Configuration", "Port Settings", "MSGID", senderId)).Return(serviceProviderMsgId).Repeat.Once();

			var mapTester = new MapTester(Assembly.GetExecutingAssembly(), extensionObjects);
			mapTester.ExecuteAssertException<XML757_2UInterchangeInclude>(input, "Exception has been thrown by the target of an invocation.");

			mockCodeMapper.VerifyAllExpectations();
			mockContextAccessor.VerifyAllExpectations();
			mockDataModelAccessor.VerifyAllExpectations();
		}
	}
}
