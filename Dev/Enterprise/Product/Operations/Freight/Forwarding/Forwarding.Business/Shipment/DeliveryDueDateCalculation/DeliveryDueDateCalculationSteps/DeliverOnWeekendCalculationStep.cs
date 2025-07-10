using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	public sealed class DeliverOnWeekendCalculationStep : IDeliveryDueDateCalculationStep
	{
		public DeliverOnWeekendCalculationStep(DeliveryDueDateCalculationContext context)
		{
			Argument.NotNull(context.DeliveryAddress, nameof(context.DeliveryAddress));
			this.serviceLevel = context.ServiceLevel;
			this.address = context.DeliveryAddress;
			this.factory = context.Factory;
		}

		readonly RefServiceLevel serviceLevel;
		readonly IDocAddress address;
		readonly BusinessObjectFactory factory;

		public IDeliveryDueDateCalculationResult Calculate(IDeliveryDueDateCalculationResult previousStepResult)
		{
			var initialDateTime = previousStepResult.DeliveryDueDate;

			if (serviceLevel == null || !serviceLevel.DeliverOnWeekend)
			{
				return DeliveryDueDateCalculationResult.Success(initialDateTime, ZString.Empty, previousStepResult);
			}
			var calculationLog = new ZStringBuilder();
			var initialTime = initialDateTime.TimeOfDay;
			var (weekend1, weekend2) = GetDeliverOnWeekendDates(initialDateTime);
			var publicHolidaysProvider = new CalendarDayTypeProvider();
			var publicHolidays = new List<ZDateTime>();

			if (!publicHolidaysProvider.IsPublicHoliday(address, OrgTimetableType.Codes.Deliver, weekend1, calculationLog))
			{
				calculationLog.AppendLine(Res.GetString("85a167ba-dd26-4e5d-8187-ae455fa56e63",
						"Service Level {0}, has delivery on weekend ticked. The next available weekend was: {1} which is {2}, so we picked that weekend as delivery due date.",
						serviceLevel.RS_Code,
						weekend1,
						weekend1.DayOfWeek));
				return DeliveryDueDateCalculationResult.Success(weekend1.Add(initialTime), calculationLog.ToString(), previousStepResult);
			}

			var closestOpeningHourFinder = new ClosestOpeningHourFinder(address, OrgTimetableType.Codes.Deliver, new CalendarDayTypeProvider(), calculationLog);
			(var nextWorkingDay, var nonWorkingDays) = closestOpeningHourFinder.FindNextWorkingDay(weekend1);

			if (!weekend2.IsEmpty && weekend2 < nextWorkingDay.Date)
			{
				if (!publicHolidaysProvider.IsPublicHoliday(address, OrgTimetableType.Codes.Deliver, weekend2, calculationLog))
				{
					calculationLog.AppendLine(Res.GetString("0b790cc2-12cd-b82b-d896-fe6804e67c25",
						"Service Level {0} has delivery on weekend ticked, but {1} is a public holiday. The next available weekend was: {2} which is {3}, so we picked that weekend as delivery due date.",
						serviceLevel.RS_Code,
						weekend1,
						weekend2,
						weekend2.DayOfWeek));
					return DeliveryDueDateCalculationResult.Success(weekend2.Add(initialTime), calculationLog.ToString(), previousStepResult);
				}
				else
				{
					publicHolidays.Add(weekend2);
				}
			}

			publicHolidays.AddRange(nonWorkingDays.Where(day => publicHolidaysProvider.IsPublicHoliday(address, OrgTimetableType.Codes.Deliver, day.Date, calculationLog)));

			calculationLog.AppendLine(Res.GetString("56471D29-54FB-4BC5-A32C-A8EF5F33F72E",
					"Service Level {0} has delivery on weekend ticked, but {1} are public holiday(s).",
					serviceLevel.RS_Code,
					string.Join(",", publicHolidays)));

			calculationLog.AppendLine(Res.GetString("4233ba44-0e56-4603-97ba-65753949dd17",
					"Delivery due date adjusted from {0} to {1}.",
					initialDateTime,
					nextWorkingDay.Date));
			return DeliveryDueDateCalculationResult.Success(nextWorkingDay.Date.Add(initialTime), calculationLog.ToString(), previousStepResult);
		}

		(ZDate weekend1, ZDate weekend2) GetDeliverOnWeekendDates(ZDateTime initialDateTime)
		{
			var (weekendDay1, weekendDay2) = DeliveryDueDateCalculationHelper.GetWeekendDays(address, factory);
			var nextWeekendDay1 = serviceLevel.RS_DeliverOnSaturday
				? DeliveryDueDateCalculationHelper.GetNextDayOfWeek(initialDateTime, weekendDay1)
				: ZDate.Empty;

			if (weekendDay2 == null)
			{
				return (nextWeekendDay1, ZDate.Empty);
			}

			var nextWeekendDay2 = serviceLevel.RS_DeliverOnSunday
				? DeliveryDueDateCalculationHelper.GetNextDayOfWeek(initialDateTime, (DayOfWeek)weekendDay2)
				: ZDate.Empty;

			var weekend1 = ZDate.Empty;
			var weekend2 = ZDate.Empty;

			if (nextWeekendDay1.IsEmpty || nextWeekendDay2.IsEmpty)
			{
				weekend1 = nextWeekendDay1.IsEmpty ? nextWeekendDay2 : nextWeekendDay1;
			}
			else
			{
				weekend1 = nextWeekendDay1 < nextWeekendDay2 ? nextWeekendDay1 : nextWeekendDay2;
				weekend2 = nextWeekendDay1 < nextWeekendDay2 ? nextWeekendDay2 : nextWeekendDay1;
			}

			return (weekend1, weekend2);
		}
	}
}
