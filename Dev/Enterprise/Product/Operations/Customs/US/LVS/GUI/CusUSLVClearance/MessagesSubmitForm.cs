using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.GUI
{
	public partial class MessagesSubmitForm : ZChildForm
	{
		public MessagesSubmitForm(CusUSLVClearanceMessageWrapper clearanceWrapper, UpdateActionCode updateAction)
			: base(clearanceWrapper)
		{
			Text = "Low Value Entries Messages";
			UpdateAction = updateAction;
			InitializeComponent();

			AddOrRemoveAdditionalColumns();
		}
		UpdateActionCode UpdateAction { get; }

		public new CusUSLVClearanceMessageWrapper BusinessEntity => (CusUSLVClearanceMessageWrapper)base.BusinessEntity;

		void AddOrRemoveAdditionalColumns()
		{
			var deleteMessageColumns = new string[]
			{
				CusUSLVConsignmentForMessaging.Schema.ReasonCode,
				CusUSLVConsignmentForMessaging.Schema.RequiredReference,
				CusUSLVConsignmentForMessaging.Schema.ReferenceNumber,
				CusUSLVConsignmentForMessaging.Schema.FilesSubmittedToDIS,
				CusUSLVConsignmentForMessaging.Schema.DISReference,
			};

			if (ActionCodeIsDeleting)
			{
				GridCusUSLVConsignments.AddToAvailableColumns(deleteMessageColumns);
			}
			else
			{
				GridCusUSLVConsignments.RemoveFromAvailableColumns(deleteMessageColumns);
			}
		}

		bool ActionCodeIsDeleting => UpdateAction == Messaging.Business.UpdateActionCode.Delete;

		void ButtonSelectAll_Click(object sender, EventArgs e)
		{
			var consignments = BusinessEntity.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>();
			SelectDeselectAllHouseBill(consignments);
		}

		void ButtonSelectAllValid_Click(object sender, EventArgs e)
		{
			var consignments = BusinessEntity.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>().Where(c => !c.HasMessageErrors && !c.Consignment.HasMessageErrors);
			SelectDeselectAllHouseBill(consignments);
		}

		void SelectDeselectAllHouseBill(IEnumerable<CusUSLVConsignmentForMessaging> consignmentsForMessaging)
		{
			var anyDeselectedBills = consignmentsForMessaging.Any(x => !x.SendToCustoms);
			consignmentsForMessaging.ForEach(c => c.SendToCustoms = anyDeselectedBills);
		}

		protected void SendButton_Click(object sender, EventArgs e)
		{
			var consignmentsToAllocateEntryNumber = new List<CusUSLVConsignmentForMessaging>();
			var consignmentsForMessaging = BusinessEntity.CusUSLVConsignmentsToSend;
			var clearance = BusinessEntity.Clearance;
			var okToSend = true;
			var isCancelledByUser = false;

			var consignmentsToSend = consignmentsForMessaging.Cast<CusUSLVConsignmentForMessaging>().Where(x => x.SendToCustoms);
			if (!Env.Security.USLVClearanceSendWithMessageErrors.IsAllowed
				&& MessageSendingHelper.CheckHasMessageErrorsOnClearanceOrConsignment(clearance, consignmentsToSend.Select(messagingConsignment => messagingConsignment.Consignment)))
			{
				Globals.Message.ShowInformation(Customs.Business.MessageSendingValidation.MessageErrorsExistWithNoSecurityRight);
			}
			else
			{
				if (consignmentsToSend.Any())
				{
					if (consignmentsToSend.Any(c => !MessageSendingHelper.IsConsignmentNotWaitingForResponse(c.Consignment)))
					{
						var dialogResult = Globals.Message.Show(
							Res.GetString("28e7cb5d-86cc-45d5-8f2a-f7d086058bb2", @"There are consignments that have been sent to customs but have not yet received a response, do you want to resend them?
Click {0} to send the entire selection or {1} to only send the messages that have not already been sent.",
								DialogResultCaptions.GetTextForDialogResult(DialogResult.Yes),
								DialogResultCaptions.GetTextForDialogResult(DialogResult.No)),
							"Warning",
							MessageBoxButtons.YesNoCancel,
							MessageBoxIcon.Warning);

						if (dialogResult == DialogResult.No)
						{
							consignmentsToSend.Where(c => !MessageSendingHelper.IsConsignmentNotWaitingForResponse(c.Consignment)).ForEach(c => c.SendToCustoms = false);
							isCancelledByUser = true;
						}
						else if (dialogResult == DialogResult.Cancel)
						{
							return;
						}
					}

					if (consignmentsToSend.Any(c => MessageSendingHelper.IsConsignmentPendingForSendOriginalMessage(c.Consignment)))
					{
						var dialogResult = Globals.Message.Show(
							Res.GetString("73c7b560-8828-4c4f-a6bd-0479ec4fb816", @"There are consignments that have an original message queued to be sent to Customs, do you want these resent?
Click {0} to send the entire selection or {1} to only send the messages that have not already been queued.",
								DialogResultCaptions.GetTextForDialogResult(DialogResult.Yes),
								DialogResultCaptions.GetTextForDialogResult(DialogResult.No)),
							"Warning",
							MessageBoxButtons.YesNoCancel,
							MessageBoxIcon.Warning);

						if (dialogResult == DialogResult.No)
						{
							consignmentsToSend.Where(c => MessageSendingHelper.IsConsignmentPendingForSendOriginalMessage(c.Consignment)).ForEach(c => c.SendToCustoms = false);
							isCancelledByUser = true;
						}
						else if (dialogResult == DialogResult.Cancel)
						{
							return;
						}
					}

					foreach (var consignmentForMessaging in consignmentsToSend)
					{
						var consignment = consignmentForMessaging.Consignment;

						if (UpdateAction == Messaging.Business.UpdateActionCode.Add && string.IsNullOrEmpty(consignment.CE_EntryNum))
						{
							consignmentsToAllocateEntryNumber.Add(consignmentForMessaging);
						}
					}
				}

				if (consignmentsToSend.Any())
				{
					if (!Env.Security.AllowMessageErrors.IsAllowed)
					{
						okToSend = CustomsSendMessageHelper.IsSupervisorApprovedSendingWithMessageErrors(BusinessEntity, clearance.Logs);
					}

					if (okToSend)
					{
						okToSend = CustomsSendMessageHelper.ContinueWithNotifications(consignmentsToSend);
					}

					if (okToSend)
					{
						var reasonForUnableToAllocateEntryNumber = consignmentsToAllocateEntryNumber.AllocateEntryNumbersForConsignments(clearance);
						if (!reasonForUnableToAllocateEntryNumber.IsEmpty)
						{
							Globals.Message.ShowWarning(reasonForUnableToAllocateEntryNumber);
						}
						else
						{
							if (!clearance.IsSendCustomsMessageMutexLocked)
							{
								var billsActuallySent = new MessageManager(clearance).SubmitToCustoms(consignmentsToSend, UpdateAction);

								if (UpdateAction == UpdateActionCode.Add)
								{
									Globals.Message.Show(Res.GetString("e991650d-d695-4f2d-884d-2d3f7b2e24a4", "{0} original message(s) has been queued.", billsActuallySent));
								}
								else
								{
									Globals.Message.Show(Res.GetString("f6b5b751-fc48-46ba-ab5f-842d89ec1cb5", "{0} {1} message(s) has been generated.", billsActuallySent, UpdateAction == UpdateActionCode.Delete ? "deletion" : UpdateAction == UpdateActionCode.Replace ? "replacement" : "update"));
								}

								DialogResult = DialogResult.Yes;
							}
							else
							{
								var mutexLockByInfo = clearance.GetSendCustomsMessageMutexLockByInfo();
								if (!string.IsNullOrEmpty(mutexLockByInfo))
								{
									Globals.Message.ShowWarning(Res.GetString("8c3eba0f-8e19-4d10-9266-4d311848868f", "{0} is sending messages for this Low Value Entries job, please try again later.", mutexLockByInfo));
								}
							}
						}
					}

					Close();
				}
				else
				{
					Globals.Message.ShowInformation(isCancelledByUser ? "Submission Cancelled By User" : "No Bill has been flagged for submission", "No Bill Submitted");
				}
			}
		}

		protected void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			BusinessEntity.ResetCusUSLVConsignmentsActions();
		}
	}
}
