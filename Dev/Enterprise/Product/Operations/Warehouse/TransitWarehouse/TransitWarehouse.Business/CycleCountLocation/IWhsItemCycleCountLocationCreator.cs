using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transit.Business
{
	public interface IWhsItemCycleCountLocationCreator
	{
		IEnumerable<WhsItemCycleCountLocation> CreateCycleCountLocations(BusinessObjectFactory factory, IEnumerable<WhsItemCycleCountLocationInfo> cycleCountLocationInfos);
	}
}
