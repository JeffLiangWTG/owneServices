using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(CargoReleaseMessageStatusCalculator))]
	sealed class CargoReleaseMessageStatusCalculatorTest : MessageStatusCalculatorTest
	{
		public override void TestCalculateStatusForOutgoingMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			AssertEquals(true, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.CargoReleaseAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingCargoReleaseOriginal, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.CargoReleaseReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingCargoReleaseReplace, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.CargoReleaseDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingCargoReleaseDelete, messageAttachee.MessageStatus);
		}

		public override void TestCalculateStatusForIncomingClearedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.CargoReleaseAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearCargoReleaseOriginal, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.CargoReleaseReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearCargoReleaseReplace, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.CargoReleaseDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearCargoReleaseDelete, messageAttachee.MessageStatus);
		}

		public override void TestCalculateStatusForIncomingPartialClearedMessage()
		{
			Assert("Partial is not relevant for CRL", true);
		}

		public override void TestCalculateStatusForIncomingRejectedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.CargoReleaseAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(ImportMessageStatusList.Codes.ErrorCargoReleaseOriginal, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.CargoReleaseReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(ImportMessageStatusList.Codes.ErrorCargoReleaseReplace, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.CargoReleaseDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(ImportMessageStatusList.Codes.ErrorCargoReleaseDelete, messageAttachee.MessageStatus);
		}

		protected override MessageStatusCalculator GetNewMessageStatusCalculator(IMessageAttachee messageAttachee) => new CargoReleaseMessageStatusCalculator(messageAttachee);

		protected override ZString GetApplicationIdentifierCode() => ApplicationIdentifierCodeList.Codes.CargoReleaseTransactions;
	}
}
