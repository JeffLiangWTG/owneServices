using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(BorderCargoReleaseMessageStatusCalculator))]
	sealed class BorderCargoReleaseMessageStatusCalculatorTest : MessageStatusCalculatorTest
	{
		public override void TestCalculateStatusForOutgoingMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			AssertEquals(true, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BorderCargoReleaseAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingBorderCargoReleaseOriginal, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BorderCargoReleaseReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingBorderCargoReleaseReplace, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BorderCargoReleaseDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingBorderCargoReleaseDelete, messageAttachee.MessageStatus);
		}

		public override void TestCalculateStatusForIncomingClearedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BorderCargoReleaseAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearBorderCargoReleaseOriginal, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BorderCargoReleaseReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearBorderCargoReleaseReplace, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BorderCargoReleaseDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearBorderCargoReleaseDelete, messageAttachee.MessageStatus);
		}

		public override void TestCalculateStatusForIncomingPartialClearedMessage()
		{
			Assert("Partial is not relevant for CRL", true);
		}

		public override void TestCalculateStatusForIncomingRejectedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BorderCargoReleaseAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(ImportMessageStatusList.Codes.ErrorBorderCargoReleaseOriginal, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BorderCargoReleaseReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(ImportMessageStatusList.Codes.ErrorBorderCargoReleaseReplace, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BorderCargoReleaseDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(ImportMessageStatusList.Codes.ErrorBorderCargoReleaseDelete, messageAttachee.MessageStatus);
		}

		protected override MessageStatusCalculator GetNewMessageStatusCalculator(IMessageAttachee messageAttachee) => new BorderCargoReleaseMessageStatusCalculator(messageAttachee);

		protected override ZString GetApplicationIdentifierCode() => ApplicationIdentifierCodeList.Codes.BorderCargoRelease;
	}
}
