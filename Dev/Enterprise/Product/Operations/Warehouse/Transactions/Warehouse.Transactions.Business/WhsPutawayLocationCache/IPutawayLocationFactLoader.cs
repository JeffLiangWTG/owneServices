using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IPutawayLocationFactLoader
	{
		IEnumerable<IInputFact> GetPutawayLocationFacts(BusinessObjectFactory factory, ZGuid warehousePK, IEnumerable<ZGuid> clientPKs, IEnumerable<ZGuid> partPKs, IEnumerable<ZGuid> skipLocationPKs = null);
	}
}
