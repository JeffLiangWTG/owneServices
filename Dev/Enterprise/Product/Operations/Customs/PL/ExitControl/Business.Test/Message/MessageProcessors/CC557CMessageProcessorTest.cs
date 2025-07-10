using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(CC557CMessageProcessor))]
sealed class CC557CMessageProcessorTest : ExitControlMessageProcessorBaseTestCase<CC557CMessageProcessor, ICC557C>
{
	protected override string ExpectedMessageFriendlyName => ExitControlMessageCodes.Descriptions.CC557;

	protected override bool ExpectedIsFailureNotification => true;

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendExportMessageErrors;

	protected override Type ExpectedMessageInterpreterType => typeof(CC557CMessageInterpreter);

	public void TestProcessMessage() => CombineAssertions(() =>
	{
		var messageNumber = 1;
		foreach (var (transmitMessageCode, expectedExitReportStatus, expectedEntryHeaderExitedStatus) in ((string, string, string)[])[
				(ExitControlMessageCodes.Descriptions.CC507, AESEntryStatusList.Codes.Rejected, ExportExitStatus.Codes.Rejected),
				(ExitControlMessageCodes.Descriptions.CC507B, AESEntryStatusList.Codes.Rejected, ExportExitStatus.Codes.Rejected),
				(ExitControlMessageCodes.Descriptions.CC570, AESEntryStatusList.Codes.Rejected, ExportExitStatus.Codes.Rejected),
				(ExitControlMessageCodes.Descriptions.CC573, AESEntryStatusList.Codes.UnknownOrNotReported, ExportExitStatus.Codes.UnknownOrNotReported),
				(ExitControlMessageCodes.Descriptions.CC590, AESEntryStatusList.Codes.UnknownOrNotReported, ExportExitStatus.Codes.UnknownOrNotReported),
				(ExitControlMessageCodes.Descriptions.CC614, AESEntryStatusList.Codes.UnknownOrNotReported, ExportExitStatus.Codes.UnknownOrNotReported),
				(ExitControlMessageCodes.Descriptions.CC615, AESEntryStatusList.Codes.Rejected, ExportExitStatus.Codes.Rejected),
				(ExitControlMessageCodes.Descriptions.CC613, AESEntryStatusList.Codes.UnknownOrNotReported, ExportExitStatus.Codes.UnknownOrNotReported),
			])
		{
			var transmitMessageNumber = $"25{messageNumber++:D7}";
			var (exitReport, transmitMessage, inboundMessage) = PrepareDataForInboundMessageTest(transmitMessageNumber, "MRN123", shouldAttachDeclaration: true);
			transmitMessage.EM_MessageSubType = transmitMessageCode;

			inboundMessage.EM_MessageSubType = ExitControlMessageCodes.Descriptions.CC557;
			inboundMessage.EM_Status = EDIMessage.Status.PreProcessedOK;

			dataProviderMock.As<ICorrelationProvider>().Setup(x => x.CorrelationIdentifier).Returns(transmitMessageNumber);

			MessageProcessor.ProcessMessage(inboundMessage);

			AssertEquals($"{transmitMessageCode}: Inbound Message Status", EDIMessageStatusList.Codes.ProcessedOK, inboundMessage.EM_Status);
			AssertEquals($"{transmitMessageCode}: Transmitted Message Status", EDIMessageStatusList.Codes.Rejected, transmitMessage.EM_Status);
			AssertEquals($"{transmitMessageCode}: Exit Report Status", expected: expectedExitReportStatus, exitReport.CER_Status);
			AssertEquals($"{transmitMessageCode}: Entry Header Exited Status", expected: expectedEntryHeaderExitedStatus, exitReport.Header.Declaration.CustomsEntryHeaders[0].CH_ExitedStatus);
		}
	});

	public void TestEmail()
	{
		const string mrn = "MRN123";
		const string correlationIdentifier = "250000001";
		const string expectedSubject = "CC557 - Rejection from the customs office of exit - MRN123 Response (Failure)";

		dataProviderMock.Setup(x => x.MRN).Returns(mrn);

		ProcessMessageAndTestEmail(expectedSubject, correlationIdentifier, mrn: mrn);
	}
}
