using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWhsReceiveLineWrapperStrategy
	{
		WhsInventoryViewCollection GetNewWhsInventoryCollection();
		WhsInventoryView LoadInventoryView(ZQuery query);
		WhsInventoryView GetNewInventoryView();
	}

	public interface IWhsReceiveLineStrategyBuilder
	{
		IWhsReceiveLineWrapperStrategy Build(WhsReceiveLine line);
	}
}
