namespace Enterprise.Customs.US.Business
{
	public class ACEQuotaQueryMessageSender : MessageSender
	{
		public delegate bool PrepareEventHandler(ImportMessageSendingActionCollection actions);

		public ACEQuotaQueryMessageSender(JobDeclaration declaration)
			: base(declaration)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
		public event PrepareEventHandler OnPrepare;

		protected override bool Prepare()
		{
			if (Actions.Count == 0)
			{
				Job.MessageInitiator.WarnUserAboutSomething("Query messages NOT sent. Make sure you have entered an invoice line, country of origin and a valid tariff.", "Message(s) Sent");
				return false;
			}

			if (OnPrepare != null)
			{
				return OnPrepare(Actions);
			}
			return false;
		}

		protected override bool GenerateMessage()
		{
			return Actions.SendMessagesWithoutSaving();
		}

		protected override string SuccessfulSendNotification
		{
			get { return SuccessfulSendMessage; }
		}
		const string SuccessfulSendMessage = "Query messages sent and attached to this job";

		ACEQuotaQuerySendingActionCollection Actions
		{
			get
			{
				if (actions == null)
				{
					actions = new ACEQuotaQuerySendingActionCollection(Job);
				}
				return actions;
			}
		}
		ACEQuotaQuerySendingActionCollection actions;
	}
}
