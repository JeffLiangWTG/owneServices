namespace Enterprise.Customs.US.Business.Testing
{
	public class StandAlonePriorNoticeMessageSenderTest : MessageSenderTest
	{
		public void TestSendMessageIfNotSaved()
		{
			shouldSave = false;
			Assert(!Sender.SendMessage());
		}

		public void TestDeletingPGADataOnSendingMessage()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableENS = true;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			Declaration.InvoiceLines.RemoveAndDeleteAll();
			var invoice = declaration.Invoices[0];
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			var fda = invoiceLine.FDAs.AddNew();
			var aceFDA = invoiceLine.ACE_FDALines.AddNew();
			AssertEquals(true, declaration.IsACECargoCertificationMode);
			Sender.SendMessage();
			AssertEquals(0, invoiceLine.FDAs.Count);
			AssertEquals(true, fda.IsDeleted);
			AssertEquals(1, invoiceLine.ACE_FDALines.Count);
			AssertEquals(false, aceFDA.IsDeleted);
		}

		public void TestOnPrepareNotSet()
		{
			shouldPrepare = false;
			Assert(!Sender.SendMessage());
		}

		public void TestOnPrepareReturnsFalse()
		{
			shouldReturnsTrueOnPrepare = false;
			Assert(!Sender.SendMessage());
		}

		#region implementation

		protected override void SetUp()
		{
			base.SetUp();
			sender = null; //on every test sender will be constructed
			Declaration.HasChanges = true; //on every test sender will check "Declaration.HasChanges" property
			shouldSave = true;
			shouldPrepare = true;
			shouldReturnsTrueOnPrepare = true;
			shouldAllowNotifications = true;
			shouldReturnsTrueOnAllowNotifications = true;
		}

		protected override MessageSender Sender
		{
			get
			{
				if (sender == null)
				{
					sender = new StandAlonePriorNoticeMessageSender(Declaration);
					sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
					{ if (shouldSave) { Factory.Save(); } });
					if (shouldPrepare)
					{
						sender.OnPrepare += new StandAlonePriorNoticeMessageSender.PrepareEventHandler(sender_OnPrepare);
					}
					if (shouldAllowNotifications)
					{
						sender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(sender_OnAllowNotifications);
					}
				}
				return sender;
			}
		}
		StandAlonePriorNoticeMessageSender sender;

		bool sender_OnAllowNotifications(Customs.Business.MessageSendingNotificationCollection notifications)
		{
			return shouldReturnsTrueOnAllowNotifications;
		}

		bool sender_OnPrepare(ImportMessageSendingActionCollection actions)
		{
			actions.SelectAll();
			actions.IsCancelled = !shouldReturnsTrueOnPrepare;
			return !actions.IsCancelled;
		}

		bool shouldSave = true;
		bool shouldPrepare = true;
		bool shouldReturnsTrueOnPrepare = true;
		bool shouldAllowNotifications = true;
		bool shouldReturnsTrueOnAllowNotifications = true;

		protected override JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.MessageInitiator = new SendsMessagesToCustoms();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableSPN = true;
					declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.ENT;
					declaration.ImportEntryNumber = "23648975";
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
					var fda = invoiceLine.FDAs.AddNew();
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
