using System.Collections.Generic;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class CFSToDoorDeliveryDueDateCalculator : DeliveryDueDateCalculator
	{
		public CFSToDoorDeliveryDueDateCalculator(DeliveryDueDateCalculationContext context) : base(context)
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
				DeliveryDueDateCalculationContext.DeliverOnWeekend
					? new DeliverOnWeekendCalculationStep(DeliveryDueDateCalculationContext)
					: new TransitToOrFromCFSWithinZoneDDDCalculationStep(DeliveryDueDateCalculationContext, OrgTimetableType.Codes.Deliver),
				ToDoorLastStep
			};
		}

		IDeliveryDueDateCalculationStep ToDoorLastStep
		{
			get
			{
				if (DeliveryDueDateCalculationContext.DeliveryDueTime != null)
				{
					return new DeliveryDueTimeCalculationStep(DeliveryDueDateCalculationContext, CalendarDayTypeProvider);
				}
				else
				{
					return new OpeningHoursDDDCalculationStep(DeliveryDueDateCalculationContext, AddressType.DeliveryAddress, OrgTimetableType.Codes.Deliver, CalendarDayTypeProvider);
				}
			}
		}
	}
}
