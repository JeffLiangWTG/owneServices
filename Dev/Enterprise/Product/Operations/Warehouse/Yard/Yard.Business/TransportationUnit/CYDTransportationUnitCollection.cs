using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDTransportationUnitCollection : ActiveBusinessObjectCollection<CYDTransportationUnit>
	{
		public CYDTransportationUnitCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}
