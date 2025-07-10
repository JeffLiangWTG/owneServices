using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InBondMessageStatusCalculator))]
	sealed class InbondMessageStatusCalculatorTest : MessageStatusCalculatorTest
	{
		public override void TestCalculateStatusForOutgoingMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			AssertEquals(true, message.IsTransmitMessage);

			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondDepartureOriginal, ABIResponseStatus.Undefined, ImportMessageStatusList.Codes.AwaitingDepartureOriginal);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondDepartureReplacement, ABIResponseStatus.Undefined, ImportMessageStatusList.Codes.AwaitingDepartureAmendment);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondDepartureDelete, ABIResponseStatus.Undefined, ImportMessageStatusList.Codes.AwaitingDepartureWithdraw);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondArrival, ABIResponseStatus.Undefined, ImportMessageStatusList.Codes.AwaitingArrival);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondExportation, ABIResponseStatus.Undefined, ImportMessageStatusList.Codes.AwaitingExportation);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondFDATransmission, ABIResponseStatus.Undefined, ImportMessageStatusList.Codes.AwaitingFDATransmission);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondTransferOfLiability, ABIResponseStatus.Undefined, ImportMessageStatusList.Codes.AwaitingTransferOfLiability);
		}

		public override void TestCalculateStatusForIncomingClearedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondDepartureOriginal, ABIResponseStatus.Cleared, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondDepartureReplacement, ABIResponseStatus.Cleared, ImportMessageStatusList.Codes.ClearDepartureAmendment);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondDepartureDelete, ABIResponseStatus.Cleared, ImportMessageStatusList.Codes.ClearDepartureWithdraw);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondArrival, ABIResponseStatus.Cleared, ImportMessageStatusList.Codes.ClearArrival);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondExportation, ABIResponseStatus.Cleared, ImportMessageStatusList.Codes.ClearExportation);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondFDATransmission, ABIResponseStatus.Cleared, ImportMessageStatusList.Codes.ClearFDATransmission);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondTransferOfLiability, ABIResponseStatus.Cleared, ImportMessageStatusList.Codes.ClearTransferOfLiability);
		}

		public override void TestCalculateStatusForIncomingPartialClearedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondDepartureOriginal, ABIResponseStatus.PartialCleared, ImportMessageStatusList.Codes.ClearDeparturePartialOriginal);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondDepartureReplacement, ABIResponseStatus.PartialCleared, ImportMessageStatusList.Codes.ClearDeparturePartialAmendment);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondDepartureDelete, ABIResponseStatus.PartialCleared, ImportMessageStatusList.Codes.ClearDeparturePartialWithdraw);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondArrival, ABIResponseStatus.PartialCleared, ImportMessageStatusList.Codes.ClearArrival);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondExportation, ABIResponseStatus.PartialCleared, ImportMessageStatusList.Codes.ClearExportation);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondFDATransmission, ABIResponseStatus.PartialCleared, ImportMessageStatusList.Codes.ClearFDATransmission);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondTransferOfLiability, ABIResponseStatus.PartialCleared, ImportMessageStatusList.Codes.ClearTransferOfLiability);
		}

		public override void TestCalculateStatusForIncomingRejectedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondDepartureOriginal, ABIResponseStatus.Rejected, ImportMessageStatusList.Codes.ErrorDepartureOriginal);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondDepartureReplacement, ABIResponseStatus.Rejected, ImportMessageStatusList.Codes.ErrorDepartureAmendment);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondDepartureDelete, ABIResponseStatus.Rejected, ImportMessageStatusList.Codes.ErrorDepartureWithdraw);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondArrival, ABIResponseStatus.Rejected, ImportMessageStatusList.Codes.ErrorArrival);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondExportation, ABIResponseStatus.Rejected, ImportMessageStatusList.Codes.ErrorExportation);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondFDATransmission, ABIResponseStatus.Rejected, ImportMessageStatusList.Codes.ErrorFDATransmission);
			AssertCalculateStatus(EM_MessageSubTypeList.Codes.InBondTransferOfLiability, ABIResponseStatus.Rejected, ImportMessageStatusList.Codes.ErrorTransferOfLiability);
		}

		protected override MessageStatusCalculator GetNewMessageStatusCalculator(IMessageAttachee messageAttachee) => new InBondMessageStatusCalculator(messageAttachee);

		protected override ZString GetApplicationIdentifierCode() => ACEApplicationIdentifierCodeList.Codes.InbondTransaction;

		void AssertCalculateStatus(ZString messageSubType, ABIResponseStatus responseStatus, ZString expectedStatus)
		{
			message.EM_MessageSubType = messageSubType;
			calculator.CalculateStatus(message, responseStatus);
			AssertEquals(expectedStatus, messageAttachee.MessageStatus);
		}
	}
}
