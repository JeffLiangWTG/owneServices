using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPutawayLocationCacheInfo
	{
		public WhsPutawayLocationCacheInfo(ZGuid locationPK, ZGuid warehousePK)
		{
			LocationPK = locationPK;
			WarehousePK = warehousePK;
		}

		public ZGuid LocationPK { get; }
		public ZGuid WarehousePK { get; }
	}
}
