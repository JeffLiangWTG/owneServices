using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class TransitToOrFromCFSWithinZoneDDDCalculationStep : IDeliveryDueDateCalculationStep
	{
		public TransitToOrFromCFSWithinZoneDDDCalculationStep(DeliveryDueDateCalculationContext context, ZString type)
		{
			Argument.NotNull(context.Factory, nameof(context.Factory));

			if (type == OrgTimetableType.Codes.Pickup)
			{
				Argument.NotNull(context.PickupAddress, nameof(context.PickupAddress));

				this.cfsAddress = context.CFSPickupAddress;
				this.nonCFSAddress = context.PickupAddress;
				this.zoneItem = context.OriginZoneItemFromPickupAddress;
			}
			else
			{
				Argument.NotNull(context.DeliveryAddress, nameof(context.DeliveryAddress));

				this.cfsAddress = context.CFSDeliveryAddress;
				this.nonCFSAddress = context.DeliveryAddress;
				this.zoneItem = context.DestinationZoneItemFromDeliveryAddress;
			}

			this.isDelivery = type == OrgTimetableType.Codes.Deliver;
			this.factory = context.Factory;
			this.serviceLevelGenericTransitTimeHasBeenUsed = context.ServiceLevelGenericTransitTimeHasBeenUsed;
		}

		readonly IDocAddress cfsAddress;
		readonly IDocAddress nonCFSAddress;
		readonly RateTransportZoneItem zoneItem;
		readonly bool isDelivery;
		readonly BusinessObjectFactory factory;
		readonly bool serviceLevelGenericTransitTimeHasBeenUsed;

		public IDeliveryDueDateCalculationResult Calculate(IDeliveryDueDateCalculationResult previousStepResult)
		{
			var initialDateTime = previousStepResult.DeliveryDueDate;
			var calculationLogBuilder = new ZStringBuilder();

			if (cfsAddress == null)
			{
				calculationLogBuilder.AppendLine(Res.GetString("966E36BD-9C98-49B5-A3CC-22A96EAA395D", "Finding Beyond Hours: Skipped"));
				return DeliveryDueDateCalculationResult.Success(initialDateTime, calculationLogBuilder.ToString(), previousStepResult);
			}

			if (!isDelivery && zoneItem == null)
			{
				calculationLogBuilder.AppendLine(Res.GetString("7fa89635-3019-41ad-8f0f-c4b7a09c6072", "Beyond Days/Hours could not be found as the Zone Item for Pickup/Delivery CFS is not present."));
				return DeliveryDueDateCalculationResult.Success(initialDateTime, calculationLogBuilder.ToString(), previousStepResult);
			}

			var stage = isDelivery ? Res.GetString("ba8e9f09-e0bc-43f7-a0f9-1534b9d037fe", "Delivery") : Res.GetString("5502b7c2-2c7c-4290-ba07-1a8ea7ea38dc", "Pickup");
			var beyondHours = zoneItem?.TQ_BeyondHours ?? 0;

			var destinationLocalDateTimeBeforeAddingBeyondHours = ConvertOriginLocalTimeToDestinationLocalTime(initialDateTime);
			if (destinationLocalDateTimeBeforeAddingBeyondHours != initialDateTime)
			{
				calculationLogBuilder.AppendLine(Res.GetString("9490fbb0-9f0d-4f5c-832e-1fe0c1871ed1", "Transit between {0} CFS and {1} address: {2} adjusted to {3}, because of the time zone difference between {4} CFS and {5} address.",
					stage, stage, initialDateTime, destinationLocalDateTimeBeforeAddingBeyondHours, stage, stage));
			}

			if (serviceLevelGenericTransitTimeHasBeenUsed)
			{
				calculationLogBuilder.AppendLine(Res.GetString("ceec8c12-39fa-4502-9705-f5c6a0bcd4fd", "Beyond days has been ignored because Service Delivery Due Time has been used."));
				return DeliveryDueDateCalculationResult.Success(destinationLocalDateTimeBeforeAddingBeyondHours, calculationLogBuilder.ToString());
			}

			var destinationLocalDateTimeAfterAddingBeyondHours = destinationLocalDateTimeBeforeAddingBeyondHours.AddHours(beyondHours);
			var beyondDaysAndHours = Res.GetString("6f359dda-9094-42c3-8c63-06a4470da737", "{0} Days + {1} Hours", beyondHours / 24, beyondHours % 24);
			calculationLogBuilder.AppendLine(Res.GetString("a58d3c5b-f499-4020-8e55-f52b84c0837b", "Beyond days for {0}: {1}",
				stage,
				beyondHours == 0 ? DeliveryDueDateCalculationHelper.EmptyValueSignForLog : beyondDaysAndHours));
			if (beyondHours > 0)
			{
				calculationLogBuilder.AppendLine(Res.GetString("8f4f5cca-5dab-42a5-a023-fe35423624cc", "Adding Beyond days/hours step for {0}: {1} adjusted to {2} based on zone definition",
					stage, destinationLocalDateTimeBeforeAddingBeyondHours, destinationLocalDateTimeAfterAddingBeyondHours));
			}

			return DeliveryDueDateCalculationResult.Success(destinationLocalDateTimeAfterAddingBeyondHours, calculationLogBuilder.ToString());
		}

		ZDateTime ConvertOriginLocalTimeToDestinationLocalTime(ZDateTime initialDateTime)
		{
			var sourceAddress = isDelivery ? cfsAddress : nonCFSAddress;
			var destinationAddress = isDelivery ? nonCFSAddress : cfsAddress;

			return DeliveryDueDateCalculationHelper.ConvertOriginLocalTimeToDestinationLocalTime(initialDateTime, sourceAddress, destinationAddress, factory);
		}
	}
}
