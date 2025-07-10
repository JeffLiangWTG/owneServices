using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWhsPutawayLocationCacheManager
	{
		void CreateCache(BusinessObjectFactory factory, IEnumerable<ZGuid> locationPK);
		IEnumerable<DataRow> GetCache(BusinessObjectFactory factory, ZGuid warehousePK, IEnumerable<ZGuid> clientPKs, IEnumerable<ZGuid> partPKs, IEnumerable<ZGuid> skipLocationPKs = null);
	}
}
