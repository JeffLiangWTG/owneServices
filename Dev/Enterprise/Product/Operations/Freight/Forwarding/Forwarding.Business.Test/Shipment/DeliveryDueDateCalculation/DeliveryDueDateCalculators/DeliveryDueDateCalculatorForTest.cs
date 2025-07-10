using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class DeliveryDueDateCalculatorForTest : DeliveryDueDateCalculator
	{
		public DeliveryDueDateCalculatorForTest(DeliveryDueDateCalculationContext context) : base(context)
		{
		}

		protected override IEnumerable<IDeliveryDueDateCalculationStep> GetCalculationSteps() => CalculationSteps;

		internal List<IDeliveryDueDateCalculationStep> CalculationSteps = new List<IDeliveryDueDateCalculationStep>();

		protected override (bool Result, ZString Message) AreAllPropertiesValid => (true, ZString.Empty);
	}
}
