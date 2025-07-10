namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWhsReceiveWrapperStrategy
	{
		WhsInventoryViewCollection GetNewWhsInventoryCollection();
		WhsDocketValidation GetNewValidation();
		WhsDocketLookups GetNewLookups();
	}

	public interface IWhsReceiveStrategyBuilder
	{
		IWhsReceiveWrapperStrategy Build(WhsReceive receive);
	}
}
