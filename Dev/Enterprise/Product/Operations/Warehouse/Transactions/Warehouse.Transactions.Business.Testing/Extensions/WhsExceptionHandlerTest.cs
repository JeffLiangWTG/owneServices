using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsExceptionHandlerTest : WhsTestCaseWithFactory
	{
		#region TestIsPreventOverCommitStockViaPickLineTriggerException

		public void TestIsPreventOverCommitStockViaPickLineTriggerException()
		{
			TestTriggerException<WhsOrder>("Over-pick attempt.",
				ex => ex.IsPreventOverCommitStockViaPickLineTriggerException());
		}

		#endregion

		#region TestIsWhsPickLine_LinkingToIncorrectTransactionLineTriggerException

		public void TestIsWhsPickLine_LinkingToIncorrectTransactionLineTriggerException()
		{
			TestTriggerException<WhsPickLine>(WhsPickLine.PreventPickLineFromLinkingToIncorrectTransactionLine,
				ex => ex.IsWhsPickLine_LinkingToIncorrectTransactionLineTriggerException());
		}

		#endregion

		#region TestIsWhsPickLine_TryingToUpdateLineWithExistingPickDateTime

		public void TestIsWhsPickLine_TryingToChangeCriticalDataForFinalizedPickLine()
		{
			TestTriggerException<WhsPickLine>(WhsPickLine.PreventCriticalFieldChangeWhenPickLineIsFinalizedTriggerID,
				ex => ex.IsWhsPickLine_TryingToChangeCriticalDataForFinalizedPickLine());
		}

		#endregion

		#region TestIsWhsPickLine_TryingToUpdateLineWithExistingPickDateTime

		public void TestIsWhsPickLine_PreventDeleteOfFinalisedPickLine()
		{
			TestTriggerException<WhsPickLine>(WhsPickLine.PreventDeleteOfFinalisedPickLineTriggerTriggerID,
				ex => ex.IsWhsPickLine_PreventDeleteOfFinalisedPickLineTriggerException());
		}

		#endregion

		#region TestIsWhsDocketLine_PreventAdjustmentInLinesWithPickLinesTriggerException

		public void TestIsWhsDocketLine_PreventAdjustmentInLinesWithPickLinesTriggerException()
		{
			TestTriggerException<WhsAdjustmentLine>(WhsDocketLine.PreventAdjustmentInLinesWithPickLinesTriggerID,
				ex => ex.IsWhsDocketLine_PreventAdjustmentInLinesWithPickLinesTriggerException());
		}

		#endregion

		#region void TestIsWhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStockTriggerException

		public void TestIsWhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStockTriggerException()
		{
			TestTriggerException<WhsReceiveLine>(WhsDocketLine.PreventFinalisationOfReceiveWithShortageOfStock,
				ex => ex.IsWhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStockTriggerException());
		}

		#endregion

		#region TestIsWhsDocketLineAndWhsPickLine_TransactionAndPickedQtyAreCorrectTriggerException

		public void TestIsWhsDocketLineAndWhsPickLine_TransactionAndPickedQtyAreCorrectTriggerException()
		{
			TestTriggerException<WhsTransferLine>(
				WarehouseErrorMessages.PreventTransactionAndPickQuantityDesyncTriggerID,
				ex => ex.IsWhsDocketLineAndWhsPickLine_TransactionAndPickedQtyIsCorrectTriggerException());
			TestTriggerException<WhsPickLine>(WarehouseErrorMessages.PreventTransactionAndPickQuantityDesyncTriggerID,
				ex => ex.IsWhsDocketLineAndWhsPickLine_TransactionAndPickedQtyIsCorrectTriggerException());
		}

		#endregion

		#region TestIsPreventOverReduceStockViaInventoryLineTriggerException

		public void TestIsPreventOverReduceStockViaInventoryLineTriggerException()
		{
			TestTriggerException<WhsInventoryView>("Attempt to over-reduce TotalUnits.",
				ex => ex.IsPreventOverReduceStockViaInventoryLineTriggerException());
		}

		#endregion

		#region TestIsPreventDesynchronisingInterWhsTransferLines

		public void TestIsPreventDesynchronisingInterWhsTransferLines()
		{
			TestTriggerException<WhsTransferLine>("Attempt to desynchronise InterWhs Child!",
				ex => ex.IsPreventDesynchronisingInterWhsTransferLines());
		}

		#endregion

		#region TestIsPreventIncorrectFinalisedDateAndStatusDocketLineTriggerException

		public void TestIsPreventIncorrectStatusDocketLineTriggerException()
		{
			TestTriggerException<WhsOrderLine>("Attempt to set incorrect WE_DocketLineStatus",
				ex => ex.IsPreventIncorrectStatusDocketLineTriggerException());
		}

		#endregion

		#region TestIsPreventIncorrectFinalisedDateDocketLineTriggerException

		public void TestIsPreventIncorrectFinalisedDateDocketLineTriggerException()
		{
			TestTriggerException<WhsOrderLine>("Attempt to set incorrect WE_FinalisedDate",
				ex => ex.IsPreventIncorrectFinalisedDateDocketLineTriggerException());
		}

		#endregion

		#region TestIsPreventOverflowLocationQuantityTriggerException

		public void TestIsPreventOverflowLocationQuantityTriggerException()
		{
			TestTriggerException<WhsOrderLine>(WhsDocketLine.PreventOverflowLocationQuantityTriggerID,
				ex => ex.IsPreventOverflowLocationQuantityTriggerException());
		}

		#endregion

		#region TestIsPreventInvalidClientAndProductForNonEmptyLocationTriggerException

		public void TestIsPreventInvalidClientAndProductForNonEmptyLocationTriggerException()
		{
			TestTriggerException<WhsStocktakeLine>("Client or Product cannot be null when exclude empty locations.",
				ex => ex.IsPreventInvalidClientAndProductForNonEmptyLocationTriggerException());
		}

		#endregion

		#region TestIsPreventChangeOfCriticalFieldsWhenDocketHasLinesTriggerException

		public void TestIsPreventChangeOfCriticalFieldsWhenDocketHasLinesTriggerException()
		{
			TestTriggerException<WhsReceive>(
				"Attempt to change docket subtype or warehouse for a job with captured lines.",
				ex => ex.IsPreventChangeOfCriticalFieldsWhenDocketHasLinesTriggerException());
		}

		#endregion

		#region TestIsPreventChangeOfWarehouseWhenDocketIsOrderTriggerException

		public void TestIsPreventChangeOfWarehouseWhenDocketIsOrderTriggerException()
		{
			TestTriggerException<WhsOrder>(
				"Attempt to change warehouse for an order with Cross Dock Location/Reserved Stock or is processing/processed.",
				ex => ex.IsPreventChangeOfWarehouseWhenDocketIsOrder());
		}

		#endregion

		#region TestIsPreventChangeOfClientWhenDocketHasLinesTriggerException

		public void TestIsPreventChangeOfClientWhenDocketHasLinesTriggerException()
		{
			TestTriggerException<WhsOrder>("Attempt to change client for a job with captured lines.",
				ex => ex.IsPreventChangeOfClientWhenDocketHasLinesTriggerException());
		}

		#endregion

		#region TestIsPreventLocationForIncorrectWarehouseTriggerException

		public void TestIsPreventLocationForIncorrectWarehouseTriggerException()
		{
			TestTriggerException<WhsOrderLine>("Locations should be in the warehouse specified on the parent Docket.",
				ex => ex.IsPreventLocationForIncorrectWarehouseTriggerException());
		}

		#endregion

		#region TestIsPreventDetachedOrderPickLinesTriggerException

		public void TestIsPreventDetachedOrderPickLinesTriggerException()
		{
			TestTriggerException<WhsOrder>("Attempt to detach order/work order docket with unreserved pick lines.",
				ex => ex.IsWhsDocket_PreventDetachedOrderPickLines());
		}

		#endregion

		#region TestIsWhsDocketAndWhsPick_EnsureHaveSameWarehouse

		public void TestIsWhsDocketAndWhsPick_EnsureHaveSameWarehouse()
		{
			TestTriggerException<WhsPick>(WhsDocket.PreventOrderAndWorkOrderWarehouseNotMatchingPicksWarehouse,
				ex => ex.IsWhsDocketAndWhsPick_EnsureHaveSameWarehouse());
			TestTriggerException<WhsOrder>(WhsDocket.PreventOrderAndWorkOrderWarehouseNotMatchingPicksWarehouse,
				ex => ex.IsWhsDocketAndWhsPick_EnsureHaveSameWarehouse());
			TestTriggerException<WhsWorkOrder>(WhsDocket.PreventOrderAndWorkOrderWarehouseNotMatchingPicksWarehouse,
				ex => ex.IsWhsDocketAndWhsPick_EnsureHaveSameWarehouse());
		}

		#endregion

		#region TestIsPreventTransactionsPickingFromDifferentWarehouse

		public void TestIsPreventTransactionsPickingFromDifferentWarehouse()
		{
			TestTriggerException<WhsPickLine>(
				"Attempt to pick from a Warehouse different from where the transaction was registered.",
				ex => ex.IsWhsPickLine_PreventTransactionsPickingFromDifferentWarehouses());
		}

		#endregion

		#region TestIsPreventAttemptToMakeAllocatedInventoryUnallocateableTriggerException

		public void TestIsPreventAttemptToMakeAllocatedInventoryUnallocateableTriggerException()
		{
			TestTriggerException<WhsInventoryView>("Attempt to make allocated inventory unallocateable.",
				ex => ex.IsPreventAttemptToMakeAllocatedInventoryUnallocateableTriggerException());
		}

		#endregion

		#region TestIsPreventAttemptToAlllocateUnallocateableInventoryTriggerException

		public void TestIsPreventAttemptToAlllocateUnallocateableInventoryTriggerException()
		{
			TestTriggerException<WhsPickLine>("Attempt to link Pick Line to an unallocateable inventory.",
				ex => ex.IsPreventAttemptToAlllocateUnallocateableInventoryTriggerException());
		}

		#endregion

		#region Implementation

		void TestTriggerException<T>(string exceptionMessage, Func<Exception, bool> isTriggerException)
			where T : BusinessObject
		{
			var row = ((INeedRow)Factory.New<T>()).Row;
			AssertEquals(false, isTriggerException(new ArgumentException()));
			AssertEquals(false,
				isTriggerException(new ZSaveException(
					new ZDataException(new ArgumentException("Different Error!"), row, TestConnection), Factory)));
			AssertEquals(true,
				isTriggerException(new ZSaveException(
					new ZDataException(new ArgumentException(exceptionMessage), row, TestConnection), Factory)));
		}

		#endregion
	}
}
