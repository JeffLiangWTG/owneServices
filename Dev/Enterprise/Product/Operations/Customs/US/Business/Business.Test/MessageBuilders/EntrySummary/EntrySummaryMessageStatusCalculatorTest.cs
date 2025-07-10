using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(EntrySummaryMessageStatusCalculator))]
	sealed class EntrySummaryMessageStatusCalculatorTest : MessageStatusCalculatorTest
	{
		public override void TestCalculateStatusForOutgoingMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Transmit;
			AssertEquals(true, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingEntrySummaryReplace, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingEntrySummaryDelete, messageAttachee.MessageStatus);
		}

		public override void TestCalculateStatusForIncomingClearedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryReplace, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryDelete, messageAttachee.MessageStatus);
		}

		public override void TestCalculateStatusForIncomingPartialClearedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.PartialCleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryOriginal, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.PartialCleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryReplace, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.PartialCleared);
			AssertEquals(ImportMessageStatusList.Codes.ClearEntrySummaryDelete, messageAttachee.MessageStatus);
		}

		public override void TestCalculateStatusForIncomingRejectedMessage()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(ImportMessageStatusList.Codes.ErrorEntrySummaryOriginal, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(ImportMessageStatusList.Codes.ErrorEntrySummaryReplace, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(ImportMessageStatusList.Codes.ErrorEntrySummaryDelete, messageAttachee.MessageStatus);
		}

		public void TestWarningStatus()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Warnings);
			AssertEquals(ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithWarnings, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.Warnings);
			AssertEquals(ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithWarnings, messageAttachee.MessageStatus);
		}

		public void TestCensusWarningStatus()
		{
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			AssertEquals(false, message.IsTransmitMessage);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.CensusWarning);
			AssertEquals(ImportMessageStatusList.Codes.EntrySummaryOriginalAcceptedWithCensusWarnings, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.EntrySummaryReplace;
			calculator.CalculateStatus(message, ABIResponseStatus.CensusWarning);
			AssertEquals(ImportMessageStatusList.Codes.EntrySummaryReplaceAcceptedWithCensusWarnings, messageAttachee.MessageStatus);
		}

		protected override MessageStatusCalculator GetNewMessageStatusCalculator(IMessageAttachee messageAttachee) => new EntrySummaryMessageStatusCalculator(messageAttachee);

		protected override ZString GetApplicationIdentifierCode() => ApplicationIdentifierCodeList.Codes.EntrySummary;
	}
}
