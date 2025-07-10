using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class CancelPTTArrivalSenderTest : PTTMessageSenderTest
	{
		public override void TestDoesSendWithMessageErrors()
		{
			Factory.Save();
			((SendsMessagesToCustoms)Declaration.MessageInitiator).ReturnTrueOnYesNoQuery = false;
			Assert(!Sender.SendMessage());
			AssertCollectionContains($"{MessageTypeDescription} message should not be sent because Permit To Transfer Arrival has not been accepted by Customs. Do you want to override and send anyway?", ((SendsMessagesToCustoms)Declaration.MessageInitiator).LastMessages);

			Environment.Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			((SendsMessagesToCustoms)Declaration.MessageInitiator).ReturnTrueOnShowUserConfirmation = false;
			Assert(!Sender.SendMessage());
			AssertCollectionContains($"{MessageTypeDescription} message should not be sent because Permit To Transfer Arrival has not been accepted by Customs. You do not have the security rights to override this error and send.", ((SendsMessagesToCustoms)Declaration.MessageInitiator).LastMessages);
		}

		protected override ZString MessageTypeDescription => EM_MessageSubTypeList.Descriptions.FTZSendPermitToTransferUnArrival;

		#region implementation

		protected override MessageSender Sender
		{
			get
			{
				if (sender == null)
				{
					sender = new PTTMessageSender(Declaration, PTTSendingOption.SendPTTUnArrival);
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
