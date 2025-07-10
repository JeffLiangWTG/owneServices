using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	class StandAlonePriorNoticeMessageManager : Customs.Business.SingleMessageManager
	{
		public StandAlonePriorNoticeMessageManager(StandAlonePriorNoticeMessageSendingAction action)
		{
			Argument.NotNull(action, "action");
			this.action = action;
		}

		JobDeclaration Declaration
		{
			get { return action.Declaration; }
		}

		readonly StandAlonePriorNoticeMessageSendingAction action;

		public override BusinessObject BusinessObject
		{
			get { return Declaration; }
		}

		protected override bool IncludeBusinessLayerNotificationsInMessageSendingNotifications
		{
			get { return false; }
		}

		public override bool HasActiveMessages
		{
			get { return false; }
		}

		public override bool CanSendOriginal
		{
			get { return true; }
		}

		public override bool CanSendWithdrawal
		{
			get { return false; }
		}

		public override bool IsWaitingForResponse
		{
			get { return false; }
		}

		public override string MessageFriendlyName
		{
			get { return "Standalone Prior Notice"; }
		}

		protected override bool ShouldWaitUntilResponded
		{
			get { return false; }
		}

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			throw new NotImplementedException();
		}

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo)
		{
			throw new NotImplementedException();
		}

		protected override Enterprise.Messaging.Business.EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			throw new NotImplementedException();
		}
	}
}
