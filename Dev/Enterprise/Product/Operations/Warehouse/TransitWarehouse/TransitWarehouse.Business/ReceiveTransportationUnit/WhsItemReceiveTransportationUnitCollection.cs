using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveTransportationUnitCollection : ActiveBusinessObjectCollection<WhsItemReceiveTransportationUnit>
	{
		public WhsItemReceiveTransportationUnitCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}
