using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Integration;
using static Enterprise.Freight.Forwarding.Business.DeliveryDueDateCalculator;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class OpeningHoursDDDCalculationStep : IDeliveryDueDateCalculationStep
	{
		public OpeningHoursDDDCalculationStep(DeliveryDueDateCalculationContext context, AddressType addressType, ZString type, CalendarDayTypeProvider calendarDayTypeProvider)
		{
			Argument.NotNull(type, nameof(type));
			Argument.NotNull(context.Factory, nameof(context.Factory));
			Argument.NotNull(calendarDayTypeProvider, nameof(calendarDayTypeProvider));

			this.calendarDayTypeProvider = calendarDayTypeProvider;
			this.address = addressType switch
			{
				AddressType.PickupAddress => context.PickupAddress,
				AddressType.CFSPickupAddress => context.CFSPickupAddress,
				AddressType.DeliveryAddress => context.DeliveryAddress,
				AddressType.CFSDeliveryAddress => context.CFSDeliveryAddress,
				_ => throw new System.NotImplementedException(),
			};

			this.type = type;
			this.deliverOnWeekend = context.DeliverOnWeekend;
			this.isXtoCFS = context.IsXtoCFS;
			this.factory = context.Factory;
		}

		readonly CalendarDayTypeProvider calendarDayTypeProvider;
		readonly IDocAddress address;
		readonly ZString type;
		readonly bool deliverOnWeekend;
		readonly bool isXtoCFS;
		readonly BusinessObjectFactory factory;

		public IDeliveryDueDateCalculationResult Calculate(IDeliveryDueDateCalculationResult previousStepResult)
		{
			var initialDateTime = previousStepResult.DeliveryDueDate;
			var calculationLogBuilder = new ZStringBuilder();

			if (DeliveryDueDateCalculationHelper.ShouldSkipFindingOpeningHours(factory, calendarDayTypeProvider, address, type, initialDateTime, previousStepResult.ArrivalTimeUsedForDeliveryCFS, deliverOnWeekend, isXtoCFS, calculationLogBuilder))
			{
				calculationLogBuilder.AppendLine(Res.GetString("7f51b3c6-7d67-43cc-bc59-9246fe1d1c7f", "Finding Opening Hours: Skipped"));
				return DeliveryDueDateCalculationResult.Success(initialDateTime, calculationLogBuilder.ToString(), previousStepResult);
			}

			(var finalDateTime, var nonWorkingDays) =
				new ClosestOpeningHourFinder(address, type, calendarDayTypeProvider, calculationLogBuilder).GetClosestOpeningHour(initialDateTime);

			if (finalDateTime != initialDateTime)
			{
				if (nonWorkingDays.Any())
				{
					calculationLogBuilder.AppendLine(
							Res.GetString("aecf716d-5b50-4593-9ab8-9e750e5fac03", "[{0}] non working days: {1}", address.AddressFull.RemoveEndLines(),
								nonWorkingDays.ToStringWithDayOfWeeks()));
				}

				calculationLogBuilder.AppendLine(
					Res.GetString("6adae649-6632-4a9e-8c0e-6a367fedf93c", "{0} wasn't matched with opening hours of [{1}]. Closest opening hour was: {2}",
					initialDateTime, address.AddressFull.RemoveEndLines(), finalDateTime));
			}

			return finalDateTime.IsValid
				? DeliveryDueDateCalculationResult.Success(finalDateTime, calculationLogBuilder.ToString(), previousStepResult)
				: DeliveryDueDateCalculationResult.Failure(ZString.Empty, calculationLogBuilder.ToString());
		}
	}
}
