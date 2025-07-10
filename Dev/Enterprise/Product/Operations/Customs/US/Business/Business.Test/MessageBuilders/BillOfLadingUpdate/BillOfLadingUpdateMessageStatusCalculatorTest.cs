using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(BillOfLadingUpdateMessageStatusCalculator))]
	sealed class BillOfLadingUpdateMessageStatusCalculatorTest : MessageStatusCalculatorTest
	{
		public override void TestCalculateStatusForOutgoingMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			AssertEquals(true, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BillOfLadingUpdate;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingBillOfLadingUpdate, messageAttachee.MessageStatus);
		}

		public override void TestCalculateStatusForIncomingClearedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BillOfLadingUpdate;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearBillOfLadingUpdate, messageAttachee.MessageStatus);
		}

		public override void TestCalculateStatusForIncomingPartialClearedMessage()
		{
			Assert("Partial is not relevant for Bill of Lading Update", true);
		}

		public override void TestCalculateStatusForIncomingRejectedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.BillOfLadingUpdate;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(ImportMessageStatusList.Codes.ErrorBillOfLadingUpdate, messageAttachee.MessageStatus);
		}

		protected override MessageStatusCalculator GetNewMessageStatusCalculator(IMessageAttachee messageAttachee) => new BillOfLadingUpdateMessageStatusCalculator(messageAttachee);

		protected override ZString GetApplicationIdentifierCode() => ApplicationIdentifierCodeList.Codes.BillofLadingUpdate;
	}
}
