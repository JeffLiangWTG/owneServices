using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Environment.Business
{
	public static class WhsWarehouseExtensions
	{
		public static ZDateTimeOffset GetWarehouseBranchDateTimeOffset(this WhsWarehouse warehouse, ZDateTime utcDateTime)
		{
			return utcDateTime.ToLocationTime(warehouse?.RelatedCompanyBranch?.HomePort);
		}

		public static ZDateTimeOffset GetWarehouseBranchLocalDateTimeOffset(this WhsWarehouse warehouse, ZDateTime localDateTime)
		{
			return localDateTime.ToDateTimeOffset(warehouse?.RelatedCompanyBranch?.HomePort);
		}
	}
}
