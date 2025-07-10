using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	public interface IDeliveryDueDateCalculationStep
	{
		IDeliveryDueDateCalculationResult Calculate(IDeliveryDueDateCalculationResult previousStepResult);
	}
}
