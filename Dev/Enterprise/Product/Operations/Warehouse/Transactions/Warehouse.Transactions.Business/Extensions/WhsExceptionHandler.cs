using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WhsExceptionHandler
	{
		#region TriggerExceptions

		#region IsPreventDesynchronisingInterWhsTransferLines

		public static bool IsPreventDesynchronisingInterWhsTransferLines(this Exception ex)
		{
			return IsTriggerException(ex, WhsDocketLine.PreventDesynchronisingInterWhsTransferLines);
		}

		#endregion

		#region IsPreventOverCommitStockViaPickLineTriggerException

		public static bool IsPreventOverCommitStockViaPickLineTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WhsPickLine.PreventOverCommitOfStockViaPickLineTriggerID);
		}

		#endregion

		#region IsWhsPickLine_LinkingToIncorrectTransactionLineTriggerException

		public static bool IsWhsPickLine_LinkingToIncorrectTransactionLineTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WhsPickLine.PreventPickLineFromLinkingToIncorrectTransactionLine);
		}

		#endregion

		#region IsWhsPickLine_TryingToChangeCriticalDataForFinalizedPickLine

		public static bool IsWhsPickLine_TryingToChangeCriticalDataForFinalizedPickLine(this Exception ex)
		{
			return IsTriggerException(ex, WhsPickLine.PreventCriticalFieldChangeWhenPickLineIsFinalizedTriggerID);
		}

		#endregion

		#region IsWhsPickLine_PreventTransactionsPickingFromDifferentWarehouses

		public static bool IsWhsPickLine_PreventTransactionsPickingFromDifferentWarehouses(this Exception ex)
		{
			return IsTriggerException(ex, WhsPickLine.PreventTransactionsPickingFromDifferentWarehouseTriggerID);
		}

		#endregion

		#region IsWhsPickLine_PreventDeleteOfFinalisedPickLineTriggerException

		public static bool IsWhsPickLine_PreventDeleteOfFinalisedPickLineTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WhsPickLine.PreventDeleteOfFinalisedPickLineTriggerTriggerID);
		}

		#endregion

		#region IsWhsDocketLine_PreventAdjustmentInLinesWithPickLinesTriggerException

		public static bool IsWhsDocketLine_PreventAdjustmentInLinesWithPickLinesTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WhsDocketLine.PreventAdjustmentInLinesWithPickLinesTriggerID);
		}

		#endregion

		#region IsWhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStockTriggerException

		public static bool IsWhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStockTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WhsDocketLine.PreventFinalisationOfReceiveWithShortageOfStock);
		}

		#endregion

		#region IsWhsDocketLineAndWhsPickLine_TransactionAndPickedQtyAreCorrectTriggerException

		public static bool IsWhsDocketLineAndWhsPickLine_TransactionAndPickedQtyIsCorrectTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WarehouseErrorMessages.PreventTransactionAndPickQuantityDesyncTriggerID);
		}

		#endregion

		#region IsPreventOverReduceStockViaInventoryLineTriggerException

		public static bool IsPreventOverReduceStockViaInventoryLineTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WhsInventoryView.PreventOverReduceStockViaInventoryLineMessageID);
		}

		#endregion

		#region IsPreventIncorrectStatusDocketLineTriggerException

		public static bool IsPreventIncorrectStatusDocketLineTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WhsDocketLine.PreventIncorrectStatusDocketLineTriggerID);
		}

		#endregion

		#region IsPreventIncorrectFinalisedDateDocketLineTriggerException

		public static bool IsPreventIncorrectFinalisedDateDocketLineTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WhsDocketLine.PreventIncorrectFinalisedDateDocketLineTriggerID);
		}

		#endregion

		#region IsPreventOverflowLocationQuantityTriggerException

		public static bool IsPreventOverflowLocationQuantityTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WhsDocketLine.PreventOverflowLocationQuantityTriggerID);
		}

		#endregion

		#region IsPreventInvalidClientAndProductForNonEmptyLocationTriggerException

		public static bool IsPreventInvalidClientAndProductForNonEmptyLocationTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WhsStocktakeLine.PreventInvalidClientAndProductForNonEmptyLocationTriggerID);
		}

		#endregion

		#region IsPreventChangeOfCriticalFieldsWhenDocketHasLinesTriggerException

		public static bool IsPreventChangeOfCriticalFieldsWhenDocketHasLinesTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WhsDocket.PreventChangeOfCriticalFieldsWhenDocketHasLines);
		}

		#endregion

		#region IsPreventChangeOfWarehouseWhenDocketIsOrder

		public static bool IsPreventChangeOfWarehouseWhenDocketIsOrder(this Exception ex)
		{
			return IsTriggerException(ex, WhsOrder.PreventChangeOfWarehouse);
		}

		#endregion

		#region IsPreventChangeOfClientWhenDocketHasLinesTriggerException

		public static bool IsPreventChangeOfClientWhenDocketHasLinesTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WhsDocket.PreventChangeOfClientWhenDocketHasLines);
		}

		#endregion

		#region IsPreventLocationForIncorrectWarehouseTriggerException

		public static bool IsPreventLocationForIncorrectWarehouseTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WhsDocketLine.PreventLocationForIncorrectWarehouseTriggerException);
		}

		#endregion

		#region IsWhsDocket_PreventDetachedOrdersPickLines

		public static bool IsWhsDocket_PreventDetachedOrderPickLines(this Exception ex)
		{
			return IsTriggerException(ex, WhsDocket.PreventDetachedOrderPickLines);
		}

		#endregion

		#region IsWhsDocketAndWhsPick_EnsureHaveSameWarehouse

		public static bool IsWhsDocketAndWhsPick_EnsureHaveSameWarehouse(this Exception ex)
		{
			return IsTriggerException(ex, WhsDocket.PreventOrderAndWorkOrderWarehouseNotMatchingPicksWarehouse);
		}

		#endregion

		#region IsPreventAttemptToMakeAllocatedInventoryUnallocateableTriggerException

		public static bool IsPreventAttemptToMakeAllocatedInventoryUnallocateableTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WhsDocketLine.PreventAttemptToMakeAllocatedInventoryUnallocateable);
		}

		#endregion

		#region IsPreventAttemptToAlllocateUnallocateableInventoryTriggerException

		public static bool IsPreventAttemptToAlllocateUnallocateableInventoryTriggerException(this Exception ex)
		{
			return IsTriggerException(ex, WhsPickLine.PreventAttemptToAlllocateUnallocateableInventory);
		}

		#endregion

		static bool IsTriggerException(Exception ex, string triggerId)
			=> ex switch
			{
				ZSaveException saveEx => saveEx.InnerException?.InnerException?.Message == triggerId, // Exception message is not translatable
				ZConcurrencyCheckFailureException concurrentEx => concurrentEx.Message.Contains(triggerId),
				_ => false,
			};

		#endregion

		#region Messages

		public static string PreventOverCommitOfStockViaPickLineTriggerMsgForUser
		{
			get
			{
				return Res.GetString("09e5c863-3cd6-4b72-9fcb-373734cc0d0f",
@"Another user has taken some of the stock that you have tried to allocate.
Close and re-open the form, then reallocate stock.");
			}
		}

		public static string WhsPickLine_PreventFromLinkingToIncorrectTransactionLineMsgForUser
		{
			get
			{
				return Res.GetString("B5512349-A4ED-4893-8E54-69FAF7E2AA41", @"Attempt to link Pick Line to incorrect Transaction Line.
Try to close and re-open the form.");
			}
		}

		public static string WhsPickLine_PreventCriticalFieldChangeWhenLineIsFinalizedMsgForUser
		{
			get
			{
				return Res.GetString("5A7700E3-2E9C-4ACC-B44B-4BB7E145BC42", @"Attempt to change critical data for a finalized Pick Line.
Try to close and re-open the form.");
			}
		}

		public static string WhsPickLine_PreventTransactionsPickingFromDifferentWarehouseMsgForUser
		{
			get
			{
				return Res.GetString("DBA69959-1CC5-4579-938A-D83B0C1A429F", @"Attempt to pick from a Warehouse different from where this transaction was registered.
Try to close and re-open the form.");
			}
		}

		public static string WhsPickLine_PreventDeleteOfFinalisedPickLineTriggerMsgForUser
		{
			get
			{
				return Res.GetString("{6AC899F7-AD1B-43FC-8954-A5B6B72ECD03}", @"Deletion of Pick Line for a finalized Pick was attempted.
Close and re-open the form.");
			}
		}

		public static string WhsPickLine_PreventTransactionsPickingFromDifferentWarehouseMsgForServiceTask
		{
			get
			{
				return Res.GetString("{AE05D919-517B-4EC7-BE68-E33A4E78886F}", "Attempt to pick from a Warehouse different from where the transaction was registered by the service task.");
			}
		}

		public static string WhsDocketLine_PreventAdjustmentInLinesWithPickLinesMsgForUser
		{
			get
			{
				return Res.GetString("42766C26-E386-45A8-864A-2F3CAE50B40E", @"Attempt to create Adjustment In line that picks inventory.
Try to close and re-open the form.");
			}
		}

		public static string WhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStockMsgForUser
		{
			get
			{
				return Res.GetString("379cdbc3-ed17-46c1-a6e1-7b9c5625abb7",
					@"Another user has modified this Receive and prevented finalization from succeeding.
Close and re-open the form and review reservations before reattempting to finalize.");
			}
		}

		public static string WhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStockMsgForServiceTask
		{
			get
			{
				return Res.GetString("8cf8dd83-5bf6-490b-93c0-a19b7be4bbf7",
					@"Another user has modified this Receive and prevented finalization from succeeding.
Review reservations on the Receive before reattempting to finalize.");
			}
		}

		public static string WhsDocketLineAndWhsPickLine_TransactionAndPickedQtyIsCorrectMsg_ForServiceTask
		{
			get
			{
				return Res.GetString("7267795B-DE4D-4486-AAD0-BAE284FD0058", @"Attempt to set Transaction and Pick qty out of sync by the current process.");
			}
		}

		public static string PreventOverReduceOfStockViaInventoryLineTriggerMsgForUser
		{
			get
			{
				return Res.GetString("2c9f6c78-3cae-4f8f-9aee-49529c363523",
@"Another user has taken some of the stock that you have tried to take.
Close and re-open the form, then reallocate stock.");
			}
		}

		public static string PreventOverCommitOfStockViaPickLineTriggerMsgForServiceTask
		{
			get { return Res.GetString("8e7eca41-ebfb-4233-befa-8c19e9d8644f", "Another job has taken some of the stock that this service task tried to allocate."); }
		}

		public static string PreventIncorrectDateOrStatusDocketLineTriggerMsgForUser
		{
			get { return Res.GetString("e2e27741-42e9-4b93-a09d-842092a3d0a5", "Changes made to this job by another user have caused the finalization or un-finalization process to fail. Close the form without saving and re-open it."); }
		}

		public static string PreventOverflowLocationQuantityTriggerIDMsgForUser
		{
			get { return Res.GetString("1c9d5a35-2801-40ce-ac7e-03ae4b5bac68", "Destination locations used in created job cannot fit the required quantity. Please try again."); }
		}

		public static string PreventInvalidClientAndProductForNonEmptyLocationTriggerMsgForUser
		{
			get { return Res.GetString("f259ffec-d134-4921-a233-25cac51ff850", "Cannot add empty location where category is exclude empty location."); }
		}

		public static string PreventSubTypeOrWarehouseChangeWhenDocketHasLinesTriggerMsgForUser
		{
			get { return Res.GetString("58c88c40-62c8-4295-A8c0-c1ec79816c84", "Cannot change the Type or Warehouse for a job with lines. Please delete all lines before changing these fields."); }
		}

		public static string PreventWarehouseChangeWhenDocketHasCrossDockLocationOrReservedStockOrIsNotNewOrEnteredTriggerMsgForUser
		{
			get
			{
				return Res.GetString("F0305A88-A370-42DE-B345-4D65214860B7",
@"Cannot change the Warehouse for an Order. Please remove the linked Cross-Dock Allocation(s) or Cross Dock Location prior to changing the Warehouse. 
If problem still exists, please close the form without saving and re-open it.");
			}
		}

		public static string WhsDocket_PreventDetachedOrderPickLinesTriggerMsgForUser
		{
			get { return Res.GetString("0118d799-f70a-4065-9Ed6-3b80e3dc0fd1", "Cannot detach order/work order with unreserved pick lines."); }
		}

		public static string WhsDocketAndPick_PreventNotMatchingWarehousesTriggerMsgForUser
		{
			get { return Res.GetString("9967EB69-A64D-46DE-9F6D-8C653D93E604", "The Warehouse of an Order / Work Order does not match Warehouse of a Pick."); }
		}

		public static string PreventDesynchronisingInterWhsTransferLinesMsgForUser
		{
			get { return Res.GetString("39f4ff59-7c37-4e66-a792-efc43ed5e009", @"Attempt to desynchronize Parent and Child Lines on an Inter-Warehouse Transfer.
Try to close and re-open the form."); }
		}

		public static string PreventLocationForIncorrectWarehouseTriggerMsgForUser => Res.GetString("cd4612e4-eeff-47aa-a68c-0b6386b338fa", "Cannot use Location from a Warehouse different to what is specified on the job.");

		public static string PreventClientChangeWhenDocketHasLinesTriggerMsgForUser => Res.GetString("4e1dddae-8a8a-48d1-9c85-7d56b2f5cbed", "Cannot change the Client for a job with lines. Please delete all lines before changing the Client.");

		public static string PreventAttemptToMakeAllocatedInventoryUnallocateableMsgForUser => Res.GetString("fda00438-f605-41a2-ba04-f969b2d0f7a1", "Cannot change the inventory's status if it has already been allocated to an order.");

		public static string PreventAttemptToAlllocateUnallocateableInventoryMsgForUser => Res.GetString("9afd7f6a-3159-4ee1-98c9-31ece5dcff36", "An invalid inventory has been allocated. Close and re-open the form then reallocate stock.");

		#endregion
	}
}
