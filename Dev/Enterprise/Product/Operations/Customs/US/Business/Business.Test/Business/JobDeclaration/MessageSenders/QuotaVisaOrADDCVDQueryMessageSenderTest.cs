namespace Enterprise.Customs.US.Business.Testing
{
	public class QuotaVisaOrADDCVDQueryMessageSenderTest : MessageSenderTest
	{
		public void TestSendMessageIfNotSaved()
		{
			shouldSave = false;
			Assert(!Sender.SendMessage());
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
		}

		protected override MessageSender Sender
		{
			get
			{
				if (sender == null)
				{
					sender = new QuotaVisaOrADDCVDQueryMessageSender(Declaration, QueryTypeForQuotaVisaADDCVD.ADDCVD);
					sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
					{ if (shouldSave) { Factory.Save(); } });
					if (shouldPrepare)
					{
						sender.OnPrepare += new QuotaVisaOrADDCVDQueryMessageSender.PrepareEventHandler(sender_OnPrepare);
					}
				}
				return sender;
			}
		}
		QuotaVisaOrADDCVDQueryMessageSender sender;

		bool sender_OnPrepare(ImportMessageSendingActionCollection actions)
		{
			actions.SelectAll();
			actions.IsCancelled = !shouldReturnsTrueOnPrepare;
			return !actions.IsCancelled;
		}

		bool shouldSave = true;
		bool shouldPrepare = true;
		bool shouldReturnsTrueOnPrepare = true;

		protected override JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
					JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
					JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
					invoiceLine.US_SupTariff = "98191124";
					invoiceLine.US_UC_NKCountryOfOrigin = "ZA";

					CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
					CusEntryLine entryLine = entry.MergedLines.AddNew();
					invoiceLine.JI_CL = entryLine.PK;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
