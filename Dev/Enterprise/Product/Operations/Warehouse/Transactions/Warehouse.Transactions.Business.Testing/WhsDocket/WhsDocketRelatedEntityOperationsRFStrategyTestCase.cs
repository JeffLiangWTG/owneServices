namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsDocketRelatedEntityOperationsRFStrategyTestCase : WhsDocketRelatedEntityOperationsStrategyTestCase
	{
		#region Implementation

		protected override WhsDocketRelatedEntityOperationsStrategy GetNewStrategy(WhsDocket docket)
		{
			return new WhsDocketRelatedEntityOperationsRFStrategy(docket);
		}

		#endregion
	}
}
