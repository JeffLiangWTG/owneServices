using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDPickupHeaderCollection : ActiveBusinessObjectCollection<CYDPickupHeader>
	{
		public CYDPickupHeaderCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}
