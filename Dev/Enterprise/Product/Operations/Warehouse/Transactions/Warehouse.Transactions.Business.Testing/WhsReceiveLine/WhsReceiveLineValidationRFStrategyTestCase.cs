
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsReceiveLineValidationRFStrategyTestCase : WhsReceiveLineValidationStrategyTestCase
	{
		#region Implementation

		protected override WhsReceiveLineValidationStrategy GetNewStrategy(WhsReceiveLine inventoryLine)
		{
			return new WhsReceiveLineValidationRFStrategy(inventoryLine);
		}

		#endregion
	}
}
