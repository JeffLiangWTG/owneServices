using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDReceiveTransportationUnit(CYDTransportationUnit transportationUnit) : ICYDYardUnitsForRating
	{
		#region ICYDYardUnitsForRating

		IEnumerable<CYDYardUnitState> ICYDYardUnitsForRating.YardUnits => transportationUnit.ReceiveYardUnits;

		IReadOnlyList<string> ICYDYardUnitsForRating.ChargeCodeGroupList => [ChargeCodeGroupList.Codes.YardTransportationUnitGateIn];

		WhsWarehouse ICYDYardUnitsForRating.Yard => transportationUnit.Yard;

		OrgHeader ICYDYardUnitsForRating.Client => transportationUnit.TransportCompanyDocAddress.Organisation;

		#endregion
	}
}
