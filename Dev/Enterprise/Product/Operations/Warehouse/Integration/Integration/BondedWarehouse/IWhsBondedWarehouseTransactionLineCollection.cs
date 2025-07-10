namespace Enterprise.Warehouse.Integration.BondedWarehouse
{
	public interface IWhsBondedWarehouseTransactionLineCollection : IWhsWarehouseTransactionLineCollection
	{
		new IWhsBondedWarehouseTransactionLineCollection Clone();
		void Add(IWhsBondedWarehouseTransactionLine line);

		void AddRange(IWhsBondedWarehouseTransactionLine[] lines);

		new IWhsBondedWarehouseTransactionLine this[int i] { get; set; }
	}
}
