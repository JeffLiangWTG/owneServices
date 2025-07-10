using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class RequestToExtensionOrClosureTIBMessageSender : MessageSender
	{
		public delegate bool PrepareEventHandler(ImportMessageSendingActionCollection actions);

		public RequestToExtensionOrClosureTIBMessageSender(JobDeclaration declaration, ImportMessageSendingMessageType messageType)
			: base(declaration)
		{
			this.messageType = messageType;
		}
		readonly ImportMessageSendingMessageType messageType;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event PrepareEventHandler OnPrepare;

		protected override bool Prepare()
		{
			if (!Job.ActiveEntryHeaders.HasTemporaryImportationBond)
			{
				Job.MessageInitiator.NotifyUserOfAnInvalidOperation(JobIsNotTemporaryImportationBond);
				return false;
			}

			if (Actions.Count == 0)
			{
				Job.MessageInitiator.WarnUserAboutSomething("There are no entries to send " + messageType + " messages for", "Send Messages");
				return false;
			}

			if (OnPrepare != null)
			{
				return OnPrepare(Actions);
			}
			return false;
		}
		public const string JobIsNotTemporaryImportationBond = "There are no entries with an entry type, TIB(Temporary Importation Bond).";

		protected override bool GenerateMessage()
		{
			return Actions.SendMessagesWithoutSaving(Job.MessageInitiator);
		}

		protected override string SuccessfulSendNotification
		{
			get { return ZString.Format(SuccessfulSendMessage, messageType == ImportMessageSendingMessageType.ExtendTIB ? "Extension" : "Closure"); }
		}
		public const string SuccessfulSendMessage = "Message(s) to Request to {0} TIB Sent";

		ImportMessageSendingActionCollection Actions
		{
			get
			{
				if (actions == null)
				{
					actions = new ImportMessageSendingActionCollection(Job, messageType);
				}
				return actions;
			}
		}
		ImportMessageSendingActionCollection actions;
	}
}
