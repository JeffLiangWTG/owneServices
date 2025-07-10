using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	// tested in UnloadWhsReceiveLineTest.cs
	public class ReceiveLineUpdaterForUnloadWithEmptyPalletIdMatching : IReceiveLinesUpdater
	{
		ZDecimal IReceiveLinesUpdater.UpdateReceiveLinesAndReturnExcessUnloadQty(WhsReceive receive, List<WhsReceiveLine> receiveLines, decimal unloadQty, string packUQ, ZGuid locationPK, string palletId, bool isDirectPutawayLocation)
		{
			if (!receiveLines.Any())
			{
				throw new ArgumentException("There are no receive lines to unload to.");
			}
			else if (receiveLines.Any(rl => rl.WE_TransactionQuantity > 0 || rl.WE_ClientOrderedUnits == 0))
			{
				throw new ArgumentException("Invalid receive lines included in the collection of receive lines to unload to.");
			}

			var excessQty = UnloadAndSplitReceiveLinesThenReturnExcessQty(receive, unloadQty, packUQ, locationPK, palletId, isDirectPutawayLocation, receiveLines,
				(receiveLine) => receiveLine.ReservedQuantity > 0,
				(receiveLine) => receiveLine.ReservedQuantity);

			excessQty = UnloadAndSplitReceiveLinesThenReturnExcessQty(receive, excessQty, packUQ, locationPK, palletId, isDirectPutawayLocation, receiveLines,
				(receiveLine) => receiveLine.WE_TransactionQuantity == 0,
				(receiveLine) => receiveLine.WE_ClientOrderedUnits);

			return excessQty;
		}

		static decimal UnloadAndSplitReceiveLinesThenReturnExcessQty(WhsReceive receive, decimal unloadQty, string packUQ, ZGuid locationPK, string palletId, bool isDirectPutawayLocation, List<WhsReceiveLine> receiveLines, Func<WhsReceiveLine, bool> receiveLineIdentifier, Func<WhsReceiveLine, ZDecimal> getQtyToConsider)
		{
			var receiveLinesToUnloadTo = unloadQty > 0 ? receiveLines.Where(receiveLineIdentifier) : Enumerable.Empty<WhsReceiveLine>();

			return receiveLinesToUnloadTo.Any()
				? UnloadAndSplitInventoriesThenReturnExcessQtyCore(receive, unloadQty, packUQ, locationPK, palletId, isDirectPutawayLocation, receiveLines, receiveLinesToUnloadTo, getQtyToConsider)
				: unloadQty;
		}

		static decimal UnloadAndSplitInventoriesThenReturnExcessQtyCore(WhsReceive receive, decimal unloadQty, string packUQ, ZGuid locationPK, string palletId, bool isDirectPutawayLocation, List<WhsReceiveLine> receiveLines, IEnumerable<WhsReceiveLine> receiveLinesToUnloadTo, Func<WhsReceiveLine, ZDecimal> getQtyToConsider)
		{
			var availableQtyToUnload = unloadQty;
			foreach (var receiveLine in receiveLinesToUnloadTo.OrderBy(rl => Math.Abs(getQtyToConsider(rl) - unloadQty)))
			{
				var qtyToUnload = Math.Min(getQtyToConsider(receiveLine), availableQtyToUnload);

				var linePackType = receiveLine.WE_F3_NKPackType;
				if (receiveLine.WE_F3_NKPackType != packUQ)
				{
					receiveLine.WE_F3_NKPackType = packUQ;
				}
				receiveLine.WE_TransactionQuantity = qtyToUnload;

				if (receiveLine.WE_ClientOrderedUnits > qtyToUnload)
				{
					var newReceiveLineFromSplit = SplitReceiveLine(receiveLine, qtyToUnload, linePackType);
					receiveLines.Add(newReceiveLineFromSplit);
					receive.Lines.Add(newReceiveLineFromSplit);
				}

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				receiveLine.Logs.AddNew(ZArchitecture.Business.Events.EditedARecord, "RF");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				receiveLine.WE_WL = locationPK;
				receiveLine.WE_PalletID = palletId;

				if (isDirectPutawayLocation)
				{
					WebServiceHelper.GenerateWarehouseConfirmedPutAwayEvent(receiveLine);
				}

				availableQtyToUnload -= qtyToUnload;
				if (availableQtyToUnload == 0)
				{
					break;
				}
			}

			return availableQtyToUnload;
		}

		static WhsReceiveLine SplitReceiveLine(WhsReceiveLine receiveLine, ZDecimal splitQty, string packType)
		{
			var newReceiveLine = (WhsReceiveLine)receiveLine.Clone();
			newReceiveLine.WE_WD = receiveLine.WE_WD;

			var reservedQuantity = receiveLine.ReservedQuantity;
			if (reservedQuantity > 0 && reservedQuantity > splitQty)
			{
				TransferReservedStockOnReceiveLines(receiveLine, newReceiveLine, reservedQuantity - splitQty);
			}

			var cloneExpectedQuantity = receiveLine.WE_ClientOrderedUnits - splitQty;
			receiveLine.WE_ClientOrderedUnits = splitQty;
			newReceiveLine.WE_ClientOrderedUnits = cloneExpectedQuantity;
			newReceiveLine.WE_F3_NKPackType = packType;
			newReceiveLine.WE_TransactionQuantity = 0;

			return newReceiveLine;
		}

		static void TransferReservedStockOnReceiveLines(WhsReceiveLine receiveLineToTransferFrom, WhsReceiveLine receiveLineToTransferTo, ZDecimal qtyToTransfer)
		{
			var reservedQtyToRemap = qtyToTransfer;

			foreach (var pickLine in receiveLineToTransferFrom.ReservedPickLines.OrderByDescending(line => line.ReservedQuantity))
			{
				if (pickLine.ReservedQuantity <= reservedQtyToRemap)
				{
					pickLine.WZ_WE_InventoryLine = receiveLineToTransferTo.PK;
					reservedQtyToRemap -= pickLine.ReservedQuantity;
				}
				else
				{
					pickLine.ReservedQuantity -= reservedQtyToRemap;
					((WhsOrderLine)pickLine.DocketLine).ReserveStockIfAbleTo(receiveLineToTransferTo.Inventory[0], reservedQtyToRemap);
					reservedQtyToRemap = 0;
				}

				if (reservedQtyToRemap == 0)
				{
					break;
				}
			}
		}
	}
}
