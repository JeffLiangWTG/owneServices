namespace Enterprise.Customs.US.Business.Testing
{
	public class RequestTariffUpdatesMessageSenderTest : MessageSenderTest
	{
		public void TestSendMessageIfNotSaved()
		{
			shouldSave = false;
			Assert(!Sender.SendMessage());
		}

		public override void TestSendMessageIfAllOK()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			base.TestSendMessageIfAllOK();
		}

		public void TestSendMessageInTheDeclarationFactory()
		{
			// public ReferenceFileRequester() : this(new BusinessObjectFactory())
			// if this constructor used, tariff request message will be created in the new Factory and will not be shown
			// when user go to the Messages tab
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = Declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			Declaration.Factory.Save();

			var sender = new RequestTariffUpdatesMessageSender(Declaration);
			sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
			{ Factory.Save(); });
			sender.SendMessage();
			AssertEquals("Precondition: InBondRelatedRecords count should contains Declaration", 1, Declaration.InBondRelatedRecords.Count);
		}

		#region implementation

		protected override void SetUp()
		{
			base.SetUp();
			sender = null; //on every test sender will be constructed			
			Declaration.HasChanges = true; //on every test sender will check "Declaration.HasChanges" property
			shouldSave = true;
		}

		protected override MessageSender Sender
		{
			get
			{
				if (sender == null)
				{
					sender = new RequestTariffUpdatesMessageSender(Declaration);
					sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
					{ if (shouldSave) { Factory.Save(); } });
				}
				return sender;
			}
		}
		RequestTariffUpdatesMessageSender sender;
		bool shouldSave = true;

		protected override JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.MessageInitiator = new SendsMessagesToCustoms();
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
