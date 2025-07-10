using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.Business;
using Moq;
using NUnit.Framework;
using PLEntryStatus = Enterprise.Customs.PL.Business.Declaration.PLEntryStatusList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC582CMessageProcessor))]
sealed class CC582CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC582CMessageProcessor, ICC582C>
{
	public void TestProcessMessage()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessage.Status.PreProcessedOK;
		var entryHeader = Factory.New<CusEntryHeader>();
		message.EM_LinkedObject = entryHeader;

		MessageProcessor.ProcessMessage(message);

		CombineAssertions(() =>
		{
			AssertEquals("CH_EntryStatus should be updated", PLEntryStatus.REQ, entryHeader.CH_EntryStatus);
			AssertEquals("CH_Status should be updated to RCV", EDIMessage.Status.Received, entryHeader.CH_Status);
			AssertEquals("EM_Status for empty ExitStoppedDate", EDIMessage.Status.ProcessedOK, message.EM_Status);
		});
	}

	public void TestEmail()
	{
		const string correlationIdentifier = "cor123";
		const string mrn = "TEST_MRN";
		const string expectedSubject = "CC582C - Query on not exited goods (not finished export operations) - " + mrn + " Response";

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		ProcessMessageAndTestEmail(expectedSubject, correlationIdentifier, mrn);
	}

	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC582;

	protected override Type ExpectedMessageInterpreterType => typeof(CC582CMessageInterpreter);

	protected override bool ExpectedIsFailureNotification => false;

	protected override void SetUp()
	{
		base.SetUp();
		exportOperationMock = new Mock<ICC582CExportOperation>();

		dataProviderMock.Setup(x => x.ExportOperation).Returns(exportOperationMock.Object);
	}

	Mock<ICC582CExportOperation> exportOperationMock;
}
