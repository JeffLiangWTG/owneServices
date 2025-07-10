using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public class StatementMessagesHandler
	{
		public void SendDeleteAddMessage(StatementDeleteAndSendingActionCollection messageSendingActions, MessageSendingNotificationCollection notifications, BusinessObjectFactory factory)
		{
			if (!Env.Security.USCustomsImportStatementMessaging.IsAllowed)
			{
				Env.Security.USCustomsImportStatementMessaging.ShowError();
			}
			else if (ContinueWithNotifications(notifications))
			{
				AutomatedClearinghouseMessageManager manager = new AutomatedClearinghouseMessageManager();

				if (GetShouldContinueToSendAfterShowingMessageSendingActions(messageSendingActions) &&
					 !messageSendingActions.IsCancelled &&
					manager.GenerateStatementDeleteAdd(messageSendingActions))
				{
					try
					{
						factory.Save();
						Globals.Message.ShowInformation(messageToShowAfterSent, "Message(s) Sent");
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
		}

		protected
#if DEBUG
 virtual
#endif
 bool GetShouldContinueToSendAfterShowingMessageSendingActions(StatementDeleteAndSendingActionCollection actions)
		{
			ZFormModaliser.ShowDialogAndDispose(new StatementSendingActionForm(actions));
			return actions.SendMessage;
		}

		protected readonly string messageToShowAfterSent = "Statement Delete/Add message(s) Sent.\r\nEntry will be updated when these messages are responded successfully.";

		#region Implementation

		public bool ContinueWithNotifications(MessageSendingNotificationCollection notifications)
		{
			bool result = false;

			if (notifications.ContainsError())
			{
				Globals.Message.ShowError("There are critical errors for sending a message. Please fix them and try again.\r\n\r\n" + notifications.NotificationsAsString());
			}
			else if (!notifications.ContainsWarning() || Globals.Message.Show("Please review the following warnings before sending a message.\r\n\r\n" + notifications.NotificationsAsString() + "\r\nAre you sure you wish to continue?", "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				result = true;
			}

			return result;
		}

		public bool ContinueWithNotifications(MessageSendingNotificationCollection notifications, string messageErrorHeaderText, string confirmationQuestionText)
		{
			return (!notifications.ContainsWarning() || Globals.Message.Show(messageErrorHeaderText + System.Environment.NewLine + System.Environment.NewLine + notifications.NotificationsAsString() +
				System.Environment.NewLine + confirmationQuestionText, "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
		}

		#endregion

	}
}
