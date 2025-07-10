using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.Business.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestsSubclassesOf(typeof(ExitControlMessageProcessorBase<>))]
public abstract class ExitControlMessageProcessorBaseTestCase<TMessageProcessor, T> : BaseMessageProcessorTestCase<TMessageProcessor, T>
	where T : class
	where TMessageProcessor : ExitControlMessageProcessorBase<T>
{
	public override void TestApplicationCode() => AssertEquals(ApplicationCodeList.Codes.PLCustomsExitControl, MessageProcessor.ApplicationCode);

	protected override IRegistryItem ExpectedEmailGroupRegistryItem => EUCustomsDataRegistry.Instance.SendExportAcknowledgements;

	protected void ProcessMessageAndTestEmail(string expectedSubject, string correlationIdentifier, string mrn)
	{
		const string emailAddress = "abc@abc.com";
		const string testUserCode = "ABC";

		var user = Factory.New<GlbStaff>();
		user.GS_Code = testUserCode;
		user.GS_EmailAddress = emailAddress;

		var (exitReport, transmitMessage, inboundMessage) = PrepareDataForInboundMessageTest(correlationIdentifier, mrn);
		transmitMessage.EM_SystemCreateUser = testUserCode;

		Env.OutgoingCustomsMailManager.EmailsCreated.Clear();
		MessageProcessor.ProcessMessage(inboundMessage);
		CombineAssertions(() => AssertEmail(emailAddress, expectedSubject, expectedBody: inboundMessage.EM_MessageInterpretation, relatedJob: exitReport.Header));
	}

	protected (CusExitReport CusExitReport, EDIMessage TransmitMessage, EDIMessage InboundMessage) PrepareDataForInboundMessageTest(string transmitMessageNumber, string mrn, bool shouldAttachDeclaration = false)
	{
		var exitHeader = Factory.New<CusExitHeader>();
		var exitConsignment = exitHeader.CusExitConsignments.AddNew();
		exitConsignment.CXC_MovementReference = mrn;
		var exitReport = exitHeader.CusExitReports.AddNew();
		exitReport.CER_CXC_Consignment = exitConsignment.PK;

		var transmitMessage = Factory.New<EDIMessage>();
		transmitMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.PLCustomsExitControl;
		transmitMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
		transmitMessage.EM_Status = Messaging.Business.EDIMessage.Status.Sent;
		transmitMessage.EM_MessageNum = transmitMessageNumber;
		transmitMessage.EM_LinkedObject = exitReport;

		var inboundMessage = Factory.New<EDIMessage>();
		inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.PLCustomsExitControl;
		inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		inboundMessage.EM_Status = EDIMessage.Status.PreProcessedOK;
		inboundMessage.EM_LinkedObject = exitReport;

		if (shouldAttachDeclaration)
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.MovementReferenceNumberSetter(mrn);
			exitHeader.Parent = jobDeclaration;
		}

		return (exitReport, transmitMessage, inboundMessage);
	}
}

sealed class ExitControlMessageProcessorBaseTest : ExitControlMessageProcessorBaseTestCase<ExitControlMessageProcessorForTest, IIncomingMessage>
{
	protected override string ExpectedMessageFriendlyName => "Test Exit Control Message Processor";

	protected override bool ExpectedIsFailureNotification => false;
}

public class ExitControlMessageProcessorForTest(LoggingInformation logger) : ExitControlMessageProcessorBase<IIncomingMessage>(logger)
{
	protected override string MessageFriendlyNameCore => "Test Exit Control Message Processor";

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IIncomingMessage messageDataProvider) =>
		ProcessingResult.Succeed;
}
