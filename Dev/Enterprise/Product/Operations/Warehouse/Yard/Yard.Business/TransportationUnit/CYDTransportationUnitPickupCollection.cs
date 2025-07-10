using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDTransportationUnitPickupCollection : ActiveBusinessObjectCollection<CYDPickup>
	{
		public CYDTransportationUnitPickupCollection(CYDTransportationUnit transportationUnit)
			: base(transportationUnit.Factory, transportationUnit, null, CYDPickupSchema.YPL_YTU_PickupTransportationUnit)
		{
		}
	}
}
