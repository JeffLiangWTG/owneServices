using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public class OrderLinesSelectionHeader : NonPersistentBusinessObject
	{
		public OrderLinesSelectionHeader(IWarehouseIntegrationSupporter parent, ZGuid[] orderLinePKs) : base(parent.Factory)
		{
			this.parent = parent;
			qualifiedOrderLineWrappers = GetOrderLineWrappersAllocatedToInventories(orderLinePKs);

			var qualifiedCount = qualifiedOrderLineWrappers.Length;
			var orderLinesCount = orderLinePKs.Length;
			Status = qualifiedCount == orderLinesCount ? OrderLinesSelectionHeaderStatus.AllQualified : (qualifiedCount == 0 ? OrderLinesSelectionHeaderStatus.NoneQualified : OrderLinesSelectionHeaderStatus.PartiallyQualified);
		}

		protected readonly IWarehouseIntegrationSupporter parent;

		readonly WhsOrderLineWrapper[] qualifiedOrderLineWrappers;

		public OrderLinesSelectionHeaderStatus Status { get; private set; }

		WhsOrderLineWrapper[] GetOrderLineWrappersAllocatedToInventories(ZGuid[] orderLinePKs)
		{
			var result = new List<WhsOrderLineWrapper>();
			if (orderLinePKs.Length > 0)
			{
				foreach (var orderLine in GetOrderLinesInLocalFactory())
				{
					var orderLineWrapper = new WhsOrderLineWrapper(orderLine, Factory);
					if (orderLineWrapper.IsAllocatedToInventories)
					{
						result.Add(orderLineWrapper);
					}
				}
			}
			return result.ToArray();

			Warehouse.Integration.IWhsDocketLine[] GetOrderLinesInLocalFactory()
			{
				return Factory.Load<Warehouse.Integration.IWhsDocketLine>(new ZQuery(WhsDocketLineSchema.PK, orderLinePKs));
			}
		}

		public void ImportInventoriesByOrderLines()
		{
			if (qualifiedOrderLineWrappers.Length > 0)
			{
				var inventoriesImporter = new DeclarationInventorySelectionHeader(parent as BaseJobDeclaration);
				var orderLineWithInventoriesDict = qualifiedOrderLineWrappers.ToDictionary(orderLine => orderLine, orderLine => orderLine.AllocatedInventories);

				orderLineWithInventoriesDict.ForEach(pair =>
				{
					var orderLineWrapper = pair.Key;
					var inventories = pair.Value;
					inventories.ForEach(inventory =>
					{
						orderLineWrapper.InventoryPickLineDict.TryGetValue(inventory, out var pickLine);
						inventoriesImporter.SelectedLines.Add(
						new WhsInventoryWrapper(inventory, inventoriesImporter, orderLineWrapper)
						{
							QuantityToDraw = pickLine.WZ_Units
						});
					});
				});
				inventoriesImporter.ImportInventories();
			}
		}
	}

	public enum OrderLinesSelectionHeaderStatus
	{
		AllQualified,
		PartiallyQualified,
		NoneQualified
	}
}
