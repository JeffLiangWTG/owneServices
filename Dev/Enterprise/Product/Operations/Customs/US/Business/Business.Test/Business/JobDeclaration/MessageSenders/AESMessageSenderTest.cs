using Enterprise.Customs.Common.US;

namespace Enterprise.Customs.US.Business.Testing
{
	public class AESMessageSenderTest : Customs.Business.Testing.MessageSenderTest
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

		public void TestSettingUS_SendWithdrawn()
		{
			DeclarationTestHelper.SetEntryFilerIDDetails("364331434", Registry.Business.Customs.US.AESEntryFilerIDTypeList.Codes.EmployerIdentificationNumber);
			var declaration = Declaration;
			Factory.Save();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals(1, declaration.CustomsEntryHeaders.Count);
			var entry = declaration.CustomsEntryHeaders[0];
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.AwaitingOriginalResponse;
			Factory.Save();

			entry.CH_Status = AESDirectCustomsEntryStatus.Codes.OriginalSEDClear;
			Factory.Save();
			entry.US_SendWithdrawn = true;
			AssertEquals(true, entry.US_SendWithdrawn);
			Sender.SendMessage();
			AssertEquals(false, entry.US_SendWithdrawn);
		}

		#region implementation

		protected override void SetUp()
		{
			base.SetUp();
			sender = null; //on every test sender will be constructed
			Declaration.HasChanges = true; //on every test sender will check "Declaration.HasChanges" property
			shouldSave = true;
			shouldPrepare = true;
		}

		AESMessageSender Sender
		{
			get
			{
				if (sender == null)
				{
					sender = new AESMessageSender(Declaration);
					sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
					{ if (shouldSave) { Factory.Save(); } });
					if (shouldPrepare)
					{
						sender.OnPrepare += new AESMessageSender.PrepareEventHandler(delegate
						{ return; });
					}
				}
				return sender;
			}
		}
		AESMessageSender sender;

		bool shouldSave = true;
		bool shouldPrepare = true;

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.MessageInitiator = new SendsMessagesToCustoms();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
