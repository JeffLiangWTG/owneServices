using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public class ReconMessageSender : MessageSender
	{
		public delegate bool PrepareEventHandler(ACEReconMessageSendingAction action);

		public ReconMessageSender(ReconDeclaration reconDeclaration, UpdateActionCode actionCode)
			: base(reconDeclaration.ReconWrappedJobDeclaration)
		{
			this.reconDeclaration = reconDeclaration;
			this.actionCode = actionCode;
		}
		readonly ReconDeclaration reconDeclaration;
		readonly UpdateActionCode actionCode;

		protected override bool GenerateMessage()
		{
			MessageManager.PopulateMessage();
			return true;
		}

		protected override bool Prepare()
		{
			if (reconDeclaration.OriginalEntries.Count == 0)
			{
				Job.MessageInitiator.NotifyUserOfAnInvalidOperation(NoEntryToSendNotificationMessage);
				return false;
			}

			if (actionCode != UpdateActionCode.Delete)
			{
				if (!IsCreditCheckOKToSend())
				{
					return false;
				}
			}

			if (OnPrepare != null)
			{
				return OnPrepare(Action);
			}

			return false;
		}
		public const string NoEntryToSendNotificationMessage = "No entry to send.\r\nPlease enter at least one entry record under Recon Declaration > Entries.";

		protected override bool ShouldValidateJob => true;

		protected override MessageSendingNotificationCollection Notifications => MessageManager.MessageSendingNotifications;

		protected override string SuccessfulSendNotification => $"Reconciliation 1 {actionCode} Message sent";

		public event PrepareEventHandler OnPrepare;

		protected ReconMessageManager MessageManager
		{
			get
			{
				if (fMessageManager == null)
				{
					if (reconDeclaration.IsACE)
					{
						fMessageManager = Action.MessageManager;
					}
					else
					{
						fMessageManager = new ReconMessageManager(new ReconDeclarationIReconciliation(reconDeclaration), actionCode);
					}
				}

				return fMessageManager;
			}
		}
		ReconMessageManager fMessageManager;

		protected ACEReconMessageSendingAction Action
		{
			get
			{
				if (reconDeclaration.IsACE && fAction == null)
				{
					fAction = new ACEReconMessageSendingAction(new ReconDeclarationIReconciliation(reconDeclaration), actionCode);
				}

				return fAction;
			}
		}
		ACEReconMessageSendingAction fAction;
	}
}
