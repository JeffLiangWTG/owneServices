using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class SendPTTArrivalSenderTest : PTTMessageSenderTest
	{
		protected override ZString MessageTypeDescription => EM_MessageSubTypeList.Descriptions.FTZPermitToTransferArrival;

		public override void TestDoesSendWithMessageErrors()
		{
			Assert("PTT Arrival message is allowed without any restriction", true);
		}

		#region implementation

		protected override MessageSender Sender
		{
			get
			{
				if (sender == null)
				{
					sender = new PTTMessageSender(Declaration, PTTSendingOption.SendPTTArrival);
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

		#endregion
	}
}
