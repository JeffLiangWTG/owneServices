using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GUI.MessagingProcess
{
	public static class CustomsMessagingGuiExtensions
	{
		public static ActionResult SendMessages(this ICustomsMessagingGui messagingGui)
		{
			return messagingGui.MessagingSupporter.SendMessages(messagingGui.GetSendMessagesGuiActionProvider());
		}

		public static ISendMessagesGuiActionProvider GetSendMessagesGuiActionProvider(this ICustomsMessagingGui messagingGui) => new SendMessagesGuiActionProvider(messagingGui);

		public static System.Windows.Forms.DialogResult ShowSummaryNotification(MessageSendingNotificationCollection msgCollection, System.Windows.Forms.MessageBoxButtons buttons, string caption, string msgPrefix = "", string msgPostFix = "", bool showErrorsAlone = false)
		{
			var msg = msgCollection.CreateSummaryNotification(showErrorsAlone);

			var icon = System.Windows.Forms.MessageBoxIcon.Error;
			if (msg.IsWarning)
			{
				icon = System.Windows.Forms.MessageBoxIcon.Warning;
			}
			else if (msg.IsInformation)
			{
				icon = System.Windows.Forms.MessageBoxIcon.Information;
			}

			var fullMsg = FormattableString.Invariant($"{msgPrefix}{msg.Message}{msgPostFix}");

			var dlgResult = Globals.Message.Show(fullMsg, caption, buttons, icon);

			return dlgResult;
		}

		public static void ConfigureProcess(this ICustomsMessagingGui messagingGui, ActionChain sendChain)
		{
			if (messagingGui is ISupportConfigureProcess config)
			{
				config.ConfigureProcess(sendChain);
			}
		}

		public static ISupportPreviewDialog GetPreviewDialogSupporter(this ICustomsMessagingGui messagingGui)
		{
			if (messagingGui is ISupportPreviewDialog sendDialog)
			{
				return sendDialog;
			}

			return null;
		}

		public static ISupportSendDialog GetSendDialogSupporter(this ICustomsMessagingGui messagingGui)
		{
			if (messagingGui is ISupportSendDialog sendDialog)
			{
				return sendDialog;
			}

			return null;
		}

		public static string PreSaveDialogCancelledMessage => Res.GetString("SendProcess|SaveCancelled", "User canceled, send aborted");
		public static string PreviewDialogCancelledMessage => Res.GetString("SendProcess|PreviewDialog", "User canceled, preview aborted");
		public static string SendDialogCancelledMessage => Res.GetString("SendProcess|SendDialog", "User canceled, send aborted");
	}
}
