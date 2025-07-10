using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using EDIMessageStatus = Enterprise.Messaging.Business.EDIMessage.Status;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC504CMessageProcessor))]
sealed class CC504CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC504CMessageProcessor, ICC504C>
{
	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC504;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC504CMessageInterpreter);

	public void TestProcessMessageFailed()
	{
		const string expectedResponseMessageStatus = EDIMessageStatus.Failed;

		var testEntryHeader = Factory.New<CusEntryHeader>();
		testEntryHeader.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;

		var responseMessage = Factory.New<EDIMessage>();
		responseMessage.EM_Status = EDIMessageStatus.PreProcessedOK;
		responseMessage.EM_LinkedObject = testEntryHeader;

		MessageProcessor.ProcessMessage(responseMessage);

		AssertEquals("responseMessage : EM_Status", expectedResponseMessageStatus, responseMessage.EM_Status);
	}

	public void TestEmail()
	{
		const string correlationIdentifier = "cor123";
		const string mrn = "TEST_MRN";
		const string expectedSubject = "CC504C - Export Declaration Amendment Acceptance" + " - " + mrn + " Response";

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		ProcessMessageAndTestEmail(expectedSubject, correlationIdentifier, entryNumMrn: mrn);
	}

	public void TestProcessMessage()
	{
		var testEntryHeader = Factory.New<CusEntryHeader>();
		testEntryHeader.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
		var transmitMessage = Factory.New<EDIMessage>();
		transmitMessage.EM_Status = EDIMessageStatus.Sent;
		transmitMessage.EM_MessageNum = ExpectedCorrelationIdentifier;
		transmitMessage.EM_ApplicationCode = ApplicationCodes.PLCustoms;
		testEntryHeader.Messages.Add(transmitMessage);

		var responseMessage = Factory.New<EDIMessage>();
		responseMessage.EM_Status = EDIMessageStatus.PreProcessedOK;
		responseMessage.EM_LinkedObject = testEntryHeader;

		MessageProcessor.ProcessMessage(responseMessage);

		const string expectedResponseMessageStatus = EDIMessageStatus.ProcessedOK;
		const string expectedTransmitMessageStatus = LogicalStatusList.Codes.Accepted;

		CombineAssertions(() =>
		{
			AssertEquals("responseMessage : EM_Status", expectedResponseMessageStatus, responseMessage.EM_Status);
			AssertEquals("transmitMessage : EM_Status", expectedTransmitMessageStatus, transmitMessage.EM_Status);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(ExpectedCorrelationIdentifier);
	}

	const string ExpectedCorrelationIdentifier = "CorrelationIdentifier";
}
