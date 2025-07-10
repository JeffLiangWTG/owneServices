using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.US.Business
{
	public static class JobDeclarationSetupPreMessagingExtensions
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference")]
		public static PreMessagingActionResult PreMessagingAction(this JobDeclaration declaration, ref Func<PublishToUniversalResult> preMessagingAction, ref Action restoreToPreMessagingState, ImportMessageSendingMessageType messageType, bool shouldSave = true)
		{
			bool isBondedWarehouse = false;
			Func<ZString> getErrorMessages = null;

			if (!declaration.IsBondedWarehousingDisabled)
			{
				if (declaration.IsOutwardBondedWarehousingEnabled)
				{
					isBondedWarehouse = declaration.IsWHSUniversalXMLActive && (declaration.HasWHSTransaction || declaration.IsExBondAutomationEnabled);
					if (isBondedWarehouse)
					{
						getErrorMessages = () => CheckRequiredFieldsForBondedWarehousing(declaration, messageType);
						ActionForOutward(declaration, ref preMessagingAction, ref restoreToPreMessagingState, messageType);
					}
				}
				else
				{
					isBondedWarehouse = declaration.IsWHSUniversalXMLActive && (declaration.HasWHSTransaction || declaration.IsInwardBondedWarehousingEnabled);
					if (isBondedWarehouse)
					{
						getErrorMessages = () => CheckRequiredFieldsForBondedWarehousing(declaration, messageType);
						ActionForInward(declaration, ref preMessagingAction, ref restoreToPreMessagingState, messageType, shouldSave);
					}
				}
			}
			return new PreMessagingActionResult() { IsBondedWarehouse = isBondedWarehouse, getErrorMessageForCheckFieldsForBondedWarehouse = getErrorMessages };
		}

		static ZString CheckRequiredFieldsForBondedWarehousing(this JobDeclaration declaration, ImportMessageSendingMessageType messageType)
		{
			return declaration.GetMessageErrorOfRequiredFieldsForBondedWarehousing(checkProduct: messageType == ImportMessageSendingMessageType.Original || messageType == ImportMessageSendingMessageType.Replacement,
												checkQuantity: messageType == ImportMessageSendingMessageType.Original || messageType == ImportMessageSendingMessageType.Replacement,
												checkEntryDetails: messageType == ImportMessageSendingMessageType.Original || messageType == ImportMessageSendingMessageType.Replacement);
		}

		static void ActionForOutward(this JobDeclaration declaration, ref Func<PublishToUniversalResult> preMessagingAction, ref Action restoreToPreMessagingState, ImportMessageSendingMessageType messageType)
		{
			switch (messageType)
			{
				case ImportMessageSendingMessageType.Deletion:
					if (declaration.HasWHSTransaction)
					{
						preMessagingAction = () => declaration.PublishHoldEventForWHSOutwardAndSaveIfNeeded(true);
						restoreToPreMessagingState = declaration.PublishAcceptEventForWHSOutwardInADifferentFactory;
					}
					break;
				case ImportMessageSendingMessageType.Original:
				case ImportMessageSendingMessageType.Replacement:
					if (declaration.HasWHSTransaction)
					{
						preMessagingAction = declaration.PublishShipmentForWHSOutwardWithPreAmendmentData;
						restoreToPreMessagingState = declaration.RestoreLatestClearedBondedWarehouseOutwardInADifferentFactory;
					}
					else
					{
						preMessagingAction = () => declaration.PublishShipmentForWHSOutward(true);
						restoreToPreMessagingState = declaration.PublishCancelEventForWHSOutwardInADifferentFactory;
					}
					break;
			}
		}

		static void ActionForInward(this JobDeclaration declaration, ref Func<PublishToUniversalResult> preMessagingAction, ref Action restoreToPreMessagingState, ImportMessageSendingMessageType messageType, bool shouldSave)
		{
			switch (messageType)
			{
				case ImportMessageSendingMessageType.Deletion:
					if (declaration.HasWHSTransaction && declaration.WarehouseTransactionStatus != WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal)
					{
						preMessagingAction = () => declaration.PublishCancelEventForWHSInwardAndSaveIfNeeded(true, WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal);
						restoreToPreMessagingState = declaration.RestoreLatestClearedBondedWarehouseInwardInADifferentFactory;
					}
					break;
				case ImportMessageSendingMessageType.Original:
				case ImportMessageSendingMessageType.Replacement:
					preMessagingAction = () => declaration.PublishShipmentForWHSInward(isHold: true, shouldSave);
					restoreToPreMessagingState = declaration.RestoreBondedWarehouseInwardInADifferentFactory;
					break;
			}
		}
	}

	public class PreMessagingActionResult
	{
		public bool IsBondedWarehouse
		{
			get;
			set;
		}

		public ZString GetErrorMessageForCheckFieldsForBondedWarehouse()
		{
			return getErrorMessageForCheckFieldsForBondedWarehouse == null ? ZString.Empty : getErrorMessageForCheckFieldsForBondedWarehouse();
		}
		internal Func<ZString> getErrorMessageForCheckFieldsForBondedWarehouse;
	}
}
