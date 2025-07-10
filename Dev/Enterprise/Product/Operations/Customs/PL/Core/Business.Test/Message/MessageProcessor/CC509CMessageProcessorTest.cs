using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;
using EDIMessageStatus = Enterprise.Messaging.Business.EDIMessage.Status;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC509CMessageProcessor))]
sealed class CC509CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC509CMessageProcessor, ICC509C>
{
	public void TestEmail_MRN()
	{
		const string mrn = "TEST_MRN";
		const string expectedSubject = "CC509C - Export cancellation decision" + " - " + mrn + " Response";
		const string correlationIdentifier = "cor123";

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		ProcessMessageAndTestEmail(expectedSubject, correlationIdentifier, entryNumMrn: mrn);
	}

	public void TestEmail_LRN()
	{
		const string lrn = "TEST_LRN";
		const string expectedSubject = "CC509C - Export cancellation decision" + " - " + lrn + " Response";
		const string correlationIdentifier = "cor123";

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.LRN).Returns(lrn);

		ProcessMessageAndTestEmail(expectedSubject, correlationIdentifier, entryNumMrn: null);
	}

	public void TestProcessMessage()
	{
		var message = Factory.New<EDIMessage>();
		var testEntryHeader = Factory.New<CusEntryHeader>();
		message.EM_LinkedObject = testEntryHeader;

		var entryStatuses = Factory.GetCachedValue<AESEntryStatusList>().GetAllCodes();
		CombineAssertions(() =>
		{
			foreach (var status in entryStatuses)
			{
				testEntryHeader.CH_EntryStatus = status;
				message.EM_Status = EDIMessageStatus.PreProcessedOK;
				MessageProcessor.ProcessMessage(message);
				AssertEquals($"{status} - No matter of the previous status new status must be always Cancelled", AESEntryStatusList.Codes.Cancelled, testEntryHeader.CH_EntryStatus);
				AssertEquals($"{status} - message status is always PRS", EDIMessageStatus.ProcessedOK, message.EM_Status);
			}
		});
	}

	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC509;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC509CMessageInterpreter);
}

