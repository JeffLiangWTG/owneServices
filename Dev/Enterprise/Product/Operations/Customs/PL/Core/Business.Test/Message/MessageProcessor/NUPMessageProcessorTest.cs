using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Integration;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(NUPMessageProcessor))]
sealed class NUPMessageProcessorTest : ImpExpMessageProcessorBaseTest<NUPMessageProcessor, IUpo>
{
	protected override string ExpectedMessageFriendlyName => $"{ApplicationCodes.PLCustoms}/{PUESC.SystemMessages.NUP}";

	protected override bool ExpectedIsFailureNotification => true;

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendExportMessageErrors;

	protected override Type ExpectedMessageInterpreterType => typeof(NUPMessageInterpreter<CusEntryHeader>);

	public void TestProcessMessage() => CombineAssertions(() =>
	{
		foreach (var (transmitMessageCode, expectedEntryHeaderStatus) in ((string, string)[])[
				(AESMessageCodes.Descriptions.CC504, MessageStatusList.Codes.AwaitingResponse),
				(AESMessageCodes.Descriptions.CC515, MessageStatusList.Codes.SentAndRejected),
				(AESMessageCodes.Descriptions.CC570, MessageStatusList.Codes.SentAndRejected),
				(AESMessageCodes.Descriptions.PW515, MessageStatusList.Codes.SentAndRejected),
			])
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			cusEntryHeader.CH_EntryStatus = MessageStatusList.Codes.AwaitingResponse;

			var testData = Factory.CreateTransmittedIncomingMessages<EDIMessage>(
				ApplicationCodes.PLCustoms,
				linkedObject: cusEntryHeader,
				messageType: EUJobMessageTypeList.Codes.Export,
				transmittedMessageSubType: transmitMessageCode,
				incomingMessageSubType: AESMessageCodes.Descriptions.NUP);

			testData.IncomingMessage.EM_Status = EDIMessage.Status.PreProcessedOK;

			MessageProcessor.ProcessMessage(testData.IncomingMessage);
			AssertEquals($"{transmitMessageCode}: Incoming message EM_Status", "PRS", testData.IncomingMessage.EM_Status);
			AssertEquals($"{transmitMessageCode}: Transmitted message EM_Status", "ERR", testData.TransmittedMessage.EM_Status);
			AssertEquals($"{transmitMessageCode}: Entry Header EM_Status", expected: expectedEntryHeaderStatus, cusEntryHeader.CH_EntryStatus);
		}
	});

	public void TestEmail()
	{
		const string correlationIdentifier = "cor123";
		const string transmitDocumentType = "DocType";
		const string expectedSubject = "NUP - Rejection Of Communication (DocType) cor123. Response (Failure)";

		dataProviderMock.As<ICorrelationProvider>().Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.TransmitDocumentType).Returns(transmitDocumentType);

		ProcessMessageAndTestEmail(expectedSubject, correlationIdentifier, entryNumMrn: null);
	}
}
