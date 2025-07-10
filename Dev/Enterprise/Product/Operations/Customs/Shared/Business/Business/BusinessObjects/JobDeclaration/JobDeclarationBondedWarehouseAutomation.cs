using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.JobDeclarationExtensions;
using Enterprise.Customs.Business.MessagingProcess.Declaration;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.Business
{
	public enum InventoryAutomationAction { Inward, Outward, ChangeOfOwnership, ChangeOfRegime }

	internal class JobDeclarationBondedWarehouseAutomation : IJobDeclarationBondedWarehouseAutomation
	{
		public JobDeclarationBondedWarehouseAutomation(IWarehouseIntegrationSupporter supporter, MessageAction action)
		{
			this.supporter = supporter;
			messageAction = action;
		}

		readonly IWarehouseIntegrationSupporter supporter;
		readonly MessageAction messageAction;
		internal Func<PublishToUniversalResult> preAction;
		internal Action restoreToPreState;
		internal bool needsToRestore;
		bool isBondedWarehouse;

		public static bool SendMessageWithBondedWarehouseAutomation(IWarehouseIntegrationSupporter supporter, Func<ISendsMessagesToCustoms> messageInitiator, InventoryAutomationAction inventoryAutomationAction, Func<bool> sendMessage, MessageAction action, Action saveFactory, bool additionalBondedWarehouseRequirement, bool reportHasChanges)
		{
			return new JobDeclarationBondedWarehouseAutomation(supporter, action).SendMessageWithBondedWhsAutomation(messageInitiator, inventoryAutomationAction, sendMessage, saveFactory, additionalBondedWarehouseRequirement, reportHasChanges);
		}

		bool SendMessageWithBondedWhsAutomation(Func<ISendsMessagesToCustoms> messageInitiator, InventoryAutomationAction inventoryAutomationAction, Func<bool> sendMessage, Action saveFactory, bool additionalBondedWarehouseRequirement, bool reportHasChanges)
		{
			if (reportHasChanges && supporter.HasChanges)
			{
				ErrorReporter.ReportOnce("SendMessageWithBondedWarehouseAutomation should not be called with a supporter that has changes.");
			}

			if (ShouldProcessWarehouse())
			{
				var whsStatus = supporter.WarehouseTransactionStatus;
				var pendingMessageError = GetPendingTransactionMessageError(whsStatus);

				if (!pendingMessageError.IsEmpty)
				{
					messageInitiator().NotifyUserOfAnInvalidOperation(pendingMessageError);
					return false;
				}

				SetupActions(whsStatus, inventoryAutomationAction, additionalBondedWarehouseRequirement);
			}
			return SendMessageWithBondedWarehouseAutomation(messageInitiator, sendMessage, isBondedWarehouse, preAction, restoreToPreState, saveFactory);
		}

		ZString GetPendingTransactionMessageError(ZString whsStatus)
		{
			var pendingMessageError = ZString.Empty;

			var isAmendmentAndInwardOrOutwardFirstTryPending = supporter.SupportModificationState && messageAction == MessageAction.Amendment && supporter.IsInwardOrOutwardFirstTryPending();
			if (!isAmendmentAndInwardOrOutwardFirstTryPending)
			{
				pendingMessageError = WarehouseTransactionStatusList.GetErrorMessageIfPending(whsStatus);
			}

			return pendingMessageError;
		}

		void SetupActions(ZString whsStatus, InventoryAutomationAction inventoryAutomationAction, bool additionalBondedWarehouseRequirement)
		{
			var hasWHSTransaction = WarehouseTransactionStatusList.HasWHSTransaction(whsStatus);
			var isAWithdrawal = messageAction == MessageAction.Withdrawal;

			switch (inventoryAutomationAction)
			{
				case InventoryAutomationAction.ChangeOfRegime:
					if (isAWithdrawal)
					{
						isBondedWarehouse = hasWHSTransaction;
						preAction = () => supporter.PublishShipmentForWHSChangeOfRegimeFromLatestClearedDataAndSaveIfNeeded(true);
						restoreToPreState = () => supporter.PublishShipmentForWHSChangeOfRegimeFromLatestClearedDataAndSaveIfNeeded(false);
					}
					else
					{
						isBondedWarehouse = additionalBondedWarehouseRequirement && (hasWHSTransaction || supporter.IsChangeOfRegimeWarehousingEnabled);
						if (isBondedWarehouse)
						{
							if (hasWHSTransaction)
							{
								preAction = supporter.PublishShipmentForWHSChangeOfRegimeWithPreAmendmentData;
								restoreToPreState = supporter.RestoreLatestClearedBondedWarehouseChangeOfRegimeInADifferentFactory;
							}
							else
							{
								preAction = () => supporter.PublishShipmentForWHSChangeOfRegime(true);
								restoreToPreState = supporter.PublishCancelEventForWHSChangeOfRegimeInADifferentFactory;
							}
						}
					}
					break;

				case InventoryAutomationAction.ChangeOfOwnership:
					if (isAWithdrawal)
					{
						isBondedWarehouse = hasWHSTransaction;
						preAction = () => supporter.PublishShipmentForWHSChangeOfOwnershipFromLatestClearedDataAndSaveIfNeeded(true);
						restoreToPreState = () => supporter.PublishShipmentForWHSChangeOfOwnershipFromLatestClearedDataAndSaveIfNeeded(false);
					}
					else
					{
						isBondedWarehouse = additionalBondedWarehouseRequirement && (hasWHSTransaction || supporter.IsChangeOfOwnershipBondedWarehousingEnabled);
						if (isBondedWarehouse)
						{
							if (hasWHSTransaction)
							{
								preAction = supporter.PublishShipmentForWHSChangeOfOwnershipWithPreAmendmentData;
								restoreToPreState = supporter.RestoreLatestClearedBondedWarehouseChangeOfOwnershipInADifferentFactory;
							}
							else
							{
								preAction = () => supporter.PublishShipmentForWHSChangeOfOwnership(true);
								restoreToPreState = supporter.PublishCancelEventForWHSChangeOfOwnershipInADifferentFactory;
							}
						}
					}
					break;

				case InventoryAutomationAction.Outward:
					if (isAWithdrawal)
					{
						isBondedWarehouse = hasWHSTransaction;
						preAction = () => supporter.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
						restoreToPreState = supporter.PublishAcceptEventForWHSOutwardInADifferentFactory;
					}
					else
					{
						isBondedWarehouse = additionalBondedWarehouseRequirement && (hasWHSTransaction || supporter.IsOutwardBondedWarehousingEnabled);
						if (isBondedWarehouse)
						{
							var hasWHSTransactionAndNotCreatedPending = supporter.HasWHSOutwardTransactionAndNotCreatedPending();
							if (hasWHSTransactionAndNotCreatedPending)
							{
								preAction = supporter.PublishShipmentForWHSOutwardWithPreAmendmentData;
								restoreToPreState = supporter.RestoreLatestClearedBondedWarehouseOutwardInADifferentFactory;
							}
							else
							{
								preAction = () => supporter.PublishShipmentForWHSOutward(true);
								restoreToPreState = supporter.CancelPublishShipmentForWHSOutward;
							}
						}
					}
					break;

				case InventoryAutomationAction.Inward:
					if (isAWithdrawal)
					{
						isBondedWarehouse = hasWHSTransaction;
						preAction = () => supporter.PublishCancelEventForWHSInwardAndSaveIfNeeded(true, Enterprise.Customs.Business.WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal);
						restoreToPreState = supporter.RestoreLatestClearedBondedWarehouseInwardInADifferentFactory;
					}
					else
					{
						isBondedWarehouse = additionalBondedWarehouseRequirement && (hasWHSTransaction || supporter.IsInwardBondedWarehousingEnabled);
						if (isBondedWarehouse)
						{
							preAction = () => supporter.PublishShipmentForWHSInward(true);
							restoreToPreState = supporter.RestoreBondedWarehouseInwardInADifferentFactory;
						}
					}
					break;
			}
		}

		bool SendMessageWithBondedWarehouseAutomation(Func<ISendsMessagesToCustoms> messageInitiator, Func<bool> sendMessage, bool isBondedWarehouse, Func<PublishToUniversalResult> preAction, Action restoreToPreState, Action saveFactory)
		{
			var result = false;
			var isOKToSend = true;
			var needsToRestore = false;
			try
			{
				if (isBondedWarehouse && preAction != null)
				{
					isOKToSend = messageInitiator().IsPublishToUniversalTransactionOK(preAction());
					needsToRestore = isOKToSend;
				}
				if (isOKToSend && sendMessage())
				{
					try
					{
						needsToRestore = isBondedWarehouse;
						saveFactory?.Invoke();
						needsToRestore = false;
						result = true;
					}
					catch (ZSaveException ex)
					{
						ZExceptionReporting.HandleSaveException(ex);
					}
				}
			}
			finally
			{
				if (needsToRestore && restoreToPreState != null)
				{
					restoreToPreState();
				}
			}
			return result;
		}

		internal bool ShouldProcessWarehouse()
		{
			return supporter.IsActive &&
					!supporter.HasManualWhsUpdate &&
					!WarehouseTransactionStatusList.IsAutomationDisabled(supporter.WarehouseTransactionStatus);
		}

		ZString IJobDeclarationBondedWarehouseAutomation.GetPendingTransactionError()
		{
			var pendingMessageError = ZString.Empty;

			if (ShouldProcessWarehouse())
			{
				pendingMessageError = GetPendingTransactionMessageError(supporter.WarehouseTransactionStatus);
			}

			return pendingMessageError;
		}

		bool IJobDeclarationBondedWarehouseAutomation.PrepareForProcessing(InventoryAutomationAction inventoryAutomationAction, bool additionalBondedWarehouseRequirement)
		{
			if (ShouldProcessWarehouse())
			{
				needsToRestore = false;
				SetupActions(supporter.WarehouseTransactionStatus, inventoryAutomationAction, additionalBondedWarehouseRequirement);
			}

			return preAction != null;
		}

		bool IJobDeclarationBondedWarehouseAutomation.ExecutePreAction()
		{
			var result = true;

			if (preAction != null)
			{
				needsToRestore = true;
				result = preAction().ResultType != UniversalResult.HadErrors;
			}
			return result;
		}

		void IJobDeclarationBondedWarehouseAutomation.ExecutePostAction()
		{
		}

		void IJobDeclarationBondedWarehouseAutomation.ExecuteRestoreAction()
		{
			if (needsToRestore)
			{
				restoreToPreState?.Invoke();
				needsToRestore = false;
			}
		}
	}
}
