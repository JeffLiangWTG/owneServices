using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.AsnMapping
{
	class LinesMapper
	{
		public LinesMapper(WhsReceive receive)
		{
			Receive = Argument.NotNull(receive, nameof(receive));
		}

		WhsReceive Receive { get; }

		#region MapInventoriesToAsnLines

		public void MapInventoriesToAsnLines(List<WhsInventoryView> inventoriesToMap, IEnumerable<WhsAsnLine> asnLines)
		{
			if (asnLines.Any())
			{
				if (!AreInventoriesAndAsnLinesForTheSameProduct(inventoriesToMap, asnLines))
				{
					throw new ArgumentException("Lines mapper works only with inventories and asnLines of the same product.");
				}

				foreach (var asnLine in asnLines
					.OrderByDescending(asnLine => asnLine.UsedAttributesAndPalletIdCount)
					.ThenBy(asnLine => asnLine.WN_LineNo)
					.ThenBy(asnLine => asnLine.WN_SubLineNo))
				{
					ReconcileAsnLineToInventories(asnLine, inventoriesToMap);
				}
			}
		}

		static bool AreInventoriesAndAsnLinesForTheSameProduct(IEnumerable<WhsInventoryView> inventories, IEnumerable<WhsAsnLine> asnLines)
		{
			return inventories.Select(x => x.WI_OP)
				.Union(asnLines.Select(x => x.WN_OP))
				.IsCountEqualTo(1);
		}

		void ReconcileAsnLineToInventories(WhsAsnLine asnLine, List<WhsInventoryView> inventoriesToMap)
		{
			var inventories = GetPossibleInventoriesForProductInfo(new ProductInfo(asnLine),
				inventoriesToMap.Where(inventory => inventory.WI_ExpectedReceiptQuantity < inventory.WI_InDocketLineUnits || inventory.WI_ExpectedReceiptQuantity == 0));

			var unitsToReconcile = asnLine.WN_Quantity;
			var sortedInventories = inventories
				.OrderByDescending(inventory => inventory.WI_LineNo == asnLine.WN_LineNo && inventory.WI_SubLineNo == asnLine.WN_SubLineNo) // exact LineNo & SubLineNo match first
				.ThenBy(inventory => inventory.UsedAttributesAndPalletIdCount)
				.ThenByDescending(inventory => inventory.WI_InDocketLineUnits - unitsToReconcile);

			foreach (var inventory in sortedInventories)
			{
				unitsToReconcile = AllocateInventoryToAsnLine(inventory, asnLine, unitsToReconcile, inventoriesToMap);

				if (unitsToReconcile == 0)
				{
					break;
				}
			}

			if (unitsToReconcile > 0)
			{
				var exactMatchedInventory = sortedInventories.FirstOrDefault(line => line.UsedAttributesAndPalletIdCount == asnLine.UsedAttributesAndPalletIdCount);
				if (exactMatchedInventory == null)
				{
					CreateNewInventoryLineFromAsnLine(asnLine, unitsToReconcile);
				}
				else
				{
					AllocateInventoryAndAssignLineNumberIfNecessary(exactMatchedInventory, asnLine, unitsToReconcile);
				}
			}
		}

		ZDecimal AllocateInventoryToAsnLine(WhsInventoryView inventory, WhsAsnLine asnLine, ZDecimal unitsToReconcile, List<WhsInventoryView> inventoriesToMap)
		{
			var qtyToApply = inventory.WI_InDocketLineUnits - inventory.WI_ExpectedReceiptQuantity;
			var leftOver = unitsToReconcile;

			if (qtyToApply > 0)
			{
				if (qtyToApply > unitsToReconcile)
				{
					if (inventory.WI_LineNo == 0 && !inventory.HasPutawayTransfer)
					{
						var splitQty = qtyToApply - unitsToReconcile;
						inventoriesToMap.Add(SplitInventory(inventory, splitQty));
					}

					qtyToApply = unitsToReconcile;
				}

				AllocateInventoryAndAssignLineNumberIfNecessary(inventory, asnLine, qtyToApply);
				leftOver -= qtyToApply;
			}

			return leftOver;
		}

		WhsInventoryView SplitInventory(WhsInventoryView inventory, ZDecimal splitQty)
		{
			inventory.WI_InDocketLineUnits -= splitQty;

			var clonedInventory = (WhsInventoryView)inventory.Clone();
			clonedInventory.WI_LineNo = 0;
			clonedInventory.WI_SubLineNo = 0;
			clonedInventory.WI_InDocketLineUnits = splitQty;
			Receive.Inventory.Add(clonedInventory);

			if (inventory.ReservedPickLines.Count > 0)
			{
				TransferReservedStockOnInventory(inventory, clonedInventory, splitQty);
			}

			return clonedInventory;
		}

		static void TransferReservedStockOnInventory(WhsInventoryView inventoryToTransferFrom, WhsInventoryView inventoryToTransferTo, ZDecimal qtyToTransfer)
		{
			var reservedQtyToRemap = qtyToTransfer;

			foreach (var pickLine in inventoryToTransferFrom.ReservedPickLines.OrderByDescending(line => line.ReservedQuantity))
			{
				if (pickLine.ReservedQuantity <= reservedQtyToRemap)
				{
					pickLine.WZ_WE_InventoryLine = inventoryToTransferTo.PK;
					reservedQtyToRemap -= pickLine.ReservedQuantity;
				}
				else
				{
					pickLine.ReservedQuantity -= reservedQtyToRemap;
					((WhsOrderLine)pickLine.DocketLine).ReserveStockIfAbleTo(inventoryToTransferTo, reservedQtyToRemap);
					reservedQtyToRemap = 0;
				}

				if (reservedQtyToRemap == 0)
				{
					break;
				}
			}
		}

		void AllocateInventoryAndAssignLineNumberIfNecessary(WhsInventoryView inventory, WhsAsnLine asnLine, ZDecimal qtyToAllocate)
		{
			if (inventory.WI_LineNo == 0)
			{
				inventory.WI_LineNo = (ZShort)asnLine.WN_LineNo;
				inventory.WI_SubLineNo = (ZShort)asnLine.WN_SubLineNo;
			}

			inventory.WI_ExpectedReceiptQuantity += qtyToAllocate;
		}

		#region CreateNewInventoryLineFromAsnLine

		WhsInventoryView CreateNewInventoryLineFromAsnLine(WhsAsnLine asnLine, ZDecimal availableQty)
		{
			var receiveLine = Receive.Lines.AddNew();
			receiveLine.WE_OP = asnLine.WN_OP;
			receiveLine.WE_StockOnHand = 0m;
			receiveLine.WE_ClientOrderedUnits = availableQty;
			receiveLine.WE_ExpiryDate = asnLine.WN_ExpiryDate;
			receiveLine.WE_PackingDate = asnLine.WN_PackingDate;
			receiveLine.WE_PartAttrib1 = asnLine.WN_PartAttrib1;
			receiveLine.WE_PartAttrib2 = asnLine.WN_PartAttrib2;
			receiveLine.WE_PartAttrib3 = asnLine.WN_PartAttrib3;
			receiveLine.WE_SerialNumber = asnLine.WN_SerialNumber;
			receiveLine.WE_PalletID = asnLine.WN_PalletId;
			receiveLine.WE_LineNo = (ZShort)asnLine.WN_LineNo;
			receiveLine.WE_SubLineNo = (ZShort)asnLine.WN_SubLineNo;

			return receiveLine.Inventory[0];
		}

		#endregion

		#endregion

		#region RemapUnfulfilledReservedStock

		public static void RemapUnfulfilledReservedStock(WhsReceive receive)
		{
			var overReservedInventoriesDictionary = new Dictionary<ZGuid, List<WhsReceiveLine>>();
			var underReservedInventoriesDictionary = new Dictionary<ZGuid, List<WhsReceiveLine>>();
			foreach (var receiveLine in receive.Lines.Cast<WhsReceiveLine>())
			{
				var reservedQty = receiveLine.ReservedQuantity;
				if (receiveLine.WE_TransactionQuantity < reservedQty)
				{
					AddReceiveLineToDictionary(receiveLine, overReservedInventoriesDictionary);
				}
				else if (receiveLine.WE_TransactionQuantity > reservedQty)
				{
					AddReceiveLineToDictionary(receiveLine, underReservedInventoriesDictionary);
				}
			}

			foreach (var key in overReservedInventoriesDictionary.Keys)
			{
				if (underReservedInventoriesDictionary.TryGetValue(key, out List<WhsReceiveLine> inventoriesToRemapReservedStocksTo))
				{
					RemapUnfulfilledReservedStockCore(overReservedInventoriesDictionary[key], inventoriesToRemapReservedStocksTo);
				}
			}
		}

		static void AddReceiveLineToDictionary(WhsReceiveLine receiveLine, Dictionary<ZGuid, List<WhsReceiveLine>> dictionary)
		{
			if (!dictionary.TryGetValue(receiveLine.WE_OP, out List<WhsReceiveLine> receiveLines))
			{
				receiveLines = new List<WhsReceiveLine>();
				dictionary.Add(receiveLine.WE_OP, receiveLines);
			}
			receiveLines.Add(receiveLine);
		}

		static void RemapUnfulfilledReservedStockCore(IEnumerable<WhsReceiveLine> receiveLinesToMapReservedStocksFrom, IEnumerable<WhsReceiveLine> receiveLinesToMapReservedStocksTo)
		{
			var reservingDocketLinesAndPickLinesToRemap = GetReservingDocketLinesAndPickLinesToRemap(receiveLinesToMapReservedStocksFrom);

			foreach (var reservingDocketLineAndPickLineToRemap in reservingDocketLinesAndPickLinesToRemap
				.OrderByDescending(line => line.DocketLineUsedAttributesCount)
				.ThenByDescending(line => line.QtyToRemap))
			{
				TransferUnfulfilledReservedStockToOtherInventories(reservingDocketLineAndPickLineToRemap, receiveLinesToMapReservedStocksTo);
			}
		}

		static IEnumerable<ReservingDocketLineAndPickLineToRemap> GetReservingDocketLinesAndPickLinesToRemap(IEnumerable<WhsReceiveLine> receiveLinesToMapReservedStocksFrom)
		{
			var reservingDocketLineAndPickLineToRemap = new List<ReservingDocketLineAndPickLineToRemap>();
			foreach (var receiveLine in receiveLinesToMapReservedStocksFrom)
			{
				var reservingPickLinesAndDocketLines = GetReservingPickLinesAndDocketLines(receiveLine);
				var reservedQtyToTransfer = receiveLine.ReservedQuantity - receiveLine.WE_TransactionQuantity;

				foreach (var reservingPickAndDocketLine in reservingPickLinesAndDocketLines
					.OrderBy(line => line.DocketLineUsedAttributesCount)
					.ThenByDescending(line => line.PickLine.ReservedQuantity))
				{
					if (reservingPickAndDocketLine.PickLine.ReservedQuantity >= reservedQtyToTransfer)
					{
						reservingPickAndDocketLine.QtyToRemap = reservedQtyToTransfer;
						reservingDocketLineAndPickLineToRemap.Add(reservingPickAndDocketLine);
						break;
					}
					else
					{
						reservingPickAndDocketLine.QtyToRemap = reservingPickAndDocketLine.PickLine.ReservedQuantity;
						reservingDocketLineAndPickLineToRemap.Add(reservingPickAndDocketLine);
						reservedQtyToTransfer -= reservingPickAndDocketLine.QtyToRemap;
					}
				}
			}

			return reservingDocketLineAndPickLineToRemap;
		}

		static IEnumerable<ReservingDocketLineAndPickLineToRemap> GetReservingPickLinesAndDocketLines(WhsReceiveLine receiveLine)
		{
			var reservedPickLinesAndDocketLines = new List<ReservingDocketLineAndPickLineToRemap>();
			foreach (var pickLine in receiveLine.ReservedPickLines)
			{
				reservedPickLinesAndDocketLines.Add(new ReservingDocketLineAndPickLineToRemap(pickLine, pickLine.DocketLine));
			}

			return reservedPickLinesAndDocketLines;
		}

		static void TransferUnfulfilledReservedStockToOtherInventories(ReservingDocketLineAndPickLineToRemap reservingDocketLineAndPickLineToRemap, IEnumerable<WhsReceiveLine> receiveLinesToMapReservedStocksTo)
		{
			var inventoriesToTransferReservedStocksTo = GetPossibleInventoriesForProductInfo(new ProductInfo(reservingDocketLineAndPickLineToRemap.ReservingDocketLine), receiveLinesToMapReservedStocksTo);
			if (inventoriesToTransferReservedStocksTo.Any())
			{
				var qtyToRemap = reservingDocketLineAndPickLineToRemap.QtyToRemap;

				foreach (var receiveLine in inventoriesToTransferReservedStocksTo
					.OrderBy(line => line.Inventory[0].UsedAttributesAndPalletIdCount)
					.ThenByDescending(line => line.WE_TransactionQuantity - line.ReservedQuantity))
				{
					var availableStockToReserve = receiveLine.WE_TransactionQuantity - receiveLine.ReservedQuantity;

					if (availableStockToReserve >= qtyToRemap)
					{
						RemapReservedQuantity(reservingDocketLineAndPickLineToRemap, qtyToRemap, receiveLine);
						break;
					}
					else
					{
						RemapReservedQuantity(reservingDocketLineAndPickLineToRemap, availableStockToReserve, receiveLine);
						qtyToRemap -= availableStockToReserve;
					}
				}
			}
		}

		static void RemapReservedQuantity(ReservingDocketLineAndPickLineToRemap reservingDocketLineAndPickLineToRemap, ZDecimal qtyToRemap, WhsDocketLine inventoryToTransferReservedStocksTo)
		{
			if (reservingDocketLineAndPickLineToRemap.PickLine.ReservedQuantity == qtyToRemap)
			{
				// all of pick line's quantity is being transferred
				reservingDocketLineAndPickLineToRemap.PickLine.WZ_WE_InventoryLine = inventoryToTransferReservedStocksTo.PK;
			}
			else
			{
				reservingDocketLineAndPickLineToRemap.PickLine.ReservedQuantity -= qtyToRemap;
				((WhsOrderLine)reservingDocketLineAndPickLineToRemap.ReservingDocketLine).ReserveStockIfAbleTo(inventoryToTransferReservedStocksTo.Inventory[0], qtyToRemap);
			}
		}

		#endregion

		#region Implementation

		#region GetPossibleInventoriesForProductInfo

		static IEnumerable<WhsDocketLine> GetPossibleInventoriesForProductInfo(ProductInfo productInfo, IEnumerable<WhsReceiveLine> receiveLines)
			=> GetPossibleInventoriesForProductInfo(productInfo, receiveLines.Select(line => line.Inventory[0]))
				.Select(line => line.InDocketLine);

		static IEnumerable<WhsInventoryView> GetPossibleInventoriesForProductInfo(ProductInfo productInfo, IEnumerable<WhsInventoryView> inventories)
		{
			var matchingInventories = new List<WhsInventoryView>();
			foreach (var inventory in inventories)
			{
				if (CanInventoryMatchToProductInfo(productInfo, inventory))
				{
					matchingInventories.Add(inventory);
				}
			}
			return matchingInventories;
		}

		static bool CanInventoryMatchToProductInfo(ProductInfo productInfo, WhsInventoryView inventory)
			=> productInfo.ProductPK == inventory.WI_OP && CanInventoryMatchToProductInfoCore(productInfo, inventory);

		static bool CanInventoryMatchToProductInfoCore(ProductInfo productInfo, WhsInventoryView inventory)
		{
			return (productInfo.PartAttrib1.IsEmpty || productInfo.PartAttrib1.EqualsIgnoringCase(inventory.WI_PartAttrib1))
				&& (productInfo.PartAttrib2.IsEmpty || productInfo.PartAttrib2.EqualsIgnoringCase(inventory.WI_PartAttrib2))
				&& (productInfo.PartAttrib3.IsEmpty || productInfo.PartAttrib3.EqualsIgnoringCase(inventory.WI_PartAttrib3))
				&& (productInfo.SerialNumber.IsEmpty || productInfo.SerialNumber.EqualsIgnoringCase(inventory.WI_SerialNumber))
				&& (productInfo.PalletId.IsEmpty || productInfo.PalletId.EqualsIgnoringCase(inventory.WI_PalletID))
				&& (productInfo.PackingDate.IsEmpty || productInfo.PackingDate == inventory.WI_PackingDate)
				&& (productInfo.ExpiryDate.IsEmpty || productInfo.ExpiryDate == inventory.WI_ExpiryDate);
		}

		#endregion

		#endregion
	}

	class ProductInfo
	{
		public ProductInfo(ZGuid productPK, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDate packingDate, ZDate expiryDate, ZString palletId)
		{
			ProductPK = productPK;
			PartAttrib1 = partAttrib1;
			PartAttrib2 = partAttrib2;
			PartAttrib3 = partAttrib3;
			SerialNumber = serialNumber;
			PackingDate = packingDate;
			ExpiryDate = expiryDate;
			PalletId = palletId;
		}

		public ProductInfo(WhsAsnLine asnLine)
			: this(asnLine.WN_OP, asnLine.WN_PartAttrib1, asnLine.WN_PartAttrib2, asnLine.WN_PartAttrib3, asnLine.WN_SerialNumber, asnLine.WN_PackingDate, asnLine.WN_ExpiryDate, asnLine.WN_PalletId)
		{
		}

		public ProductInfo(WhsDocketLine docketLine)
			: this(docketLine.WE_OP, docketLine.WE_PartAttrib1, docketLine.WE_PartAttrib2, docketLine.WE_PartAttrib3, docketLine.WE_SerialNumber, docketLine.WE_PackingDate, docketLine.WE_ExpiryDate, docketLine.WE_PalletID)
		{
		}

		public ZGuid ProductPK { get; private set; }
		public ZString PartAttrib1 { get; private set; }
		public ZString PartAttrib2 { get; private set; }
		public ZString PartAttrib3 { get; private set; }
		public ZString SerialNumber { get; private set; }
		public ZDate PackingDate { get; private set; }
		public ZDate ExpiryDate { get; private set; }
		public ZString PalletId { get; private set; }
	}

	class ReservingDocketLineAndPickLineToRemap
	{
		public ReservingDocketLineAndPickLineToRemap(WhsPickLine pickLine, WhsDocketLine reservingDocketLine)
		{
			PickLine = pickLine;
			ReservingDocketLine = reservingDocketLine;
		}

		public WhsPickLine PickLine { get; }
		public WhsDocketLine ReservingDocketLine { get; }
		public ZDecimal QtyToRemap { get; set; }
		public int DocketLineUsedAttributesCount => LineUsedAttributesCount(this.ReservingDocketLine);

		int LineUsedAttributesCount(WhsDocketLine docketLine)
		{
			int result = 0;
			if (!docketLine.WE_PartAttrib1.IsEmpty)
			{
				result++;
			}
			if (!docketLine.WE_PartAttrib2.IsEmpty)
			{
				result++;
			}
			if (!docketLine.WE_PartAttrib3.IsEmpty)
			{
				result++;
			}
			if (!docketLine.WE_SerialNumber.IsEmpty)
			{
				result++;
			}
			if (!docketLine.WE_PackingDate.IsEmpty)
			{
				result++;
			}
			if (!docketLine.WE_ExpiryDate.IsEmpty)
			{
				result++;
			}
			if (!docketLine.WE_PalletID.IsEmpty)
			{
				result++;
			}

			return result;
		}
	}
}
