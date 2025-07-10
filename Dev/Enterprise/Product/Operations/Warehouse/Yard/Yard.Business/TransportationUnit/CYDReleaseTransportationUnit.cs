using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDReleaseTransportationUnit(CYDTransportationUnit transportationUnit) : ICYDYardUnitsForRating
	{
		#region ICYDYardUnitsForRating

		IEnumerable<CYDYardUnitState> ICYDYardUnitsForRating.YardUnits => transportationUnit.ReleaseYardUnits;

		IReadOnlyList<string> ICYDYardUnitsForRating.ChargeCodeGroupList => [ChargeCodeGroupList.Codes.YardTransportationUnitGateOut];

		WhsWarehouse ICYDYardUnitsForRating.Yard => transportationUnit.Yard;

		OrgHeader ICYDYardUnitsForRating.Client => transportationUnit.TransportCompanyDocAddress.Organisation;

		#endregion
	}
}
