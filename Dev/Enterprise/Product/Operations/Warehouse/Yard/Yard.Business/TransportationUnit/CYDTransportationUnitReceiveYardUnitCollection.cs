using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDTransportationUnitReceiveYardUnitCollection : ActiveBusinessObjectCollection<CYDYardUnitState>
	{
		public CYDTransportationUnitReceiveYardUnitCollection(CYDTransportationUnit transportationUnit)
			: base(transportationUnit.Factory, transportationUnit, null, CYDYardUnitStateSchema.YUS_YTU_ReceiveTransportationUnit)
		{
		}
	}
}
