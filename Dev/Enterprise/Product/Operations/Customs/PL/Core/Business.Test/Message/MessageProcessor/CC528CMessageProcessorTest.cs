using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using EDIMessageStatus = Enterprise.Messaging.Business.EDIMessage.Status;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC528CMessageProcessor))]
sealed class CC528CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC528CMessageProcessor, ICC528C>
{
	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC528;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC528CMessageInterpreter);

	public void TestProcessMessage_Fail_MrnAlreadyAssigned()
	{
		const string mrn = "MRN1";
		const string expectedMRN = "SomeMRN";
		var jobDeclaration = Factory.New<JobDeclaration>();
		var testEntryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
		testEntryHeader.MovementReferenceNumberSetter(expectedMRN);
		Factory.Save();
		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessageStatus.PreProcessedOK;
		message.EM_LinkedObject = testEntryHeader;
		var interchange = Factory.New<EDIInterchange>();
		message.EM_EI = interchange.PK;

		dataProviderMock.Setup(x => x.MRN).Returns(mrn);
		MessageProcessor.ProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("Message should get status fail", EDIMessageStatus.Failed, message.EM_Status);
			AssertNotEquals("Mrn should not be updated", mrn, testEntryHeader.MovementReferenceNumber);
			AssertEquals("Mrn should stay the same", expectedMRN, testEntryHeader.MovementReferenceNumber);
			AssertEquals("CH_EntryStatus should not be changed", string.Empty, testEntryHeader.CH_EntryStatus);
		});
	}

	public void TestProcessMessage_Fail_EmptyMrnNode()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessageStatus.PreProcessedOK;
		var testEntryHeader = Factory.New<CusEntryHeader>();
		message.EM_LinkedObject = testEntryHeader;
		var interchange = Factory.New<EDIInterchange>();
		message.EM_EI = interchange.PK;

		MessageProcessor.ProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("Message should get status fail", EDIMessageStatus.Failed, message.EM_Status);
			AssertEquals("CH_EntryStatus should not be changed", string.Empty, testEntryHeader.CH_EntryStatus);
		});
	}

	public void TestProcessMessage()
	{
		const string expectedMRN = "MRN1";
		var expectedDate = new DateTime(2024, 01, 02, 03, 04, 05);
		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessageStatus.PreProcessedOK;
		var testEntryHeader = Factory.New<CusEntryHeader>();
		message.EM_LinkedObject = testEntryHeader;
		var interchange = Factory.New<EDIInterchange>();
		message.EM_EI = interchange.PK;

		dataProviderMock.Setup(x => x.MRN).Returns(expectedMRN);
		dataProviderMock.Setup(x => x.DeclarationAcceptanceDate).Returns(expectedDate);
		MessageProcessor.ProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("Message should get status Processed", EDIMessageStatus.ProcessedOK, message.EM_Status);
			AssertEquals("CH_EntryStatus should be changed to Mrn Allocated", AESEntryStatusList.Codes.MrnAllocated, testEntryHeader.CH_EntryStatus);
			AssertEquals("MovementReferenceNumberIssueDate", expectedDate, testEntryHeader.MovementReferenceNumberIssueDate);
			AssertEquals("MovementReferenceNumber", expectedMRN, testEntryHeader.MovementReferenceNumber);
		});
	}

	public void TestEmail()
	{
		const string mrn = "TEST_MRN";
		const string expectedSubject = "CC528C - Export MRN Allocation" + " - " + mrn + " Response";
		const string correlationIdentifier = "cor123";

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		ProcessMessageAndTestEmail(expectedSubject, correlationIdentifier, entryNumMrn: null);
	}
}
