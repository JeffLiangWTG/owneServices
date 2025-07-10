using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;

namespace Enterprise.Customs.US.DataTransfer.Universal
{
	class ACECargoReleaseAutoSendEventProcessor : AutoSendMessage
	{
		protected override CusEntryHeader GetDependentEntry(JobDeclaration declaration)
		{
			return declaration.ActiveEntryHeaders.SimplifiedEntry;
		}

		protected override ValidationModes GetValidationMode()
		{
			return ValidationModes.CargoRelease;
		}

		protected override ZString GetMessageTypeInSubject(bool autoSendMessageSuccess)
		{
			if (autoSendMessageSuccess)
			{
				return "<br><font color='#FF0000'><b>ACE Cargo Release has been sent successfully</b></font><br>";
			}
			else
			{
				return "<br><font color='#FF0000'><b>ACE Cargo Release has failed to be sent</b></font><br>";
			}
		}

		protected override ZBool IsCurrentAction(EntryHeaderMessageSendingAction action)
		{
			return action.IsACECargoRelease;
		}

		protected override ZBool SendMessageCore(JobDeclaration declaration, out ZString errorMessage, ImportMessageSendingActionCollection actions)
		{
			errorMessage = ZString.Empty;
			return actions.SendMessagesWithoutSaving(declaration.MessageInitiator);
		}

		protected override ImportMessageStatusList.MessageType MessageType
		{
			get { return ImportMessageStatusList.MessageType.ACECargoRelease; }
		}

		protected override ZString AdditionalValidate(CusEntryHeader entry, ImportMessageSendingMessageType messageSendingType)
		{
			if (messageSendingType == ImportMessageSendingMessageType.Original)
			{
				return !entry.CanSendOriginal ? CannotSendOriginalNotificationText : "";
			}
			else
			{
				return !entry.CanSendWithdrawal ? CannotSendReplacementNotificationText : "";
			}
		}
		const string CannotSendOriginalNotificationText = "The entry has has been lodged, Please use the 'Brokerage -> Send Replacement/Amendment Messages' option to send a Replace/Update Message.";
	}
}
