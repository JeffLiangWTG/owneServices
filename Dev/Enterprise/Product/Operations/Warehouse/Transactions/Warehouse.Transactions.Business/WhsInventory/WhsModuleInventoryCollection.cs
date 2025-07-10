using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsModuleInventoryCollection : WhsInventoryViewCollection
	{
		public WhsModuleInventoryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new WhsModuleInventory this[int index]
		{
			get { return (WhsModuleInventory)base[index]; }
		}

		public new WhsModuleInventory AddNew()
		{
			return (WhsModuleInventory)base.AddNew();
		}
	}
}
