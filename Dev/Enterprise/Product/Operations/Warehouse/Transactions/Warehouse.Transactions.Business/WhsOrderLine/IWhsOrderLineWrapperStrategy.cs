namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IWhsOrderLineWrapperStrategy
	{
		void WE_TransactionQuantity_SetAfter(bool valueChanged);
		bool ShouldUpdateWeightAndVolumeOfDocketFromDocketLine { get; }
	}

	public interface ITrackingOrderLineStrategyBuilder
	{
		IWhsOrderLineWrapperStrategy Build(WhsOrderLine order);
	}
}
