using System;
using CargoWise.Types;
using Enterprise.Customs.Business.JobDeclarationExtensions;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	class ACEENSAutoSendEventProcessor : AutoSendMessage
	{
		protected override CusEntryHeader GetDependentEntry(JobDeclaration declaration)
		{
			return declaration.ActiveEntryHeaders.EntrySummaryEntry;
		}

		protected override ValidationModes GetValidationMode()
		{
			return ValidationModes.EntrySummary;
		}

		protected override ZString GetMessageTypeInSubject(bool autoSendMessageSuccess)
		{
			if (autoSendMessageSuccess)
			{
				return "<br><font color='#FF0000'><b>Entry Summary has been sent successfully</b></font><br>";
			}
			else
			{
				return "<br><font color='#FF0000'><b>Entry Summary has failed to be sent</b></font><br>";
			}
		}

		protected override ZBool IsCurrentAction(EntryHeaderMessageSendingAction action)
		{
			return action.IsEntrySummary;
		}

		protected override ZBool SendMessageCore(JobDeclaration declaration, out ZString errorMessage, ImportMessageSendingActionCollection actions)
		{
			var autoSendMessageSuccess = false;
			Func<PublishToUniversalResult> preMessagingAction = null;
			Action restoreToPreMessagingState = null;
			var needsToRestoreToPreMessagingState = false;
			try
			{
				var preMessagingActionResult = declaration.PreMessagingAction(ref preMessagingAction, ref restoreToPreMessagingState, ImportMessageSendingMessageType.Original);
				errorMessage = preMessagingActionResult.GetErrorMessageForCheckFieldsForBondedWarehouse();
				if (errorMessage.IsEmpty)
				{
					var isBondedWarehouse = preMessagingActionResult.IsBondedWarehouse;
					var isContinue = true;
					if (isBondedWarehouse && preMessagingAction != null)
					{
						var universalResult = preMessagingAction();
						isContinue = declaration.MessageInitiator.IsPublishToUniversalTransactionOK(universalResult);
						if (!isContinue)
						{
							errorMessage = universalResult.ErrorMessage;
						}
						needsToRestoreToPreMessagingState = isContinue;
					}
					if (isContinue)
					{
						autoSendMessageSuccess = actions.SendMessagesWithoutSaving(declaration.MessageInitiator);
						needsToRestoreToPreMessagingState = false;
					}
				}
			}
			finally
			{
				if (needsToRestoreToPreMessagingState && restoreToPreMessagingState != null)
				{
					restoreToPreMessagingState();
				}
			}

			return autoSendMessageSuccess;
		}

		protected override ZString AdditionalValidate(CusEntryHeader entry, ImportMessageSendingMessageType messageSendingType)
		{
			var result = ZString.Empty;
			if (messageSendingType == ImportMessageSendingMessageType.Replacement)
			{
				result = !entry.CanSendWithdrawal ? CannotSendReplacementNotificationText : "";
			}
			return result;
		}

		protected override ImportMessageStatusList.MessageType MessageType
		{
			get { return ImportMessageStatusList.MessageType.EntrySummary; }
		}
	}
}
