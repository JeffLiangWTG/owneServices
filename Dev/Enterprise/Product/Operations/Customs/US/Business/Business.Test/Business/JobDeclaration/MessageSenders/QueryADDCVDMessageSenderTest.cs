namespace Enterprise.Customs.US.Business.Testing
{
	public class QueryADDCVDMessageSenderTest : MessageSenderTest
	{
		public void TestSendMessageIfNotSaved()
		{
			shouldSave = false;
			Assert(!Sender.SendMessage());
		}

		public void TestNoMessageSentForACS()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			Assert(!Sender.SendMessage());
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
					sender = new QueryADDCVDMessageSender(Declaration);
					sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
					{ if (shouldSave) { Factory.Save(); } });
				}
				return sender;
			}
		}
		QueryADDCVDMessageSender sender;

		bool shouldSave = true;

		protected override JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.MessageInitiator = new SendsMessagesToCustoms();
					JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
					invoice.US_UC_NKCountryOfOrigin = "FR";
					JobComInvoiceLine line = invoice.JobComInvoiceLines.AddNew();
					line.JI_Tariff = USCTariff.CottonFeeApplicable;
					line.US_ADDCaseNo = "1";
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
