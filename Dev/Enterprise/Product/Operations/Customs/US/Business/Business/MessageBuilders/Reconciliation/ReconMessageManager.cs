using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public class ReconMessageManager
	{
		public ReconMessageManager(ReconDeclarationIReconciliation recon, UpdateActionCode actionCode)
		{
			this.iReconciliation = recon;
			this.reconDeclarationWrapper = recon;
			this.actionCode = actionCode;
		}

		public ReconMessageManager(ACEReconMessageSendingAction action)
		{
			this.msgSendingAction = action;
			this.iReconciliation = action.ReconDeclaration;
			this.reconDeclarationWrapper = action.ReconDeclaration;
			this.actionCode = action.ActionCode;
		}

		readonly IReconciliation iReconciliation;
		readonly ReconDeclarationIReconciliation reconDeclarationWrapper;
		readonly ACEReconMessageSendingAction msgSendingAction;
		readonly UpdateActionCode actionCode;

		public void PopulateMessage()
		{
			MQEDIMessage message;
			var convertedActionCode = UpdateActionCodeConverter.ConvertToString(actionCode);
			if (reconDeclarationWrapper.ReconDeclaration.IsACE)
			{
				var aceBuilder = new ACEReconciliationMessageBuilder(convertedActionCode, iReconciliation, msgSendingAction.CertificationSignature, msgSendingAction.IsPaid);
				message = aceBuilder.Generate();

				var paymentFinalized = msgSendingAction.PaymentFinalized;
				if (msgSendingAction.IsPaymentFinalizedRequired && !paymentFinalized.IsEmpty)
				{
					reconDeclarationWrapper.US_Paid = paymentFinalized;
				}
			}
			else
			{
				var builder = new ReconciliationMessageBuilder(UpdateActionCodeConverter.ConvertToString(actionCode), iReconciliation);
				message = builder.Generate();
			}

			iReconciliation.AddMessage(message);
			CalculateStatus(message);

			if (actionCode == UpdateActionCode.Add)
			{
				iReconciliation.LogCustomsCommencedIfNeeded();
			}

			var entry = reconDeclarationWrapper.ReconDeclaration.ReconEntry.GetEntry();
			entry.PopulateEntrySubmittedDateIfRequired();

			try
			{
				iReconciliation.Factory.Save();
			}
			catch (ZSaveException e)
			{
				ZExceptionReporting.HandleSaveException(e);
			}
		}

		void CalculateStatus(MQEDIMessage message)
		{
			if (actionCode == UpdateActionCode.Add)
			{
				message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconOriginal;
			}
			else if (actionCode == UpdateActionCode.Delete)
			{
				message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconDelete;
			}
			else if (actionCode == UpdateActionCode.Replace)
			{
				message.EM_MessageSubType = EM_MessageSubTypeList.Codes.ReconReplace;
			}

			ZString status = new ReconMessageStatusCalculator().Calculate(message, ABIResponseStatus.Undefined, false);

			if (status != "")
			{
				iReconciliation.MessageStatus = status;
			}
			message.EM_SendWithMessageErrors = MessageSendingNotifications.Count > 0;
		}

		public MessageSendingNotificationCollection MessageSendingNotifications
		{
			get
			{
				if (messageSendingNotifications == null)
				{
					messageSendingNotifications = GetMessageSendingNotifications();
				}
				return messageSendingNotifications;
			}
		}
		MessageSendingNotificationCollection messageSendingNotifications;

		MessageSendingNotificationCollection GetMessageSendingNotifications()
		{
			var notifications = new MessageSendingNotificationCollection();
			var reconDeclaration = reconDeclarationWrapper.ReconDeclaration;
			if (actionCode == UpdateActionCode.Add)
			{
				if (!reconDeclaration.CanSendOriginal)
				{
					notifications.AddError("System cannot send a reconciliation Add message, as the entry has already been added.");
				}
				else if (reconDeclaration.ReconEntryNumber.IsEmpty)
				{
					reconDeclaration.ReconWrappedJobDeclaration.ReloadImportEntryNumber();
					IAllocateNumberSupporter supporter = reconDeclaration;
					if (reconDeclaration.ReconEntryNumber.IsEmpty && !supporter.LockNumberAllocationMutex)
					{
						notifications.AddError(supporter.GetNumberAllocationMutexLockInfo() + " is in the process of allocating Entry Number for this job; system cannot send the data as it will result in a different Entry Number being allocated.\r\nPlease retry sending when the other user has finished.");
					}
				}
			}
			else if (!reconDeclaration.CanSendWithdrawal)
			{
				if (reconDeclaration.US_EntryFilerCode.IsEmpty || reconDeclaration.ReconEntryNumber.IsEmpty)
				{
					notifications.AddError("Entry Filer Code and Reconciliation Entry Number are required when sending a " + actionCode + " message.");
				}
				else
				{
					notifications.AddWarning("The " + actionCode + " message you are going to send is likely to be rejected, as the entry has not been lodged at Customs yet. Proceed with sending only if the entry has been successfully lodged from a legacy system.");
				}
			}

			if (notifications.ErrorCount == 0)
			{
				MessageSendingValidation messageSendingValidation;

				messageSendingValidation = MessageSendingValidation.New(reconDeclaration, new CustomsNotificationCollector(reconDeclaration, true, false, CustomsNotificationCollector.PropertyDescriptionType.HumanReadableName).GetMessageErrors());

				string errorExistHeaderText = MessageSendingValidation.ErrorExistHeaderText;
				string messageErrorsExistHeaderText = "";
				string messageErrorConfirmationQuestionText = MessageSendingValidation.MessageErrorConfirmationQuestionText;
				if (notifications.WarningCount == 0)
				{
					messageErrorsExistHeaderText = MessageSendingValidation.MessageErrorsExistHeaderText;
				}
				var reconNotifications = messageSendingValidation.CheckBusinessObjectLevelValidation(errorExistHeaderText, messageErrorsExistHeaderText, messageErrorConfirmationQuestionText);
				if (messageSendingValidation.TopLevelBusinessObjectForValidation.HasMessageErrors && !Env.Security.USReconSendWithMessageErrors.IsAllowed)
				{
					notifications.AddError(Customs.Business.SingleMessageManager.MessageErrorsExistWithNoSecurityRight + " " + Env.Security.USReconSendWithMessageErrors.DisplayTextPathToSecurityRight);
				}
				notifications.AddRange(reconNotifications);

				if (reconDeclaration.RelatedStatement != null)
				{
					notifications.AddWarning(ValidationConstants.EntrySummary.AlreadyOnStatement("reconciliation entry"));
				}
			}

			return notifications;
		}
	}
}
