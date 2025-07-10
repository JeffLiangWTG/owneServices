namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IAvailableInventoryFactManagerFactory
	{
		IAvailableInventoryFactManager GetNewManager();
	}
}
