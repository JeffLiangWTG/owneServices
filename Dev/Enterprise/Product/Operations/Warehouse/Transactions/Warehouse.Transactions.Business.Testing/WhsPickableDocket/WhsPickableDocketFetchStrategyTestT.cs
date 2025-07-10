namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	abstract class WhsPickableDocketFetchStrategyTest<T> : WhsDocketFetchStrategyTest<T> where T : WhsPickableDocket
	{
		protected override WhsDocketFetchStrategy GetNewFetchStrategy()
		{
			return new WhsPickableDocketFetchStrategy(Factory.New<T>());
		}
	}
}
