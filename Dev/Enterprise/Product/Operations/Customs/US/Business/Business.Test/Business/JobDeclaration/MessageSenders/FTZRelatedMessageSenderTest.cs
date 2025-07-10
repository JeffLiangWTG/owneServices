namespace Enterprise.Customs.US.Business.Testing
{
	public abstract class FTZRelatedMessageSenderTest : MessageSenderTest
	{
		public void TestSendMessageIfNotSaved()
		{
			shouldSave = false;
			Assert(!Sender.SendMessage());
		}

		public void TestOnAllowNotificationsNotSet()
		{
			shouldAllowNotifications = false;
			Assert(!Sender.SendMessage());
		}

		public void TestOnAllowNotificationsReturnsFalse()
		{
			shouldReturnTrueOnAllowNotifications = false;
			Assert(!Sender.SendMessage());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			sender = null; //on every test sender will be constructed			
			Declaration.HasChanges = true; //on every test sender will check "Declaration.HasChanges" property
			shouldSave = true;
			shouldAllowNotifications = true;
			shouldReturnTrueOnAllowNotifications = true;
		}
		protected bool shouldSave = true;
		protected bool shouldAllowNotifications = true;
		protected bool shouldReturnTrueOnAllowNotifications = true;
		protected FTZRelatedMessageSender sender;

		protected override JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.MessageInitiator = new SendsMessagesToCustoms();
					declaration.US_EntryFilerCode = "XJ5";
					declaration.JE_MessageType = JobMessageTypeList.Codes.FTZ;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
