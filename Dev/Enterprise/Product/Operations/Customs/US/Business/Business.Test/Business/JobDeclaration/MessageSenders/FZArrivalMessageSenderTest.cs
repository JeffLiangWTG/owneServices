using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class FZArrivalMessageSenderTest : FTZRelatedMessageSenderTest
	{
		public override void TestSendMessageIfAllOK()
		{
			Assert("No Bills entered - nothing to send", !Sender.SendMessage());
			var bill = Declaration.Bills.AddNew();
			var itNo = bill.ITAndSplitDetails.AddNew();
			itNo.US_ITNumber = "V10030056";
			Assert("Should be sent", Sender.SendMessage());
			AssertEquals(FTZMessageStatusList.Codes.AwaitingGoodsArrival, Declaration.FTZArrivalStatus);
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
					sender = new FZArrivalMessageSender(Declaration);
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
