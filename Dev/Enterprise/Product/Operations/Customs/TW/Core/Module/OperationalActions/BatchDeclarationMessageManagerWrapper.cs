using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Business.MessageManagers;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using CusEntryInstruction = Enterprise.Customs.TW.Business.CusEntryInstruction;
using MessageManager = Enterprise.Customs.TW.Business.MessageManagers.MessageManager;

namespace Enterprise.Customs.TW.Module.OperationalActions
{
	public class BatchDeclarationMessageManagerWrapper : DeclarationMessageManagerWrapper
	{
		public BatchDeclarationMessageManagerWrapper(JobDeclaration declaration, JobDeclarationMessageSendingObjectParent wrapper, IOperationalActionSectionLog log, IMessageNotificationCollector notificationCollector, ISendsMessagesToCustoms sender, ZString messageType, ZBool ignoreAllWarnings, ZBool suppressNotificationPopout, ZBool reMergeAndCalculate) : base(declaration, wrapper, notificationCollector, sender, messageType)
		{
			ReMergeAndCalculate = reMergeAndCalculate;
			IgnoreMessageWarnings = ignoreAllWarnings;
			LogNotificationWrapper = new OperationalActionLogAndUserNotificationWrapper(notificationCollector, notificationCollector, log, ignoreAllWarnings, suppressNotificationPopout);
		}

		OperationalActionLogAndUserNotificationWrapper LogNotificationWrapper { get; }

		ZBool ReMergeAndCalculate { get; }
		ZBool IgnoreMessageWarnings { get; }

		protected override bool RunPreSendValidation()
		{
			var result = false;
			Declaration.IsTopLevel = false;
			Wrapper.RegisterEditableChildObject(Declaration);
			var sendingValidation = MessageSendingValidation.New(Wrapper, null);
			var notifications = sendingValidation.CheckBusinessObjectLevelValidation();
			if (notifications.ContainsError())
			{
				LogNotificationWrapper.ShowError(notifications.ErrorNotificationsAsString(), MessageManager.Constants.CannotSendMessageCaption);
			}
			else if (IgnoreMessageWarnings || !notifications.ContainsWarning() || LogNotificationWrapper.ShowConfirmation(notifications.NotificationsAsString(), MessageManager.Constants.ContinueToSend))
			{
				result = true;
			}
			if (result)
			{
				result = CheckEntryAndCredential();
			}
			return result;
		}

		bool CheckEntryAndCredential()
		{
			bool result = MessageManager.CheckDeclarationNumber();
			if (result)
			{
				var header = Declaration.ActiveEntryHeaders[0];
				var entryStatus = header.CH_EntryStatus;
				var status = header.CH_Status;
				var entryInstruction = Declaration.CustomsEntryInstructions?.OfType<CusEntryInstruction>()?.FirstOrDefault();
				if (entryInstruction == null || !entryInstruction.CEI_DateForDuty.IsToday)
				{
					result = false;
					LogNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Error, OperationalActionLogAndUserNotificationWrapper.Constants.DeclarationDateIsNotToday);
				}
				else if (!entryStatus.IsEmpty && !header.IsRejectedByCustoms)
				{
					result = false;
					LogNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Error, OperationalActionLogAndUserNotificationWrapper.Constants.EntryStatusMustBeEmptyOrREJ);
				}
				else if (!status.IsEmpty && !status.StartsWith("ER", StringComparison.CurrentCulture))
				{
					result = false;
					LogNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Error, OperationalActionLogAndUserNotificationWrapper.Constants.MessageStatusMustBeEmptyOrER);
				}
				else
				{
					var credential = Declaration.GetCredential();
					if (credential == null || credential.GP_PasswordStatus != PasswordStatusList.Codes.Valid)
					{
						result = false;
						LogNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Error, OperationalActionLogAndUserNotificationWrapper.Constants.ValidCredentialIsMissing);
					}
				}
			}
			return result;
		}

		protected override MessageManager CreateMessageManagerCore()
		{
			return new MessageManager(Wrapper, LogNotificationWrapper, true);
		}

		protected override bool BeforeSendMessage()
		{
			var result = base.BeforeSendMessage();
			if (result)
			{
				result = RunReMergeAndCalculate();
			}
			return result;
		}

		bool RunReMergeAndCalculate()
		{
			bool result = true;
			if (Declaration.JE_ApplicationCode != DeclarationApplicationCodeList.Codes.Builtin)
			{
				result = false;
				LogNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Error, OperationalActionLogAndUserNotificationWrapper.Constants.TheJobDoesNotHaveBLT);
			}
			else
			{
				if (Declaration.ActiveEntryHeaders.Count <= 0 || ReMergeAndCalculate)
				{
					Declaration.DoMerge(Sender);
					Declaration.Factory.Save();
				}
			}
			return result;
		}

		protected override void AfterSendMessage(bool result)
		{
			base.AfterSendMessage(result);
			LogNotificationWrapper.NotifyFormat(OperationalActionLogErrorLevel.Informational, "{0}: {1}", Declaration.GetDeclarationIdLink(), result ? OperationalActionLogAndUserNotificationWrapper.Constants.SubmitSucceeded : OperationalActionLogAndUserNotificationWrapper.Constants.SubmitFailed);
		}
	}
}
