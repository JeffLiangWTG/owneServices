using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWhsCycleCountLocationCreator
	{
		WhsCycleCountLocation CreateCycleCountLocation(WhsLocation location, int priority = 0);

		WhsCycleCountLocation CreateCycleCountLocation(BusinessObjectFactory factory, ZGuid locationPK, ZString granularity, int priority = 0);

		IEnumerable<WhsCycleCountLocation> CreateCycleCountLocations(BusinessObjectFactory factory, IEnumerable<ZGuid> locationPKs, int priority = 0);

		IEnumerable<WhsCycleCountLocation> CreateCycleCountLocations(BusinessObjectFactory factory, IEnumerable<WhsCycleCountLocationInfo> cycleCountLocationInfos);
	}
}
