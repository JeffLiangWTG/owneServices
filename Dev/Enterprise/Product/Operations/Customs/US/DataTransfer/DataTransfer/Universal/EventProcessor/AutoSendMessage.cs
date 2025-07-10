using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.DataTransfer
{
	public abstract class AutoSendMessage
	{
		public ZString Process(JobDeclaration declaration, StmALog autoSendEvent)
		{
			var result = ZString.Empty;
			var entry = GetDependentEntry(declaration);
			if (entry != null && autoSendEvent != null)
			{
				var messageSendingType = autoSendEvent.SL_Reference.Contains(eBondLogger.MessageSendTypeOriginal, StringComparison.CurrentCulture) ? ImportMessageSendingMessageType.Original : ImportMessageSendingMessageType.Replacement;
				var shouldSendWithMessageError = autoSendEvent.SL_Reference.EndsWith(eBondLogger.SendWithMessageError, StringComparison.CurrentCulture);

				var notificationText = AdditionalValidate(entry, messageSendingType);
				notificationText = notificationText.IsEmpty ? ValidateBeforeSendMessage(declaration, GetValidationMode(), shouldSendWithMessageError) : ZString.Empty;

				var autoSendMessageSuccess = false;
				if (notificationText.IsEmpty
#if DEBUG
					|| SetSendWithMessageErrorForTest
#endif
					)
				{
					var actions = new ImportMessageSendingActionCollection(declaration, messageSendingType);
					var entryHeaderActions = actions.EntryHeaderActions;

					var messageSendAction = entryHeaderActions.FirstOrDefault(x => IsCurrentAction(x));
					if (messageSendAction == null)
					{
						messageSendAction = new EntryHeaderMessageSendingAction(entry, MessageType, actions);
						actions.Add(messageSendAction);
					}

					var messageSendingSetting = new AutoSendMessageCusAddInfo.Loader(declaration.Factory).Load(entry);

					if (messageSendingSetting != null)
					{
						DeserializeMessageSendingAction(messageSendAction, entry, messageSendingSetting);

						actions.Cast<ImportMessageSendingAction>().ForEach(x => x.US_SendMessage = x.PK == messageSendAction.PK);

						var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
						declaration.MessageInitiator = messageInitiator;
						autoSendMessageSuccess = SendMessageCore(declaration, out notificationText, actions);

						if (autoSendMessageSuccess)
						{
							autoSendEvent.Cancel();
							messageSendingSetting.Delete();
						}
					}
					else
					{
						notificationText += "There is no message send action setting record found for this entry. System cannot proceed. Please open the declaration and send a message manually.";
					}
				}

				result = GetMessageTypeInSubject(autoSendMessageSuccess);
				if (!autoSendMessageSuccess)
				{
					result += string.Format(CultureInfo.CurrentCulture, "Please fix the following errors or message errors then send a message manually.\r\n{0}", notificationText);
				}
			}
			return result;
		}

		protected const string CannotSendReplacementNotificationText = "There is no entries to send a replacement messsage for.";

		static void DeserializeMessageSendingAction(EntryHeaderMessageSendingAction action, CusEntryHeader entry, AutoSendMessageCusAddInfo messageSendingSetting)
		{
			action.Deserialize(messageSendingSetting.B7_AddInfoData);
			var explanation = entry.PSCExplanation;
			if (explanation != null)
			{
				action.US_PSCExplanation = explanation.B7_AddInfoData;
			}
		}

		ZString ValidateBeforeSendMessage(JobDeclaration declaration, ValidationModes validationModes, bool shouldSendWithMessageError)
		{
			var result = ZString.Empty;
			declaration.ValidationModes = validationModes;
			declaration.LoadChildEditableObjects();
			declaration.RunPreSaveValidation();

			if (declaration.HasErrors)
			{
				result = declaration.GetErrors().ToUniqueMessageListString();
			}

			if (declaration.HasMessageErrors && !shouldSendWithMessageError)
			{
				result += (!result.IsEmpty ? "\r\n" : string.Empty) + declaration.GetMessageErrors().ToUniqueMessageListString();
			}
			return result;
		}

		protected abstract CusEntryHeader GetDependentEntry(JobDeclaration declaration);
		protected abstract ValidationModes GetValidationMode();
		protected abstract ZString GetMessageTypeInSubject(bool autoSendMessageSuccess);
		protected abstract ZBool IsCurrentAction(EntryHeaderMessageSendingAction action);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		protected abstract ZBool SendMessageCore(JobDeclaration declaration, out ZString errorMessage, ImportMessageSendingActionCollection actions);

		protected virtual ZString AdditionalValidate(CusEntryHeader entry, ImportMessageSendingMessageType messageSendingType)
		{
			return ZString.Empty;
		}

		protected abstract ImportMessageStatusList.MessageType MessageType { get; }

#if DEBUG
		bool SetSendWithMessageErrorForTest
		{
			get { return true; }
		}
#endif
	}
}
