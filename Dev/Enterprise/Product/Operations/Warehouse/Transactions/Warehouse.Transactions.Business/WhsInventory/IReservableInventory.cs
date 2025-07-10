namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IReservableInventory
	{
		WhsPickLineCollection ReservedPickLines { get; }
	}
}
