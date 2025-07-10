using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDTransportationUnitReleaseYardUnitCollection : ActiveBusinessObjectCollection<CYDYardUnitState>
	{
		public CYDTransportationUnitReleaseYardUnitCollection(CYDTransportationUnit transportationUnit)
			: base(transportationUnit.Factory, transportationUnit, null, CYDYardUnitStateSchema.YUS_YTU_DispatchTransportationUnit)
		{
		}
	}
}
