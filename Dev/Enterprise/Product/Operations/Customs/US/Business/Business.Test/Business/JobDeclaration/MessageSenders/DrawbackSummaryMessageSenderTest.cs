using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class DrawbackSummaryMessageSenderTest : MessageSenderTest
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

		#region implementation

		protected override void SetUp()
		{
			base.SetUp();
			sender = null; //on every test sender will be constructed			
			Declaration.HasChanges = true; //on every test sender will check "Declaration.HasChanges" property
			shouldSave = true;
			shouldAllowNotifications = true;
			shouldReturnTrueOnAllowNotifications = true;
		}

		protected override MessageSender Sender
		{
			get
			{
				if (sender == null)
				{
					sender = new DrawbackSummaryMessageSender(Declaration, UpdateActionCode.Add);
					sender.OnSave += new Customs.Business.MessageSender.SaveEventHandler(delegate
					{ if (shouldSave) { Factory.Save(); } });
					if (shouldAllowNotifications)
					{
						sender.OnAllowNotifications += new Customs.Business.MessageSender.AllowNotificationsEventHandler(delegate(MessageSendingNotificationCollection notifications)
						{ return shouldReturnTrueOnAllowNotifications; });
					}
				}
				return sender;
			}
		}
		DrawbackSummaryMessageSender sender;

		bool shouldSave = true;
		bool shouldAllowNotifications = true;
		bool shouldReturnTrueOnAllowNotifications = true;

		protected override JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.MessageInitiator = new SendsMessagesToCustoms();
					declaration.US_EntryFilerCode = "XJ5";
					declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
					declaration.US_DRWFilingMethod = DrawbackMethodOfFilingList.Codes.ABI;
				}
				return declaration;
			}
		}
		JobDeclaration declaration;

		#endregion
	}
}
