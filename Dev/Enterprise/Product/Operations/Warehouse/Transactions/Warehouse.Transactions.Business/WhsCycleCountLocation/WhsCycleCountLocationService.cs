using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsCycleCountLocationService : IWhsCycleCountLocationService
	{
		#region MarkInventoriesLostInCycleCount

		public void MarkInventoriesLostInCycleCount(ZGuid[] cycleCountPKs)
		{
			var factory = new BusinessObjectFactory();
			MarkInventoriesLostInCycleCountCore(factory, cycleCountPKs);
			factory.Save();
		}

#if DEBUG
		public
#endif

		void MarkInventoriesLostInCycleCountCore(BusinessObjectFactory factory, ZGuid[] cycleCountPKs)
		{
			foreach (var cycleCountPK in cycleCountPKs)
			{
				var cycleCountFound = factory.Load<WhsCycleCountLocation>(cycleCountPK);
				if (!cycleCountFound.IsBulkCycleCount && !cycleCountFound.WCL_EndTime.IsEmpty && cycleCountFound.HasOpenVariances)
				{
					MarkInventoriesLostInCycleCount_CycleCountLocation(factory, cycleCountFound);
					if (cycleCountFound.Variances.Any(v => v.WCC_WL_ExpectedStockLocation.IsValid))
					{
						var alreadyHeldInventories = new Dictionary<WhsInventoryView, ZDecimal>();
						MarkInventoriesLostInCycleCount_SerialNumberInExpectedLocation(factory, cycleCountFound, alreadyHeldInventories);
						MarkInventoriesLostInCycleCount_PalletIDInExpectedLocation(factory, cycleCountFound, alreadyHeldInventories);
					}
				}
			}
		}

		#region MarkInventoriesLostInCycleCount_CycleCountLocation

		void MarkInventoriesLostInCycleCount_CycleCountLocation(BusinessObjectFactory factory, WhsCycleCountLocation cycleCountLocation)
		{
			var inventories = WhsCycleCountLocationHelper.GetInventoriesMatchingNegativeVariances(factory, cycleCountLocation);
			if (inventories.Length > 0)
			{
				AddFetchHintForPickLinesAndGenAddOnColumn(factory, inventories);

				var missedVariances = cycleCountLocation.Variances.Where(v => v.WCC_VarianceQty < 0);
				foreach (var variance in missedVariances)
				{
					var matchedInventories = inventories.Where(i =>
						i.WI_OP == variance.WCC_OP_Product
						&& i.WI_OH_Client == variance.WCC_OH_Client
						&& i.WI_PalletID.EqualsIgnoringCase(variance.WCC_PalletID)
						&& i.WI_PartAttrib1.EqualsIgnoringCase(variance.WCC_PartAttrib1)
						&& i.WI_PartAttrib2.EqualsIgnoringCase(variance.WCC_PartAttrib2)
						&& i.WI_PartAttrib3.EqualsIgnoringCase(variance.WCC_PartAttrib3)
						&& i.WI_SerialNumber.EqualsIgnoringCase(variance.WCC_SerialNumber)
						&& i.WI_ExpiryDate == variance.WCC_ExpiryDate
						&& i.WI_PackingDate == variance.WCC_PackingDate);

					var qtyToHold = (ZDecimal)(Math.Abs(variance.WCC_VarianceQty) - matchedInventories.Where(i => i.WI_HeldCode == InventoryHoldCodes.Codes.LostInCycleCount).Sum(i => i.WI_TotalUnits));
					if (qtyToHold > 0)
					{
						var availableInventories = matchedInventories.Where(i => i.WI_InventoryStatus == InventoryStatus.Codes.Available);
						qtyToHold = HoldInventories(availableInventories, qtyToHold, new Dictionary<WhsInventoryView, ZDecimal>());

						if (qtyToHold > 0)
						{
							var heldInventories = matchedInventories.Where(i => i.WI_InventoryStatus == InventoryStatus.Codes.Held && i.WI_HeldCode != InventoryHoldCodes.Codes.LostInCycleCount);
							HoldInventories(heldInventories, qtyToHold, new Dictionary<WhsInventoryView, ZDecimal>());
						}
					}
				}
			}
		}

		#endregion

		#region MarkInventoriesLostInCycleCount_PalletIDInExpectedLocation

		void MarkInventoriesLostInCycleCount_PalletIDInExpectedLocation(BusinessObjectFactory factory, WhsCycleCountLocation cycleCountLocation, Dictionary<WhsInventoryView, ZDecimal> alreadyHeldInventories)
		{
			var inventories = WhsCycleCountLocationHelper.GetUnexpectedInventoriesWithPalletID(factory, cycleCountLocation);
			AddFetchHintForPickLinesAndGenAddOnColumn(factory, inventories);

			HoldInventories(inventories, decimal.MaxValue, alreadyHeldInventories); // hold everything
		}

		#endregion

		#region MarkInventoriesLostInCycleCount_SerialNumberInExpectedLocation

		void MarkInventoriesLostInCycleCount_SerialNumberInExpectedLocation(BusinessObjectFactory factory, WhsCycleCountLocation cycleCountLocation, Dictionary<WhsInventoryView, ZDecimal> alreadyHeldInventories)
		{
			var inventories = WhsCycleCountLocationHelper.GetUnexpectedInventoriesWithSerialNumber(factory, cycleCountLocation);
			AddFetchHintForPickLinesAndGenAddOnColumn(factory, inventories);

			HoldInventories(inventories, decimal.MaxValue, alreadyHeldInventories); // hold everything
		}

		#endregion

		#region HoldInventories

		ZDecimal HoldInventories(IEnumerable<WhsInventoryView> matchedInventories, ZDecimal qtyToHold, Dictionary<WhsInventoryView, ZDecimal> alreadyHeldInventories)
		{
			foreach (var inventory in matchedInventories)
			{
				if (alreadyHeldInventories.TryGetValue(inventory, out var qtyAlreadyHeld))
				{
					qtyToHold -= qtyAlreadyHeld;
				}
				else
				{
					var qtyHeld = WhsCycleCountLocationHelper.HoldInventory(inventory, InventoryHoldCodes.Codes.LostInCycleCount, qtyToHold);
					alreadyHeldInventories[inventory] = qtyHeld;
					qtyToHold -= qtyHeld;
				}

				if (qtyToHold == 0)
				{
					break;
				}
			}

			return qtyToHold;
		}

		#endregion

		#region AddFetchHintForPickLines

		void AddFetchHintForPickLinesAndGenAddOnColumn(BusinessObjectFactory factory, IEnumerable<WhsInventoryView> inventories)
		{
			var inventoriesForFetchHint = inventories.Where(i => i.WI_InventoryStatus == InventoryStatus.Codes.Available || (i.WI_InventoryStatus == InventoryStatus.Codes.Held && i.WI_HeldCode != InventoryHoldCodes.Codes.LostInCycleCount));
			inventoriesForFetchHint.ForEach(i =>
			{
				factory.AddFetchHint(GenAddOnColumnSchema.XA_ParentID, i.WI_WE_InDocketLine);
				factory.AddFetchHint(WhsPickLineSchema.Instance, GetReservedPickLineFetchHintQuery(i));
				factory.AddFetchHint(WhsPickLineSchema.Instance, GetCommittedPickLineFetchHintQuery(i));
				factory.AddFetchHint(WhsInventoryHoldChangeLogSchema.Instance, new ZQuery(WhsInventoryHoldChangeLogSchema.WHL_WE_ParentDocketLine, i.WI_WE_InDocketLine));
			});
		}

		ZQuery GetReservedPickLineFetchHintQuery(WhsInventoryView inventory)
		{
			var query = new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, inventory.WI_WE_InDocketLine);
			var reservedPickLineQuery = new ZQuery();
			reservedPickLineQuery.AddToFilter(WhsPickLineSchema.WZ_OriginalReservedQty, SQLComparisonOperator.GreaterThan, 0m);
			reservedPickLineQuery.AddToFilter(WhsPickLineSchema.WZ_PickedDateTime, null);
			query.AddToFilter(reservedPickLineQuery);

			return query;
		}

		ZQuery GetCommittedPickLineFetchHintQuery(WhsInventoryView inventory)
		{
			var query = new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, inventory.WI_WE_InDocketLine);
			query.AddToFilter(WhsPickLineSchema.WZ_PickedDateTime, null);

			return query;
		}

		#endregion

		#endregion
	}
}
