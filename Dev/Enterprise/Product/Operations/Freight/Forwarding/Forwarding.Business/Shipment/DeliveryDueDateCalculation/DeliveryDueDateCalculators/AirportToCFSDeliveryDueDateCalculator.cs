using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class AirportToCFSDeliveryDueDateCalculator : DeliveryDueDateCalculator
	{
		public AirportToCFSDeliveryDueDateCalculator(DeliveryDueDateCalculationContext context) : base(context)
		{
		}

		protected override IEnumerable<IDeliveryDueDateCalculationStep> GetCalculationSteps()
		{
			return new IDeliveryDueDateCalculationStep[]
			{
				new TransitFromCFSToCFSDDDCalculationStep(DeliveryDueDateCalculationContext, AddressType.CFSDeliveryAddress ),
				new OpeningHoursDDDCalculationStep(DeliveryDueDateCalculationContext,  AddressType.CFSDeliveryAddress, OrgTimetableType.Codes.Deliver, CalendarDayTypeProvider),
				new PreparationTimeAtCFSDDDCalculationStep(DeliveryDueDateCalculationContext, OrgTimetableType.Codes.Deliver, CalendarDayTypeProvider),
			};
		}
	}
}
