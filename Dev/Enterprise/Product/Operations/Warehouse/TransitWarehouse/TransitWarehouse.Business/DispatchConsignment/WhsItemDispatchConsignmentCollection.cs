using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemDispatchConsignmentCollection : ActiveBusinessObjectCollection<WhsItemDispatchConsignment>
	{
		public WhsItemDispatchConsignmentCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}