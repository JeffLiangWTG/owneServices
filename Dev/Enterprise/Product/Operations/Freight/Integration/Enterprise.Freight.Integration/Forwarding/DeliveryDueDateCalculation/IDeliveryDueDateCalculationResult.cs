using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IDeliveryDueDateCalculationResult
	{
		ZDateTime DeliveryDueDate { get; }
		ZBool IsSuccess { get; }
		ZString ErrorMessage { get; }

		ZString CalculationLog { get; }

		ZBool ArrivalTimeUsedForDeliveryCFS { get; }

		ZBool ServiceLevelGenericTransitTimeHasBeenUsed { get; }
	}
}
