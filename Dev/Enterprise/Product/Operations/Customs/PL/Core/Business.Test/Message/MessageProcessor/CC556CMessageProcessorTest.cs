using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using EDIMessageStatus = Enterprise.Messaging.Business.EDIMessage.Status;
using ProcessingStatus = Enterprise.Messaging.Business.EDIMessage.Status;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CC556CMessageProcessor))]
sealed class CC556CMessageProcessorTest : ImpExpMessageProcessorBaseTest<CC556CMessageProcessor, ICC556C>
{
	public void TestProcessMessageCore_Failed()
	{
		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessageStatus.PreProcessedOK;
		var testEntryHeader = Factory.New<CusEntryHeader>();
		message.EM_LinkedObject = testEntryHeader;
		var interchange = Factory.New<EDIInterchange>();
		message.EM_EI = interchange.PK;

		MessageProcessor.ProcessMessage(message);

		AssertEquals("Correlated message does not exist", ProcessingStatus.Failed, message.EM_Status);
	}

	public void TestProcessMessageCore_NotCC515() => TestProcessMessageCore(AESMessageCodes.Descriptions.CC511);

	public void TestProcessMessageCore_CC515() => TestProcessMessageCore(AESMessageCodes.Descriptions.CC515);

	public void TestProcessMessageCore(ZString messageSubType)
	{
		var mrnForEntryNumberWithEntryHeader = "MRN1";
		var entryHeader = Factory.CreateEntryHeaderWithEntryNumber(mrnForEntryNumberWithEntryHeader);
		Factory.Save();
		var testEdiMessage = Factory.CreateCoreMessage(messageText: string.Empty, linkedObject: entryHeader);
		testEdiMessage.EM_MessageNum = CorrelationIdentifier;
		testEdiMessage.EM_Status = EDIMessage.Status.Sent;
		testEdiMessage.EM_MessageSubType = messageSubType;

		var message = Factory.New<EDIMessage>();
		message.EM_Status = EDIMessageStatus.PreProcessedOK;
		var interchange = Factory.New<EDIInterchange>();
		message.EM_EI = interchange.PK;
		message.EM_LinkedObject = entryHeader;

		MessageProcessor.ProcessMessage(message);
		CombineAssertions(() =>
		{
			AssertEquals("Correlated message exists", ProcessingStatus.ProcessedOK, message.EM_Status);
			AssertEquals("Correlated message status is changed to REJ", EDIMessageStatusList.Codes.Rejected, testEdiMessage.EM_Status);
			if (messageSubType == AESMessageCodes.Descriptions.CC511)
			{
				AssertNotEquals("Entry Header status is not changed to REJ correlated message is not CC515", AESEntryStatusList.Codes.Rejected, entryHeader.CH_EntryStatus);
			}
			else
			{
				AssertEquals("Entry Header status is changed to REJ for outgoing message CC515", AESEntryStatusList.Codes.Rejected, entryHeader.CH_EntryStatus);
			}
		});
	}

	public void TestEmail_MRN()
	{
		const string correlationIdentifier = "cor123";
		const string mrn = "TEST_MRN";
		const string expectedSubject = "CC556 - Rejection of the customs declaration or its amendment" + " - " + mrn + " Response (Failure)";

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		ProcessMessageAndTestEmail(expectedSubject, correlationIdentifier, entryNumMrn: mrn);
	}

	public void TestEmail_NoMRN()
	{
		const string correlationIdentifier = "cor123";
		const string expectedSubject = "CC556 - Rejection of the customs declaration or its amendment" + " - " + correlationIdentifier + " Response (Failure)";

		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);

		ProcessMessageAndTestEmail(expectedSubject, correlationIdentifier, entryNumMrn: null);
	}

	protected override string ExpectedMessageFriendlyName => AESMessageCodes.Descriptions.CC556;

	protected override bool ExpectedIsFailureNotification => true;

	protected override Type ExpectedMessageInterpreterType => typeof(CC556CMessageInterpreter);

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendExportMessageErrors;

	protected override void SetUp()
	{
		base.SetUp();
		dataProviderMock.Setup(x => x.CorrelationIdentifier).Returns(CorrelationIdentifier);
	}
	const string CorrelationIdentifier = "cor123";
}
