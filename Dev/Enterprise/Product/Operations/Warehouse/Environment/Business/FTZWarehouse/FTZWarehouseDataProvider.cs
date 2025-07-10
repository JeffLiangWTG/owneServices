using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public class FTZWarehouseDataProvider : IFTZWarehouseDataProvider
	{
		public bool IsFTZWarehouseDetailedTrackingEnabled(IOrgAddress warehouseAddress)
		{
			Argument.NotNull(warehouseAddress, nameof(warehouseAddress));

			var query = GetFTZWarehouseQuery(warehouseAddress);
			query.AddToFilter(WhsWarehouseSchema.WW_FTZIsDetailedTrackingEnabled, true);
			var warehouse = warehouseAddress.Factory.LoadTop1<WhsWarehouse>(query);

			return warehouse != null;
		}

		public static ZQuery GetFTZWarehouseQuery(IOrgAddress warehouseAddress)
		{
			var query = new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, warehouseAddress.PK);
			query.AddToFilter(WhsWarehouseSchema.WW_IsActive, true);
			query.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, WarehouseTypes.Codes.FreeTradeZone);

			return query;
		}
	}
}
