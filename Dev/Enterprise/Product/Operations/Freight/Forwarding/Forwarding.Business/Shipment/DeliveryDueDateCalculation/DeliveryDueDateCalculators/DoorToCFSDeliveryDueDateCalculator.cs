using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class DoorToCFSDeliveryDueDateCalculator : DeliveryDueDateCalculator
	{
		public DoorToCFSDeliveryDueDateCalculator(DeliveryDueDateCalculationContext context) : base(context)
		{
		}

		protected override IEnumerable<IDeliveryDueDateCalculationStep> GetCalculationSteps()
		{
			return new IDeliveryDueDateCalculationStep[]
			{
				new OpeningHoursDDDCalculationStep(DeliveryDueDateCalculationContext, AddressType.PickupAddress, OrgTimetableType.Codes.Pickup, CalendarDayTypeProvider),
				new TransitToOrFromCFSWithinZoneDDDCalculationStep(DeliveryDueDateCalculationContext, OrgTimetableType.Codes.Pickup),
				new OpeningHoursDDDCalculationStep(DeliveryDueDateCalculationContext, AddressType.CFSDeliveryAddress, OrgTimetableType.Codes.Pickup, CalendarDayTypeProvider),
				new PreparationTimeAtCFSDDDCalculationStep(DeliveryDueDateCalculationContext, OrgTimetableType.Codes.Pickup, CalendarDayTypeProvider),
				new TransitFromCFSToCFSDDDCalculationStep(DeliveryDueDateCalculationContext, AddressType.CFSDeliveryAddress),
				new OpeningHoursDDDCalculationStep(DeliveryDueDateCalculationContext, AddressType.CFSDeliveryAddress, OrgTimetableType.Codes.Deliver, CalendarDayTypeProvider),
				new PreparationTimeAtCFSDDDCalculationStep(DeliveryDueDateCalculationContext, OrgTimetableType.Codes.Deliver, CalendarDayTypeProvider),
			};
		}
	}
}
