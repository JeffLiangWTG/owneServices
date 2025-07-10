using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDTransportationUnitDeliveryCollection : ActiveBusinessObjectCollection<CYDDelivery>
	{
		public CYDTransportationUnitDeliveryCollection(CYDTransportationUnit transportationUnit)
			: base(transportationUnit.Factory, transportationUnit, null, CYDDeliverySchema.YDL_YTU_DeliveryTransportationUnit)
		{
		}
	}
}
