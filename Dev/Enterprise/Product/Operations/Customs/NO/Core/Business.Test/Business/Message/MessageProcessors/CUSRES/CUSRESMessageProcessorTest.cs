using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Edifact.V902.Elements;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CUSRESMessageProcessor))]
sealed class CUSRESMessageProcessorTest : TestCaseWithFactory
{
	public void TestProcessMessage_WhenEmptyLinkedObjectPassed()
	{
		CombineAssertions(() =>
		{
			const string responseMessage = "UNH+724180+CUSRES:1:902:UN:NEP+9355964402023012600011401'" +
					"BGM+830++137:20230126:102+11'" +
					"DTM+255:20220413:102'" +
					"DTM+58:20230126:102'GIS+5::'" +
					"RFF+ABT:4410912300030065'" +
					"UNT+6+724180'";

			var message = ProcessMessage(responseMessage, null);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
			AssertLoggedInformation(message, $"EDIMessage with PK: [{message.PK}], Linked object is not CusEntryHeader.");
		});
	}

	public void TestProcessMessage_WhenWrongCUSRESMessageStringPassed()
	{
		CombineAssertions(() =>
		{
			const string responseMessageNotCUSRES = "UNH+724180+CUS:1:902:UN:NEP+9355964402023012600011401'" +
					"BGM+830++137:20230126:102+11'" +
					"DTM+255:20220413:102'" +
					"DTM+58:20230126:102'GIS+5::'" +
					"RFF+ABT:4410912300030065'" +
					"UNT+6+724180'";

			var message = ProcessMessage(responseMessageNotCUSRES, entryHeader);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
			AssertLoggedInformation(message, $"EDIMessage with PK: [{message.PK}], Not a CUSRES message.");
		});
	}

	public void TestProcessMessage_WhenWrongMessageTypePassedInBGMSegment()
	{
		CombineAssertions(() =>
		{
			const string responseMessageWrongMsgTypeInBGM = "UNH+724180+CUSRES:1:902:UN:NEP+9355964402023012600011401'" +
				"BGM+999++137:20230126:102+11'" +
				"DTM+255:20220413:102'" +
				"DTM+58:20230126:102'GIS+5::'" +
				"RFF+ABT:4410912300030065'" +
				"UNT+6+724180'";

			var message = ProcessMessage(responseMessageWrongMsgTypeInBGM, entryHeader);
			var logErrorMessage = $"System was not able to identify the message type: [999] for EDIMessage with PK: [{message.PK}].";
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
			AssertLoggedInformation(message, logErrorMessage);
		});
	}

	public void TestProcessMessage_WhenUnknownMessageTypeAndMessageFunctionPassed()
	{
		CombineAssertions(() =>
		{
			const string responseMessageUnknownMessageTypeAndMessageFunction = "UNH+724180+CUSRES:1:902:UN:NEP+9355964402023012600011401'" +
				"BGM+830++137:20230126:102+11'" +
				"DTM+255:20220413:102'" +
				"DTM+58:20230126:102'" +
				"GIS+0::'" +
				"FTX+AAP+++000:Melding'" +
				"RFF+ABT:4410912300030065'" +
				"UNT+8+724180'";

			var message = ProcessMessage(responseMessageUnknownMessageTypeAndMessageFunction, entryHeader);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
			AssertLoggedInformation(message, $"EDIMessage with PK: [{message.PK}], Unknown MessageText Code for MessageType: 830, Message Function: 11.");
		});
	}

	public void TestProcessMessage_ImportExportDeclarationResponse()
	{
		AssertProcessMessage_ImportExportDeclarationResponse(DocumentMessageNameCodedList.GoodsDeclarationForExportation);
		AssertProcessMessage_ImportExportDeclarationResponse(DocumentMessageNameCodedList.GoodsDeclarationForImportation);
	}

	public void TestProcessMessage_ImportExportDeclarationNotAccepted()
	{
		AssertProcessMessage_ImportExportDeclarationNotAccepted(DocumentMessageNameCodedList.GoodsDeclarationForExportation);
		AssertProcessMessage_ImportExportDeclarationNotAccepted(DocumentMessageNameCodedList.GoodsDeclarationForImportation);
	}

	public void TestProcessMessage_ImportExportDeclarationMessageFunctionZZ()
	{
		AssertProcessMessage_ImportExportDeclarationMessageFunctionZZ(DocumentMessageNameCodedList.GoodsDeclarationForExportation);
		AssertProcessMessage_ImportExportDeclarationMessageFunctionZZ(DocumentMessageNameCodedList.GoodsDeclarationForImportation);
	}

	public void TestProcessMessage_CustomsDeliveryNote()
	{
		var expiryDateTime = new ZDateTime(2023, 01, 27);
		var issueDateTime = new ZDateTime(2023, 01, 26);
		AssertProcessMessage_CustomsDeliveryNote(MessageFunctionCodedList.Response, UniversalReferenceConstants.CusEntryStatus.UAR, issueDateTime, expiryDateTime);
		AssertProcessMessage_CustomsDeliveryNote(MessageFunctionCodedList.Approval, UniversalReferenceConstants.CusEntryStatus.TKR, issueDateTime, expiryDateTime);
	}

	public void TestProcessMessage_CustomsDeliveryNoteUnknownFunction()
	{
		CombineAssertions("When unknown message function passed", () =>
		{
			const string responseMessageUnknownMessageFunction = "UNH+724180+CUSRES:1:902:UN:NEP+9355964402023012600024901'" +
				"BGM+932++137:20230126:102+99'" +
				"NAD+EX+911705907::NO1+NOBLE HARVEST AS'" +
				"NAD+DT+935596440::NO1'" +
				"DTM+58:20230126:102'" +
				"DTM+255:20230127:102'" +
				"TAX+4+161:272'" +
				"RFF+ABT:4410912300030065'" +
				"UNT+9+724180'";

			var message = ProcessMessage(responseMessageUnknownMessageFunction, entryHeader);
			AssertEquals("EM_Status", EDIMessageStatusList.Codes.Error, message.EM_Status);
			AssertLoggedInformation(message, $"EDIMessage with PK: [{message.PK}], Unknown MessageFunction: [99] for MessageType: [932]", LogType.Warning);
		});
	}

	void AssertProcessMessage_CustomsDeliveryNote(string messageFunction, string entryStatus, ZDateTime issueDateTime, ZDateTime expiryDateTime)
	{
		CombineAssertions($"When MessageFunction: {messageFunction}", () =>
		{
			var responseMessage = "UNH+724180+CUSRES:1:902:UN:NEP+9355964402023012600024901'" +
			$"BGM+932++137:20230126:102+{messageFunction}'" +
			"NAD+EX+911705907::NO1+NOBLE HARVEST AS'" +
			"NAD+DT+935596440::NO1'" +
			"DTM+58:20230126:102'" +
			"DTM+255:20230127:102'" +
			"TAX+4+161:272'" +
			"RFF+ABT:4410912300030065'" +
			"UNT+9+724180'";

			AssertProcessMessage(responseMessage, entryStatus, CustomsEntryPhaseStatusList.Codes.Finalized);
			AssertEquals("Entry Number", "4410912300030065", entryHeader.EntryReleaseNumber);
			AssertEquals("Issue Date", issueDateTime, entryHeader.EntryReleaseNumberIssueDate);
			AssertEquals("Expiry Date", expiryDateTime, entryHeader.EntryReleaseNumberExpiryDate);
			AssertEquals("Release Date", issueDateTime, entryHeader.CH_EntryReleaseDate);

			var hasLog = entryHeader.Logs.HasLogWith(log => log.SL_SE_NKEvent == Events.CustomsEntryStatusCode && log.SL_Reference == entryStatus);
			Assert("Event generated", hasLog);
		});
	}

	void AssertProcessMessage_ImportExportDeclarationNotAccepted(string messageType)
	{
		var importOrExportString = (messageType == DocumentMessageNameCodedList.GoodsDeclarationForExportation) ? "NEP" : "NEP-I";
		var responseMessage = $"UNH+724180+CUSRES:1:902:UN:{importOrExportString}+9355964402023012600011401'" +
				$"BGM+{messageType}++137:20230126:102+27'" +
				"DTM+255:20220413:102'" +
				"DTM+58:20230126:102'" +
				"GIS+5::'" +
				"FTX+AAP+++999:Ikke mulig med forhnds deklarering(godsnr 307)'" +
				"FTX+AAP+++950:Melding'" +
				"RFF+ABT:4410912300030065'" +
				"UNT+9+724180'";

		var entryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		var assertMessage = $"When MessageType: {messageType}, EntryStatus is empty, ";
		AssertProcessMessageBasedOnReferenceAndDateForDuty(assertMessage + "BGMReference is equal to the DeclarationId and DateForDuty is smaller than CreationDate",
			ZString.Empty, "9355964402023012600011401", ZString.Empty, new ZDateTime(2023, 1, 25), ZDateTime.Empty);
		AssertProcessMessageBasedOnReferenceAndDateForDuty(assertMessage + "BGMReference is not equal to the DeclarationId and DateForDuty is smaller than CreationDate",
			ZString.Empty, "9355964402023012600011404", "9355964402023012600011404", new ZDateTime(2023, 1, 25), new ZDateTime(2023, 1, 25));
		AssertProcessMessageBasedOnReferenceAndDateForDuty(assertMessage + "BGMReference is equal to the DeclarationId and DateForDuty is Greater than CreationDate",
			ZString.Empty, "9355964402023012600011401", ZString.Empty, new ZDateTime(2023, 1, 27), new ZDateTime(2023, 1, 27));
		AssertProcessMessageBasedOnReferenceAndDateForDuty(assertMessage + "BGMReference is equal to the DeclarationId and DateForDuty is equal to the CreationDate",
			ZString.Empty, "9355964402023012600011401", ZString.Empty, new ZDateTime(2023, 1, 26), ZDateTime.Empty);
		AssertProcessMessageBasedOnReferenceAndDateForDuty(assertMessage + "BGMReference is equal to the DeclarationId and DateForDuty is equal to the CreationDate, but time differ",
			ZString.Empty, "9355964402023012600011401", ZString.Empty, new ZDateTime(2023, 1, 26, 15, 15, 15), ZDateTime.Empty);

		assertMessage = $"When MessageType: {messageType}, EntryStatus is not empty, ";
		AssertProcessMessageBasedOnReferenceAndDateForDuty(assertMessage + "BGMReference is equal to the DeclarationId and DateForDuty is equal to the CreationDate",
			"QUE", "9355964402023012600011401", "9355964402023012600011401", new ZDateTime(2023, 1, 26), new ZDateTime(2023, 1, 26));

		void AssertProcessMessageBasedOnReferenceAndDateForDuty(string messageForAssertion, string entryStatus, string bgmRef, string bgmRefExp, ZDateTime dateForDuty, ZDateTime dateForDutyExp)
		{
			CombineAssertions(messageForAssertion, () =>
			{
				entryInstruction.CEI_DateForDuty = dateForDuty;
				entryHeader.CH_BGMReference = bgmRef;
				entryHeader.CH_EntryStatus = entryStatus;
				var message = ProcessMessage(responseMessage, entryHeader);

				AssertEquals("Message status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				AssertEquals("CH_Status", UniversalReferenceConstants.CusEntryStatus.Rejected, entryHeader.CH_Status);
				AssertEquals("CH_BGMReference", bgmRefExp, entryHeader.CH_BGMReference);
				AssertEquals("CEI_DateForDuty", dateForDutyExp, entryInstruction.CEI_DateForDuty);
			});
		}
	}

	void AssertProcessMessage_ImportExportDeclarationMessageFunctionZZ(string messageType)
	{
		var importOrExportString = (messageType == DocumentMessageNameCodedList.GoodsDeclarationForExportation) ? "NEP" : "NEP-I";
		var responseMessage = $"UNH+724180+CUSRES:1:902:UN:{importOrExportString}+9355964402023012600011401'" +
				$"BGM+{messageType}++137:20230126:102+ZZ'" +
				"DTM+255:20220413:102'" +
				"DTM+58:20230126:102'" +
				"GIS+5::'" +
				"FTX+AAP+++950:Melding'" +
				"RFF+ABT:4410912300030065'" +
				"UNT+8+724180'";

		CombineAssertions($"When MessageType: {messageType}", () =>
		{
			AssertProcessMessage(responseMessage, UniversalReferenceConstants.CusEntryStatus.IUR, CustomsEntryPhaseStatusList.Codes.Finalized);
		});
	}

	void AssertProcessMessage_ImportExportDeclarationResponse(string messageType)
	{
		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			EDIMessageConstants.MessageTextCodes.Code_972,
			ProcessingIndicatorCodedList.GoodsRequiredForExamination,
			UniversalReferenceConstants.CusEntryStatus.MEM,
			entryHeader.CH_PhaseStatus);

		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			EDIMessageConstants.MessageTextCodes.Code_950,
			ProcessingIndicatorCodedList.GoodsRequiredForExamination,
			UniversalReferenceConstants.CusEntryStatus.MEC,
			string.Empty);

		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			EDIMessageConstants.MessageTextCodes.Code_958,
			ProcessingIndicatorCodedList.GoodsRequiredForExamination,
			entryHeader.CH_EntryStatus,
			CustomsEntryPhaseStatusList.Codes.Reminder);

		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			EDIMessageConstants.MessageTextCodes.Code_980,
			ProcessingIndicatorCodedList.GoodsRequiredForExamination,
			entryHeader.CH_EntryStatus,
			CustomsEntryPhaseStatusList.Codes.Reminder);

		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			EDIMessageConstants.MessageTextCodes.Code_981,
			ProcessingIndicatorCodedList.GoodsRequiredForExamination,
			entryHeader.CH_EntryStatus,
			CustomsEntryPhaseStatusList.Codes.Reminder);

		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			EDIMessageConstants.MessageTextCodes.Code_982,
			ProcessingIndicatorCodedList.GoodsRequiredForExamination,
			entryHeader.CH_EntryStatus,
			CustomsEntryPhaseStatusList.Codes.Reminder);

		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			EDIMessageConstants.MessageTextCodes.Code_983,
			ProcessingIndicatorCodedList.GoodsRequiredForExamination,
			entryHeader.CH_EntryStatus,
			CustomsEntryPhaseStatusList.Codes.Reminder);

		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			EDIMessageConstants.MessageTextCodes.Code_279,
			ProcessingIndicatorCodedList.GoodsRequiredForExamination,
			UniversalReferenceConstants.CusEntryStatus.IUR,
			CustomsEntryPhaseStatusList.Codes.Finalized);

		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			EDIMessageConstants.MessageTextCodes.Code_357,
			ProcessingIndicatorCodedList.GoodsRequiredForExamination,
			UniversalReferenceConstants.CusEntryStatus.IUR,
			CustomsEntryPhaseStatusList.Codes.Finalized);

		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			EDIMessageConstants.MessageTextCodes.Code_735,
			ProcessingIndicatorCodedList.GoodsRequiredForExamination,
			UniversalReferenceConstants.CusEntryStatus.IUR,
			CustomsEntryPhaseStatusList.Codes.Finalized);

		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			EDIMessageConstants.MessageTextCodes.Code_736,
			ProcessingIndicatorCodedList.GoodsRequiredForExamination,
			UniversalReferenceConstants.CusEntryStatus.IUR,
			CustomsEntryPhaseStatusList.Codes.Finalized);

		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			EDIMessageConstants.MessageTextCodes.Code_956,
			ProcessingIndicatorCodedList.GoodsRequiredForExamination,
			UniversalReferenceConstants.CusEntryStatus.IUR,
			CustomsEntryPhaseStatusList.Codes.Finalized);

		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			EDIMessageConstants.MessageTextCodes.Code_973,
			ProcessingIndicatorCodedList.GoodsRequiredForExamination,
			UniversalReferenceConstants.CusEntryStatus.IUR,
			CustomsEntryPhaseStatusList.Codes.Finalized);

		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			"UWN",
			ProcessingIndicatorCodedList.GoodsRequiredForExamination,
			UniversalReferenceConstants.CusEntryStatus.MEG,
			string.Empty);

		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			"UWN",
			ProcessingIndicatorCodedList.AllDocumentsOrAsSpecifiedToBeProduced,
			UniversalReferenceConstants.CusEntryStatus.MED,
			string.Empty);

		AssertProcessMessageBasedOnGivenInputs(
			messageType,
			MessageFunctionCodedList.Response,
			"UWN",
			"UWN",
			UniversalReferenceConstants.CusEntryStatus.MEC,
			entryHeader.CH_PhaseStatus);
	}

	void AssertProcessMessageBasedOnGivenInputs(
		string messageType,
		string messageFunction,
		string messageTextCode,
		string requestedControlAction,
		string expEntryStatus,
		string expPhaseStatus)
	{
		var importOrExportString = (messageType == DocumentMessageNameCodedList.GoodsDeclarationForExportation) ? "NEP" : "NEP-I";
		var messageForCombineAssertion = $"When MessageType: {messageType}, MessageFunction: {messageFunction}, MessageTextCode: {messageTextCode}, ReqControlAction: {requestedControlAction}";
		var responseMessage = $"UNH+724180+CUSRES:1:902:UN:{importOrExportString}+9355964402023012600011401'" +
				$"BGM+{messageType}++137:20230126:102+{messageFunction}'" +
				"DTM+255:20220413:102'" +
				"DTM+58:20230126:102'" +
				$"GIS+{requestedControlAction}::'" +
				"FTX+AAP+++999:Ikke mulig med forhnds deklarering(godsnr 307)'" +
				$"FTX+AAP+++{messageTextCode}:Melding'" +
				"RFF+ABT:4410912300030065'" +
				"UNT+9+724180'";

		CombineAssertions(messageForCombineAssertion, () =>
		{
			AssertProcessMessage(responseMessage, expEntryStatus, expPhaseStatus);
		});
	}

	void AssertProcessMessage(string responseMessage, string expEntryStatus, string expPhaseStatus)
	{
		var message = ProcessMessage(responseMessage, entryHeader);

		AssertEquals("Message status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		AssertEquals("EntryStatus", expEntryStatus, entryHeader.CH_EntryStatus);
		AssertEquals("PhaseStatus", expPhaseStatus, entryHeader.CH_PhaseStatus);
	}

	void AssertLoggedInformation(EDIMessage message, string errorMessage, LogType logType = LogType.Error)
	{
		loggerMock
			.Verify(l => l.Log(It.Is<LogType>(t => t == LogType.Information), It.Is<string>(m => m == $"Started processing EDIMessage with PK: [{message.PK}]")),
				Times.Once);

		loggerMock
			.Verify(l => l.Log(It.Is<LogType>(t => t == logType), It.Is<string>(s => s.Contains(errorMessage))),
				Times.Once);

		loggerMock
			.Verify(l => l.Log(It.Is<LogType>(t => t == LogType.Information), It.Is<string>(m => m == $"Finished processing EDIMessage with PK: [{message.PK}]")),
				Times.Once);
	}

	protected override void SetUp()
	{
		base.SetUp();

		entryHeader = Factory.New<CusEntryHeader>();
		processor = new CUSRESMessageProcessor();
		loggerMock = new Mock<LoggingInformation>();
	}

	CusEntryHeader entryHeader;
	IMessageProcessor processor;
	Mock<LoggingInformation> loggerMock;

	EDIMessage ProcessMessage(string cusresMessage, BusinessObject entryHeader)
	{
		var responseMessage = Factory.New<EDIMessage>();

		responseMessage.EM_MessageType = EDIMessageConstants.MessageTypes.CUSRES;
		responseMessage.EM_MessageText = cusresMessage;
		responseMessage.EM_LinkedObject = entryHeader;
		responseMessage.EM_LinkTable = CusEntryHeader.Schema.TableName;

		processor.ProcessMessage(responseMessage, loggerMock.Object);

		return responseMessage;
	}
}
