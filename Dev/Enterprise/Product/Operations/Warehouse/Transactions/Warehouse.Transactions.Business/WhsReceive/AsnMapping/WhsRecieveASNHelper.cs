using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.Business.AsnMapping
{
	public static class WhsReceiveASNHelper
	{
		#region ReconcileASN

		public static void ReconcileASN(WhsReceive receive)
		{
			if (receive.AsnLines.Count > 0)
			{
				ClearExpectedReceiptQuantityOnInventories(receive);
				MapInventoriesToASN(receive);
				LinesMapper.RemapUnfulfilledReservedStock(receive);
				DeleteZeroTransactionAndExpectedQtyInventories(receive);
				DeleteBOMLinksForPartialReturnedLines(receive);
			}
		}

		#endregion

		#region ClearExpectedReceiptQuantityOnInventories

		static void ClearExpectedReceiptQuantityOnInventories(WhsReceive receive)
		{
			foreach (WhsInventoryView inventory in receive.Inventory)
			{
				inventory.WI_ExpectedReceiptQuantity = 0m;
			}
		}

		#endregion

		#region MapInventoriesToASN

		static void MapInventoriesToASN(WhsReceive receive)
		{
			var receiveLinesMapper = new LinesMapper(receive);

			foreach (var productToAnalyse in AllProductsFromReceivesAndASNs(receive.Inventory.Cast<WhsInventoryView>(), receive.AsnLines.Cast<WhsAsnLine>()))
			{
				var inventoriesForProduct = receive.Inventory.Cast<WhsInventoryView>().Where(x => productToAnalyse.Equals(x.WI_OP)).ToList();
				var asnLinesForProduct = receive.AsnLines.Cast<WhsAsnLine>().Where(x => productToAnalyse.Equals(x.WN_OP));

				receiveLinesMapper.MapInventoriesToAsnLines(inventoriesForProduct, asnLinesForProduct);
			}
		}

		#region AllProductsFromReceivesAndASNs

		static List<ZGuid> AllProductsFromReceivesAndASNs(IEnumerable<WhsInventoryView> inventories, IEnumerable<WhsAsnLine> asnLines)
		{
			var productsFromInventory = inventories.Select(x => x.WI_OP);
			var productsFromASN = asnLines.Select(x => x.WN_OP);

			return productsFromInventory.Union(productsFromASN).Distinct().ToList();
		}

		#endregion

		#endregion

		#region DeleteZeroTransactionAndExpectedQtyInventories

		static void DeleteZeroTransactionAndExpectedQtyInventories(WhsReceive receive)
		{
			foreach (var inventory in receive.Inventory.Cast<WhsInventoryView>()
				.Where(inventory => inventory.WI_InDocketLineUnits == 0 && inventory.WI_ExpectedReceiptQuantity == 0 && inventory.InDocketLine.ReservedQuantity == 0)
				.ToArray())
			{
				receive.Inventory.RemoveAndDelete(inventory);
			}
		}

		#endregion

		#region DeleteBOMLinksForPartialReturnedLines

		static void DeleteBOMLinksForPartialReturnedLines(WhsReceive receive)
		{
			foreach (WhsReceiveLine line in receive.Lines)
			{
				if (line.WE_TransactionQuantity != line.WE_ClientOrderedUnits)
				{
					line.BOMComponentLinks.DeleteAll();
				}
			}
		}

		#endregion
	}
}
