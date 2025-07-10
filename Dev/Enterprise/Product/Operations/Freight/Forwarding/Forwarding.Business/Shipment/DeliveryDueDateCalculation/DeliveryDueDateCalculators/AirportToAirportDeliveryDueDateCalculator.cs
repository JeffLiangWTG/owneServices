using System.Collections.Generic;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class AirportToAirportDeliveryDueDateCalculator : DeliveryDueDateCalculator
	{
		public AirportToAirportDeliveryDueDateCalculator(DeliveryDueDateCalculationContext context) : base(context)
		{
		}

		protected override IEnumerable<IDeliveryDueDateCalculationStep> GetCalculationSteps()
		{
			return System.Array.Empty<IDeliveryDueDateCalculationStep>();
		}

		public override IDeliveryDueDateCalculationResult CalculateDeliveryDueDate() => DeliveryDueDateCalculationContext.CalculateTimeOfArrivalToAirport();
	}
}
