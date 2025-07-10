using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public record WarehouseUNDGValidationResult
	{
		public ZString OverLimitMessage { get; }
		public ZString OverThresholdMessage { get; }

		public WarehouseUNDGValidationResult(string overLimitMessage, string overThresholdMessage = null)
		{
			OverLimitMessage = overLimitMessage;
			OverThresholdMessage = overThresholdMessage;
		}
	}
}
