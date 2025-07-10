using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.MessageManagers
{
	public interface IUserNotification
	{
		string ShowQuestion(string message, string caption, int answerLength, CodeDescriptionPairList answerList, string defaultAnswer = null);
		bool ShowConfirmation(string message, string caption, string confirmationPrompt, string confirmationString);
		bool ShowConfirmation(string message, string caption, bool warning = false);
		void ShowWarning(string message, string caption);
		void ShowError(string message, string caption);
		void ShowInformation(string message, string caption);
	}

	public interface IMessageManagerEventHandler
	{
		void OnMessageQueuedForSending();
	}

	public abstract class EDIFACTMessageManager : SingleMessageManager
	{
		protected EDIFACTMessageManager(IEDIFACTMessageAttachee dataWrapper, EDIFACTMessageStatusCalculator statusCalculator, IUserNotification notification)
		{
			DataWrapper = Argument.NotNull(dataWrapper, "dataWrapper");
			StatusCalculator = statusCalculator;
			this.notification = notification;
		}

		#region Send Message

		public bool SendMessage(MessageSubTypes actionCode, bool runPreSaveValidation = true)
		{
			OnMessageSending();
			var messageSendingResult = false;
			ZString messageText;
			if (CanSendThisMessage(actionCode, out messageText))
			{
				var sendWithMessageErrors = false;
				if (DefineActionCodeIfUndefined(ref actionCode))
				{
					if (RunRationalityCheckAndAskForConfirmation(actionCode, runPreSaveValidation, out sendWithMessageErrors))
					{
						var messages = PopulateMessage(actionCode);
						if (messages.Length > 0)
						{
							if (runPreSaveValidation)
							{
								foreach (var message in messages)
								{
									message.EM_SendWithMessageErrors = sendWithMessageErrors;
								}
							}

							if (ShouldSendMessagesInTestMode)
							{
								foreach (var message in messages)
								{
									message.EM_IsTestMessage = true;
								}
							}

							RunAdditionalEDIMessageModification(messages);

							try
							{
								OnMessageQueuedForSending(actionCode);

								if (!ShouldSuspendFactorySave)
								{
									OnSavingToDatabase();
									BusinessObject.Factory.Save();
								}

								ShowQueuedForSending(actionCode);
								messageSendingResult = true;
							}
							catch (ZSaveException e)
							{
								HandleSaveException(e);
							}
						}
					}
				}
			}
			else
			{
				ActionWhenMessageCanNotSend();
				ShowMessageNotSent(actionCode, messageText);
			}
			return messageSendingResult;
		}

		protected virtual void HandleSaveException(ZSaveException e)
		{
			ZExceptionReporting.HandleSaveException(e);
		}

		protected virtual void RunAdditionalEDIMessageModification(IEnumerable<EDIMessage> messages)
		{
		}

		#region Validation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		protected virtual bool CanSendThisMessage(MessageSubTypes actionCode, out ZString messageText)
		{
			messageText = ShouldJobBeSavedBeforeSendingMessage && DataWrapper.HasChanges ? Res.GetString("2778f6aa-06e6-411e-9fa6-b1b4a1dbae7e", "Job not yet saved, Please save before sending.") : string.Empty;
			return string.IsNullOrEmpty(messageText);
		}

		protected virtual bool ShouldJobBeSavedBeforeSendingMessage
		{
			get { return true; }
		}

		bool RunBusinessObjectValidation(out bool sendWithMessageErrors)
		{
			bool shouldPopulateMessages;

			var notifications = ValidateBusinessObject();
			shouldPopulateMessages = ShouldContinueWithNotifications(notifications, notification);
			sendWithMessageErrors = notifications.ContainsWarning();
			return shouldPopulateMessages;
		}

		protected MessageSendingNotificationCollection ValidateBusinessObject()
		{
			try
			{
				OnValidationIsBeingRun();

				return MessageSendingValidation.New(BusinessObject, null, SendWithMessageErrorsSecurityCheckpoint, DataWrapper.RefreshValidationBeforeSendMessage).CheckBusinessObjectLevelValidation(ShouldSendMessagesInTestMode);
			}
			finally
			{
				OnValidationCompleted();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		protected virtual bool RunRationalityCheckAndAskForConfirmation(MessageSubTypes actionCode, bool runPreSaveValidation, out bool sendWithMessageErrors)
		{
			var result = false;
			sendWithMessageErrors = false;
			result = !IsWaitingForResponse || ShowAwaitingCustomsResponse();
			result = result && (!runPreSaveValidation || RunBusinessObjectValidation(out sendWithMessageErrors));
			return result;
		}

		#endregion

		#region DefineActionCodeAndPopulateMessage

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		protected virtual bool DefineActionCodeIfUndefined(ref MessageSubTypes actionCode)
		{
			var actionCodeDefined = actionCode != MessageSubTypes.Undefined;
			if (!actionCodeDefined && StatusCalculator != null)
			{
				actionCode = StatusCalculator.IsLodged(DataWrapper.JobStatus) ? MessageSubTypes.Change : MessageSubTypes.Create;
				actionCodeDefined = true;
			}
			return actionCodeDefined;
		}

		protected virtual EDIMessage[] PopulateMessage(MessageSubTypes actionCode)
		{
			var messages = new List<EDIMessage>();
			var builder = GetMessageBuilder(actionCode);
			foreach (var builderResult in builder.PopulateMessages().GetBuilderResults())
			{
				var message = builderResult.Message;
				DataWrapper.AddMessage(message);
				if (StatusCalculator != null)
				{
					DataWrapper.MessageStatus = StatusCalculator.GetMessageAwaitingStatus(message);
				}

				messages.Add(message);
			}
			UpdateMessageDetails(messages);
			return messages.ToArray();
		}

		void UpdateMessageDetails(IEnumerable<EDIMessage> messages)
		{
			foreach (var ediMessage in messages)
			{
				UpdateMessageDetailsCore(ediMessage);
			}
		}

		protected virtual void UpdateMessageDetailsCore(EDIMessage message)
		{
		}

		#endregion

		#region Notifications

		protected virtual bool ShowAwaitingCustomsResponse()
		{
			return notification.ShowConfirmation(AwaitingCustomsResponseMessage, WarningCaption);
		}

		protected virtual string AwaitingCustomsResponseMessage
		{
			get { return Res.GetString("EC04135D-BA1B-4D0B-814D-31A7E174C3E6", "This job is waiting for a Customs response.\r\nAre you sure that you want to resend to Customs?"); }
		}

		public static bool ShouldContinueWithNotifications(MessageSendingNotificationCollection notifications, IUserNotification notification)
		{
			var result = false;
			if (notifications.ContainsError())
			{
				notification.ShowWarning(notifications.NotificationsAsString(), WarningCaption);
			}
			else
			{
				var caption = Res.GetString("23878cd0-23e1-43df-9bf3-ab1bd267d68d", "Continue?");
				result = !notifications.ContainsWarning() || notification.ShowConfirmation(notifications.NotificationsAsString(), caption);
			}
			return result;
		}

		protected virtual void ShowQueuedForSending(MessageSubTypes actionCodeToSend)
		{
			var message = Res.GetString("5817154e-20a3-4888-86ba-c8893a82cd2a", "{0} {1} message queued for sending.", GetActionCodeDescription(actionCodeToSend), MessageFriendlyName);
			var caption = Res.GetString("1a53b0d7-c6c2-4d38-8ee9-bd0554105256", "Message queued");
			notification.ShowInformation(message, caption);
		}

		protected virtual string GetActionCodeDescription(MessageSubTypes actionCodeToSend)
		{
			return actionCodeToSend.ToString();
		}

		protected virtual void ActionWhenMessageCanNotSend() { }

		protected virtual void ShowMessageNotSent(MessageSubTypes actionCodeToSend, string messageText)
		{
			if (!string.IsNullOrEmpty(messageText))
			{
				var friendlyName = GetMessageFriendlyNameForNotSentPopup(actionCodeToSend);
				var message = Res.GetString("59d1c4b8-861f-4619-80c7-196efc692e51", "System cannot send {0}{1} message as {2}", Grammar.Instance.IndefiniteArticlePrefix(friendlyName), friendlyName, messageText);
				var caption = Res.GetString("768da99c-f0e2-4d73-aaa3-a1d5ff3acf56", "Message has not been sent");
				notification.ShowError(message, caption);
			}
		}

		protected virtual string GetMessageFriendlyNameForNotSentPopup(MessageSubTypes actionCodeToSend)
		{
			return MessageFriendlyName;
		}

		protected static string WarningCaption
		{
			get { return Res.GetString("f3299710-30cb-4e22-9cab-c7c54dee2feb", "Warning"); }
		}

		#endregion

		#region Events

		protected virtual void OnMessageSending()
		{
		}

		protected virtual void OnSavingToDatabase()
		{
		}

		protected virtual void OnMessageQueuedForSending(MessageSubTypes actionCode)
		{
			if (EventHandler != null)
			{
				EventHandler.OnMessageQueuedForSending();
			}
		}

		protected virtual void OnValidationIsBeingRun()
		{
		}

		protected virtual void OnValidationCompleted()
		{
		}

		IMessageManagerEventHandler EventHandler
		{
			get
			{
				var result = DataWrapper as IMessageManagerEventHandler;
				if (result == null && DataWrapper != null)
				{
					result = DataWrapper.TopLevelBusinessObject as IMessageManagerEventHandler;
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region Overrides

		public override bool IsWaitingForResponse
		{
			get { return StatusCalculator?.IsAwaitingReply(DataWrapper.MessageStatus) ?? false; }
		}

		public override bool CanSendWithdrawal
		{
			get { return !DataWrapper.JobStatus.IsEmpty && DataWrapper.JobStatus != EntryStatusList.Codes.Cancelled; }
		}

		public override bool CanSendOriginal
		{
			get { return true; }
		}

		protected override EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo)
		{
			return PopulateMessage(MessageSubTypes.Withdraw);
		}

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			return PopulateMessage(MessageSubTypes.Undefined);
		}

		protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			return PopulateMessage(MessageSubTypes.Undefined);
		}

		public override BusinessObject BusinessObject
		{
			get { return DataWrapper.TopLevelBusinessObject; }
		}

		#endregion

		protected abstract IMessageBuilder GetMessageBuilder(MessageSubTypes actionCode);

		protected IEDIFACTMessageAttachee DataWrapper { get; private set; }

		protected EDIFACTMessageStatusCalculator StatusCalculator { get; private set; }

		internal bool ShouldSuspendFactorySave { get; set; }

		protected readonly IUserNotification notification;
	}
}
