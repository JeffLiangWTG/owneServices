using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	// tested in UnloadWhsReceiveLineTest.cs
	public class ReceiveLinesUpdaterForUnload : IReceiveLinesUpdater
	{
		ZDecimal IReceiveLinesUpdater.UpdateReceiveLinesAndReturnExcessUnloadQty(WhsReceive receive, List<WhsReceiveLine> receiveLines, decimal unloadQty, string packUQ, ZGuid locationPK, string palletId, bool isDirectPutawayLocation)
		{
			if (!receiveLines.Any())
			{
				throw new ArgumentException("There are no receive lines to unload to.");
			}

			var availableQtyToUnload = UnloadToReceiveLinesAndReturnExcessQty(receiveLines, unloadQty, packUQ, locationPK, isDirectPutawayLocation,
				(receiveLine) => receiveLine.ReservedQuantity > receiveLine.WE_TransactionQuantity,
				(receiveLine) => receiveLine.ReservedQuantity);

			availableQtyToUnload = UnloadToReceiveLinesAndReturnExcessQty(receiveLines, availableQtyToUnload, packUQ, locationPK, isDirectPutawayLocation,
				(receiveLine) => receiveLine.WE_TransactionQuantity > 0 && receiveLine.WE_TransactionQuantity < receiveLine.WE_ClientOrderedUnits,
				(receiveLine) => receiveLine.WE_ClientOrderedUnits);

			availableQtyToUnload = UnloadToReceiveLinesAndReturnExcessQty(receiveLines, availableQtyToUnload, packUQ, locationPK, isDirectPutawayLocation,
				(receiveLine) => receiveLine.WE_ClientOrderedUnits > 0,
				(receiveLine) => receiveLine.WE_ClientOrderedUnits);

			return availableQtyToUnload;
		}

		static decimal UnloadToReceiveLinesAndReturnExcessQty(IEnumerable<WhsReceiveLine> receiveLines, decimal qtyToUnload, string packUQ, ZGuid locationPK, bool isDirectPutawayLocation, Func<WhsReceiveLine, bool> receiveLineIdentifier, Func<WhsReceiveLine, ZDecimal> getQtyToConsider)
		{
			var receiveLinesToUnloadTo = qtyToUnload > 0 ? receiveLines.Where(receiveLineIdentifier) : Enumerable.Empty<WhsReceiveLine>();

			return receiveLinesToUnloadTo.Any()
				? UnloadToInventoriesAndReturnExcessQtyCore(receiveLinesToUnloadTo, qtyToUnload, packUQ, locationPK, isDirectPutawayLocation, getQtyToConsider)
				: qtyToUnload;
		}

		static decimal UnloadToInventoriesAndReturnExcessQtyCore(IEnumerable<WhsReceiveLine> receiveLines, decimal qtyToUnload, string packUQ, ZGuid locationPK, bool isDirectPutawayLocation, Func<WhsReceiveLine, ZDecimal> getQtyToConsider)
		{
			var availableQtyToUnload = qtyToUnload;
			foreach (var receiveLine in receiveLines.OrderBy(rl => rl.WE_LineNo))
			{
				var unfulfilledQty = getQtyToConsider(receiveLine) - receiveLine.WE_TransactionQuantity;
				if (unfulfilledQty > 0)
				{
					if (packUQ == receiveLine.WE_F3_NKPackType)
					{
						availableQtyToUnload = UnloadToLineAndReturnExcessQtyCore(receiveLine, unfulfilledQty, availableQtyToUnload, locationPK, isDirectPutawayLocation);
					}
					else if (receiveLine.WE_ClientOrderedUnits > 0 && receiveLine.WE_TransactionQuantity == 0)
					{
						receiveLine.WE_F3_NKPackType = packUQ;
						availableQtyToUnload = UnloadToLineAndReturnExcessQtyCore(receiveLine, unfulfilledQty, availableQtyToUnload, locationPK, isDirectPutawayLocation);
					}
				}

				if (availableQtyToUnload == 0)
				{
					break;
				}
			}

			return availableQtyToUnload;
		}

		static decimal UnloadToLineAndReturnExcessQtyCore(WhsReceiveLine receiveLine, decimal unfulfilledQty, decimal availableQtyToUnload, ZGuid locationPK, bool isDirectPutawayLocation)
		{
			if (unfulfilledQty >= availableQtyToUnload)
			{
				receiveLine.WE_TransactionQuantity += availableQtyToUnload;
				availableQtyToUnload = 0;
			}
			else
			{
				receiveLine.WE_TransactionQuantity += unfulfilledQty;
				availableQtyToUnload -= unfulfilledQty;
			}

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			receiveLine.Logs.AddNew(ZArchitecture.Business.Events.EditedARecord, "RF");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			if (receiveLine.WE_WL.IsEmpty)
			{
				receiveLine.WE_WL = locationPK;
			}

			if (isDirectPutawayLocation)
			{
				WebServiceHelper.GenerateWarehouseConfirmedPutAwayEvent(receiveLine);
			}

			return availableQtyToUnload;
		}
	}
}
