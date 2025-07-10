using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(GlobalBusinessIdentifierMessageStatusCalculator))]
	class GlobalBusinessIdentifierMessageStatusCalculatorTest : MessageStatusCalculatorTest
	{
		public override void TestCalculateStatusForIncomingClearedMessage()
		{
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(GBISubmissionStatusList.Codes.ClearGBIAdd, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierUpdate;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(GBISubmissionStatusList.Codes.ClearGBIUpdate, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			AssertEquals(GBISubmissionStatusList.Codes.ClearGBIDelete, messageAttachee.MessageStatus);

			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Undefined;
			var expectedExceptionMessage = string.Format(GlobalBusinessIdentifierMessageStatusCalculator.InvalidMessageSubTypeExceptionMessage
				, GlobalBusinessIdentifierMessageStatusCalculator.Cleared
				, MessageSubTypeCodes.Codes.Undefined
				, messageAttachee.BusinessObjectPK);
			AssertExceptionThrown(typeof(DeveloperNotificationException), expectedExceptionMessage, () =>
			{
				calculator.CalculateStatus(message, ABIResponseStatus.Cleared);
			});
		}

		public override void TestCalculateStatusForIncomingPartialClearedMessage()
		{
			Assert(true);
		}

		public override void TestCalculateStatusForIncomingRejectedMessage()
		{
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(GBISubmissionStatusList.Codes.ErrorGBIAdd, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierUpdate;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(GBISubmissionStatusList.Codes.ErrorGBIUpdate, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			AssertEquals(GBISubmissionStatusList.Codes.ErrorGBIDelete, messageAttachee.MessageStatus);

			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Undefined;
			var expectedExceptionMessage = string.Format(GlobalBusinessIdentifierMessageStatusCalculator.InvalidMessageSubTypeExceptionMessage
				, GlobalBusinessIdentifierMessageStatusCalculator.Rejected
				, MessageSubTypeCodes.Codes.Undefined
				, messageAttachee.BusinessObjectPK);
			AssertExceptionThrown(typeof(DeveloperNotificationException), expectedExceptionMessage, () =>
			{
				calculator.CalculateStatus(message, ABIResponseStatus.Rejected);
			});
		}

		public override void TestCalculateStatusForOutgoingMessage()
		{
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierAdd;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(GBISubmissionStatusList.Codes.AwaitingGBIAdd, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierUpdate;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(GBISubmissionStatusList.Codes.AwaitingGBIUpdate, messageAttachee.MessageStatus);

			message.EM_MessageSubType = EM_MessageSubTypeList.Codes.GlobalBusinessIdentifierDelete;
			calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			AssertEquals(GBISubmissionStatusList.Codes.AwaitingGBIDelete, messageAttachee.MessageStatus);

			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Undefined;
			var expectedExceptionMessage = string.Format(GlobalBusinessIdentifierMessageStatusCalculator.InvalidMessageSubTypeExceptionMessage
				, GlobalBusinessIdentifierMessageStatusCalculator.Awaiting
				, MessageSubTypeCodes.Codes.Undefined
				, messageAttachee.BusinessObjectPK);
			AssertExceptionThrown(typeof(DeveloperNotificationException), expectedExceptionMessage, () =>
			{
				calculator.CalculateStatus(message, ABIResponseStatus.Undefined);
			});
		}

		protected override ZString GetApplicationIdentifierCode()
		{
			return ACEApplicationIdentifierCodeList.Codes.GBIReferenceCreateUpdateDelete;
		}

		protected override MessageStatusCalculator GetNewMessageStatusCalculator(IMessageAttachee messageAttachee)
		{
			return new GlobalBusinessIdentifierMessageStatusCalculator(messageAttachee);
		}

		protected override IMessageAttachee GetMessageAttachee()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TST001";
			org.OH_FullName = "Test 001";
			org.OH_RL_NKClosestPort = "USLAX";

			return new GlobalBusinessIdentifierData(OrgHeaderWrapper.New(org));
		}
	}
}
