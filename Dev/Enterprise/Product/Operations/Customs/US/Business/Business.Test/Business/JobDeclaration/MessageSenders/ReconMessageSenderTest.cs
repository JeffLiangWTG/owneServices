using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class ReconMessageSenderTest : MessageSenderTest
	{
		class ReconMessageSenderForTest : ReconMessageSender
		{
			public ReconMessageSenderForTest(ReconDeclaration reconDeclaration, UpdateActionCode actionCode) : base(reconDeclaration, actionCode)
			{
			}

			public ACEReconMessageSendingAction Action_Exposed
			{
				get
				{
					return Action;
				}
			}

			public ReconMessageManager MessageManager_Exposed
			{
				get
				{
					return MessageManager;
				}
			}
		}

		public void TestMessageManagerAndAction()
		{
			var reconMessageSender = new ReconMessageSenderForTest(Recon, UpdateActionCode.Add);
			var messageManager = reconMessageSender.MessageManager_Exposed;
			var action = reconMessageSender.Action_Exposed;
			AssertNotNull(messageManager);
			AssertNotNull(action);
			AssertEquals(false, action.CertificationSignature);

			action.CertificationSignature = true;
			var messageManager2 = reconMessageSender.MessageManager_Exposed;
			var action2 = reconMessageSender.Action_Exposed;
			AssertEquals(messageManager.GetHashCode(), messageManager2.GetHashCode());
			AssertEquals(action.GetHashCode(), action2.GetHashCode());
			AssertEquals(true, action2.CertificationSignature);
		}

		public override void TestSendMessageIfAllOK()
		{
			var entry = Recon.OriginalEntries.AddNew();
			((ReconMessageSender)Sender).OnPrepare += new ReconMessageSender.PrepareEventHandler(delegate
			{ return true; });
			Sender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate
			{ return true; });
			Sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
			{ Factory.Save(); });

			bool sentSuccessfuly = Sender.SendMessage();
			Assert(sentSuccessfuly);
			CargoWise.Common.ErrorReporter.Clear();
		}

		protected override MessageSender Sender
		{
			get
			{
				if (sender == null)
				{
					sender = new ReconMessageSender(Recon, UpdateActionCode.Add);
				}
				return sender;
			}
		}
		MessageSender sender;

		ReconDeclaration Recon
		{
			get
			{
				if (recon == null)
				{
					recon = ReconDeclaration.Get(Declaration);
				}
				return recon;
			}
		}
		ReconDeclaration recon;

		protected override JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
					declaration.US_EnableENS = true;
					declaration.US_EnableCRL = false;
					declaration.MessageInitiator = new SendsMessagesToCustoms();
					declaration.ValidationModes = ValidationModes.EntrySummary;

					JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
					JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
				}
				return declaration;
			}
		}
		JobDeclaration declaration;
	}
}
