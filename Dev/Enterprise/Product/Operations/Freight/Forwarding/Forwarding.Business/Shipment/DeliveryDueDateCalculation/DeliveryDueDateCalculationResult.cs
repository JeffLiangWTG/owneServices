using CargoWise.Types;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	public class DeliveryDueDateCalculationResult : IDeliveryDueDateCalculationResult
	{
		DeliveryDueDateCalculationResult(ZDateTime deliveryDueDate, ZBool isSuccess, ZString errorMessage, ZString calculationLog, bool arrivalTimeUsedForDeliveryCFS = false, bool serviceLevelGenericTransitTimeHasBeenUsed = false)
		{
			DeliveryDueDate = deliveryDueDate;
			IsSuccess = isSuccess;
			ErrorMessage = errorMessage;
			CalculationLog = calculationLog;
			ArrivalTimeUsedForDeliveryCFS = arrivalTimeUsedForDeliveryCFS;
			ServiceLevelGenericTransitTimeHasBeenUsed = serviceLevelGenericTransitTimeHasBeenUsed;
		}

		public ZDateTime DeliveryDueDate { get; }

		public ZBool IsSuccess { get; }

		public ZString ErrorMessage { get; }

		public ZString CalculationLog { get; }

		public ZBool ArrivalTimeUsedForDeliveryCFS { get; }

		public ZBool ServiceLevelGenericTransitTimeHasBeenUsed { get; }

		public static DeliveryDueDateCalculationResult Success(ZDateTime deliveryDueDate, ZString calculationLog, IDeliveryDueDateCalculationResult previousResult) => Success(deliveryDueDate, calculationLog, previousResult.ArrivalTimeUsedForDeliveryCFS, previousResult.ServiceLevelGenericTransitTimeHasBeenUsed);

		public static DeliveryDueDateCalculationResult Success(ZDateTime deliveryDueDate, ZString calculationLog, bool arrivalTimeUsedForDeliveryCFS = false) => new DeliveryDueDateCalculationResult(deliveryDueDate, true, ZString.Empty, calculationLog, arrivalTimeUsedForDeliveryCFS);

		public static DeliveryDueDateCalculationResult Success(ZDateTime deliveryDueDate, ZString calculationLog, bool arrivalTimeUsedForDeliveryCFS, bool serviceLevelGenericTransitTimeHasBeenUsed) => new DeliveryDueDateCalculationResult(deliveryDueDate, true, ZString.Empty, calculationLog, arrivalTimeUsedForDeliveryCFS, serviceLevelGenericTransitTimeHasBeenUsed);

		public static DeliveryDueDateCalculationResult Failure(ZString errorMessage, ZString calculationLog) => new DeliveryDueDateCalculationResult(ZDateTime.Empty, false, errorMessage, calculationLog);
	}
}
