using System;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public static class ZFormExceptionHandler
	{
		public static bool HandleSaveExceptionForTriggers(Exception ex)
		{
			var result = true;
			if (ex.IsPreventOverCommitStockViaPickLineTriggerException())
			{
				Globals.Message.ShowError(WhsExceptionHandler.PreventOverCommitOfStockViaPickLineTriggerMsgForUser);
			}
			else if (ex.IsWhsPickLine_LinkingToIncorrectTransactionLineTriggerException())
			{
				Globals.Message.ShowError(WhsExceptionHandler.WhsPickLine_PreventFromLinkingToIncorrectTransactionLineMsgForUser);
			}
			else if (ex.IsWhsPickLine_TryingToChangeCriticalDataForFinalizedPickLine())
			{
				Globals.Message.ShowError(WhsExceptionHandler.WhsPickLine_PreventCriticalFieldChangeWhenLineIsFinalizedMsgForUser);
			}
			else if (ex.IsWhsPickLine_PreventDeleteOfFinalisedPickLineTriggerException())
			{
				Globals.Message.ShowError(WhsExceptionHandler.WhsPickLine_PreventDeleteOfFinalisedPickLineTriggerMsgForUser);
			}
			else if (ex.IsWhsDocketLine_PreventAdjustmentInLinesWithPickLinesTriggerException())
			{
				Globals.Message.ShowError(WhsExceptionHandler.WhsDocketLine_PreventAdjustmentInLinesWithPickLinesMsgForUser);
			}
			else if (ex.IsWhsDocketLineAndWhsPickLine_TransactionAndPickedQtyIsCorrectTriggerException())
			{
				Globals.Message.ShowError(WarehouseErrorMessages.TransactionAndPickedQtyIsCorrectMsgForUser);
			}
			else if (ex.IsWhsPickLine_PreventTransactionsPickingFromDifferentWarehouses())
			{
				Globals.Message.ShowError(WhsExceptionHandler.WhsPickLine_PreventTransactionsPickingFromDifferentWarehouseMsgForUser);
			}
			else if (ex.IsPreventOverReduceStockViaInventoryLineTriggerException())
			{
				Globals.Message.ShowError(WhsExceptionHandler.PreventOverReduceOfStockViaInventoryLineTriggerMsgForUser);
			}
			else if (ex.IsPreventIncorrectFinalisedDateDocketLineTriggerException() || ex.IsPreventIncorrectStatusDocketLineTriggerException())
			{
				Globals.Message.ShowError(WhsExceptionHandler.PreventIncorrectDateOrStatusDocketLineTriggerMsgForUser);
			}
			else if (ex.IsPreventOverflowLocationQuantityTriggerException())
			{
				Globals.Message.ShowError(WhsExceptionHandler.PreventOverflowLocationQuantityTriggerIDMsgForUser);
			}
			else if (ex.IsPreventInvalidClientAndProductForNonEmptyLocationTriggerException())
			{
				Globals.Message.ShowError(WhsExceptionHandler.PreventInvalidClientAndProductForNonEmptyLocationTriggerMsgForUser);
			}
			else if (ex.IsPreventChangeOfCriticalFieldsWhenDocketHasLinesTriggerException())
			{
				Globals.Message.ShowError(WhsExceptionHandler.PreventSubTypeOrWarehouseChangeWhenDocketHasLinesTriggerMsgForUser);
			}
			else if (ex.IsPreventChangeOfWarehouseWhenDocketIsOrder())
			{
				Globals.Message.ShowError(WhsExceptionHandler.PreventWarehouseChangeWhenDocketHasCrossDockLocationOrReservedStockOrIsNotNewOrEnteredTriggerMsgForUser);
			}
			else if (ex.IsWhsDocket_PreventDetachedOrderPickLines())
			{
				Globals.Message.ShowError(WhsExceptionHandler.WhsDocket_PreventDetachedOrderPickLinesTriggerMsgForUser);
			}
			else if (ex.IsWhsDocketAndWhsPick_EnsureHaveSameWarehouse())
			{
				Globals.Message.ShowError(WhsExceptionHandler.WhsDocketAndPick_PreventNotMatchingWarehousesTriggerMsgForUser);
			}
			else if (ex.IsPreventDesynchronisingInterWhsTransferLines())
			{
				Globals.Message.ShowError(WhsExceptionHandler.PreventDesynchronisingInterWhsTransferLinesMsgForUser);
			}
			else if (ex.IsPreventLocationForIncorrectWarehouseTriggerException())
			{
				Globals.Message.ShowError(WhsExceptionHandler.PreventLocationForIncorrectWarehouseTriggerMsgForUser);
			}
			else if (ex.IsPreventChangeOfClientWhenDocketHasLinesTriggerException())
			{
				Globals.Message.ShowError(WhsExceptionHandler.PreventClientChangeWhenDocketHasLinesTriggerMsgForUser);
			}
			else if (ex.IsPreventAttemptToMakeAllocatedInventoryUnallocateableTriggerException())
			{
				Globals.Message.ShowError(WhsExceptionHandler.PreventAttemptToMakeAllocatedInventoryUnallocateableMsgForUser);
			}
			else if (ex.IsPreventAttemptToAlllocateUnallocateableInventoryTriggerException())
			{
				Globals.Message.ShowError(WhsExceptionHandler.PreventAttemptToAlllocateUnallocateableInventoryMsgForUser);
			}
			else if (ex.IsWhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStockTriggerException())
			{
				Globals.Message.ShowError(WhsExceptionHandler.WhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStockMsgForUser);
			}
			else
			{
				result = false;
			}

			return result;
		}
	}
}
