using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchTransportationUnitCollection : ActiveBusinessObjectCollection<WhsItemDispatchTransportationUnit>
	{
		public WhsItemDispatchTransportationUnitCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}
