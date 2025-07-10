namespace Enterprise.Customs.US.Business.Testing
{
	public class ACEQuotaQueryMessageSenderTest : MessageSenderTest
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
					sender = new ACEQuotaQueryMessageSender(Declaration);
					sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
					{ if (shouldSave) { Factory.Save(); } });
					if (shouldPrepare)
					{
						sender.OnPrepare += new ACEQuotaQueryMessageSender.PrepareEventHandler(sender_OnPrepare);
					}
				}
				return sender;
			}
		}
		ACEQuotaQueryMessageSender sender;

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
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();

					var tariff = new USCTariff.Loader(Factory).LoadBestMatch("0201305000", CargoWise.Types.ZDateTime.Today);
					if (tariff == null)
					{
						tariff = Factory.New<USCTariff>();
						tariff.UE_Tariff = "0201305000";
						tariff.UE_DateFrom = CargoWise.Types.ZDateTime.Today.AddYears(-1);
						tariff.UE_DateTo = CargoWise.Types.ZDateTime.Today.AddYears(1);
					}
					invoiceLine.JI_Tariff = tariff.UE_Tariff;

					var supTariff = new USCTariff.Loader(Factory).LoadBestMatch("98191124", CargoWise.Types.ZDateTime.Today);
					if (supTariff == null)
					{
						supTariff = Factory.New<USCTariff>();
						supTariff.UE_Tariff = "98191124";
						supTariff.UE_DateFrom = CargoWise.Types.ZDateTime.Today.AddYears(-1);
						supTariff.UE_DateTo = CargoWise.Types.ZDateTime.Today.AddYears(1);
					}
					invoiceLine.US_SupTariff = supTariff.UE_Tariff;
					invoiceLine.US_UC_NKCountryOfOrigin = "ZA";

					var entry = declaration.CustomsEntryHeaders.AddNew();
					var entryLine = entry.MergedLines.AddNew();
					invoiceLine.JI_CL = entryLine.PK;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
