using System;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using AESEntryStatus = Enterprise.Customs.Common.EU.AESEntryStatusList.Codes;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;
using EDIMessageStatus = Enterprise.Messaging.Business.EDIMessage.Status;
using EntryHeaderStatus = Enterprise.Customs.Common.Shared.MessageStatusList.Codes;
using PLEntryStatus = Enterprise.Customs.PL.Business.Declaration.PLEntryStatusList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC529CMessageProcessor))]
sealed class CC529CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC529CMessageProcessor, ICC529C>
{
	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC529;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC529CMessageInterpreter);

	public void TestProcessMessage_MrnAllocated()
	{
		const string testMRN = "MRN01";
		var testDateTime = new DateTime(2000, 10, 30);

		var exportOperationMock = new Mock<ICC529CExportOperation>();
		exportOperationMock.Setup(x => x.DeclarationAcceptanceDate).Returns(testDateTime);
		exportOperationMock.Setup(x => x.ReleaseDate).Returns(testDateTime);
		dataProviderMock.Setup(x => x.ExportOperation).Returns(exportOperationMock.Object);
		dataProviderMock.Setup(x => x.MRN).Returns(testMRN);

		var testEntryHeader = Factory.New<CusEntryHeader>();
		testEntryHeader.CH_EntryStatus = PLEntryStatus.MRN;
		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessageStatus.PreProcessedOK;
		message.EM_LinkedObject = testEntryHeader;

		MessageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals("CH_EntryStatus", AESEntryStatus.ReleasedForExport, testEntryHeader.CH_EntryStatus);
			AssertEquals("CH_Status", EntryHeaderStatus.AcknowledgedChange, testEntryHeader.CH_Status);
			AssertEquals("CH_EntryReleaseDate", testDateTime, testEntryHeader.CH_EntryReleaseDate.ToDateTime());
			AssertEquals("EM_Status", EDIMessageStatus.ProcessedOK, message.EM_Status);
		});
	}

	public void TestProcessMessage_MrnNotAllocated_NumberMustBeCreated()
	{
		const string testMRN = "MRN01";
		var testDateTime = new DateTime(2000, 10, 30);

		var exportOperationMock = new Mock<ICC529CExportOperation>();
		exportOperationMock.Setup(x => x.DeclarationAcceptanceDate).Returns(testDateTime);
		exportOperationMock.Setup(x => x.ReleaseDate).Returns(testDateTime);
		dataProviderMock.Setup(x => x.ExportOperation).Returns(exportOperationMock.Object);
		dataProviderMock.Setup(x => x.MRN).Returns(testMRN);

		var testEntryHeader = Factory.New<CusEntryHeader>();
		testEntryHeader.CH_EntryStatus = PLEntryStatus.ICO;
		testEntryHeader.CH_Status = EntryHeaderStatus.AwaitingChange;
		testEntryHeader.CH_EntryReleaseDate = ZDateTime.BrettsBirthday;
		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessageStatus.PreProcessedOK;
		message.EM_LinkedObject = testEntryHeader;

		var getNumberQuery = new ZQuery()
			.AddToFilter(CusEntryNumSchema.CE_ParentID, testEntryHeader.PK)
			.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber)
			.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Poland);
		AssertEquals("Pre-requirement: No number exists", false, Factory.Exists(typeof(CusEntryNumber), getNumberQuery));

		MessageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals("CH_EntryStatus", PLEntryStatus.ICO, testEntryHeader.CH_EntryStatus);
			AssertEquals("CH_Status", EntryHeaderStatus.AwaitingChange, testEntryHeader.CH_Status);
			AssertEquals("CH_EntryReleaseDate", ZDateTime.BrettsBirthday, testEntryHeader.CH_EntryReleaseDate);
			AssertEquals("EM_Status", EDIMessageStatus.ProcessedOK, message.EM_Status);

			var createdMRN = Factory.Load<CusEntryNumber>(getNumberQuery).Single();
			AssertEquals("Created MRN number", testMRN, createdMRN.CE_EntryNum);
			AssertEquals("number issue date", testDateTime, createdMRN.CE_IssueDate);
		});
	}

	public void TestProcessMessage_MrnNotAllocated_NumberDontNeedToBeCreated()
	{
		const string testMRN = "MRN01";
		var testDateTime = new DateTime(2000, 10, 30);

		var exportOperationMock = new Mock<ICC529CExportOperation>();
		exportOperationMock.Setup(x => x.DeclarationAcceptanceDate).Returns(testDateTime);
		exportOperationMock.Setup(x => x.ReleaseDate).Returns(testDateTime);
		dataProviderMock.Setup(x => x.ExportOperation).Returns(exportOperationMock.Object);
		dataProviderMock.Setup(x => x.MRN).Returns(testMRN);

		var testEntryHeader = Factory.New<CusEntryHeader>();
		testEntryHeader.CH_EntryStatus = PLEntryStatus.ICO;
		testEntryHeader.CH_Status = EntryHeaderStatus.AwaitingChange;
		testEntryHeader.CH_EntryReleaseDate = ZDateTime.BrettsBirthday;
		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessageStatus.PreProcessedOK;
		message.EM_LinkedObject = testEntryHeader;

		var entryNumber = testEntryHeader.Factory.New<CusEntryNumber>();
		entryNumber.CE_EntryNum = testMRN;
		entryNumber.CE_IssueDate = testDateTime;
		entryNumber.Parent = testEntryHeader;
		entryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
		entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Poland;

		MessageProcessor.ProcessMessage(message);

		var getNumberQuery = new ZQuery()
			.AddToFilter(CusEntryNumSchema.CE_ParentID, testEntryHeader.PK)
			.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber)
			.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Poland);

		CombineAssertions(() =>
		{
			AssertEquals("CH_EntryStatus", PLEntryStatus.ICO, testEntryHeader.CH_EntryStatus);
			AssertEquals("CH_Status", EntryHeaderStatus.AwaitingChange, testEntryHeader.CH_Status);
			AssertEquals("CH_EntryReleaseDate", ZDateTime.BrettsBirthday, testEntryHeader.CH_EntryReleaseDate);
			AssertEquals("EM_Status", EDIMessageStatus.ProcessedOK, message.EM_Status);

			AssertEquals("Still 1 number", 1, Factory.Load<CusEntryNumber>(getNumberQuery).Length);
		});
	}

	public void TestEmail_MRN()
	{
		const string correlationIdentifier = "cor123";
		const string mrn = "TEST_MRN";
		const string expectedSubject = "CC529C - Release for export" + " - " + mrn + " Response";
		var declarationAcceptanceDate = new DateTime(2000, 10, 30);

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		dataProviderMock.Setup(x => x.ExportOperation)
			.Returns(Mock.Of<ICC529CExportOperation>(x => x.DeclarationAcceptanceDate == declarationAcceptanceDate));

		ProcessMessageAndTestEmail(expectedSubject, correlationIdentifier, entryNumMrn: mrn);
	}

	public void TestEmail_LRN()
	{
		const string correlationIdentifier = "cor123";
		const string lrn = "TEST_LRN";
		const string expectedSubject = "CC529C - Release for export" + " - " + lrn + " Response";
		var declarationAcceptanceDate = new DateTime(2000, 10, 30);

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.LRN).Returns(lrn);
		dataProviderMock.Setup(x => x.ExportOperation)
			.Returns(Mock.Of<ICC529CExportOperation>(x => x.DeclarationAcceptanceDate == declarationAcceptanceDate));

		ProcessMessageAndTestEmail(expectedSubject, correlationIdentifier, entryNumMrn: null);
	}
}
