using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDYardUnitStateCollection : ActiveBusinessObjectCollection<CYDYardUnitState>
	{
		public CYDYardUnitStateCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}
