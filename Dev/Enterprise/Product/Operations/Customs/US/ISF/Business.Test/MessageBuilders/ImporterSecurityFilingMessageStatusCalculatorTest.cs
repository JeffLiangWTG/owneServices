using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(ImporterSecurityFilingMessageStatusCalculator))]
	sealed class ImporterSecurityFilingMessageStatusCalculatorTest : MessageStatusCalculatorTest
	{
		public override void TestCalculateStatusForOutgoingMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			AssertEquals(true, message.IsTransmitMessage);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(MessageStatusList.Codes.AwaitingISFAdd, messageAttachee.MessageStatus);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(MessageStatusList.Codes.AwaitingISFReplace, messageAttachee.MessageStatus);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(MessageStatusList.Codes.AwaitingISFDelete, messageAttachee.MessageStatus);
		}

		public override void TestCalculateStatusForIncomingClearedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(MessageStatusList.Codes.ClearISFAdd, messageAttachee.MessageStatus);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(MessageStatusList.Codes.ClearISFReplace, messageAttachee.MessageStatus);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(MessageStatusList.Codes.ClearISFDelete, messageAttachee.MessageStatus);
		}

		public override void TestCalculateStatusForIncomingPartialClearedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.PartialCleared);
			AssertEquals(MessageStatusList.Codes.ClearWithWarningISFAdd, messageAttachee.MessageStatus);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.PartialCleared);
			AssertEquals(MessageStatusList.Codes.ClearWithWarningISFReplace, messageAttachee.MessageStatus);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.PartialCleared);
			AssertEquals(MessageStatusList.Codes.ClearWithWarningISFDelete, messageAttachee.MessageStatus);
		}

		public override void TestCalculateStatusForIncomingRejectedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(MessageStatusList.Codes.ErrorISFAdd, messageAttachee.MessageStatus);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(MessageStatusList.Codes.ErrorISFReplace, messageAttachee.MessageStatus);
			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ISFDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(MessageStatusList.Codes.ErrorISFDelete, messageAttachee.MessageStatus);
		}

		protected override MessageStatusCalculator GetNewMessageStatusCalculator(IMessageAttachee messageAttachee) => new ImporterSecurityFilingMessageStatusCalculator(messageAttachee);

		protected override ZString GetApplicationIdentifierCode() => ApplicationIdentifierCodeList.Codes.ImporterSecurityFiling;
	}
}
