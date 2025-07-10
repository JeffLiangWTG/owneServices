using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class CFSToCFSDeliveryDueDateCalculator : DeliveryDueDateCalculator
	{
		public CFSToCFSDeliveryDueDateCalculator(DeliveryDueDateCalculationContext context) : base(context)
		{
		}

		protected override IEnumerable<IDeliveryDueDateCalculationStep> GetCalculationSteps()
		{
			return new IDeliveryDueDateCalculationStep[]
		{
				new OpeningHoursDDDCalculationStep(DeliveryDueDateCalculationContext, AddressType.CFSPickupAddress, OrgTimetableType.Codes.Pickup, CalendarDayTypeProvider),
				new PreparationTimeAtCFSDDDCalculationStep(DeliveryDueDateCalculationContext, OrgTimetableType.Codes.Pickup, CalendarDayTypeProvider),
				new TransitFromCFSToCFSDDDCalculationStep(DeliveryDueDateCalculationContext, AddressType.CFSDeliveryAddress),
				new OpeningHoursDDDCalculationStep(DeliveryDueDateCalculationContext, AddressType.CFSDeliveryAddress, OrgTimetableType.Codes.Deliver, CalendarDayTypeProvider),
				new PreparationTimeAtCFSDDDCalculationStep(DeliveryDueDateCalculationContext, OrgTimetableType.Codes.Deliver, CalendarDayTypeProvider),
			};
		}
	}
}
