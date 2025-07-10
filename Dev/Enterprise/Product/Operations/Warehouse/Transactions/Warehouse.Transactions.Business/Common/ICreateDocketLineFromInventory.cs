using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ICreateDocketLineFromInventory
	{
		WhsDocketLine CreateDocketLineFromInventory(WhsInventoryView inventory);
		void AcceptInventoryLinesFromSearchGrid(BusinessObject[] inventory);
	}
}
