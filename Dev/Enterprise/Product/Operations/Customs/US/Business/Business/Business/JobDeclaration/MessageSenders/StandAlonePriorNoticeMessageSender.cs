namespace Enterprise.Customs.US.Business
{
	public class StandAlonePriorNoticeMessageSender : MessageSender
	{
		public delegate bool PrepareEventHandler(ImportMessageSendingActionCollection actions);

		public StandAlonePriorNoticeMessageSender(JobDeclaration declaration)
			: base(declaration)
		{
		}

		public event PrepareEventHandler OnPrepare;

		protected override bool Prepare()
		{
			ClearIrrelevantPGAData();
			IPriorNoticeHeader header = Job;

			if (!Job.IsACEENTStandAlonePriorNotice && !Job.IsACEBLNStandAlonePriorNotice && !Job.IsFTZPGAStandAlonePriorNotice && header.StandalonePriorNoticeLines.Count == 0)
			{
				Job.MessageInitiator.WarnUserAboutSomething("There are no lines to send Prior Notice messages for.\r\nOnly those FDA lines which are not disclaimed and do not have a Confirmation Number will be sent.", "Send Messages");
				return false;
			}

			if (OnPrepare != null)
			{
				return OnPrepare(Actions);
			}

			return false;
		}

		void ClearIrrelevantPGAData()
		{
			var data = (USLinkedToDeclarationData)Job.GetNewLinkedToDeclarationData();
			foreach (JobComInvoiceLine invoiceLine in Job.InvoiceLines)
			{
				invoiceLine.UpdateOGAPGADetailsOnMessaging(data);
			}
		}

		protected override bool GenerateMessage()
		{
			return Actions.SendMessagesWithoutSaving(Job.MessageInitiator);
		}

		protected override string SuccessfulSendNotification
		{
			get { return SuccessfulSendMessage; }
		}
		const string SuccessfulSendMessage = "Prior Notice Message(s) Sent";

		protected override bool ShouldValidateJob
		{
			get { return true; }
		}

		protected override bool ValidateJob()
		{
			Job.ValidationModes = ValidationModes.StandAlonePriorNotice;
			return base.ValidateJob();
		}

		ImportMessageSendingActionCollection Actions
		{
			get { return actions ?? (actions = new ImportMessageSendingActionCollection(Job, ImportMessageSendingMessageType.StandAlonePriorNotice)); }
		}
		ImportMessageSendingActionCollection actions;
	}
}
