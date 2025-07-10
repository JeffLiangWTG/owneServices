using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class PreparationTimeAtCFSDDDCalculationStep : IDeliveryDueDateCalculationStep
	{
		public PreparationTimeAtCFSDDDCalculationStep(DeliveryDueDateCalculationContext context, ZString type, CalendarDayTypeProvider calendarDayTypeProvider)
		{
			Argument.NotNull(type, nameof(type));
			Argument.NotNull(context.Factory, nameof(context.Factory));
			Argument.NotNull(calendarDayTypeProvider, nameof(calendarDayTypeProvider));

			this.calendarDayTypeProvider = calendarDayTypeProvider;
			this.cfsAddress = type == OrgTimetableType.Codes.Deliver ? context.CFSDeliveryAddress : context.CFSPickupAddress;
			this.type = type;
			this.holdForPickupTime = context.HoldForPickupTime;
			this.isXtoCFS = context.IsXtoCFS;
			this.deliverOnWeekend = context.DeliverOnWeekend;
			this.factory = context.Factory;
		}

		readonly CalendarDayTypeProvider calendarDayTypeProvider;
		readonly OrgAddress cfsAddress;
		readonly ZString type;
		readonly TimeSpan? holdForPickupTime;
		readonly bool deliverOnWeekend;
		readonly bool isXtoCFS;
		readonly BusinessObjectFactory factory;

		public IDeliveryDueDateCalculationResult Calculate(IDeliveryDueDateCalculationResult previousStepResult)
		{
			var calculationLogBuilder = new ZStringBuilder();
			var initialDateTime = previousStepResult.DeliveryDueDate;

			if (deliverOnWeekend)
			{
				calculationLogBuilder.AppendLine(Res.GetString("f22fae4d-6dd2-49ed-80aa-4026c454982d", "Processing time Skipped because of Delivery on Weekend"));
				return DeliveryDueDateCalculationResult.Success(initialDateTime, calculationLogBuilder.ToString(), previousStepResult);
			}

			if (cfsAddress == null)
			{
				calculationLogBuilder.AppendLine(Res.GetString("FB8D7F20-EF04-4069-8358-166C0E0FAD2E", "Finding Processing and Cutoff Time: Skipped"));
				return DeliveryDueDateCalculationResult.Success(initialDateTime, calculationLogBuilder.ToString(), previousStepResult);
			}

			if (IsDateTimeAddressClosureTime(initialDateTime))
			{
				var beforeFindingClosestOpening = initialDateTime;
				calculationLogBuilder.AppendLine(
							Res.GetString("ab8d0720-7a3a-45f5-8a31-2c78e76134eb", "Adding processing time step for {0}: {1} is after address closure time. Checking for next available time...", type, beforeFindingClosestOpening));

				var finder = new ClosestOpeningHourFinder(cfsAddress, type, calendarDayTypeProvider, calculationLogBuilder);
				(var closestOpenDateTime, var nonWorkingDays) = finder.GetClosestOpeningHour(initialDateTime.AddMinutes(1));
				if (nonWorkingDays.Any())
				{
					calculationLogBuilder.AppendLine(
							Res.GetString("f22fae4d-6dd2-49ed-80aa-4026c454982b", "[{0}] non working days: {1}", cfsAddress.AddressFull.RemoveEndLines(),
								nonWorkingDays.ToStringWithDayOfWeeks()));
				}
				calculationLogBuilder.AppendLine(
						Res.GetString("6066cac6-d277-4310-bdb2-5d9ed4c1c318", "{0} adjusted to {1} due to opening hours of [{2}]",
						beforeFindingClosestOpening, closestOpenDateTime, cfsAddress.AddressFull.RemoveEndLines()));
				initialDateTime = closestOpenDateTime;
			}

			var (foundTimeTableDateTime, matchedTimetable) = GetMatchedTimetable(initialDateTime, previousStepResult.ArrivalTimeUsedForDeliveryCFS, calculationLogBuilder);
			if (matchedTimetable == null)
			{
				return DeliveryDueDateCalculationResult.Failure(ZString.Empty,
					Res.GetString("b38924a0-e868-4969-aaa0-0db77ca9933c", "Adding processing time step for {0}: Failed to calculate the Delivery Due Date as no matching time table found for [{1}]", type, cfsAddress.AddressFull.RemoveEndLines()));
			}

			var processingTimeInMinutes = GetProcessingTimeInMinutesForTimetable(matchedTimetable);

			calculationLogBuilder.AppendLine(Res.GetString("774d0d52-92af-4bc3-9dfa-6ce82e2faddb", "Processing time at {0} CFS for {1}: {2}",
				type, foundTimeTableDateTime.DayOfWeek, GetProcessingTime(processingTimeInMinutes)));

			(var timeAfterProcessing, var nonWorkingDays2) = AddMinutesBasedOnOpeningHours(foundTimeTableDateTime, processingTimeInMinutes, previousStepResult.ArrivalTimeUsedForDeliveryCFS, calculationLogBuilder);
			if (nonWorkingDays2.Any())
			{
				calculationLogBuilder.AppendLine(
						Res.GetString("f22fae4d-6dd2-49ed-80aa-4026c454982a", "[{0}] non working days: {1}", cfsAddress.AddressFull.RemoveEndLines(),
							nonWorkingDays2.ToStringWithDayOfWeeks()));
			}
				calculationLogBuilder.AppendLine(Res.GetString("23ae78d5-dccf-40bb-a1a8-fcd8afff4d97", "Adding processing time step for {0}: {1} adjusted to {2} based on time table of {3} CFS",
					type, foundTimeTableDateTime, timeAfterProcessing, type));

			var timetableAfterProcessing = DeliveryDueDateCalculationHelper.GetMatchedTimetable(timeAfterProcessing, cfsAddress, type, true);
			var finalResult = TryResetAdjustedFinalResultIfNecessary(calculationLogBuilder, timeAfterProcessing, timetableAfterProcessing);

			if (!holdForPickupTime.HasValue)
			{
				return DeliveryDueDateCalculationResult.Success(finalResult, calculationLogBuilder.ToString(), previousStepResult);
			}

			return DeliveryDueDateCalculationHelper.SetHoldForPickupTime(finalResult, calculationLogBuilder, holdForPickupTime, calendarDayTypeProvider, cfsAddress);
		}

		ZDateTime TryResetAdjustedFinalResultIfNecessary(ZStringBuilder calculationLogBuilder, ZDateTime timeAfterProcessing, OrgTimetable timetableAfterProcessing)
		{
			var finalResult = timeAfterProcessing;

			if (timetableAfterProcessing != null)
			{
				var cutoffTime = GetCutoffTimeForTimetable(timetableAfterProcessing);
				if (!cutoffTime.IsEmpty && (!isXtoCFS || type != OrgTimetableType.Codes.Deliver))
				{
					calculationLogBuilder.AppendLine(Res.GetString("4cc96d1b-ac2e-4a8e-ae89-2079ecad3bd0", "Cutoff Time on {0} for {1} CFS: {2}", timeAfterProcessing.DayOfWeek, type, cutoffTime.ToShortTimeString()));
					if (timeAfterProcessing.TimeOfDay > cutoffTime.TimeOfDay)
					{
						calculationLogBuilder.AppendLine(Res.GetString("f5ee35f6-5119-40a1-87ad-0632963eda81", "Calculated time is after Cutoff time. Finding next business day."));

						var finder = new ClosestOpeningHourFinder(cfsAddress, type, calendarDayTypeProvider, calculationLogBuilder);
						var cutoffAddedDay = timeAfterProcessing.Date.AddDays(1);
						(var closestOpenDay, var nonWorkingDays) = finder.GetClosestOpeningHour(cutoffAddedDay);

						if (nonWorkingDays.Any())
						{
							calculationLogBuilder.AppendLine(
								Res.GetString("ae232e22-10fd-490c-8bd9-b18740a48ae8", "[{0}] non working days: {1}", cfsAddress.AddressFull.RemoveEndLines(),
									nonWorkingDays.ToStringWithDayOfWeeks()));
						}
						calculationLogBuilder.AppendLine(Res.GetString("ee519304-5b4f-4e18-8d81-4343b87b865d", "Applying Cutoff time for {0}: {1} adjusted to {2} based on time table of {3} CFS",
							type, timeAfterProcessing, closestOpenDay, type));
						finalResult = closestOpenDay;
					}
				}
			}
			return finalResult;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		(ZDateTime, IEnumerable<ZDateTime>) AddMinutesBasedOnOpeningHours(ZDateTime dateTime, int minutes, bool useArrivalTime, ZStringBuilder logger)
		{
			List<ZDateTime> nonWorkingDays = new List<ZDateTime>();

			if (DeliveryDueDateCalculationHelper.ShouldSkipFindingOpeningHours(factory, calendarDayTypeProvider, cfsAddress, type, dateTime, useArrivalTime, deliverOnWeekend, isXtoCFS, logger))
			{
				return (dateTime.AddMinutes(minutes), nonWorkingDays);
			}

			while (minutes > 0)
			{
				var (startPrepareTime, nonWorkingDaysStep) = new ClosestOpeningHourFinder(cfsAddress, type, calendarDayTypeProvider, logger).GetClosestOpeningHour(dateTime);
				var (_, matchedTimetable) = GetMatchedTimetable(startPrepareTime, useArrivalTime, logger);

				if (matchedTimetable == null)
				{
					return (ZDateTime.Empty, nonWorkingDays);
				}

				nonWorkingDays.AddRange(nonWorkingDaysStep);

				if (matchedTimetable.OTT_TimeTo.TimeOfDay == startPrepareTime.TimeOfDay)
				{
					matchedTimetable = cfsAddress.Timetables.FirstOrDefault(t =>
						t.OTT_Type == type
						&& t.IsAppliedToDayOfWeek(dateTime.DayOfWeek)
						&& t.OTT_TimeFrom.TimeOfDay == dateTime.TimeOfDay);
					if (matchedTimetable == null)
					{
						dateTime = dateTime.AddMinutes(1);
						continue;
					}
				}

				var preparationTimeCanBeDoneInMinutes = (int)(matchedTimetable.OTT_TimeTo.TimeOfDay - startPrepareTime.TimeOfDay).TotalMinutes;
				var minutesToAdd = Math.Min(preparationTimeCanBeDoneInMinutes, minutes);

				minutes -= minutesToAdd;
				dateTime = startPrepareTime.AddMinutes(minutesToAdd);
			}

			return (dateTime, nonWorkingDays);
		}

		ZString GetProcessingTime(ZInt processingTime)
		{
			if (processingTime <= 0)
			{
				return DeliveryDueDateCalculationHelper.EmptyValueSignForLog;
			}

			if (processingTime % 60 == 0)
			{
				return Res.GetString("9f6da6b2-4bcc-49cd-94dd-d950eabaf282", "{0} Hours", processingTime / 60);
			}

			var result = Res.GetString("40e2739d-4641-4f1e-a5ea-7d3bd637fcb5", "{0} Hours and {1} Minutes", processingTime / 60, processingTime % 60);
			return result;
		}

		bool IsDateTimeAddressClosureTime(ZDateTime dateTime)
		{
			return cfsAddress.Timetables.Any(t =>
				t.OTT_Type == type
				&& t.IsAppliedToDayOfWeek(dateTime.DayOfWeek)
				&& dateTime.TimeOfDay == t.OTT_TimeTo.TimeOfDay);
		}

		(ZDateTime foundTimeTableDateTime, OrgTimetable timetable) GetMatchedTimetable(ZDateTime dateTime, bool useArrivalTime, ZStringBuilder calculationLogBuilder)
		{
			if (DeliveryDueDateCalculationHelper.ShouldSkipFindingOpeningHours(factory, calendarDayTypeProvider, cfsAddress, type, dateTime, useArrivalTime, deliverOnWeekend, isXtoCFS, calculationLogBuilder))
			{
				var timetableIgnoringOpeningHours = DeliveryDueDateCalculationHelper.GetMatchedTimetable(dateTime, cfsAddress, type, true);
				if (timetableIgnoringOpeningHours != null)
				{
					calculationLogBuilder.AppendLine("\r\n" +
						Res.GetString("ea8eae34-c615-451c-8932-c401d40580e8", "Processing Time - Find Opening Hours: Skipped"));
					return (dateTime, timetableIgnoringOpeningHours);
				}
				else
				{
					calculationLogBuilder.AppendLine("\r\n" +
						Res.GetString("257ba15b-f1b4-4ba5-8909-c9b3615d5769", "Processing Time - Find Opening Hours: Cannot skip. No timetable found for date {0}", dateTime));
				}
			}

			var timetable = DeliveryDueDateCalculationHelper.GetMatchedTimetable(dateTime, cfsAddress, type);
			if (timetable == null)
			{
				(var closestOpenDate, var nonWorkingDays) = new ClosestOpeningHourFinder(cfsAddress, type, calendarDayTypeProvider, calculationLogBuilder).GetClosestOpeningHour(dateTime);
				timetable = DeliveryDueDateCalculationHelper.GetMatchedTimetable(closestOpenDate, cfsAddress, type);

				if (nonWorkingDays.Any())
				{
					calculationLogBuilder.AppendLine(
							Res.GetString("bf76cb47-b1d1-4072-964d-5a63a74c5e4b", "[{0}] non working days: {1}", cfsAddress.AddressFull.RemoveEndLines(),
								nonWorkingDays.ToStringWithDayOfWeeks()));
				}

				calculationLogBuilder.AppendLine(
						Res.GetString("b60de5d3-1383-4dcd-954e-da70afd986c5", "Adding processing time step for {0}: {1} wasn't matched with opening hours of [{2}]. Closest opening hour was: {3}",
						type, dateTime, cfsAddress.AddressFull.RemoveEndLines(), closestOpenDate));
				return (closestOpenDate, timetable);
			}

			return (dateTime, timetable);
		}

		ZInt GetProcessingTimeInMinutesForTimetable(OrgTimetable timetable)
		{
			if (timetable.OTT_ProcessingTimeInMinutes > 0)
			{
				return timetable.OTT_ProcessingTimeInMinutes;
			}

			return cfsAddress.Timetables
				.Where(tt => tt.OTT_Type == timetable.OTT_Type && tt.DayOfWeek == timetable.DayOfWeek && tt.OTT_ProcessingTimeInMinutes > 0)
				.Select(tt => tt.OTT_ProcessingTimeInMinutes)
				.FirstOrDefault();
		}

		ZDateTime GetCutoffTimeForTimetable(OrgTimetable timetable)
		{
			if (!timetable.OTT_CutOffTime.IsDefault)
			{
				return timetable.OTT_CutOffTime;
			}

			return cfsAddress.Timetables
				.Where(tt => tt.OTT_Type == timetable.OTT_Type && tt.DayOfWeek == timetable.DayOfWeek && !tt.OTT_CutOffTime.IsDefault)
				.Select(tt => tt.OTT_CutOffTime)
				.FirstOrDefault();
		}
	}
}
