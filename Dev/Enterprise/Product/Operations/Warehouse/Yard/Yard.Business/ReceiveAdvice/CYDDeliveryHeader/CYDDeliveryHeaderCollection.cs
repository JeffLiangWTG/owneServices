using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Yard.Business
{
	public class CYDDeliveryHeaderCollection : ActiveBusinessObjectCollection<CYDDeliveryHeader>
	{
		public CYDDeliveryHeaderCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}
