using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC551CMessageProcessor))]
sealed class CC551CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC551CMessageProcessor, ICC551C>
{
	public void TestProcessMessage() => CombineAssertions("CH_EntryStatus should be REJ", () =>
	{
		AssertEquals("when CH_EntryStatus is CON.", AESEntryStatusList.Codes.Rejected, ProcessMessageThenGetUpdatedEntryStatus(AESEntryStatusList.Codes.ControlledForExport));
		AssertEquals("when CH_EntryStatus is MRN.", AESEntryStatusList.Codes.Rejected, ProcessMessageThenGetUpdatedEntryStatus(AESEntryStatusList.Codes.MrnAllocated));
		AssertEquals("when CH_EntryStatus is ACK.", AESEntryStatusList.Codes.Rejected, ProcessMessageThenGetUpdatedEntryStatus(LogicalStatusList.Codes.Acknowledged));
		AssertEquals("when CH_EntryStatus is SNT.", AESEntryStatusList.Codes.Rejected, ProcessMessageThenGetUpdatedEntryStatus(LogicalStatusList.Codes.Sent));
	});

	public void TestEmail()
	{
		const string correlationIdentifier = "cor123";
		const string mrn = "TEST_MRN";
		const string expectedSubject = "IE551 – Refusal to release the goods for export procedure - " + mrn + " Response";

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		ProcessMessageAndTestEmail(expectedSubject, correlationIdentifier, entryNumMrn: mrn);
	}

	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC551;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC551CMessageInterpreter);

	string ProcessMessageThenGetUpdatedEntryStatus(string entryStatus)
	{
		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessage.Status.PreProcessedOK;

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_EntryStatus = entryStatus;
		message.EM_LinkedObject = entryHeader;

		var interchange = Factory.New<EDIInterchange>();
		message.EM_EI = interchange.PK;

		MessageProcessor.ProcessMessage(message);

		return entryHeader.CH_EntryStatus;
	}
}
