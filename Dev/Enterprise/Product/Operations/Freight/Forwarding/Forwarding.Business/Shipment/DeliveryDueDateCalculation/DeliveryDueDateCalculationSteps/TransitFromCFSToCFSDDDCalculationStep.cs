using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Forwarding.Business.DeliveryDueDateCalculator;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class TransitFromCFSToCFSDDDCalculationStep : IDeliveryDueDateCalculationStep
	{
		public TransitFromCFSToCFSDDDCalculationStep(DeliveryDueDateCalculationContext context, AddressType deliveryAddressType)
		{
			Argument.NotNull(context.ServiceLevel, nameof(context.ServiceLevel));
			Argument.NotNullOrEmpty(context.Mode, nameof(context.Mode));
			Argument.NotNull(context.Factory, nameof(context.Factory));

			this.destinationCFS = deliveryAddressType switch
			{
				AddressType.DeliveryAgentAddress => context.DeliveryAgentAddress,
				AddressType.CFSDeliveryAddress => context.CFSDeliveryAddress,
				_ => throw new NotImplementedException(),
			};
			this.originCFS = context.CFSPickupAddress;
			this.originZone = context.OriginZone;
			this.destinationZone = context.DestinationZone;
			this.serviceLevel = context.ServiceLevel;
			this.mode = context.Mode;
			this.factory = context.Factory;
		}

		readonly IDocAddress originCFS;
		readonly RateTransportZone originZone;
		readonly IDocAddress destinationCFS;
		readonly RateTransportZone destinationZone;
		readonly RefServiceLevel serviceLevel;
		readonly ZString mode;
		readonly BusinessObjectFactory factory;

		public IDeliveryDueDateCalculationResult Calculate(IDeliveryDueDateCalculationResult previousStepResult)
		{
			var initialDateTime = previousStepResult.DeliveryDueDate;

			if (originZone != null && destinationZone != null && originZone.PK == destinationZone.PK)
			{
				var message = Res.GetString("af305ab5-3844-4893-9eac-2a31457aabb6", "No transit time has been selected because origin zone is the same as destination zone");
				return DeliveryDueDateCalculationResult.Success(initialDateTime, message);
			}

			return GetTransitTime(originZone, destinationZone, initialDateTime);
		}

		IDeliveryDueDateCalculationResult GetTransitTime(RateTransportZone originZone, RateTransportZone destinationZone, ZDateTime initialDateTime)
		{
			var calculationLogBuilder = new ZStringBuilder();
			ZInt transitTimeHours = 0;
			var transitTimeArrival = TimeSpan.MinValue;
			var transitTimeDetailFound = false;
			var result = initialDateTime;

			if (originZone != null && destinationZone != null)
			{
				var destinationLocalInitialDateTime = DeliveryDueDateCalculationHelper.ConvertOriginLocalTimeToDestinationLocalTime(initialDateTime, originCFS, destinationCFS, factory);
				if (destinationLocalInitialDateTime != initialDateTime)
				{
					result = destinationLocalInitialDateTime;
					calculationLogBuilder.AppendLine(Res.GetString("12415c33-a3ac-43e7-9505-a56fdeeb32a0",
						"Time Conversion between {0} and {1}: {2} adjusted to {3}, because of the time zone difference between Pickup CFS and Delivery CFS.",
						GetReadableName(originCFS, originZone), GetReadableName(destinationCFS, destinationZone), initialDateTime, destinationLocalInitialDateTime));
				}

				var ranker = new ColumnValueRanker();
				ranker.Add(RefTransitTimeSchema.RTT_RS_NKServiceLevel, serviceLevel.RS_Code);
				ranker.Add(RefTransitTimeSchema.RTT_Mode, GetRefTransitTimeModeFallbacks().Cast<object>().ToArray());

				var transitTimeQueryWithZones = new ZQuery();
				transitTimeQueryWithZones.AddToFilter(RefTransitTimeSchema.RTT_TZ_OriginDomesticZone, SQLComparisonOperator.Equal, originZone.PK);
				transitTimeQueryWithZones.AddToFilter(RefTransitTimeSchema.RTT_TZ_DestinationDomesticZone, SQLComparisonOperator.Equal, destinationZone.PK);

				var transitTime = ranker.GetBestMatch<RefTransitTime>(factory, transitTimeQueryWithZones);

				var failureMessage = Res.GetString("b1aee90e-cf6e-45a3-a511-6bd73f23f674",
				"Finding Transit Time: Failed to calculate the Delivery Due Date because there is no Transit Time record (or Service Level > Default Transit Time for the current service level) exists between the Origin and Destination Transport Zones for the selected Transport Mode and/or Service Level. Please review the Transit Time setup within Maintain > Locations > Transit Time or select a different Transport Mode and/or Service Level.");

				if (transitTime == null && (serviceLevel == null || serviceLevel.RS_DefaultTransitHours == 0))
				{
					return DeliveryDueDateCalculationResult.Failure(errorMessage: failureMessage, calculationLog: failureMessage);
				}

				transitTimeDetailFound = FindTransitTimeHoursAndArrivalTimeForDayOfWeek(DeliveryDueDateCalculationHelper.GetDayOfWeek(initialDateTime.DayOfWeek), transitTime, destinationLocalInitialDateTime,
						out transitTimeHours, out transitTimeArrival);
			}

			if (!transitTimeDetailFound && (serviceLevel == null || serviceLevel.RS_DefaultTransitHours == 0))
			{
				return DeliveryDueDateCalculationResult.Failure(errorMessage: TransitTimeNotFoundErrorMessage, calculationLog: TransitTimeNotFoundErrorMessage);
			}

			var arrivalTimeUsedForDeliveryCFS = false;
			var serviceLevelGenericTransitTimeHasBeenUsed = false;

			if (transitTimeDetailFound)
			{
				calculationLogBuilder.AppendLine(
					Res.GetString("f837b5af-8412-4dd3-b580-6350421871f6", "Transit time detail between Origin and Destination found for the specified day of week: {0}",
						initialDateTime.DayOfWeek));
			}
			else if (serviceLevel != null && serviceLevel.RS_DefaultTransitHours > 0)
			{
				transitTimeHours = serviceLevel.RS_DefaultTransitHours;
				transitTimeArrival = serviceLevel.RS_DefaultArrivalTime.TimeOfDay;
				calculationLogBuilder.AppendLine(
					Res.GetString("97bd95d1-e815-4224-8c8f-46e0094b7ce5", "No transit time detail between Origin and Destination found for the specified day of week: {0}",
						initialDateTime.DayOfWeek));
				calculationLogBuilder.AppendLine(
						Res.GetString("f08777dd-a556-4cc2-b0d1-f1f8bb43006c", "Service Level > Default Transit Days/Arrival Time used for the calculation"));
				serviceLevelGenericTransitTimeHasBeenUsed = true;
			}

			if (transitTimeArrival.Ticks != 0)
			{
				var newValue = result.AddDays(transitTimeHours / 24);
				newValue = newValue.Date.Add(transitTimeArrival);
				calculationLogBuilder.AppendLine(Res.GetString("00faf829-e8b8-41f7-94af-d05819c23dca", "Transit Time: {0} Days", transitTimeHours == 0 ? "-" : (transitTimeHours / 24).ToString()));
				calculationLogBuilder.AppendLine(Res.GetString("f42f4fbe-9079-4647-a824-e63461ba8527", "Arrival Time: {0}", transitTimeArrival));
				calculationLogBuilder.AppendLine(Res.GetString("6c52044d-bec3-4ef6-bd8c-9efa57e2da7a",
					"Adding Transit Time step: {0} adjusted to {1} based on Transit Time record (Or Service level default)", result, newValue));
				arrivalTimeUsedForDeliveryCFS = true;
				result = newValue;
			}
			else
			{
				var newValue = result.AddHours(transitTimeHours);
				calculationLogBuilder.AppendLine(Res.GetString("08459b52-77c6-4032-80ae-ad272a62c200", "Transit Time: {0} Hours", transitTimeHours));
				calculationLogBuilder.AppendLine(Res.GetString("46891510-a393-4850-b3a1-7d3732034072", "Arrival Time: -"));
				calculationLogBuilder.AppendLine(Res.GetString("32461a0d-1cfb-42a5-ae50-5f33b7252d70",
					"Adding Transit Time step: {0} adjusted to {1} based on Transit Time record (Or Service level default)", result, newValue));
				result = newValue;
			}

			return DeliveryDueDateCalculationResult.Success(result, calculationLogBuilder.ToString(), arrivalTimeUsedForDeliveryCFS, serviceLevelGenericTransitTimeHasBeenUsed);
		}

		ZString GetReadableName(IDocAddress cfs, RateTransportZone zone)
		{
			return Res.GetString("deb75da0-fa03-435f-89fd-2c0bb8708d74", "{0} ({1})", zone?.TZ_ZoneName, cfs?.E2_City);
		}

		bool FindTransitTimeHoursAndArrivalTimeForDayOfWeek(ZByte dayOfWeek, RefTransitTime transitTime, ZDateTime destinationInitTime, out ZInt transitTimeHours, out TimeSpan transitTimeArrival)
		{
			transitTimeHours = 0;
			transitTimeArrival = TimeSpan.Zero;

			if (transitTime != null && transitTime.RefTransitTimeDetails.Any())
			{
				foreach (var detail in transitTime.RefTransitTimeDetails.Where(c => c.RTD_DayOfWeek == dayOfWeek && c.RTD_TransitHours > 0))
				{
					var timeAfterTransit = destinationInitTime.AddDays(detail.TransitDays);
					if ((detail.RTD_EffectiveDate.IsEmpty || detail.RTD_EffectiveDate.ToDateTime() <= destinationInitTime)
						&& (detail.RTD_EndDate.IsEmpty || timeAfterTransit <= detail.RTD_EndDate.ToDateTime()))
					{
						transitTimeHours = detail.RTD_TransitHours;
						transitTimeArrival = detail.RTD_ArrivalTime.TimeOfDay;
						return true;
					}
				}
			}

			return false;
		}

		IEnumerable<ZString> GetRefTransitTimeModeFallbacks()
		{
			var modeFallbacks = new List<ZString> { mode };

			if (mode == Core.Constants.RateMode.ALL)
			{
				return modeFallbacks.ToArray();
			}

			switch (mode)
			{
				case Core.Constants.RateMode.ULD:
				case Core.Constants.RateMode.LSE:
					modeFallbacks.Add(Core.Constants.RateMode.AIR);
					break;
				case Core.Constants.RateMode.LCL:
				case Core.Constants.RateMode.FCL:
					modeFallbacks.Add(Core.Constants.RateMode.SEA);
					break;
				case Core.Constants.RateMode.LRO:
				case Core.Constants.RateMode.FRO:
				case Core.Constants.RateMode.FTL:
					modeFallbacks.Add(Core.Constants.RateMode.ROA);
					break;
				case Core.Constants.RateMode.LRA:
				case Core.Constants.RateMode.FRA:
				case Core.Constants.RateMode.FWL:
					modeFallbacks.Add(Core.Constants.RateMode.RAI);
					break;
			}
			modeFallbacks.Add(Core.Constants.RateMode.ALL);
			return modeFallbacks;
		}

		string TransitTimeNotFoundErrorMessage => Res.GetString("04ce4c5f-3539-435e-bdff-8c64e5eae917", "Finding Transit Time: Failed to calculate the Delivery Due Date because the Shipment's dates are outside of the validity period defined on the Transit Time record, please check the Effective/End Dates on the Transit Time module.");
	}
}
