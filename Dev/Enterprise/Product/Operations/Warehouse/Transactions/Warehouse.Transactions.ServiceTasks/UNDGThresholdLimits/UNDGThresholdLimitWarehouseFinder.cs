using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	public class UNDGThresholdLimitWarehouseFinder : IUNDGThresholdLimitWarehouseFinder
	{
		public IReadOnlyCollection<WhsWarehouse> LoadWarehousesWithDGLimits()
		{
			var warehouseQuery = new ZQuery(WhsWarehouseSchema.WW_IsActive, true);
			var applicableWarehouses = new[] { WarehouseTypes.Codes.Product, WarehouseTypes.Codes.Transit, WarehouseTypes.Codes.FreeTradeZone };
			warehouseQuery.AddToFilter(WhsWarehouseSchema.WW_IsDangerousGoodsManagementEnabled, true);
			warehouseQuery.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, applicableWarehouses);
			warehouseQuery.OrderBy = WhsWarehouseSchema.WW_WarehouseName.Name;

			return new BusinessObjectFactory().Load<WhsWarehouse>(warehouseQuery);
		}
	}
}
