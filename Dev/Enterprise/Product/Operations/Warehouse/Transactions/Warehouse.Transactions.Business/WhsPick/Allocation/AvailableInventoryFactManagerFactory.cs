namespace Enterprise.Warehouse.Transactions.Business
{
	class AvailableInventoryFactManagerFactory : IAvailableInventoryFactManagerFactory
	{
		public IAvailableInventoryFactManager GetNewManager() => new AvailableInventoryFactManager();
	}
}
