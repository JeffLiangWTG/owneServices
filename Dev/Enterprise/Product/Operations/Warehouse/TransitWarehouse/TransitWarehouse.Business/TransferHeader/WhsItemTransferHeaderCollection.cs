using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemTransferHeaderCollection : ActiveBusinessObjectCollection<WhsItemTransferHeader>
	{
		public WhsItemTransferHeaderCollection(BusinessObjectFactory factory) : base(factory) { }
	}
}
