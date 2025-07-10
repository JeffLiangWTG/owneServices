using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using EDIMessage = Enterprise.Customs.PL.Business.EDIMessage;

namespace Enterprise.Customs.PL.ServiceTasks.Testing;

[TestedType(typeof(MessageRetrievingService))]
sealed class MessageRetrievingServiceTest : ServiceTaskTestCase<MessageRetrievingService>
{
	public void TestServiceAttribute()
		=> AssertSingleHostedServiceAttribute(
			expectedCode: "PLI",
			expectedDescription: "Polish Customs Message Retriever",
			expectedCategory: "PLC");

	public void TestHostedServiceBindingAttribute()
		=> CommonServiceTestHelper.AssertSingleHostedServiceAttribute<MessageRetrievingService>(
			expectedCode: "PLI",
			expectedDescription: "Polish Customs Message Retriever",
			expectedCategory: "PLC",
			expectedServiceType: typeof(MessageRetrievingService),
			expectedRequiresCompanyInCountry: "PL",
			expectedCanRunInAnyBranch: true,
			expectedMinimumPeriod: "60Seconds");

	public void TestProcessCorrectAcceptDocumentResponse()
	{
		const string expectedApplicationReference = "20ce80cf-ebce-40ae-b880-716ddc4138e1";

		var responseXml = Assembly.GetExecutingAssembly().GetTestFile("Enterprise.Customs.PL.ServiceTasks.Test.TestFiles.AcceptDocumentResponseCorrectBody.xml");
		var (requestMessage, responseInterchange) = Factory.PrepareRequestWithResponse(
			responseXml,
			applicationCode: ApplicationCode.PLCustoms);
		Factory.Save();

		_ = InitialiseAndRunTaskSchedule(new MessageRetrievingService());
		responseInterchange.Reload();
		requestMessage.Reload();

		AssertEquals("Interchange status", EDIInterchange.Status.Received, responseInterchange.EI_Status);
		AssertEquals("Validating request message application reference", expectedApplicationReference, requestMessage.EM_ApplicationReference);
	}

	public void Test_GetDocumentsResponseDocument_PLC()
	{
		const string multipleDocumentsTestFile = "Enterprise.Customs.PL.ServiceTasks.Test.TestFiles.GetDocumentsResponsePLC.xml";
		var xmlString = Assembly.GetExecutingAssembly().GetTestFile(multipleDocumentsTestFile);

		var messageExchange = GetDocumentsResponseDocumentUnpackingTestHelper.CreateMessageExchange(
			Factory,
			xmlString,
			ApplicationCode.PLCustoms,
			EUJobMessageTypeList.Codes.Export);
		var cusPollingTransaction = messageExchange.CusPollingTransaction;
		var interchange = messageExchange.ResponseInterchange;

		var cusEntryHeader = Factory.NewWithValidTestData<CusEntryHeader>();

		var transmitMessage1 = Factory.New<EDIMessage>();
		transmitMessage1.EM_ApplicationCode = ApplicationCode.PLCustoms;
		transmitMessage1.EM_MessageType = EUJobMessageTypeList.Codes.Export;
		transmitMessage1.EM_MessageSubType = "525";
		transmitMessage1.EM_MessageNum = "230010732";
		transmitMessage1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		transmitMessage1.EM_Status = EDIMessage.Status.Sent;
		transmitMessage1.EM_GB = interchange.EI_GB;
		transmitMessage1.EM_LinkedObject = cusEntryHeader;

		var transmitMessage2 = Factory.New<EDIMessage>();
		transmitMessage2.EM_ApplicationCode = ApplicationCode.PLCustoms;
		transmitMessage2.EM_MessageType = EUJobMessageTypeList.Codes.Export;
		transmitMessage2.EM_MessageSubType = "525";
		transmitMessage2.EM_MessageNum = "230010733";
		transmitMessage2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		transmitMessage2.EM_Status = EDIMessage.Status.Sent;
		transmitMessage2.EM_GB = interchange.EI_GB;
		transmitMessage2.EM_LinkedObject = cusEntryHeader;
		Factory.Save();

		transmitMessage1.EM_MessageNum = "230010732";
		transmitMessage2.EM_MessageNum = "230010733";
		Factory.Save();

		var expectedCreatedMessageParams = new[] {
			(MessageType: EUJobMessageTypeList.Codes.Export, MessageSubType: "529", MessageNum: "23IE529-P10732"),
			(MessageType: EUJobMessageTypeList.Codes.Export, MessageSubType: "528", MessageNum: "23IE528-P11182"),
		};
		var expectedInterchangeLogMessages = new[] {
			(LogType: Events.InterchangeInProgress, Message: "PLC:EXP/529 message was created from document [0]=CC529C_23IE529-P10732.xml."),
			(LogType: Events.InterchangeInProgress, Message: "PLC:EXP/528 message was created from document [1]=CC528C_23IE528-P11182.xml."),
			(LogType: Events.InterchangeAcknowledged, Message: "GetDocumentsResponse acknowledgements of IE529, IE528 is processed, created 2 messages."),
			(LogType: Events.ErrorReport, Message: "Transmit message not found: OdrzucenieKomunikatu_23OdrzKom-P12527.xml."),
		};
		var expectedServiceLogMessages = new[] {
			"Information|GetDocumentsResponse acknowledgements of IE529, IE528 is processed, created 2 messages. Interchange number: {InterchangeNum}, PLC/EXP.",
			"Error|Transmit message not found: OdrzucenieKomunikatu_23OdrzKom-P12527.xml. Interchange number: {InterchangeNum}, PLC/EXP.",
			"Information|Interchange '{InterchangeNum}' has been processed successfully.",
		}.Select(x => x.Replace("{InterchangeNum}", interchange.EI_InterchangeNum))
		.ToArray();

		var serviceLog = InitialiseAndRunTaskSchedule(new MessageRetrievingService());
		interchange.Reload();

		CombineAssertions(() =>
		{
			AssertEquals("Created messages count", expectedCreatedMessageParams.Length, interchange.ContainedMessages.Count);

			foreach (var (message, index) in interchange.ContainedMessages.OfType<BaseEDIMessage>().Select((x, i) => (x, i)))
			{
				Assert($"Validating created message number {index} ", expectedCreatedMessageParams.Any(expected
					=> message.EM_MessageType == expected.MessageType
						&& message.EM_MessageSubType == expected.MessageSubType
						&& (string.IsNullOrEmpty(expected.MessageNum) || message.EM_MessageNum == expected.MessageNum)));
			}

			expectedInterchangeLogMessages.ForEach(x => interchange.AssertHasExactLogMessage(
				$"Interchange log {x.LogType} [{x.Message}]", x.LogType, x.Message));

			serviceLog.AssertContainsMessages(expectedServiceLogMessages);
			Assert("CusPollingTransaction has no changes.", !cusPollingTransaction.HasChanges);
		});
	}

	public void Test_GetDocumentsResponseDocument_0Documents()
	{
		const string multipleDocumentsTestFile = "Enterprise.Customs.PL.ServiceTasks.Test.TestFiles.GetDocumentsResponseWith0Documents.xml";
		var xmlString = Assembly.GetExecutingAssembly().GetTestFile(multipleDocumentsTestFile);

		var messageExchange = GetDocumentsResponseDocumentUnpackingTestHelper.CreateMessageExchange(
			Factory,
			xmlString,
			ApplicationCode.PLCustoms,
			EUJobMessageTypeList.Codes.Export);
		var interchange = messageExchange.ResponseInterchange;
		var cusPollingTransaction = messageExchange.CusPollingTransaction;
		Factory.Save();

		var expectedInterchangeLogMessages = new[] {
			(LogType: Events.InterchangeReceived, Message: "During GetDocumentsResponse Interchange processing no documents were successfully processed.")
		};

		InitialiseAndRunTaskSchedule(new MessageRetrievingService());
		interchange.Reload();

		CombineAssertions(() =>
		{
			AssertEquals("Interchange status", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Created messages count", 0, interchange.ContainedMessages.Count);

			foreach (var (eventType, message) in expectedInterchangeLogMessages)
			{
				AssertCollectionContains($"Interchange log {eventType} [{message}] ", interchange.Logs.GetAllLogs().Cast<StmALog>(),
					x => x.SL_Reference.Contains(message) && x.SL_SE_NKEvent == eventType.Code);
			}

			Assert("CusPollingTransaction has no changes.", !cusPollingTransaction.HasChanges);
		});
	}

	public void Test_GetDocumentsResponseDocument_NCTS()
	{
		const string multipleDocumentsTestFile = "Enterprise.Customs.PL.ServiceTasks.Test.TestFiles.GetDocumentsResponseNCTS.xml";
		var xmlString = Assembly.GetExecutingAssembly().GetTestFile(multipleDocumentsTestFile);

		CreateTestOutboundMessageWithLinkedObject("850cce3f-0091-4c24-8177-87ffa8266d7f", EDIMessage.ApplicationCodes.PLCustomsNCTS, MessageTypeList.Codes.NctsDeparture);
		CreateTestOutboundMessageWithLinkedObject("1f27f64f-8793-4775-9c87-f7d8b9440c77", EDIMessage.ApplicationCodes.PLCustomsNCTS, MessageTypeList.Codes.NctsDeparture);

		var messageExchange = GetDocumentsResponseDocumentUnpackingTestHelper.CreateMessageExchange(
			Factory,
			xmlString,
			ApplicationCode.PLCustomsNCTS,
			MessageTypeList.Codes.NctsDeparture);
		var cusPollingTransaction = messageExchange.CusPollingTransaction;
		var interchange = messageExchange.ResponseInterchange;

		var nctsMovementHeader = Factory.NewWithValidTestData<TestCusInBondHeaderInheritor>();
		var transmitMessage = Factory.New<EDIMessage>();
		transmitMessage.EM_ApplicationCode = ApplicationCode.PLCustomsNCTS;
		transmitMessage.EM_MessageType = EUJobMessageTypeList.Codes.NctsDeparture;
		transmitMessage.EM_MessageSubType = "015";
		transmitMessage.EM_MessageNum = "23IE015001X156";
		transmitMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		transmitMessage.EM_Status = EDIMessage.Status.Sent;
		transmitMessage.EM_GB = interchange.EI_GB;
		transmitMessage.EM_LinkedObject = nctsMovementHeader;
		Factory.Save();

		transmitMessage.EM_MessageNum = "23IE015001X156";
		Factory.Save();

		var expectedCreatedMessageParams = new[] {
			(MessageType: EUJobMessageTypeList.Codes.NctsDeparture, MessageSubType: "056"),
			(MessageType: EUJobMessageTypeList.Codes.NctsDeparture, MessageSubType: "NPP"),
			(MessageType: EUJobMessageTypeList.Codes.NctsDeparture, MessageSubType: "NPP"),
		};
		var expectedInterchangeLogMessages = new[] {
			(LogType: Events.InterchangeInProgress, Message: "Found GetDocumentsResponse"),
			(LogType: Events.ErrorReport, Message: "Transmit messages not found: OdrzucenieKomunikatu_23OdrzKom-666.xml, OdrzucenieKomunikatu_23OdrzKom-P11372.xml."),
			(LogType: Events.InterchangeInProgress, Message: "PLN:DEP/NPP message was created from document [1]=NPP.xml."),
			(LogType: Events.InterchangeInProgress, Message: "PLN:DEP/NPP message was created from document [2]=NPP.xml."),
			(LogType: Events.InterchangeInProgress, Message: "PLN:DEP/056 message was created from document [3]=IE056.xml."),
			(LogType: Events.InterchangeAcknowledged, Message: "GetDocumentsResponse acknowledgements of UPP, IE056PL is processed, created 3 messages."),
		};
		var expectedServiceLogMessages = new[] {
				"Error|Transmit messages not found: OdrzucenieKomunikatu_23OdrzKom-666.xml, OdrzucenieKomunikatu_23OdrzKom-P11372.xml. Interchange number: {InterchangeNum}, PLN/DEP.",
				"Information|GetDocumentsResponse acknowledgements of UPP, IE056PL is processed, created 3 messages. Interchange number: {InterchangeNum}, PLN/DEP.",
				"Information|Interchange '{InterchangeNum}' has been processed successfully.",
			}.Select(x => x.Replace("{InterchangeNum}", interchange.EI_InterchangeNum))
		.ToArray();

		var serviceLog = InitialiseAndRunTaskSchedule(new MessageRetrievingService());
		interchange.Reload();

		CombineAssertions(() =>
		{
			AssertEquals("Created messages count", 3, interchange.ContainedMessages.Count);

			foreach (var (message, index) in interchange.ContainedMessages.OfType<BaseEDIMessage>().Select((x, i) => (x, i)))
			{
				Assert($"Validating created message number {index} ", expectedCreatedMessageParams.Any(expected
					=> (string.IsNullOrEmpty(expected.MessageType) || message.EM_MessageType == expected.MessageType)
						&& message.EM_MessageSubType == expected.MessageSubType));
			}

			var interchangeLogs = interchange.Logs.GetAllLogs().Cast<StmALog>().ToList();
			foreach (var (eventType, message) in expectedInterchangeLogMessages)
			{
				AssertCollectionContains($"Interchange log {eventType} [{message}] ", interchangeLogs,
					x => x.SL_Reference.Contains(message) && x.SL_SE_NKEvent == eventType.Code);
			}

			serviceLog.AssertContainsMessages(expectedServiceLogMessages);

			Assert("CusPollingTransaction has no changes.", !cusPollingTransaction.HasChanges);
		});

		void CreateTestOutboundMessageWithLinkedObject(string applicationReference, string applicationCode, string messageType)
		{
			var message = Factory.New<TestEDIMessage>();
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_ApplicationReference = applicationReference;
			message.EM_ApplicationCode = applicationCode;
			message.EM_MessageType = messageType;
			message.EM_LinkedObject = Factory.NewWithValidTestData<TestCusInBondHeaderInheritor>();
			Factory.Save();
		}
	}

	public void Test_GetDocumentsResponseDocument_ExitControl()
	{
		const string multipleDocumentsTestFile = "Enterprise.Customs.PL.ServiceTasks.Test.TestFiles.GetDocumentsResponsePLE.xml";
		var xmlString = Assembly.GetExecutingAssembly().GetTestFile(multipleDocumentsTestFile);

		var receiveInterchangeWithTransmitEntities = GetDocumentsResponseDocumentUnpackingTestHelper.CreateMessageExchange(
			Factory,
			xmlString,
			ApplicationCode.PLCustomsExitControl,
			"EXT");
		var cusPollingTransaction = receiveInterchangeWithTransmitEntities.CusPollingTransaction;
		var interchange = receiveInterchangeWithTransmitEntities.ResponseInterchange;

		var cusEntryHeader = Factory.NewWithValidTestData<CusEntryHeader>();

		var transmitMessage1 = Factory.New<EDIMessage>();
		transmitMessage1.EM_ApplicationCode = ApplicationCode.PLCustomsExitControl;
		transmitMessage1.EM_MessageType = "EXT";
		transmitMessage1.EM_MessageSubType = "507";
		transmitMessage1.EM_MessageNum = "24IE507-P00001";
		transmitMessage1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		transmitMessage1.EM_Status = EDIMessage.Status.Sent;
		transmitMessage1.EM_GB = interchange.EI_GB;
		transmitMessage1.EM_LinkedObject = cusEntryHeader;
		Factory.Save();

		transmitMessage1.EM_MessageNum = "24IE507-P00001";
		Factory.Save();

		var expectedCreatedMessageParams = new[] { (MessageType: "EXT", MessageSubType: "525", MessageNum: "24IE507-P00001") };
		var expectedInterchangeLogMessages = new[] {
			(LogType: Events.InterchangeInProgress, Message: "Found GetDocumentsResponse"),
			(LogType: Events.InterchangeInProgress, Message: "PLX:EXT/525 message was created from document [0]=CC525C_24IE525-P00001.xml."),
			(LogType: Events.InterchangeAcknowledged, Message: "GetDocumentsResponse acknowledgement of IE525 is processed, created 1 messages."),
		};

		var expectedServiceLogMessages = new[] {
			"Information|GetDocumentsResponse acknowledgement of IE525 is processed, created 1 messages. Interchange number: {InterchangeNum}, PLX/EXT.",
			"Information|Interchange '{InterchangeNum}' has been processed successfully.",
		}.Select(x => x.Replace("{InterchangeNum}", interchange.EI_InterchangeNum))
		.ToArray();

		var serviceLog = InitialiseAndRunTaskSchedule(new MessageRetrievingService());
		interchange.Reload();

		CombineAssertions(() =>
		{
			AssertEquals("Created messages count", expectedCreatedMessageParams.Length, interchange.ContainedMessages.Count);

			foreach (var (message, index) in interchange.ContainedMessages.OfType<BaseEDIMessage>().Select((x, i) => (x, i)))
			{
				Assert($"Validating created message number {index} ", expectedCreatedMessageParams.Any(expected
					=> message.EM_MessageType == expected.MessageType
						&& message.EM_MessageSubType == expected.MessageSubType
						&& (string.IsNullOrEmpty(expected.MessageNum) || message.EM_MessageNum == expected.MessageNum)));
			}

			expectedInterchangeLogMessages.ForEach(x => interchange.AssertHasExactLogMessage(
				$"Interchange log {x.LogType} [{x.Message}]", x.LogType, x.Message));

			serviceLog.AssertContainsMessages(expectedServiceLogMessages);

			Assert("CusPollingTransaction has no changes.", !cusPollingTransaction.HasChanges);
		});
	}

	public void Test_GetDocumentsResponseDocument_NCTS_StoresAttachmentsAsEDocs()
	{
		const string multipleDocumentsTestFile = "Enterprise.Customs.PL.ServiceTasks.Test.TestFiles.GetDocumentsResponseNCTS.xml";
		var xmlString = Assembly.GetExecutingAssembly().GetTestFile(multipleDocumentsTestFile);

		var receiveInterchangeWithTransmitEntities = GetDocumentsResponseDocumentUnpackingTestHelper.CreateMessageExchange(
			Factory,
			xmlString,
			ApplicationCode.PLCustomsNCTS,
			MessageTypeList.Codes.NctsDeparture);
		var cusPollingTransaction = receiveInterchangeWithTransmitEntities.CusPollingTransaction;
		var interchange = receiveInterchangeWithTransmitEntities.ResponseInterchange;

		var nctsMovementHeader = Factory.NewWithValidTestData<TestCusInBondHeaderInheritor>();
		var transmitMessage = Factory.New<EDIMessage>();
		transmitMessage.EM_ApplicationCode = ApplicationCode.PLCustomsNCTS;
		transmitMessage.EM_MessageType = EUJobMessageTypeList.Codes.NctsDeparture;
		transmitMessage.EM_MessageSubType = "015";
		transmitMessage.EM_MessageNum = "23IE015001X156";
		transmitMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		transmitMessage.EM_Status = EDIMessage.Status.Sent;
		transmitMessage.EM_GB = interchange.EI_GB;
		transmitMessage.EM_LinkedObject = nctsMovementHeader;
		Factory.Save();

		transmitMessage.EM_MessageNum = "23IE015001X156";
		Factory.Save();

		InitialiseAndRunTaskSchedule(new MessageRetrievingService());
		interchange.Reload();

		var documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
		var documentFactory = documentFactoryProvider.GetFactory(Factory);

		var ncts056InboundMessage = interchange.ContainedMessages
			.OfType<BaseEDIMessage>()
			.Single(msg => msg.EM_MessageType == EUJobMessageTypeList.Codes.NctsDeparture && msg.EM_MessageSubType == "056");

		AssertNotNull(ncts056InboundMessage);
		var storageMain = documentFactory.GetStorageMainForPK(ncts056InboundMessage.PK);
		AssertNotNull(storageMain);
		Assert(storageMain.AllEDocs.Count == 2);
		var document1 = storageMain.AllEDocs[0];
		var document2 = storageMain.AllEDocs[1];

		CombineAssertions(() =>
		{
			AssertEquals("IE056.pdf", document1.FileName);
			AssertEquals("TAD", document1.DocType);
			AssertEquals("PDF", document1.DataType);

			AssertEquals("IE056[2].pdf", document2.FileName);
			AssertEquals("TAD", document2.DocType);
			AssertEquals("PDF", document2.DataType);

			Assert("CusPollingTransaction has no changes.", !cusPollingTransaction.HasChanges);
		});
	}

	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => new[] {
		new TaskNudgeInformationForTest(
			table: "EDIInterchange",
			queueName: "Polish Customs Message Retriever Import/Export",
			predicates:
			[
				"EI_Status=QUE",
				"EI_ReceiveTransmit=RCV",
				"EI_IsActive=Y",
				"EI_ApplicationCode=PLC",
			]),
		new TaskNudgeInformationForTest(
			table: "EDIInterchange",
			queueName: "Polish Customs Message Retriever NCTS",
			predicates:
			[
				"EI_Status=QUE",
				"EI_ReceiveTransmit=RCV",
				"EI_IsActive=Y",
				"EI_ApplicationCode=PLN",
			]),
		new TaskNudgeInformationForTest(
			table: "EDIInterchange",
			queueName: "Polish Customs Message Retriever Exit Control",
			predicates:
			[
				"EI_Status=QUE",
				"EI_ReceiveTransmit=RCV",
				"EI_IsActive=Y",
				"EI_ApplicationCode=PLX",
			]),
	};

	sealed class TestEDIMessage(BusinessObjectFactory factory, DataRow row) : BaseEDIMessage(factory, row);

	class TestCusInBondHeaderInheritor(BusinessObjectFactory factory, DataRow row) : CusInBondHeader(factory, row)
	{
		protected override Type BillTypeCore => throw new NotImplementedException();

		protected override Type MovementHeaderTypeCore => throw new NotImplementedException();

		protected override CusInBondMoveHeaderCollection GetMovementHeaders()
		{
			throw new NotImplementedException();
		}

		protected override ICusInBondBillCollection GetNewBillsCollection()
		{
			throw new NotImplementedException();
		}

		public override GlbBranch Branch => MasterFiles.Business.GlbBranch.CurrentBranch;
	}
}
