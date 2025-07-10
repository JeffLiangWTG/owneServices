using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business;

public static class WhsRMAHelper
{
	public static List<WhsRMAOrderLine> GetWhsRMAOrderLines(IEnumerable<WhsOrderLine> orderLines, WhsOrder parentOrder)
		=> GetWhsRMAOrderLines(orderLines, parentOrder, ZGuid.Empty);

	public static List<WhsRMAOrderLine> GetWhsRMAOrderLines(IEnumerable<WhsOrderLine> orderLines, WhsOrder parentOrder, ZGuid returnReceivePKToExclude)
	{
		Argument.NotNull(orderLines, nameof(orderLines));
		Argument.NotNull(parentOrder, nameof(parentOrder));

		var result = new List<WhsRMAOrderLine>();

		var validOrderLines = orderLines.Cast<WhsPickableDocketLine>().Where(orderLine => orderLine.WE_WE_ParentDocketLine.IsEmpty).ToList();
		if (validOrderLines.Count > 0)
		{
			var inventoryListGroupByInventory = validOrderLines.SelectMany(orderLine => orderLine.PickLines.Select(p => (OriginalInventory: p.InventoryLineForAvailableInventory, Quantity: p.WZ_Units))).GroupBy(tuple => tuple.OriginalInventory).Select(g => (OriginalInventory: g.Key, Quantity: g.Sum(v => v.Quantity))).ToList();
			var inventoryListGroupByAttrAndBOMFlag = inventoryListGroupByInventory.GroupBy(t =>
				new
				{
					AttributeKey = GetKey(t.OriginalInventory),
					HasBOMComponentsAndNotCreatedFromPickByBOM = t.OriginalInventory.BOMComponentLinks.Any() && !t.OriginalInventory.IsCreatedFromPickByBOM
				}).ToList();

			var orderReturnedInventoriesInfo = GetReturnedInventoriesInfo(parentOrder, returnReceivePKToExclude);
			foreach (var grouping in inventoryListGroupByAttrAndBOMFlag)
			{
				if (grouping.Key.HasBOMComponentsAndNotCreatedFromPickByBOM)
				{
					foreach (var tupleElement in grouping)
					{
						var rmaLine = WhsRMAOrderLine.GetWhsRMAOrderLine(tupleElement.OriginalInventory, parentOrder, tupleElement.Quantity, GetAvailableQuantityToReturn(orderReturnedInventoriesInfo, tupleElement.OriginalInventory, tupleElement.Quantity), grouping.Key.HasBOMComponentsAndNotCreatedFromPickByBOM);
						result.Add(rmaLine);
					}
				}
				else
				{
					var inventory = grouping.First().OriginalInventory;
					var quantity = grouping.Sum(g => g.Quantity);
					var rmaLine = WhsRMAOrderLine.GetWhsRMAOrderLine(inventory, parentOrder, quantity, GetAvailableQuantityToReturn(orderReturnedInventoriesInfo, inventory, quantity), grouping.Key.HasBOMComponentsAndNotCreatedFromPickByBOM);
					result.Add(rmaLine);
				}
			}
		}
		return result;
	}

	static Dictionary<string, decimal> GetReturnedInventoriesInfo(WhsOrder order, ZGuid receivePkToExclude)
	{
		return order.RelatedReceives
			.Where(receive => receive.PK != receivePkToExclude)
			.SelectMany(receive => receive.Lines)
			.GroupBy(GetKey)
			.ToDictionary(grouping => grouping.Key, grouping => grouping.Sum(line => line.WE_TransactionQuantity));
	}

	static decimal GetAvailableQuantityToReturn(Dictionary<string, decimal> returnedInventoryInfo, WhsDocketLine originalInventory, decimal orderQuantity)
	{
		var availableQty = orderQuantity;
		if (returnedInventoryInfo.TryGetValue(GetKey(originalInventory), out var returnedQuantity))
		{
			availableQty = returnedQuantity >= orderQuantity
				? 0m
				: orderQuantity - returnedQuantity;
		}

		return availableQty;
	}

	public static string GetKey(WhsDocketLine docketLine)
	{
		var attributes = new[]
		{
			new ZString(docketLine.WE_OP).ToUpper(),
			docketLine.WE_PartAttrib1.ToUpper(),
			docketLine.WE_PartAttrib2.ToUpper(),
			docketLine.WE_PartAttrib3.ToUpper(),
			docketLine.WE_SerialNumber.ToUpper(),
			GetDateKey(docketLine.WE_ExpiryDate),
			GetDateKey(docketLine.WE_PackingDate)
		};

		return string.Join("|", attributes);

		ZString GetDateKey(ZDate date) => date.ToShortDateString();
	}

	public static void BuildReceiveLine(WhsReceive receive, WhsRMAOrderLine rmaLine)
	{
		receive.Factory.SuspendValidation();

		try
		{
			var returnDocketLine = BuildReceiveLineCore(receive, rmaLine);

			if (rmaLine.ShouldCopyBOMLinks)
			{
				CopyBOMComponentLinks(rmaLine.ParentInventory, returnDocketLine, useTransactionQuantity: false);
			}
		}
		finally
		{
			receive.Factory.ResumeValidation();
		}

		WhsReceiveLine BuildReceiveLineCore(WhsReceive receive, WhsRMAOrderLine rmaLine)
		{
			var receiveLine = receive.Lines.AddNew();
			receiveLine.WE_OP = rmaLine.ProductPK;
			receiveLine.WE_ClientOrderedUnits = rmaLine.QuantityToReturn;
			receiveLine.WE_ExpiryDate = rmaLine.ExpiryDate;
			receiveLine.WE_PackingDate = rmaLine.PackingDate;
			receiveLine.WE_PartAttrib1 = rmaLine.PartAttrib1;
			receiveLine.WE_PartAttrib2 = rmaLine.PartAttrib2;
			receiveLine.WE_PartAttrib3 = rmaLine.PartAttrib3;
			receiveLine.WE_SerialNumber = rmaLine.SerialNumber;
			return receiveLine;
		}
	}

	public static void CopyBOMComponentLinks(WhsDocketLine originalInventory, WhsDocketLine newInventory, bool useTransactionQuantity = true)
	{
		Argument.NotNull(originalInventory, nameof(originalInventory));
		Argument.NotNull(newInventory, nameof(newInventory));

		if (originalInventory.IsInventoryLine)
		{
			var factory = newInventory.Factory;

			var ratio = (useTransactionQuantity ? newInventory.WE_TransactionQuantity : newInventory.WE_ClientOrderedUnits) / originalInventory.WE_TransactionQuantity;
			foreach (var pivot in originalInventory.BOMComponentLinks)
			{
				var newPivot = factory.New<WhsBOMInventoryPivot>();
				newPivot.WIP_WE_ComponentLine = pivot.WIP_WE_ComponentLine;
				newPivot.WIP_WE_InventoryLine = newInventory.PK;
				newPivot.WIP_ComponentQuantity = pivot.WIP_ComponentQuantity * ratio;
			}
		}
	}

	public static void AddFetchHint(IEnumerable<WhsDocketLine> orderLines, BusinessObjectFactory factory)
	{
		var inventoryPKs = orderLines.SelectMany(o => o.PickLines.Select(p => p.InventoryLineForAvailableInventory.PK));
		var query = new ZQuery();
		query.AddToFilter(WhsBOMInventoryPivotSchema.WIP_WE_InventoryLine, inventoryPKs);
		factory.AddFetchHint(WhsBOMInventoryPivotSchema.Instance, query);
	}
}
