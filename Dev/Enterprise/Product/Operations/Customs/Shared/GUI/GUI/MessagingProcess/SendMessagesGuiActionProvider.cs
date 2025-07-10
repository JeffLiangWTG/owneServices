using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessagingProcess;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.Business.MessagingProcess.CustomsSupervisorOverrides;

namespace Enterprise.Customs.GUI.MessagingProcess
{
	public sealed class SendMessagesGuiActionProvider : ISendMessagesGuiActionProvider
	{
		public SendMessagesGuiActionProvider(ICustomsMessagingGui messagingGui)
		{
			this.messagingGui = Argument.NotNull(messagingGui, nameof(messagingGui));
		}
		readonly ICustomsMessagingGui messagingGui;

		#region ShowPreSendValidationNotifications
		ActionStep ISendMessagesGuiActionProvider.ShowPreSendValidationNotifications => ShowPreSendValidationNotifications;
		ActionResult ShowPreSendValidationNotifications(ActionResult previousResult)
		{
			var result = previousResult;

			if (result.Notifications.Any())
			{
				var dlgResult = CustomsMessagingGuiExtensions.ShowSummaryNotification(result.Notifications, System.Windows.Forms.MessageBoxButtons.YesNo,
					Res.GetString("SendProcess|PreSendNotification|Validation", "Validation"),
					Res.GetString("SendProcess|PreSendNotification|PleaseReview", "Please review the following notifications:\n\n"),
					Res.GetString("SendProcess|PreSendNotification|ContinueQuestion", "\n\nDo you wish to continue?"),
					false);

				result.Success = dlgResult == System.Windows.Forms.DialogResult.Yes;
				result.PreviousNotifications.AddRange(result.Notifications);
				result.Notifications.Clear();
			}

			return result;
		}
		#endregion

		#region ShowFormPreSaveDialog
		ActionStep ISendMessagesGuiActionProvider.ShowFormPreSaveDialog => ShowFormPreSaveDialog;
		ActionResult ShowFormPreSaveDialog(ActionResult previousResult)
		{
			var result = previousResult;

			var response = PlugIn.CustomsPlugIn.FormPreSaved(messagingGui.MessagingSupporter.TopLevelBusinessObject, messagingGui.TopLevelBusinessObjectForm);

			if (!response)
			{
				result.AppendErrorNotification(CustomsMessagingGuiExtensions.PreSaveDialogCancelledMessage);
				result.Success = false;
			}

			return result;
		}
		#endregion

		#region ShowSendDialog
		ActionStep ISendMessagesGuiActionProvider.ShowSendDialog => ShowSendDialog;
		ActionResult ShowSendDialog(ActionResult previousResult)
		{
			var result = previousResult;
			var supporter = messagingGui.GetSendDialogSupporter();

			if (supporter != null)
			{
				result = ShowOkCancelDialog(supporter.GetSendDialog(result), result, supporter.SendDialogCancelledMessage ?? CustomsMessagingGuiExtensions.SendDialogCancelledMessage);
			}

			return result;
		}
		#endregion

		#region ShowSendMessagesWithErrorsSecurityCheckpointOverride
		ActionStep ISendMessagesGuiActionProvider.ShowSendMessagesWithErrorsSecurityCheckpointOverride => ShowSendMessagesWithErrorsSecurityCheckpointOverride;
		ActionResult ShowSendMessagesWithErrorsSecurityCheckpointOverride(ActionResult previousResult)
		{
			var result = previousResult;

			if (result.ContainsWarning() && messagingGui.MessagingSupporter.Provider.GetSendMessagesWithErrorsOverrideSecurityCheckpoint() is SecurityCheckpoint overrideCheckPoint)
			{
				var topLevelBizObj = messagingGui.MessagingSupporter.TopLevelBusinessObject as EnterpriseBusinessObject;
				if (topLevelBizObj != null)
				{
					var supervisorOverrides = new CustomsSupervisorOverrides(topLevelBizObj, overrideCheckPoint, CustomsSupervisorOverridesContext.SendMessagesWithErrors);
					result.Success = SupervisorOverridesHelper.IsSupervisorApproved(supervisorOverrides, topLevelBizObj.Logs);
				}
				else
				{
					throw new DeveloperNotificationException("Top Level business object should be 'EnterpriseBusinessObject' to allow support of Logs for Supervisor Overrides");
				}

				if (!result.Success)
				{
					result.Notifications.AddError(Res.GetString("MessagingProcess|SendWithErrorsOverride", "Supervisor override not completed for sending messages with errors"));
				}
			}

			return result;
		}
		#endregion

		#region ShowCreditAndDPSCheckOverride
		ActionStep ISendMessagesGuiActionProvider.ShowCreditAndDPSCheckOverride => ShowCreditAndDPSCheckOverride;
		ActionResult ShowCreditAndDPSCheckOverride(ActionResult previousResult)
		{
			var result = previousResult;

			if (result.PassThroughData is CreditCheckResult creditCheckResult && !creditCheckResult.IsAllowedToProceed &&
				messagingGui.MessagingSupporter.Provider.GetCreditAndDPSCheckSupporter() is ISupportCreditAndDPSCheck checkSupporter)
			{
				CreditCheckAndDPSHelper.ProcessOverride(creditCheckResult, checkSupporter.DocumentDeliveryObject, checkSupporter.CreditRestrictionMessageCaption);

				result.Success = creditCheckResult.IsAllowedToProceed;
				if (!result.Success)
				{
					result.Notifications.AddError(creditCheckResult.Message);
				}
			}

			return result;
		}
		#endregion

		#region ShowPreviewDialog
		ActionStep ISendMessagesGuiActionProvider.ShowPreviewDialog => ShowPreviewDialog;
		ActionResult ShowPreviewDialog(ActionResult previousResult)
		{
			var result = previousResult;
			var supporter = messagingGui.GetPreviewDialogSupporter();

			if (supporter != null)
			{
				result = ShowOkCancelDialog(supporter.GetPreviewDialog(result), result, supporter.PreviewDialogCancelledMessage ?? CustomsMessagingGuiExtensions.PreviewDialogCancelledMessage);
			}

			return result;
		}
		#endregion

		#region ShowResultNotifications
		ActionStep ISendMessagesGuiActionProvider.ShowResultNotifications => ShowResultNotifications;
		ActionResult ShowResultNotifications(ActionResult previousResult)
		{
			var result = previousResult;
			if (result.Notifications?.Any() ?? false)
			{
				CustomsMessagingGuiExtensions.ShowSummaryNotification(result.Notifications, System.Windows.Forms.MessageBoxButtons.OK, Res.GetString("CustomsMessagingGUI|SendMessages", "Sending Messages"), "", "", true);
			}

			return result;
		}
		#endregion

		#region ConfigureProcess
		void ISendMessagesGuiActionProvider.ConfigureProcess(ActionChain sendChain)
		{
			messagingGui.ConfigureProcess(sendChain);
		}
		#endregion

		public static ActionResult ShowOkCancelDialog(IDialog dialog, ActionResult previousResult, string cancelMessage)
		{
			var result = previousResult;
			if (dialog != null)
			{
				if (!dialog.TypeOfForm.IsSubclassOf(typeof(ZChildForm)))
				{
					throw new DeveloperNotificationException("Dialogs for Messaging Process should always be ZChildForms");
				}
				result.DataSource = dialog.DataSource;

				using (var form = (ZChildForm)Activator.CreateInstance(dialog.TypeOfForm, dialog.DataSource))
				{
					if (form != null)
					{
						var okToContinue = ZFormModaliser.ShowDialogWithoutDispose(form) == System.Windows.Forms.DialogResult.OK;
						result.Success = okToContinue;

						if (!okToContinue)
						{
							result.AppendErrorNotification(cancelMessage);
						}
					}
				}
			}

			return result;
		}
	}
}
