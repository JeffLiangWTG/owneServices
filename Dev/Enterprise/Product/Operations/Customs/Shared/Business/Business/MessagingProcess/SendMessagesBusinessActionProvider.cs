using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public sealed class SendMessagesBusinessActionProvider : ISendMessagesBusinessActionProvider
	{
		public SendMessagesBusinessActionProvider(ICustomsMessagingSupporter messagingSupporter)
		{
			this.messagingSupporter = Argument.NotNull(messagingSupporter, nameof(messagingSupporter));
		}
		readonly ICustomsMessagingSupporter messagingSupporter;

		#region SendMessagesSecurityCheckpoint
		ActionStep ISendMessagesBusinessActionProvider.SendMessagesSecurityCheckpoint => SendMessagesSecurityCheckpoint;
		ActionResult SendMessagesSecurityCheckpoint(ActionResult previousResult)
		{
			var result = previousResult;

			var checkpoint = messagingSupporter.Provider.GetSendMessagesSecurityCheckpoint();

			if (!(checkpoint?.IsAllowed ?? true))
			{
				result.Success = false;
				result.Notifications.AddError(checkpoint.ErrorMessageForNotAllowed);
			}

			return result;
		}
		#endregion

		#region PreSendValidation
		ActionStep ISendMessagesBusinessActionProvider.PreSendValidation => PreSendValidation;
		ActionResult PreSendValidation(ActionResult previousResult)
		{
			var result = previousResult;

			result.AppendNotifications(messagingSupporter.RunPreSendValidation(result));

			foreach (var messenger in messagingSupporter.Messengers)
			{
				result.AppendNotifications(messenger.RunCommonPreSendValidation());
			}

			result.UpdateSuccess();

			return result;
		}
		#endregion

		#region SendMessagesWithErrorsSecurityCheckpoint
		ActionStep ISendMessagesBusinessActionProvider.SendMessagesWithErrorsSecurityCheckpoint => SendMessagesWithErrorsSecurityCheckpoint;
		ActionResult SendMessagesWithErrorsSecurityCheckpoint(ActionResult previousResult)
		{
			var result = previousResult;

			if (result.ContainsWarning())
			{
				var checkpoint = messagingSupporter.Provider.GetSendMessagesWithErrorsSecurityCheckpoint();

				if (!(checkpoint?.IsAllowed ?? true))
				{
					result.Success = false;
					result.Notifications.AddError(checkpoint.ErrorMessageForNotAllowed);
				}
			}

			return result;
		}
		#endregion

		#region CreditAndDPSCheck
		ActionStep ISendMessagesBusinessActionProvider.CreditAndDPSCheck => CreditAndDPSCheck;
		ActionResult CreditAndDPSCheck(ActionResult previousResult)
		{
			var result = previousResult;

			if (messagingSupporter.Provider.GetCreditAndDPSCheckSupporter() is ISupportCreditAndDPSCheck checkSupporter)
			{
				result.PassThroughData = CreditCheckAndDPSHelper.RunCheck(checkSupporter.DocumentDeliveryObject, checkSupporter.MustRunCreditCheck, checkSupporter.DefaultApprovalRequestReason);
			}

			return result;
		}
		#endregion

		#region CreateMessages
		ActionStep ISendMessagesBusinessActionProvider.CreateMessages => CreateMessages;
		ActionResult CreateMessages(ActionResult previousResult)
		{
			var result = previousResult;

			result.PreviousNotifications.AddRange(result.Notifications);
			result.Notifications.Clear();

			var messages = new List<EDIMessage>();
			foreach (var messenger in messagingSupporter.Messengers)
			{
				if (messenger.ShouldCreateMessage(previousResult))
				{
					var message = messenger.CreateMessage();
					if (message != null)
					{
						message.EM_LinkedObject = messenger.Owner.MessageOwner;
						message.EM_Status = EDIMessage.Status.Captured; // Will be deleted/queued/discarded once process finalised
						message.EM_IsTestMessage = messagingSupporter.Provider.IsInTestMode;
						message.EM_SendWithMessageErrors = previousResult.PreviousNotifications.Any(x => x.IsWarning || x.IsError);
						messages.Add(message);
					}
				}
			}

			result.EDIMessages = messages;

			if (!result.EDIMessages?.Any() ?? true)
			{
				result.AppendErrorNotification(Res.GetString("89CE636B-3C64-4FA0-8355-AD8D9E18684C", "No messages were created"));
			}

			result.UpdateSuccess();

			return result;
		}
		#endregion

		#region SignMessages
		ActionStep ISendMessagesBusinessActionProvider.SignMessages => SignMessages;
		ActionResult SignMessages(ActionResult previousResult)
		{
			var result = previousResult;

			if (result.EDIMessages?.Any() ?? false)
			{
				foreach (var messenger in messagingSupporter.Messengers)
				{
					var resultMsg = messenger.SignMessages(GetActiveMessagesForMessenger(messenger, result.EDIMessages), previousResult);

					if (!string.IsNullOrEmpty(resultMsg))
					{
						result.AppendErrorNotification(resultMsg);
					}
				}
			}
			else
			{
				result.AppendErrorNotification(Res.GetString("34775008-3B5E-48F6-BC0A-E23212EEF23D", "No messages to sign"));
			}

			result.UpdateSuccess();

			return result;
		}
		#endregion

		#region PreviewDialogFailure
		ActionStep ISendMessagesBusinessActionProvider.PreviewDialogFailure => PreviewDialogFailure;
		ActionResult PreviewDialogFailure(ActionResult previousResult) => SendMessagesActionProviderHelper.DeleteMessages(previousResult);
		#endregion

		#region SignMessagesFailure
		ActionStep ISendMessagesBusinessActionProvider.SignMessagesFailure => SignMessagesFailure;
		ActionResult SignMessagesFailure(ActionResult previousResult) => SendMessagesActionProviderHelper.DeleteMessages(previousResult);
		#endregion

		#region UpdateStatues
		ActionStep ISendMessagesBusinessActionProvider.ProcessUpdates => ProcessUpdates;
		ActionResult ProcessUpdates(ActionResult previousResult)
		{
			var result = previousResult;

			var hooked = new List<(EDIMessage, ICustomsMessenger)>();

			try
			{
				foreach (var messenger in messagingSupporter.Messengers)
				{
					var success = messenger.ProcessUpdates(result);

					var setupHandler = success && messenger.HasMessagePlaceholderGenerator();

					foreach (var msg in GetActiveMessagesForMessenger(messenger, result?.EDIMessages))
					{
						if (success)
						{
							messenger.ProcessCommonUpdates(msg);
						}

						msg.EM_Status = success ?
							EDIMessage.Status.Queued :
							EDIMessage.Status.Discarded;

						if (setupHandler)
						{
							msg.Saving += messenger.OnEDIMessageSaving;
							hooked.Add((msg, messenger));
						}
					}

					result.Success = result.Success && success;
				}

				messagingSupporter.TopLevelBusinessObject.Factory.Save();
			}
			catch (Exception ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				result.Success = false;
				result.AppendErrorNotification(Res.GetString("7CACE623-0A18-4E97-8707-C25B1AB0F9DD", "An error was encountered while saving changes."));
			}
			finally
			{
				foreach (var (msg, messenger) in hooked)
				{
					msg.Saving -= messenger.OnEDIMessageSaving;
				}
			}

			if (result.Success)
			{
				var msgCount = result.EDIMessages?.Where(x => x.EM_Status == EDIMessage.Status.Queued).Count() ?? 0;
				result.AppendInformationNotification(Res.GetString("BF51BF6C-7B08-4802-B7A3-0B081DFDF94C", "{0} Message(s) queued for sending", msgCount));
			}

			return result;
		}
		#endregion

		#region RefreshBusinessObjects
		ActionStep ISendMessagesBusinessActionProvider.RefreshBusinessObjects => RefreshBusinessObjects;
		ActionResult RefreshBusinessObjects(ActionResult previousResult)
		{
			var parentOwner = messagingSupporter.TopLevelBusinessObject as IEDIMessageCollectionOwner;

			messagingSupporter.Messengers.ForEach(x =>
			{
				var owner = x.Owner;
				if (owner?.Messages is IBusinessObjectCollection messages)
				{
					(messages as BusinessObjectCollection)?.Reload(false);
					messages.RefreshBindingIncludingChildren();
				}
				x.CleanUp();

				if (owner == parentOwner)
				{
					parentOwner = null;
				}
			});

			if (parentOwner?.Messages is IBusinessObjectCollection messages)
			{
				(messages as BusinessObjectCollection)?.Reload(false);
				messages.RefreshBindingIncludingChildren();
			}

			return previousResult;
		}
		#endregion

		#region ConfigureProcess
		void ISendMessagesBusinessActionProvider.ConfigureProcess(ActionChain sendChain)
		{
			messagingSupporter.Provider.ConfigureProcess(sendChain);
		}
		#endregion

		IReadOnlyCollection<EDIMessage> GetActiveMessagesForMessenger(ICustomsMessenger messenger, IReadOnlyCollection<EDIMessage> messages)
			=> messages?.Where(x => x.EM_LinkedObject == messenger.Owner.MessageOwner && !x.IsDeleted).ToList() ?? new List<EDIMessage>();
	}
}
