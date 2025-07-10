namespace Enterprise.Customs.US.Business.Testing
{
	public class RequestToExtendTIBMessageSenderTest : MessageSenderTest
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

		public void TestSendClosureMessageIfNotSaved()
		{
			shouldSave = false;
			Assert(!ClosureMessageSender.SendMessage());
		}

		public void TestClosureOnPrepareNotSet()
		{
			shouldPrepare = false;
			Assert(!ClosureMessageSender.SendMessage());
		}

		public void TestClosureOnPrepareReturnsFalse()
		{
			shouldReturnsTrueOnPrepare = false;
			Assert(!ClosureMessageSender.SendMessage());
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
					sender = new RequestToExtensionOrClosureTIBMessageSender(Declaration, ImportMessageSendingMessageType.ExtendTIB);
					sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
					{ if (shouldSave) { Factory.Save(); } });
					if (shouldPrepare)
					{
						sender.OnPrepare += new RequestToExtensionOrClosureTIBMessageSender.PrepareEventHandler(sender_OnPrepare);
					}
				}
				return sender;
			}
		}
		RequestToExtensionOrClosureTIBMessageSender sender;

		MessageSender ClosureMessageSender
		{
			get
			{
				if (closureMessageSender == null)
				{
					closureMessageSender = new RequestToExtensionOrClosureTIBMessageSender(Declaration, ImportMessageSendingMessageType.ClosureTIB);
					closureMessageSender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
					{ if (shouldSave) { Factory.Save(); } });
					if (shouldPrepare)
					{
						closureMessageSender.OnPrepare += new RequestToExtensionOrClosureTIBMessageSender.PrepareEventHandler(sender_OnPrepare);
					}
				}

				return closureMessageSender;
			}
		}
		RequestToExtensionOrClosureTIBMessageSender closureMessageSender;

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
					declaration.MessageInitiator = new SendsMessagesToCustoms();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
					CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.AddNew();
					ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
