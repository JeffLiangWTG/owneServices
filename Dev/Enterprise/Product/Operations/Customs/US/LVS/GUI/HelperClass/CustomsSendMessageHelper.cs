using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using MessageSendingValidation = Enterprise.Customs.US.LVS.Business.MessageSendingValidation;
using SupervisorOverrides = Enterprise.Customs.US.LVS.Business.SupervisorOverrides;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.GUI
{
	public static class CustomsSendMessageHelper
	{
		public static void InitMessagingAction(IEnumerable<CusUSLVConsignment> consignments, UpdateActionCode updateAction)
		{
			consignments.ForEach(consignment => consignment.InitAction(updateAction));
		}

		public static List<CusUSLVConsignmentForMessaging> PrepareMessagesToSend(IEnumerable<CusUSLVConsignment> consignments, UpdateActionCode updateAction, bool excludeInvalidConsignments = true)
		{
			var billsToSend = GetValidBillsToSend(consignments, excludeInvalidConsignments);

			if (!billsToSend.Any())
			{
				return new List<CusUSLVConsignmentForMessaging>();
			}
			else
			{
				if (IgnoreAndSendInvalidConsignmentStatus(billsToSend, updateAction))
				{
					billsToSend = PromptUserWhichConsignmentWithMessageErrorsToSend(billsToSend, updateAction);

					return billsToSend.ToList();
				}

				return new List<CusUSLVConsignmentForMessaging>();
			}
		}

		public static ZString AllocateEntryNumbers(CusUSLVClearance clearance, IEnumerable<CusUSLVConsignmentForMessaging> billsToSend)
		{
			var billsWithNoEntryNumber = billsToSend.Where(x => x.Consignment.CE_EntryNum.IsEmpty);
			if (billsWithNoEntryNumber.Any())
			{
				var reasonForUnableToAllocateEntryNumber = billsWithNoEntryNumber.ToList().AllocateEntryNumbersForConsignments(clearance);
				if (!reasonForUnableToAllocateEntryNumber.IsEmpty)
				{
					Globals.Message.ShowWarning(reasonForUnableToAllocateEntryNumber);
					return reasonForUnableToAllocateEntryNumber;
				}
			}

			return ZString.Empty;
		}

		public static int SendToCustoms(CusUSLVClearance clearance, IEnumerable<CusUSLVConsignmentForMessaging> billsToSend, UpdateActionCode updateAction)
		{
			var billsActuallySent = 0;

			var billsWithEntryNumber = billsToSend.Where(x => !x.Consignment.CE_EntryNum.IsEmpty);
			if (billsWithEntryNumber.Any() && CustomsSendMessageHelper.ContinueWithNotifications(billsToSend))
			{
				if (!clearance.IsSendCustomsMessageMutexLocked)
				{
					billsActuallySent = new Business.MessageManager(clearance).SubmitToCustoms(billsWithEntryNumber, updateAction);
				}
				else
				{
					var mutexLockByInfo = clearance.GetSendCustomsMessageMutexLockByInfo();
					if (!string.IsNullOrEmpty(mutexLockByInfo))
					{
						Globals.Message.ShowWarning(MutexLockErrorMessage(mutexLockByInfo));
					}
				}
			}

			return billsActuallySent;
		}

		public static bool USLVClearanceImportMessagingIsAllowed()
		{
			if (!Env.Security.USLVClearanceImportMessaging.IsAllowed)
			{
				Env.Security.USLVClearanceImportMessaging.ShowError();
				return false;
			}

			return true;
		}

		public static ZString GetDialogBoxCaption(UpdateActionCode updateAction)
		{
			switch (updateAction)
			{
				case UpdateActionCode.Add:
					return SendOriginalMessageCaption;
				case UpdateActionCode.Replace:
					return SendReplacementMessageCaption;
				case UpdateActionCode.Delete:
					return SendDeletionMessageCaption;
				default:
					return ZString.Empty;
			}
		}

		public static ZString GetIgnoreStatusCheckMessage(UpdateActionCode updateAction)
		{
			switch (updateAction)
			{
				case UpdateActionCode.Add:
					return SendOriginalMessageIgnoreStatusCheck;
				case UpdateActionCode.Replace:
					return SendReplacementMessageIgnoreStatusCheck;
				default:
					return ZString.Empty;
			}
		}

		static IEnumerable<CusUSLVConsignmentForMessaging> GetValidBillsToSend(IEnumerable<CusUSLVConsignment> consignments, bool excludeInvalidConsignments)
		{
			var bills = consignments.OfType<CusUSLVConsignment>()
				.Where(x => x.ULB_IsActive
					&& MessageSendingHelper.IsConsignmentNotWaitingForResponse(x)
					&& !x.Shipment.IsSendCustomsMessageMutexLocked
					&& !MessageSendingHelper.IsConsignmentPendingForSendOriginalMessage(x));

			if (excludeInvalidConsignments)
			{
				bills = bills.Where(c => MessageSendingHelper.HasValidStatusOnConsignment(c));
			}

			return bills.Select(c => new CusUSLVConsignmentForMessaging(c)).ToList();
		}

		static bool IgnoreAndSendInvalidConsignmentStatus(IEnumerable<CusUSLVConsignmentForMessaging> billsToSend, UpdateActionCode updateAction)
		{
			if (billsToSend.Any(x => !MessageSendingHelper.HasValidStatusOnConsignment(x.Consignment)))
			{
				var ignoreStatusCheckDialogResult = Globals.Message.Show(GetIgnoreStatusCheckMessage(updateAction), GetDialogBoxCaption(updateAction), MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
				if (ignoreStatusCheckDialogResult == DialogResult.No)
				{
					return false;
				}
			}

			return true;
		}

		static IEnumerable<CusUSLVConsignmentForMessaging> PromptUserWhichConsignmentWithMessageErrorsToSend(IEnumerable<CusUSLVConsignmentForMessaging> billsToSend, UpdateActionCode updateAction)
		{
			MessageSendingHelper.ValidateConsignmentsIncludingChildren(billsToSend.Select(x => x.Consignment));
			var firstBillWithErrors = MessageSendingHelper.GetFirstConsignmentHasMessageErrors(billsToSend);
			var billsWithoutMessageErrors = MessageSendingHelper.GetConsignmentsWithoutMessageErrors(billsToSend);
			if (firstBillWithErrors != null)
			{
				if (!Env.Security.USLVClearanceSendWithMessageErrors.IsAllowed)
				{
					Globals.Message.ShowInformation(SendMessageErrorsExistWithNoSecurityRight);
					return new List<CusUSLVConsignmentForMessaging>();
				}
				else
				{
					var ignoreMessageError = IgnoreErrorMessageCaption(DialogResultCaptions.GetTextForDialogResult(DialogResult.Yes), DialogResultCaptions.GetTextForDialogResult(DialogResult.No), DialogResultCaptions.GetTextForDialogResult(DialogResult.Cancel));
					var ignoreMessageErrorDialogResult = Globals.Message.Show(ignoreMessageError, GetDialogBoxCaption(updateAction), MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
					if (ignoreMessageErrorDialogResult == DialogResult.Cancel)
					{
						return new List<CusUSLVConsignmentForMessaging>();
					}
					else if (ignoreMessageErrorDialogResult == DialogResult.No)
					{
						billsToSend = billsWithoutMessageErrors.ToList();
					}
				}
			}

			if (firstBillWithErrors != null && !Env.Security.AllowMessageErrors.IsAllowed && !IsSupervisorApprovedSendingWithMessageErrors(firstBillWithErrors.Consignment, firstBillWithErrors.Consignment.Logs))
			{
				return new List<CusUSLVConsignmentForMessaging>();
			}

			return billsToSend;
		}

		public static bool IsSupervisorApprovedSendingWithMessageErrors(IBusiness businessEntity, Logs logs)
		{
			var supervisorOverrides = new SupervisorOverrides(businessEntity, Customs.Business.SupervisorOverridesContext.SendingMessages);
			return SupervisorOverridesHelper.IsSupervisorApproved(supervisorOverrides, logs);
		}

		public static bool ContinueWithNotifications(IEnumerable<CusUSLVConsignmentForMessaging> consignments)
		{
			bool result = false;
			var notifications = new MessageSendingNotificationCollection();
			consignments.ForEach(x => notifications.AddRange(MessageSendingValidation.New(x.Consignment).CheckBusinessObjectLevelValidation(string.Format("House Bill: {0}", x.HouseBill), string.Format("House Bill: {0}", x.HouseBill), string.Empty)));
			if (notifications.ContainsError())
			{
				Globals.Message.ShowError(MessageSendingValidation.ErrorExistHeaderText + "\n" + notifications.NotificationsAsString());
			}
			else if (!notifications.ContainsWarning()
				|| Globals.Message.Show(MessageSendingValidation.MessageErrorsExistHeaderText + "\n" + notifications.NotificationsAsString() + "\n" + MessageSendingValidation.MessageErrorConfirmationQuestionText, "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				result = true;
			}

			return result;
		}

		static ZString MutexLockErrorMessage(string mutexLockByInfo) => ResString.GetMultilingualString("9a083dc2-9fc3-4596-805b-b7477c18a7a4", "{0} is sending messages for this Low Value Entries job, please try again later.", mutexLockByInfo);

		static ZString IgnoreErrorMessageCaption(string yesDialog, string noDialog, string cancelDialog) => ResString.GetMultilingualString("f0db3b9a-f683-42d6-bee3-b5a02da90945", @"It is likely that your message(s) will be rejected by Customs, as one or more Consignments contain message errors.
Select {0} to send the message(s) despite these errors, {1} to send all messages without errors or {2} to return to the Bill menu.", yesDialog, noDialog, cancelDialog);

		static readonly MultilingualString SendMessageErrorsExistWithNoSecurityRight = ResString.GetMultilingualString("c07edb20-7796-422e-9909-02379a694317", "There are message errors on the jobs and you don't have security rights to send with message errors.");
		static readonly MultilingualString SendOriginalMessageCaption = ResString.GetMultilingualString("beb50288-1e22-4c5e-add3-ec09c67abc5f", "Send Original Messages");
		static readonly MultilingualString SendReplacementMessageCaption = ResString.GetMultilingualString("eae0947e-ebe6-4a9e-8f6a-6ddcc55b9468", "Send Replacement Messages");
		static readonly MultilingualString SendDeletionMessageCaption = ResString.GetMultilingualString("99caf929-f89e-4806-88a9-10a89878bdec", "Send Deletion Messages");
		static readonly MultilingualString SendOriginalMessageIgnoreStatusCheck = ResString.GetMultilingualString("54778340-3dfa-4053-821c-7c8d34753a7e", @"ACE Cargo Release message has already been accepted by customs for one of the selected Consignments. You should use the Send Replacement/Update Messages option to make ACE Cargo Release changes for those consignments.
Would you like to continue?");
		static readonly MultilingualString SendReplacementMessageIgnoreStatusCheck = ResString.GetMultilingualString("6b829b45-5e3c-4329-a36f-1b8214b8cdc2", @"There is no ACE Cargo Release acceptance message on file for one of the selected Consignments. A replace will be rejected if it was never accepted at ABI.
Would you like to continue?");
	}
}
