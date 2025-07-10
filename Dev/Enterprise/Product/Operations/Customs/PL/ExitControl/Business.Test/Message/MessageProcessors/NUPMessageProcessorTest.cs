using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(NUPMessageProcessor))]
sealed class NUPMessageProcessorTest : ExitControlMessageProcessorBaseTestCase<NUPMessageProcessor, IUpo>
{
	protected override string ExpectedMessageFriendlyName => $"{Messaging.Business.EDIMessage.ApplicationCodes.PLCustomsExitControl}/{PUESC.SystemMessages.NUP}";

	protected override bool ExpectedIsFailureNotification => true;

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendExportMessageErrors;

	protected override Type ExpectedMessageInterpreterType => typeof(NUPMessageInterpreter<CusExitReport>);

	public void TestProcessMessage() => CombineAssertions(() =>
	{
		var messageNumber = 1;
		foreach (var (transmitMessageCode, expectedExitReportStatus, expectedEntryHeaderExitedStatus) in ((string, string, string)[])[
				(ExitControlMessageCodes.Descriptions.CC507, AESEntryStatusList.Codes.ExitOfGoodsIsUnsatisfactory, ExportExitStatus.Codes.ExitOfGoodsIsUnsatisfactory),
				(ExitControlMessageCodes.Descriptions.CC507B, AESEntryStatusList.Codes.ExitOfGoodsIsUnsatisfactory, ExportExitStatus.Codes.ExitOfGoodsIsUnsatisfactory),
				(ExitControlMessageCodes.Descriptions.CC570, AESEntryStatusList.Codes.ExitOfGoodsIsUnsatisfactory, ExportExitStatus.Codes.ExitOfGoodsIsUnsatisfactory),
				(ExitControlMessageCodes.Descriptions.CC573, AESEntryStatusList.Codes.UnknownOrNotReported, ExportExitStatus.Codes.UnknownOrNotReported),
				(ExitControlMessageCodes.Descriptions.CC590, AESEntryStatusList.Codes.UnknownOrNotReported, ExportExitStatus.Codes.UnknownOrNotReported),
				(ExitControlMessageCodes.Descriptions.CC614, AESEntryStatusList.Codes.UnknownOrNotReported, ExportExitStatus.Codes.UnknownOrNotReported),
				(ExitControlMessageCodes.Descriptions.CC615, AESEntryStatusList.Codes.ExitOfGoodsIsUnsatisfactory, ExportExitStatus.Codes.ExitOfGoodsIsUnsatisfactory),
				(ExitControlMessageCodes.Descriptions.CC613, AESEntryStatusList.Codes.UnknownOrNotReported, ExportExitStatus.Codes.UnknownOrNotReported),
			])
		{
			var transmitMessageNumber = $"25{messageNumber++:D7}";
			var (exitReport, transmitMessage, inboundMessage) = PrepareDataForInboundMessageTest(transmitMessageNumber, "MRN123", shouldAttachDeclaration: true);
			transmitMessage.EM_MessageSubType = transmitMessageCode;

			inboundMessage.EM_MessageSubType = ExitControlMessageCodes.Descriptions.NUP;
			inboundMessage.EM_Status = EDIMessage.Status.PreProcessedOK;

			dataProviderMock.As<ICorrelationProvider>().Setup(x => x.CorrelationIdentifier).Returns(transmitMessageNumber);

			MessageProcessor.ProcessMessage(inboundMessage);

			AssertEquals($"{transmitMessageCode}: Inbound Message Status", EDIMessageStatusList.Codes.ProcessedOK, inboundMessage.EM_Status);
			AssertEquals($"{transmitMessageCode}: Transmitted Message Status", EDIMessageStatusList.Codes.Error, transmitMessage.EM_Status);
			AssertEquals($"{transmitMessageCode}: Exit Report Status", expected: expectedExitReportStatus, exitReport.CER_Status);
			AssertEquals($"{transmitMessageCode}: Entry Header Exited Status", expected: expectedExitReportStatus, exitReport.Header.Declaration.CustomsEntryHeaders[0].CH_ExitedStatus);
		}
	});

	public void TestEmail()
	{
		const string correlationIdentifier = "cor123";
		const string transmitDocumentType = "DocType";
		const string expectedSubject = "NUP - Rejection Of Communication (DocType) cor123. Response (Failure)";

		dataProviderMock.As<ICorrelationProvider>().Setup(x => x.CorrelationIdentifier).Returns(correlationIdentifier);
		dataProviderMock.Setup(x => x.TransmitDocumentType).Returns(transmitDocumentType);

		ProcessMessageAndTestEmail(expectedSubject, correlationIdentifier, mrn: "MRN123");
	}
}
