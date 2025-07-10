using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class ZFormExtensionsTest : WhsGuiTestCaseWithFactory
	{
		#region TestHandleSaveException_WhsPickLine

		public void TestHandleSaveException_WhsPickLine()
		{
			var row = ((INeedRow)Factory.New<WhsPickLine>()).Row;
			AssertHandleSaveExceptionForTriggers(row, "Over-pick attempt.", WhsExceptionHandler.PreventOverCommitOfStockViaPickLineTriggerMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WhsPickLine.PreventPickLineFromLinkingToIncorrectTransactionLine, WhsExceptionHandler.WhsPickLine_PreventFromLinkingToIncorrectTransactionLineMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WhsPickLine.PreventCriticalFieldChangeWhenPickLineIsFinalizedTriggerID, WhsExceptionHandler.WhsPickLine_PreventCriticalFieldChangeWhenLineIsFinalizedMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WarehouseErrorMessages.PreventTransactionAndPickQuantityDesyncTriggerID, WarehouseErrorMessages.TransactionAndPickedQtyIsCorrectMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WhsPickLine.PreventTransactionsPickingFromDifferentWarehouseTriggerID, WhsExceptionHandler.WhsPickLine_PreventTransactionsPickingFromDifferentWarehouseMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WhsPickLine.PreventAttemptToAlllocateUnallocateableInventory, WhsExceptionHandler.PreventAttemptToAlllocateUnallocateableInventoryMsgForUser);
		}

		#endregion

		#region TestHandleSaveExceptionForOverReduceTriggers

		public void TestHandleSaveExceptionForOverReduceTriggers()
		{
			var row = ((INeedRow)Factory.New<WhsInventoryView>()).Row;
			AssertHandleSaveExceptionForTriggers(row, "Attempt to over-reduce TotalUnits.", AssertLastMessageIsOverReduceStock);
		}

		public static void AssertLastMessageIsOverReduceStock()
		{
			AssertEquals(
				"Another user has taken some of the stock that you have tried to take.\r\n" +
				"Close and re-open the form, then reallocate stock.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region TestHandleSaveExceptionForDocketLineTriggers

		public void TestHandleSaveExceptionForDocketLineTriggers()
		{
			var row = ((INeedRow)Factory.New<WhsOrderLine>()).Row;
			AssertHandleSaveExceptionForTriggers(row, WhsDocketLine.PreventAdjustmentInLinesWithPickLinesTriggerID, WhsExceptionHandler.WhsDocketLine_PreventAdjustmentInLinesWithPickLinesMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WhsDocketLine.PreventDesynchronisingInterWhsTransferLines, WhsExceptionHandler.PreventDesynchronisingInterWhsTransferLinesMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WhsDocketLine.PreventLocationForIncorrectWarehouseTriggerException, WhsExceptionHandler.PreventLocationForIncorrectWarehouseTriggerMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WhsDocketLine.PreventIncorrectStatusDocketLineTriggerID, WhsExceptionHandler.PreventIncorrectDateOrStatusDocketLineTriggerMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WhsDocketLine.PreventIncorrectFinalisedDateDocketLineTriggerID, WhsExceptionHandler.PreventIncorrectDateOrStatusDocketLineTriggerMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WarehouseErrorMessages.PreventTransactionAndPickQuantityDesyncTriggerID, WarehouseErrorMessages.TransactionAndPickedQtyIsCorrectMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WhsDocketLine.PreventAttemptToMakeAllocatedInventoryUnallocateable, WhsExceptionHandler.PreventAttemptToMakeAllocatedInventoryUnallocateableMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WhsDocketLine.PreventFinalisationOfReceiveWithShortageOfStock, WhsExceptionHandler.WhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStockMsgForUser);
		}

		#endregion

		#region TestHandleSaveExceptionForPreventOverflowLocationQuantityTrigger

		public void TestHandleSaveExceptionForPreventOverflowLocationQuantityTrigger()
		{
			var row = ((INeedRow)Factory.New<WhsOrderLine>()).Row;
			AssertHandleSaveExceptionForTriggers(row, WhsDocketLine.PreventOverflowLocationQuantityTriggerID, AssertLastMessagePreventOverflowLocationQuantityTriggerIDMsgForUser);
		}

		public static void AssertLastMessagePreventOverflowLocationQuantityTriggerIDMsgForUser()
		{
			AssertEquals(WhsExceptionHandler.PreventOverflowLocationQuantityTriggerIDMsgForUser, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region TestPreventInvalidClientAndProductForNonEmptyLocationTrigger

		public void TestPreventInvalidClientAndProductForNonEmptyLocationTrigger()
		{
			var row = ((INeedRow)Factory.New<WhsStocktakeLine>()).Row;
			AssertHandleSaveExceptionForTriggers(row, WhsStocktakeLine.PreventInvalidClientAndProductForNonEmptyLocationTriggerID, AssertPreventInvalidClientAndProductForNonEmptyLocationTriggerMsgForUser);
		}

		public static void AssertPreventInvalidClientAndProductForNonEmptyLocationTriggerMsgForUser()
		{
			AssertEquals(WhsExceptionHandler.PreventInvalidClientAndProductForNonEmptyLocationTriggerMsgForUser, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region TestHandleSaveException_WhsPick

		public void TestHandleSaveException_WhsPick()
		{
			var row = ((INeedRow)Factory.New<WhsPick>()).Row;
			AssertHandleSaveExceptionForTriggers(row, WhsDocket.PreventOrderAndWorkOrderWarehouseNotMatchingPicksWarehouse, WhsExceptionHandler.WhsDocketAndPick_PreventNotMatchingWarehousesTriggerMsgForUser);
		}

		#endregion

		#region TestHandleSaveException_WhsDocket

		public void TestHandleSaveException_WhsDocket()
		{
			var row = ((INeedRow)Factory.New<WhsOrder>()).Row;
			AssertHandleSaveExceptionForTriggers(row, WhsDocket.PreventChangeOfCriticalFieldsWhenDocketHasLines, WhsExceptionHandler.PreventSubTypeOrWarehouseChangeWhenDocketHasLinesTriggerMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WhsOrder.PreventChangeOfWarehouse, WhsExceptionHandler.PreventWarehouseChangeWhenDocketHasCrossDockLocationOrReservedStockOrIsNotNewOrEnteredTriggerMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WhsDocket.PreventChangeOfClientWhenDocketHasLines, WhsExceptionHandler.PreventClientChangeWhenDocketHasLinesTriggerMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WhsDocket.PreventDetachedOrderPickLines, WhsExceptionHandler.WhsDocket_PreventDetachedOrderPickLinesTriggerMsgForUser);
			AssertHandleSaveExceptionForTriggers(row, WhsDocket.PreventOrderAndWorkOrderWarehouseNotMatchingPicksWarehouse, WhsExceptionHandler.WhsDocketAndPick_PreventNotMatchingWarehousesTriggerMsgForUser);

			var row2 = ((INeedRow)Factory.New<WhsWorkOrder>()).Row;
			AssertHandleSaveExceptionForTriggers(row2, WhsDocket.PreventOrderAndWorkOrderWarehouseNotMatchingPicksWarehouse, WhsExceptionHandler.WhsDocketAndPick_PreventNotMatchingWarehousesTriggerMsgForUser);
		}

		#endregion

		#region AssertHandleSaveExceptionForTriggers

		void AssertHandleSaveExceptionForTriggers(DataRow row, string expectedTriggerExceptionMessage, string expectedLastNotificationMessage)
		{
			AssertHandleSaveExceptionForTriggers(row, expectedTriggerExceptionMessage,
				() => AssertEquals(expectedLastNotificationMessage, UnitTestUserNotification.Instance.LastMessage.Text));
		}

		void AssertHandleSaveExceptionForTriggers(DataRow row, string expectedTriggerExceptionMessage, Action assertLastMessageShown)
		{
			UnitTestUserNotification.Instance.ClearMessages();
			AssertEquals(false, ZFormExceptionHandler.HandleSaveExceptionForTriggers(new ArgumentException()));
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			AssertEquals(false, ZFormExceptionHandler.HandleSaveExceptionForTriggers(new ZSaveException(new ZDataException(new ArgumentException("Different Exception!"), row, TestConnection), Factory)));
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

			AssertEquals(true, ZFormExceptionHandler.HandleSaveExceptionForTriggers(new ZSaveException(new ZDataException(new ArgumentException(expectedTriggerExceptionMessage), row, TestConnection), Factory)));
			assertLastMessageShown();
		}

		#endregion
	}
}
