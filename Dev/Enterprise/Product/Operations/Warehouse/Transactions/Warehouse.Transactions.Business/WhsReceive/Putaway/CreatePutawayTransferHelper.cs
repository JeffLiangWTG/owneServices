using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class CreatePutawayTransferHelper : ICreatePutawayTransferHelper
	{
		public string CreateTransfer(
			BusinessObjectFactory factory,
			IWhsWarehouse warehouse,
			IEnumerable<IWhsInventoryView> inventory,
			bool isMultiPalletPutaway,
			string staffCode)
		{
			string errorMessage = null;
			var inventoriesToPutaway = inventory.Cast<WhsInventoryView>().Where(inventory => inventory.WI_InDocketLineUnits > 0 && !inventory.WI_PalletID.IsEmpty).ToArray();
			if (inventoriesToPutaway.Length > 0 && warehouse is WhsWarehouse whs)
			{
				errorMessage = CreatePutawayTransfer(inventoriesToPutaway.GroupBy(i => i.WI_PalletID.ToUpper()), isMultiPalletPutaway, factory, whs, staffCode);
			}
			else
			{
				errorMessage = Res.GetString("73b42adc-2034-4d7f-a003-51c76c2deb4f", "There are no items to putaway.");
			}

			return errorMessage;
		}

		string CreatePutawayTransfer(
			IEnumerable<IGrouping<ZString, WhsInventoryView>> palletIDsAndAssociatedInventories,
			bool multiPallet,
			BusinessObjectFactory factory,
			WhsWarehouse warehouse,
			string staffCode)
		{
			WhsTransfer transfer = null;
			string errorMessage = null;
			var newTransferLines = new List<WhsTransferLine>();
			foreach (var palletIDAndInvs in palletIDsAndAssociatedInventories)
			{
				var inventories = palletIDAndInvs.ToArray();
				AddInventoryToPickLineFetchHints(inventories, factory);
				var inventoryWithPutawayTransfer = inventories.FirstOrDefault(inventory => inventory.HasPutawayTransfer);
				if (inventoryWithPutawayTransfer != null)
				{
					if (IsPalletIdOnInventoriesWithPutawayTransferAndAlsoUnloadedToDockdoor(inventories, inventoryWithPutawayTransfer))
					{
						errorMessage = Res.GetString("EB843062-7FEF-4185-98CF-E006F097E2EA", "Putaway Failed: Inventories with Pallet ID {0} are currently being putaway and unloaded at the same time.", palletIDAndInvs.Key);
					}
					else if (IsPutawayAlreadyCompletedOnPalletID(inventoryWithPutawayTransfer))
					{
						errorMessage = Res.GetString("9bb7a3e0-1b91-4fc5-8bc1-4328e1787292", "Putaway has been already completed for the Pallet ID {0}.", palletIDAndInvs.Key);
					}
					else
					{
						var transferLine = ((WhsReceiveLine)inventoryWithPutawayTransfer.InDocketLine).PutawayTransferLine;
						if (transferLine.WE_GS_NKPutawayBy == ZString.Empty)
						{
							transferLine.WE_GS_NKPutawayBy = staffCode;
						}
					}
				}
				else
				{
					(transfer, errorMessage) = CreatePutawayTransfer(inventories, palletIDAndInvs.Key, multiPallet, factory, staffCode);
					if (transfer != null && transfer.Lines.Count > 0)
					{
						newTransferLines.AddRange(transfer.Lines.Cast<WhsTransferLine>());
					}
				}

				if (!errorMessage.IsNullOrEmpty())
				{
					break;
				}
			}

			if (errorMessage.IsNullOrEmpty())
			{
				errorMessage = CheckDGLimitCapacity(newTransferLines.ToArray(), factory, warehouse);
			}

			return errorMessage;
		}

		string CheckDGLimitCapacity(IReadOnlyCollection<WhsDocketLine> docketLines, BusinessObjectFactory factory, WhsWarehouse warehouse)
		{
			string message = null;
			var substancesWithExceededLimits = DocketUNDGValidationHelper.CheckUNDGTotalsForWarehouse(
				factory,
				warehouse,
				ZGuid.Empty,
				docketLines).OverLimitMessage;

			if (!string.IsNullOrEmpty(substancesWithExceededLimits))
			{
				var limitError = Res.GetString("ca2e5d91-2adb-4f5e-8ab4-1a91e5d6c18d", "The following UNDG Limits will exceed 100% of warehouse capacity limit by putting away this job:");
				message = limitError + "\r\n" + substancesWithExceededLimits;
			}

			return message;
		}

		bool IsPalletIdOnInventoriesWithPutawayTransferAndAlsoUnloadedToDockdoor(WhsInventoryView[] inventories, WhsInventoryView inventoryWithPutawayTransfer) =>
			inventories.Except(new[] { inventoryWithPutawayTransfer }).Any(inventory => inventory.IsReceivedIntoDockDoor && !inventory.HasPutawayTransfer);

		bool IsPutawayAlreadyCompletedOnPalletID(WhsInventoryView inventoryWithPutawayTransfer)
			=> PutawayHelper.GetPutawayTransferLineFromInventory(inventoryWithPutawayTransfer).WE_OriginalInventoryStatus == InventoryStatus.Codes.Putaway;

		(WhsTransfer, string) CreatePutawayTransfer(WhsInventoryView[] inventories, string palletID, bool multiPallet, BusinessObjectFactory factory, string staffCode)
		{
			WhsTransfer transfer = null;
			string errorMessage = null;
			var firstInventory = inventories[0];
			var validStatuses = new ZString[] { InventoryStatus.Codes.Arrived, InventoryStatus.Codes.Received, InventoryStatus.Codes.Putaway };
			var valid = validStatuses.Contains(firstInventory.WI_InventoryStatus) && inventories.All(i => i.WI_InventoryStatus == firstInventory.WI_InventoryStatus);
			if (!valid)
			{
				errorMessage = Res.GetString("ce91d3c7-34d6-44b4-a18e-50f54a834f1f", "Putaway transfer creation failed because Pallet ID {0} must be entirely either {1}, {2} or {3}.",
					palletID, InventoryStatus.Descriptions.Arrived, InventoryStatus.Descriptions.Received, InventoryStatus.Descriptions.Putaway);
			}
			else if (inventories.Select(l => l.WI_OH_Client).Distinct().Skip(1).Any())
			{
				errorMessage = Res.GetString("f42b5fd9-8848-4b04-b08a-8c876cd73588", "Putaway transfer creation failed because this Pallet ID {0} has products belong to multiple clients.", palletID);
			}
			else
			{
				(transfer, errorMessage) = CreatePutawayTransferCore(inventories, multiPallet, factory, staffCode);
			}

			return (transfer, errorMessage);
		}

		(WhsTransfer, string) CreatePutawayTransferCore(WhsInventoryView[] inventoryLines, bool multiPallet, BusinessObjectFactory factory, string staffCode)
		{
			var referenceInventory = inventoryLines[0];
			var transfer = GetOrCreateWhsTransferWithFetchHints(inventoryLines, referenceInventory.Warehouse.PK, referenceInventory.WI_OH_Client, factory);
			string errorMessage = null;

			var transferLinesDictionary = new Dictionary<ZString, WhsTransferLine>();
			var reserveLinesDictionary = new Dictionary<ZGuid, List<WhsPickLine>>();

			factory.AddFetchHint(typeof(WhsBondedWarehouseAttribute), new ZQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, inventoryLines.Select(i => i.PK).ToArray()));

			var defaultInboundDockDoor = transfer.Warehouse.WW_DefaultInboundDockDoor;
			var newTransferLines = new HashSet<WhsTransferLine>();
			foreach (var receivedInventory in inventoryLines)
			{
				var transfer_destination = ZGuid.Invalid;
				if (!receivedInventory.IsReceivedIntoDockDoor)
				{
					transfer_destination = receivedInventory.WI_WL;
					receivedInventory.WI_WL = defaultInboundDockDoor;
				}

				var transferLine = CreateTransferLineFromInventoryOneToOne(transfer, receivedInventory, staffCode, transfer_destination, multiPallet);

				// it will move reserve lines to new transfer line,
				// if reserve more than transfer quantity it will split and move transfer quantity part with same Original reserve Quantity
				var reservePickLines = receivedInventory.ReservedPickLines.ToList();
				if (reservePickLines.Count > 0)
				{
					AddReserveLinesToDictionary(reserveLinesDictionary, transferLine, (WhsReceiveLine)receivedInventory.InDocketLine, reservePickLines);
				}

				newTransferLines.Add(transferLine);
			}

			// we pick all the Transfer Lines as the Pallet has been physically picked
			var now = ZDateTimeOffset.Now;
			foreach (var transferLine in newTransferLines)
			{
				PickTransferLineAndMoveReservePickLines(reserveLinesDictionary, now, transferLine);
			}

			transfer.RunPreSaveValidation();

			if (transfer.HasErrors)
			{
				var message = new ZStringBuilder(Res.GetString("d3440e6c-ce28-406c-82fc-ae6c05e51bbc", "Putaway Transfer creation failed."));
				message.AppendLine(transfer.NotificationsIncludingChildren.ToUniqueMessageListString());
				errorMessage = message.ToString();
			}
			else if (!multiPallet)
			{
				AddProcessTasksFetchHints(inventoryLines.Select(inventory => inventory.WI_WD_Proxy).Distinct(), factory);
				AddProcessTasksFetchHints(new[] { transfer.PK }, factory);
			}

			return (transfer, errorMessage);
		}

		WhsTransferLine CreateTransferLineFromInventoryOneToOne(WhsTransfer transfer, WhsInventoryView inventory, string staffCode, ZGuid transfer_destination, bool multiPallet)
		{
			var transferLine = (WhsTransferLine)transfer.CreateDocketLineFromInventory(inventory);
			transferLine.GS_NKPickedBy = staffCode;
			transferLine.WE_GS_NKPutawayBy = multiPallet ? staffCode : ZString.Empty;
			if (transfer_destination.IsValid)
			{
				transferLine.WE_WL = transfer_destination;
			}

			var pickLine = transferLine.PickLines.AddNew();
			pickLine.WZ_WE_InventoryLine = inventory.PK;
			pickLine.WZ_Units = inventory.WI_AvailableToTransferQuantity;
			pickLine.WZ_GS_NKAssignedTo = staffCode;

			return transferLine;
		}

		WhsTransfer GetOrCreateWhsTransferWithFetchHints(IEnumerable<WhsInventoryView> inventories, ZGuid warehousePK, ZGuid clientPK, BusinessObjectFactory factory)
		{
			AddWhsReceiveFetchHints(inventories.Select(s => s.WI_WD), factory);
			var receives = IEnumerableExtensions.DistinctBy(inventories, d => d.WI_WD).Select(inv => inv.Docket);
			AddReceiveLinesFetchHints(receives, factory);
			var allLines = receives.SelectMany(receive => receive.Lines);

			AddPutawayTransferFetchHints(allLines, factory);
			var putawayTransferLines = allLines
				.Cast<WhsReceiveLine>()
				.Select(line => line.PutawayTransferLine)
				.Where(putawayTransferLine => putawayTransferLine != null);

			var putawayTransfer = putawayTransferLines?
				.Select(putawayTransferLine => putawayTransferLine.Docket)
				.Where(transfer => !transfer.IsFinalised)
				.OrderBy(docket => docket.WD_DocketID)
				.FirstOrDefault();

			if (putawayTransfer == null)
			{
				putawayTransfer = CreateWhsTransfer(warehousePK, clientPK, factory);
				putawayTransfer.WD_IsPutawayTransfer = true;
			}

			return putawayTransfer;
		}

		WhsTransfer CreateWhsTransfer(ZGuid warehousePK, ZGuid clientPK, BusinessObjectFactory factory)
		{
			WhsTransfer transfer = factory.New<WhsTransfer>();
			transfer.WD_OH_Client = clientPK;
			transfer.WD_WW_Whs = warehousePK;
			transfer.WD_DocketSubType = TransferType.Codes.Internal;
			return transfer;
		}

		void AddPutawayTransferFetchHints(IEnumerable<IWhsDocketLine> receiveLines, BusinessObjectFactory factory)
		{
			var inventoriesQuery = new ZQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, receiveLines.Select(receiveLine => receiveLine.PK));
			factory.AddFetchHint(typeof(WhsInventoryView), inventoriesQuery);

			var docketsQuery = new ZQuery(WhsDocketSchema.PK, receiveLines.Select(receiveLine => receiveLine.WE_WD));
			factory.AddFetchHint(typeof(WhsReceive), docketsQuery);

			var allInventories = receiveLines.SelectMany(line => ((WhsDocketLine)line).Inventory).Cast<WhsInventoryView>();
			AddInventoryToPickLineFetchHints(allInventories, factory);

			var allPickLines = allInventories.SelectMany(inv => inv.AllPickLines);
			foreach (var pickLine in allPickLines)
			{
				factory.AddFetchHint(typeof(WhsDocketLine), pickLine.WZ_WE_TransactionLine);
			}
			LoadAllPutawayTransfers(allPickLines, factory);
		}

		void AddInventoryToPickLineFetchHints(IEnumerable<WhsInventoryView> inventories, BusinessObjectFactory factory)
		{
			var query = new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, inventories.Select(inventory => inventory.WI_WE_InDocketLine));
			factory.AddFetchHint(typeof(WhsPickLine), query);
		}

		IEnumerable<WhsDocket> LoadAllPutawayTransfers(IEnumerable<WhsPickLine> allPickLines, BusinessObjectFactory factory)
		{
			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.PK, allPickLines.Select(l => l.PK));

			var transferLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			transferLineSubQuery.AddSubQuery(pickLineSubQuery, JoinCondition.And);

			var putawayTransferQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			putawayTransferQuery.AddSubQuery(transferLineSubQuery, JoinCondition.And);
			putawayTransferQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);

			return factory.Load<WhsDocket>(putawayTransferQuery);
		}

		void AddReserveLinesToDictionary(Dictionary<ZGuid, List<WhsPickLine>> reserveLinesDictionary, WhsTransferLine transferLine, WhsReceiveLine receiveLine, IEnumerable<WhsPickLine> inventoryReservePickLines)
		{
			var reserveLinesToAdd = receiveLine.ReservedQuantity > receiveLine.WE_TransactionQuantity
				? SplitReserveLines(receiveLine, inventoryReservePickLines)
				: inventoryReservePickLines;

			AddReserveLinesToDictionaryCore(reserveLinesDictionary, transferLine, reserveLinesToAdd);
		}

		void AddReserveLinesToDictionaryCore(Dictionary<ZGuid, List<WhsPickLine>> reserveLinesDictionary, WhsTransferLine transferLine, IEnumerable<WhsPickLine> inventoryReservePickLines)
		{
			if (reserveLinesDictionary.ContainsKey(transferLine.PK))
			{
				reserveLinesDictionary[transferLine.PK].AddRange(inventoryReservePickLines);
			}
			else
			{
				reserveLinesDictionary.Add(transferLine.PK, inventoryReservePickLines.ToList());
			}
		}

		void PickTransferLineAndMoveReservePickLines(Dictionary<ZGuid, List<WhsPickLine>> reserveLinesDictionary, ZDateTimeOffset pickTime, WhsTransferLine transferLine)
		{
			if (reserveLinesDictionary.TryGetValue(transferLine.PK, out var reservePickLines))
			{
				reservePickLines.ForEach(pickLine => pickLine.WZ_WE_InventoryLine = transferLine.PK);
			}
			transferLine.PickedTime = pickTime;

			if (reservePickLines != null)
			{
				MergeDuplicateReserveLines(reservePickLines, transferLine.Factory);
			}
		}

		void MergeDuplicateReserveLines(List<WhsPickLine> reservePickLines, BusinessObjectFactory factory)
		{
			var grouping = reservePickLines.GroupBy(l => (l.WZ_WE_InventoryLine, l.WZ_WE_TransactionLine));
			foreach (var duplicate in grouping.Where(g => g.Count() > 1))
			{
				WhsPickLine.ReserveInventory_Unsafe(factory, duplicate.Key.WZ_WE_InventoryLine, duplicate.Key.WZ_WE_TransactionLine, duplicate.Sum(pl => pl.WZ_Units));
				duplicate.ToList().ForEach(line => line.Delete());
			}
		}

		IEnumerable<WhsPickLine> SplitReserveLines(WhsReceiveLine receiveLine, IEnumerable<WhsPickLine> inventoryReservePickLines)
		{
			var reserveQtyToRetain = receiveLine.WE_TransactionQuantity;
			var newReceiveLine = SplitAndReturnNewReceiveLine(receiveLine, reserveQtyToRetain);
			var reservePickLinesToReturn = new List<WhsPickLine>();

			foreach (var pickLine in inventoryReservePickLines.OrderBy(pickLine => Math.Abs(pickLine.ReservedQuantity - reserveQtyToRetain)))
			{
				if (reserveQtyToRetain > 0)
				{
					reserveQtyToRetain = RetainOrSplitReservedPickLineAndReturnUnassignedQty(reserveQtyToRetain, newReceiveLine, pickLine);
					reservePickLinesToReturn.Add(pickLine);
				}
				else
				{
					pickLine.WZ_WE_InventoryLine = newReceiveLine.PK;
				}
			}

			return reservePickLinesToReturn;
		}

		static ZDecimal RetainOrSplitReservedPickLineAndReturnUnassignedQty(ZDecimal reserveQtyToRetain, WhsReceiveLine newReceiveLine, WhsPickLine pickLine)
		{
			var qtyToReturn = reserveQtyToRetain;
			if (pickLine.ReservedQuantity <= reserveQtyToRetain)
			{
				qtyToReturn -= pickLine.ReservedQuantity;
			}
			else
			{
				var qtyToReserveToNewInventory = pickLine.ReservedQuantity - reserveQtyToRetain;
				pickLine.ReservedQuantity = reserveQtyToRetain;
				((WhsOrderLine)pickLine.DocketLine).ReserveStockIfAbleTo(newReceiveLine.Inventory[0], qtyToReserveToNewInventory);
				qtyToReturn = 0;
			}

			return qtyToReturn;
		}

		WhsReceiveLine SplitAndReturnNewReceiveLine(WhsReceiveLine receiveLineToSplit, ZDecimal qtyToSplit)
		{
			var newReceiveLine = (WhsReceiveLine)receiveLineToSplit.Clone(new BusinessObjectCloneArgs(new[] { WhsDocketLineSchema.Constants.WE_WL }));
			receiveLineToSplit.WE_ClientOrderedUnits = qtyToSplit;

			receiveLineToSplit.Docket.Lines.Add(newReceiveLine);
			newReceiveLine.WE_ClientOrderedUnits -= qtyToSplit;
			newReceiveLine.WE_TransactionQuantity = 0;

			return newReceiveLine;
		}

		#region AddFetchHints

		static void AddProcessTasksFetchHints(IEnumerable<ZGuid> parentPKs, BusinessObjectFactory factory)
		{
			foreach (var pk in parentPKs)
			{
				factory.AddFetchHint(ProcessTasksSchema.Instance, new ZQuery(ProcessTasksSchema.P9_ParentID, pk));
			}
		}

		static void AddWhsReceiveFetchHints(IEnumerable<ZGuid> pks, BusinessObjectFactory factory)
		{
			var docketToDocketLineQuery = new ZQuery(WhsDocketSchema.PK, pks);
			factory.AddFetchHint(typeof(WhsDocket), docketToDocketLineQuery);
		}

		static void AddReceiveLinesFetchHints(IEnumerable<WhsDocket> receives, BusinessObjectFactory factory)
		{
			foreach (var receive in receives)
			{
				var docketToDocketLineQuery = new ZQuery(WhsDocketLineSchema.WE_WD, receive.PK);

				// reproducing the same query used in active business objects
				var additionalDocketLineFilter = new ZQuery();
				additionalDocketLineFilter.AddToFilter(new ZQuery(WhsDocketLineSchema.WE_IsOriginalInventory, ZBool.True));
				additionalDocketLineFilter.AddToFilter(new ZQuery(), JoinCondition.And);

				docketToDocketLineQuery.AddToFilter(additionalDocketLineFilter);
				factory.AddFetchHint(typeof(WhsDocketLine), docketToDocketLineQuery);
			}
		}

		#endregion
	}
}
