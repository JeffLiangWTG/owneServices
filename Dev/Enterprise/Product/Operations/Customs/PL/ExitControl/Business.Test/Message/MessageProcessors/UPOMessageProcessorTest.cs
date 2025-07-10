using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.PL.Business.Testing;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Constants;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(UPOMessageProcessor))]
sealed class UPOMessageProcessorTest : ExitControlMessageProcessorBaseTestCase<UPOMessageProcessor, IUpo>
{
	protected override string ExpectedMessageFriendlyName => $"{ApplicationCodes.PLCustomsExitControl}/{PUESC.SystemMessages.UPO}";

	protected override bool ExpectedIsFailureNotification => false;

	public void TestProcessMessage() => CombineAssertions(() =>
	{
		var testData = new ExitControlTestData(Factory);
		var exitReport = testData.ExitReport;

		var transmittedIncomingMessages = Factory.CreateTransmittedIncomingMessages<EDIMessage>(
			ApplicationCodes.PLCustomsExitControl,
			linkedObject: exitReport,
			messageType: EdiMessageMessageType.ExitControl,
			transmittedMessageSubType: "XXX",
			PUESC.SystemMessages.UPO);
		var message = transmittedIncomingMessages.IncomingMessage;

		exitReport.CER_MessageStatus = LogicalStatusList.Codes.Sent;
		messageProcessor.ProcessMessage(message);
		AssertEquals("Status updated to Accepted", LogicalStatusList.Codes.Accepted, exitReport.CER_MessageStatus);

		message.EM_Status = EDIMessage.Status.PreProcessedOK;
		exitReport.CER_MessageStatus = LogicalStatusList.Codes.Error;
		messageProcessor.ProcessMessage(message);
		AssertEquals("Status not updated", LogicalStatusList.Codes.Error, exitReport.CER_MessageStatus);
	});
}
