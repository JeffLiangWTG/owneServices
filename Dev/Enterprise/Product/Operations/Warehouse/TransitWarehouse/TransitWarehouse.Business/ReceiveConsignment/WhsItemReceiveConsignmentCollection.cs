using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemReceiveConsignmentCollection : ActiveBusinessObjectCollection<WhsItemReceiveConsignment>
	{
		public WhsItemReceiveConsignmentCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}
