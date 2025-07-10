using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class OrderToReturnInfoCache
	{
		public OrderToReturnInfoCache(BusinessObjectFactory factory, WhsReceive receive)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
			Parent = Argument.NotNull(receive, nameof(receive));

			OrderToReturn = new Lazy<WhsOrder>(() => Parent.IsReturnReceive ? Parent.OrderToReturn : null);

			ProductPKsToReturn = new Lazy<HashSet<ZGuid>>(() => OrderToReturn.Value?.Lines.Select(line => line.WE_OP).ToHashSet() ?? new HashSet<ZGuid>());

			OrderLinesToReturnCache = new Lazy<List<WhsRMAOrderLine>>(() =>
			{
				var rmaOrderLineList = new List<WhsRMAOrderLine>();

				var orderToReturn = OrderToReturn.Value;
				if (orderToReturn != null)
				{
					var orderLines = orderToReturn.Lines.Cast<WhsOrderLine>();
					WhsRMAHelper.AddFetchHint(orderLines, Factory);
					rmaOrderLineList = WhsRMAHelper.GetWhsRMAOrderLines(orderLines, orderToReturn, Parent.PK);
				}

				return rmaOrderLineList;
			});

			InventoryKeyToReceiveLinesDictionary = new Lazy<Dictionary<string, List<WhsReceiveLine>>>(() =>
			{
				var inventoryKeyToReceiveLinesDictionary = new Dictionary<string, List<WhsReceiveLine>>();
				foreach(WhsReceiveLine line in Parent.Lines)
				{
					var inventoryKey = WhsRMAHelper.GetKey(line);
					if (!inventoryKeyToReceiveLinesDictionary.TryGetValue(inventoryKey, out var receiveLines))
					{
						inventoryKeyToReceiveLinesDictionary[inventoryKey] = receiveLines = new List<WhsReceiveLine>();
					}

					receiveLines.Add(line);
				}

				return inventoryKeyToReceiveLinesDictionary;
			});

			InventoryKeyToRMAOrderLineDictionary = new Lazy<Dictionary<string, List<WhsRMAOrderLine>>>(() =>
			{
				var inventoryKeyToRMAOrderLineDictionary = new Dictionary<string, List<WhsRMAOrderLine>>();
				foreach (var line in OrderLinesToReturnCache.Value)
				{
					if (!inventoryKeyToRMAOrderLineDictionary.TryGetValue(line.Key, out var rmaOrderLines))
					{
						inventoryKeyToRMAOrderLineDictionary[line.Key] = rmaOrderLines = new List<WhsRMAOrderLine>();
					}

					rmaOrderLines.Add(line);
				}

				return inventoryKeyToRMAOrderLineDictionary;
			});
		}

		BusinessObjectFactory Factory { get; }
		WhsReceive Parent { get; }

		public bool AnyOrderLinesToReturnWithAvailableQuantity
			=> OrderLinesToReturnCache.Value.Any(line => line.AvailableQtyToReturn > 0m);

		public IReadOnlyCollection<WhsReceiveLine> GetReceiveLinesWithInventoryKey(string inventoryKey)
		{
			if (!InventoryKeyToReceiveLinesDictionary.Value.TryGetValue(inventoryKey, out var receiveLines))
			{
				receiveLines = new List<WhsReceiveLine>();
			}

			return receiveLines;
		}

		public IReadOnlyCollection<WhsRMAOrderLine> GetOrderLinesWithInventoryKey(string inventoryKey)
		{
			if (!InventoryKeyToRMAOrderLineDictionary.Value.TryGetValue(inventoryKey, out var rmaOrderLines))
			{
				rmaOrderLines = new List<WhsRMAOrderLine>();
			}

			return rmaOrderLines;
		}

		public IReadOnlyCollection<WhsRMAOrderLine> OrderLinesToReturn => OrderLinesToReturnCache.Value;

		public bool IsValidProductToReturn(ZGuid productPK)
			=> OrderToReturn.Value == null || ProductPKsToReturn.Value.Contains(productPK);

		Lazy<List<WhsRMAOrderLine>> OrderLinesToReturnCache { get; }

		Lazy<Dictionary<string, List<WhsReceiveLine>>> InventoryKeyToReceiveLinesDictionary { get; }

		Lazy<Dictionary<string, List<WhsRMAOrderLine>>> InventoryKeyToRMAOrderLineDictionary { get; }

		Lazy<HashSet<ZGuid>> ProductPKsToReturn { get; }

		Lazy<WhsOrder> OrderToReturn { get; }
	}
}
