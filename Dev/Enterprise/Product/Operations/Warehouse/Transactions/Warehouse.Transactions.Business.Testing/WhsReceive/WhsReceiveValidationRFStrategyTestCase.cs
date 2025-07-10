namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReceiveValidationRFStrategyTestCase : WhsReceiveValidationStrategyTestCase
	{
		#region Implementation

		protected override WhsReceiveValidationStrategy GetNewStrategy(WhsReceive receive)
		{
			return new WhsReceiveValidationRFStrategy(receive);
		}

		#endregion
	}
}
