using System.Collections;
using System.Collections.Generic;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.Testing
{
	public class TestHelperMultiMessageManager : MultiMessageManager
	{
		public TestHelperMultiMessageManager(IMessageManageableBizObj topLevelBusinessObject) : base()
		{
			this.fTopLevelBusinessObject = topLevelBusinessObject;
			this.ShowNotificationsAfterSaveExposed = false;
		}

		protected override bool ShowNotificationsAfterSaveCore
		{
			get
			{
				return ShowNotificationsAfterSaveExposed;
			}
		}
		public bool ShowNotificationsAfterSaveExposed;

		public bool ShouldSendMessagesInTestModeForTesting
		{
			get { return ShouldSendMessagesInTestMode; }
		}

		public override IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return fTopLevelBusinessObject; }
		}
		readonly IMessageManageableBizObj fTopLevelBusinessObject;

		public MessageSendingNotificationCollection GetAllAmendmentNotifications(params SingleMessageManager[] singleManager)
		{
			return new AmendmentManagerCollector(singleManager, false).Notifications;
		}

		public MessageSendingNotificationCollection GetAllOriginalNotifications(params SingleMessageManager[] singleManager)
		{
			return new OriginalManagerCollector(singleManager, true).Notifications;
		}

		public MessageSendingNotificationCollection GetAllWithdrawalNotifications(params SingleMessageManager[] singleManager)
		{
			return new WithdrawalManagerCollector(singleManager, true).Notifications;
		}

		public SingleMessageManager[] GetAllManagersWithChanges(SingleMessageManager[] allMessageManagers, SingleMessageManager[] managersToExclude)
		{
			ArrayList includedManagers = new ArrayList();
			foreach (SingleMessageManager manager in allMessageManagers)
			{
				if (((IList)managersToExclude).IndexOf(manager) == -1)
				{
					includedManagers.Add(manager);
				}
			}
			return new AmendmentManagerCollector((SingleMessageManager[])includedManagers.ToArray(typeof(SingleMessageManager)), false).ApplicableManagers;
		}

		public override bool AllowManualAmendments
		{
			get { return allowManualAmendments; }
		}
		public bool allowManualAmendments;

		public TestHelperSingleMessageManager[] allMessageManagers;
		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			return allMessageManagers;
		}

		public bool SendWheneverPossibleOnceMessagingActiveExposedCore;
		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return SendWheneverPossibleOnceMessagingActiveExposedCore; }
		}

		protected override bool CanAllMangersBeSavedExceptThosePassedIn(ISendsMessagesToCustoms sender, SingleMessageManager[] managersToExcludeFromChangesCheck, bool detectingAmendment)
		{
			if (canSaveOverriden)
			{
				return canSaveOverride;
			}
			else
			{
				return base.CanAllMangersBeSavedExceptThosePassedIn(sender, managersToExcludeFromChangesCheck, detectingAmendment);
			}
		}

		protected override bool CanSendOriginal(ISendsMessagesToCustoms sender, params SingleMessageManager[] managersToSend)
		{
			if (canSendOverriden)
			{
				return canSendOverride;
			}
			else
			{
				return base.CanSendOriginal(sender, managersToSend);
			}
		}

		protected override bool CanAmend(ISendsMessagesToCustoms sender, params SingleMessageManager[] managers)
		{
			if (canAmendOverriden)
			{
				return canAmendOverride;
			}
			else
			{
				return base.CanAmend(sender, managers);
			}
		}

		protected override bool CanWithdraw(ISendsMessagesToCustoms sender, params SingleMessageManager[] managers)
		{
			if (canWithdrawOverriden)
			{
				return canWithdrawOverride;
			}
			else
			{
				return base.CanWithdraw(sender, managers);
			}
		}

		protected override bool Withdraw(ISendsMessagesToCustoms sender, SingleMessageManager[] messagesToSendManagers)
		{
			MessageSentCalled = false;
			if (withdrawResultOverriden)
			{
				base.Withdraw(sender, messagesToSendManagers);
				return withdrawResultOverride;
			}
			else
			{
				return base.Withdraw(sender, messagesToSendManagers);
			}
		}

		protected override bool Amend(ISendsMessagesToCustoms sender, SingleMessageManager[] messagesToSendManagers)
		{
			MessageSentCalled = false;
			if (amendResultOverriden)
			{
				base.Amend(sender, messagesToSendManagers);
				return amendResultOverride;
			}
			else
			{
				return base.Amend(sender, messagesToSendManagers);
			}
		}

		protected override IList<EDIMessage> SendOriginal(ISendsMessagesToCustoms sender, SingleMessageManager[] messagesToSendManagers)
		{
			MessageSentCalled = false;
			if (sendResultOverriden)
			{
				base.SendOriginal(sender, messagesToSendManagers);
				return sendResultOverride;
			}
			else
			{
				return base.SendOriginal(sender, messagesToSendManagers);
			}
		}

		protected override void OnMessagesSent(ISendsMessagesToCustoms sender)
		{
			base.OnMessagesSent(sender);
			MessageSentCalled = true;
		}
		public bool MessageSentCalled;

		protected override void OnOneOrMoreAmendmentsSent()
		{
			base.OnOneOrMoreAmendmentsSent();
			AmendmentMessagesSentCalled = true;
		}
		public bool AmendmentMessagesSentCalled;

		protected override void OnOneOrMoreOriginalsSent()
		{
			base.OnOneOrMoreOriginalsSent();
			OriginalMessageSentCalled = true;
		}
		public bool OriginalMessageSentCalled;

		protected override void OnOneOrMoreWithdrawalsSent()
		{
			base.OnOneOrMoreWithdrawalsSent();
			WithdrawalMessageSentCalled = true;
		}
		public bool WithdrawalMessageSentCalled;

		bool canSaveOverride;
		bool canSaveOverriden;
		public bool CanSaveOverride
		{
			set
			{
				canSaveOverriden = true;
				canSaveOverride = value;
			}
		}

		bool canSendOverride;
		bool canSendOverriden;
		public bool CanSendOverride
		{
			set
			{
				canSendOverriden = true;
				canSendOverride = value;
			}
		}

		bool canAmendOverride;
		bool canAmendOverriden;
		public bool CanAmendOverride
		{
			set
			{
				canAmendOverriden = true;
				canAmendOverride = value;
			}
		}

		bool canWithdrawOverride;
		bool canWithdrawOverriden;
		public bool CanWithdrawOverride
		{
			set
			{
				canWithdrawOverriden = true;
				canWithdrawOverride = value;
			}
		}

		bool withdrawResultOverride;
		bool withdrawResultOverriden;
		public bool WithdrawResultOverride
		{
			set
			{
				withdrawResultOverriden = true;
				withdrawResultOverride = value;
			}
		}

		bool amendResultOverride;
		bool amendResultOverriden;
		public bool AmendResultOverride
		{
			set
			{
				amendResultOverriden = true;
				amendResultOverride = value;
			}
		}

		IList<EDIMessage> sendResultOverride;
		bool sendResultOverriden;
		public IList<EDIMessage> SendResultOverride
		{
			set
			{
				sendResultOverriden = true;
				sendResultOverride = value;
			}
		}

		public bool InitialisedCalled;
		protected override void Initialise()
		{
			InitialisedCalled = true;
			base.Initialise();
		}

		public bool checkTopLevelBusinessObjectsChildrenOverride;
		protected override bool CheckTopLevelBusinessObjectsChildren
		{
			get
			{
				return checkTopLevelBusinessObjectsChildrenOverride;
			}
		}
	}
}
