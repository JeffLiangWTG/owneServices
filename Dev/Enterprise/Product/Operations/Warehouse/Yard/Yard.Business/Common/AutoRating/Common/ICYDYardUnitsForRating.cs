using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public interface ICYDYardUnitsForRating
	{
		IEnumerable<CYDYardUnitState> YardUnits { get; }

		IReadOnlyList<string> ChargeCodeGroupList { get; }

		WhsWarehouse Yard { get; }

		OrgHeader Client { get; }
	}
}
