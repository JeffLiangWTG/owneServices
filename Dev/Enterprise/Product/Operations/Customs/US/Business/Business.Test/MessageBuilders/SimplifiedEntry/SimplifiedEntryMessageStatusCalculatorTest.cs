using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(SimplifiedEntryMessageStatusCalculator))]
	sealed class SimplifiedEntryMessageStatusCalculatorTest : MessageStatusCalculatorTest
	{
		public override void TestCalculateStatusForOutgoingMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			AssertEquals(true, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingACECargoReleaseReplace, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingACECargoReleaseUpdate, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingACECargoReleaseReplace, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingACECargoReleaseDelete, messageAttachee.MessageStatus);
		}

		public override void TestCalculateStatusForIncomingClearedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearACECargoReleaseAdd, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearACECargoReleaseReplace, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearACECargoReleaseUpdate, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearACECargoReleaseDelete, messageAttachee.MessageStatus);
		}

		public void TestCalculateStatusForIncomingPendingMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.PendingReview);
			AssertEquals(ImportMessageStatusList.Codes.ReplaceRequestPending, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.PendingReview);
			AssertEquals(ImportMessageStatusList.Codes.CancellationRequestPending, messageAttachee.MessageStatus);
		}

		public override void TestCalculateStatusForIncomingPartialClearedMessage()
		{
			Assert("Partial is not relevant for Simplified Entry", true);
		}

		public override void TestCalculateStatusForIncomingRejectedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(ImportMessageStatusList.Codes.ErrorACECargoReleaseAdd, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(ImportMessageStatusList.Codes.ErrorACECargoReleaseReplace, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseUpdate;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(ImportMessageStatusList.Codes.ErrorACECargoReleaseUpdate, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(ImportMessageStatusList.Codes.ErrorACECargoReleaseDelete, messageAttachee.MessageStatus);
		}

		public void TestCalculateWaitingForReviewStatus()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ACECargoReleaseAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.PendingReview);
			AssertEquals(ImportMessageStatusList.Codes.CancellationRequestPending, messageAttachee.MessageStatus);
		}

		protected override MessageStatusCalculator GetNewMessageStatusCalculator(IMessageAttachee messageAttachee) => new SimplifiedEntryMessageStatusCalculator(messageAttachee);

		protected override ZString GetApplicationIdentifierCode() => ACEApplicationIdentifierCodeList.Codes.CargoRelease;
	}
}
