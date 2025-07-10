using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC599CMessageProcessor))]
sealed class CC599CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC599CMessageProcessor, ICC599C>
{
	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC599;

	protected override bool ExpectedIsFailureNotification => false;

	protected override Type ExpectedMessageInterpreterType => typeof(CC599CMessageInterpreter);

	public void TestProcessMessage_Exported()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessage.Status.PreProcessedOK;
		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
		message.EM_LinkedObject = entryHeader;
		var interchange = Factory.New<EDIInterchange>();
		message.EM_EI = interchange.PK;

		MessageProcessor.ProcessMessage(message);

		var cancelledNoteDescription = $"The message with interchange {interchange.EI_InterchangeNum} is discarded, because the present status is Canceled.";
		CombineAssertions(() =>
		{
			AssertEquals("CH_EntryStatus for empty ExitStoppedDate", AESEntryStatusList.Codes.Exported, entryHeader.CH_EntryStatus);
			AssertEquals("EM_Status for empty ExitStoppedDate", EDIMessage.Status.ProcessedOK, message.EM_Status);
			message.AssertHasNoLogMessagePart("Message has discarded message log", Events.ErrorReport, cancelledNoteDescription);
		});
	}

	public void TestProcessMessage_ExitReleaseRejected()
	{
		exitControlResult.Setup(x => x.ExitStoppedDate).Returns(ZDateTime.UtcNow.ToDateTime());
		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessage.Status.PreProcessedOK;
		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.MrnAllocated;
		message.EM_LinkedObject = entryHeader;
		var interchange = Factory.New<EDIInterchange>();
		message.EM_EI = interchange.PK;

		MessageProcessor.ProcessMessage(message);

		var cancelledNoteDescription = $"The message with interchange {interchange.EI_InterchangeNum} is discarded, because the present status is Canceled.";
		CombineAssertions(() =>
		{
			AssertEquals("CH_EntryStatus for valid ExitStoppedDate", AESEntryStatusList.Codes.ExitReleaseRejected, entryHeader.CH_EntryStatus);
			AssertEquals("EM_Status for valid ExitStoppedDate", EDIMessage.Status.ProcessedOK, message.EM_Status);
			message.AssertHasNoLogMessagePart("Message has discarded message log", Events.ErrorReport, cancelledNoteDescription);
		});
	}

	public void TestProcessMessage_Discarded()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessage.Status.PreProcessedOK;
		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.CH_EntryStatus = AESEntryStatusList.Codes.Cancelled;
		message.EM_LinkedObject = entryHeader;
		var interchange = Factory.New<EDIInterchange>();
		message.EM_EI = interchange.PK;

		MessageProcessor.ProcessMessage(message);

		var cancelledNoteDescription = $"The message with interchange {interchange.EI_InterchangeNum} is discarded, because the present status is Canceled.";
		CombineAssertions(() =>
		{
			AssertEquals("CH_EntryStatus should not be changed", AESEntryStatusList.Codes.Cancelled, entryHeader.CH_EntryStatus);
			AssertEquals("If entry header is already released cancelled", EDIMessage.Status.Discarded, message.EM_Status);
			message.AssertHasExactLogMessage("Message has discarded message log", Events.ErrorReport, cancelledNoteDescription);
			serviceLogger.AssertHasExactLogMessage("Service has discarded message log", LogType.Error, cancelledNoteDescription);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		exitControlResult = new Mock<IExitControlResult>();
		dataProviderMock.Setup(x => x.ExitControlResult).Returns(exitControlResult.Object);
	}

	Mock<IExitControlResult> exitControlResult;
}
