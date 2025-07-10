using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.JobDeclarationExtensions;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using BondedWarehousingHelper = Enterprise.Customs.US.Business.BondedWarehousingHelper;
using MessageSender = Enterprise.Customs.Business.MessageSender;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.GUI
{
	sealed class MessageSenderController
	{
		public MessageSenderController(JobDeclaration declaration, Lazy<ZForm> mainForm, Func<ImportMessageSendingActionCollection, bool> showSendingActionFormFunc)
		{
			this.declaration = declaration;
			this.mainForm = mainForm;
			this.showSendingActionFormFunc = showSendingActionFormFunc;
		}

		readonly JobDeclaration declaration;
		readonly Lazy<ZForm> mainForm;
		readonly Func<ImportMessageSendingActionCollection, bool> showSendingActionFormFunc;

		#region Send Message

		public void SendMessages<T>(ImportMessageSendingMessageType originalAmendmentOrWithdraw, ImportMessageSendingMessageTypeAdditionalFilter additionalFilter = ImportMessageSendingMessageTypeAdditionalFilter.None) where T : MainMessageSender
		{
			if (!Env.Security.ImportMessaging.IsAllowed)
			{
				Env.Security.ImportMessaging.ShowError();
			}
			else
			{
				if (eBondLogger.ShouldAddAutoSendLog(declaration) && eBondLogger.GetATHlogs(declaration).Any() && (originalAmendmentOrWithdraw == ImportMessageSendingMessageType.Original || originalAmendmentOrWithdraw == ImportMessageSendingMessageType.Replacement))
				{
					Globals.Message.ShowWarning(Res.GetString("4428875C-1278-4895-A745-B6A068FA4F38", "You have already indicated to send a message automatically when a bond is known to be added. Please wait until an eBond Status message arrives or go to the Misc tab and indicate the bond status manually if a bond has been added. You will be able to send a message then."));
				}
				else
				{
					Func<PublishToUniversalResult> preMessagingAction = null;
					Action restoreToPreMessagingState = null;

					PreMessagingActionResult preMessagingActionResult = null;
					if (!eBondLogger.ShouldAddAutoSendLog(declaration))
					{
						preMessagingActionResult = declaration.PreMessagingAction(ref preMessagingAction, ref restoreToPreMessagingState, originalAmendmentOrWithdraw);
					}

					if (preMessagingActionResult == null || !preMessagingActionResult.IsBondedWarehouse || CheckHasChangesAndMergeIfRelatedEntryDoesNotExist(new[] { CusEntryHeaderMessageTypeList.Codes.EntrySummary }))
					{
						SendMessageWithBondedWarehouseAutomation<T>(originalAmendmentOrWithdraw, preMessagingActionResult, preMessagingAction, restoreToPreMessagingState, additionalFilter);
					}
				}
			}
		}

		void SendMessageWithBondedWarehouseAutomation<T>(ImportMessageSendingMessageType originalAmendmentOrWithdraw, PreMessagingActionResult preMessagingActionResult, Func<PublishToUniversalResult> preMessagingAction, Action restoreToPreMessagingState, ImportMessageSendingMessageTypeAdditionalFilter additionalFilter) where T : MainMessageSender
		{
			var needsToRestoreToPreMessagingState = false;
			try
			{
				var messageSender = CreateMessageSender<T>(declaration, originalAmendmentOrWithdraw, additionalFilter);

				messageSender.AfterATHLogAdded += AfterATHLogAdded;
				messageSender.OnPrepare += (actions =>
				{
					var isOkToContinue = showSendingActionFormFunc?.Invoke(actions) ?? true;

					if (isOkToContinue && actions.EntryHeaderActions.Any(x => x.IsEntrySummary && x.US_SendMessage) && !eBondLogger.ShouldAddAutoSendLog(declaration) && preMessagingActionResult != null)
					{
						var errorMessages = preMessagingActionResult.GetErrorMessageForCheckFieldsForBondedWarehouse();
						if (!errorMessages.IsEmpty)
						{
							declaration.MessageInitiator.NotifyUserOfAnInvalidOperation(errorMessages);
							isOkToContinue = false;
						}

						var isBondedWarehouse = preMessagingActionResult.IsBondedWarehouse;
						if (isBondedWarehouse && isOkToContinue && preMessagingAction != null)
						{
							isOkToContinue = declaration.MessageInitiator.IsPublishToUniversalTransactionOK(preMessagingAction());
							needsToRestoreToPreMessagingState = isOkToContinue;
		
							if (isOkToContinue)
							{
								UpdateWarehouseWithdrawal(declaration);
							}
						}
					}

					return isOkToContinue;
				});

				messageSender.OnAllowNotifications += ContinueWithNotifications;

				using (ZFormPostingButtonsStrategy.DeferredUpdateSaveButtonsBasedOnHasChanges(mainForm.Value))
				{
					if (messageSender.SendMessage())
					{
						needsToRestoreToPreMessagingState = false;
					}
					else
					{
						messageSender.RestoreMPFAndDutyLastCalcDateIfNeeded();
					}
				}
			}
			finally
			{
				if (needsToRestoreToPreMessagingState)
				{
					restoreToPreMessagingState?.Invoke();
				}
			}
		}

		void UpdateWarehouseWithdrawal(JobDeclaration declaration)
		{
			var enteredQty = declaration.US_QtyBeingWithdrawn;
			var isNeedUpdate = enteredQty.IsEmpty;
			if (!isNeedUpdate)
			{
				var calculatedQty = declaration.InvoiceLines.OfType<JobComInvoiceLine>().Sum(x => x.JI_InvoiceQuantity);
				if (enteredQty != calculatedQty)
				{
					var result = Globals.Message.Show(string.Format(CalculatedQtyNotMatchEnteredQty, calculatedQty, enteredQty), Res.GetString("6665E5BD-FE82-4ACF-80FD-B0CD56F575D7", "Warning"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.OK);
					isNeedUpdate = result == DialogResult.OK;
				}
			}

			if (isNeedUpdate)
			{
				((BondedWarehousingHelper)declaration.BondedWarehousingHelper).UpdateWarehouseWithdrawal();
			}
		}

		public string CalculatedQtyNotMatchEnteredQty = "Calculated Withdrawal Quantity ({0}) does not match the entered quantity ({1}), do you want the system to update this value?";

		T CreateMessageSender<T>(params object[] args) where T : MessageSender
		{
			var result = (T)Activator.CreateInstance(typeof(T), args);
			result.OnSave += () => mainForm.Value?.FireSaveButton();

			return result;
		}

		void AfterATHLogAdded()
		{
			Globals.Message.ShowInformation(Res.GetString("A8ABF56C-915E-4CD8-A8CE-515A87D4E593", "System has added a log successfully. System will send a message automatically when an eBond Status message arrives and a bond is known to be added."));
		}

		#endregion

		#region ContinueWithNotifications

		public bool ContinueWithNotifications(MessageSendingNotificationCollection notifications)
		{
			bool result = false;

			if (notifications.ContainsError())
			{
				Globals.Message.ShowError(notifications.NotificationsAsString());
			}
			else if (!notifications.ContainsWarning() || Globals.Message.Show(notifications.NotificationsAsString(), "Continue?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				var supervisorOverrides = new Business.SupervisorOverrides(declaration, Customs.US.Business.SupervisorOverridesContext.SendingMessages);

				if (Customs.GUI.SupervisorOverridesHelper.IsSupervisorApproved(supervisorOverrides, declaration.Logs))
				{
					result = true;
				}
			}

			return result;
		}

		#endregion

		#region CheckHasChangesAndMergeIfRelatedEntryDoesNotExist

		public bool CheckHasChangesAndMergeIfRelatedEntryDoesNotExist(string[] messageTypes)
		{
			bool okToContinue = true;
			var topLevelBusinessObject = (BusinessObject)declaration.Shipment ?? declaration;
			if (topLevelBusinessObject.HasChanges && Globals.Message.Show("The data must be saved.\n\nDo you want to save and proceed?", "Save Data", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.No)
			{
				okToContinue = false;
			}

			if (okToContinue && messageTypes.Length > 0 && declaration.ActiveEntryHeaders.Find(new ZQuery(CusEntryHeaderSchema.CH_MessageType, messageTypes)).Length == 0)
			{
				okToContinue = PerformMerge();
			}

			if (okToContinue && topLevelBusinessObject.HasChanges)
			{
				okToContinue = mainForm.Value?.FireSaveButton() == ContinueWithSave.Yes;
			}

			return okToContinue;
		}

		#endregion

		#region PerformMerge

		public bool PerformMerge()
		{
			if (declaration != null)
			{
				if (declaration.DoesImportEntryNumberNeedsToBeSpecified && !declaration.LockImportEntryNumberAllocationMutex)
				{
					Globals.Message.Show(CannotMergeImportEntryNumberAllocationInProgress(declaration.GetImportEntryNumberAllocationMutexLockInfo()));
					return false;
				}

				declaration.DoMerge();
			}

			return true;
		}

		public static string CannotMergeImportEntryNumberAllocationInProgress(string lockInfo)
		{
			return lockInfo + " is in the process of allocating Import Entry Number for this job; system cannot merge this data as it will result in a different Import Entry Number being allocated.\r\nPlease retry merging when the other user has finished.";
		}

		#endregion
	}
}
