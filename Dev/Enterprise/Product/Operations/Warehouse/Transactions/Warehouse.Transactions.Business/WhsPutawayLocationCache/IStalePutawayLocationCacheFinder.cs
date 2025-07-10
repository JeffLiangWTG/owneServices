using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IStalePutawayLocationCacheFinder
	{
		IEnumerable<WhsPutawayLocationCacheInfo> FindCacheEntriesThatNeedUpdating(BusinessObjectFactory factory);
		IEnumerable<ZGuid> FindCacheEntriesThatNeedUpdating(BusinessObjectFactory factory, ZGuid warehousePK);
	}
}
