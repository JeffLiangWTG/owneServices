using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	public class PTTMessageSenderTest : FTZRelatedMessageSenderTest
	{
		public virtual void TestDoesSendWithMessageErrors()
		{
			Factory.Save();
			((SendsMessagesToCustoms)Declaration.MessageInitiator).ReturnTrueOnYesNoQuery = false;
			Assert(!Sender.SendMessage());
			AssertCollectionContains($"{MessageTypeDescription} message should not be sent because the admission data has not been accepted by Customs. Do you want to override and send anyway?", ((SendsMessagesToCustoms)Declaration.MessageInitiator).LastMessages);

			Environment.Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			((SendsMessagesToCustoms)Declaration.MessageInitiator).ReturnTrueOnShowUserConfirmation = false;
			Assert(!Sender.SendMessage());
			AssertCollectionContains($"{MessageTypeDescription} message should not be sent because the admission data has not been accepted by Customs. You do not have the security rights to override this error and send.", ((SendsMessagesToCustoms)Declaration.MessageInitiator).LastMessages);
		}

		public override void TestSendMessageIfAllOK()
		{
			Declaration.AdmissionStatus = FTZMessageStatusList.Codes.ClearFTZAdmissionAdd;
			Declaration.US_F_DirectDelivery = true;
			bool sentSuccessfuly = Sender.SendMessage();
			Assert(sentSuccessfuly);
			CargoWise.Common.ErrorReporter.Clear();
		}

		protected virtual ZString MessageTypeDescription => EM_MessageSubTypeList.Descriptions.FTZPermitToTransfer;

		#region implementation

		protected override MessageSender Sender
		{
			get
			{
				if (sender == null)
				{
					sender = new PTTMessageSender(Declaration, PTTSendingOption.SendPTTMessage);
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
