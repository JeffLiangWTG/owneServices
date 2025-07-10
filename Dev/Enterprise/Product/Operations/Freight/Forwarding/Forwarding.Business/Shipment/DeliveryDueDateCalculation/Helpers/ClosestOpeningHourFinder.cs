using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ClosestOpeningHourFinder
	{
		public ClosestOpeningHourFinder(IDocAddress address, ZString pickupOrDeliveryType, CalendarDayTypeProvider calendarDayTypeProvider, ZStringBuilder logger)
		{
			Argument.NotNull(calendarDayTypeProvider, nameof(calendarDayTypeProvider));
			Argument.NotNull(address, nameof(address));
			Argument.NotNull(pickupOrDeliveryType, nameof(pickupOrDeliveryType));
			Argument.NotNull(logger, nameof(logger));

			this.calendarDayTypeProvider = calendarDayTypeProvider;
			this.address = address;
			this.pickupOrDeliveryType = pickupOrDeliveryType;
			this.logger = logger;
		}

		readonly CalendarDayTypeProvider calendarDayTypeProvider;
		readonly IDocAddress address;
		readonly ZString pickupOrDeliveryType;
		readonly ZStringBuilder logger;

		public (ZDateTime ClosestOpeningHour, IEnumerable<ZDateTime> NonWorkingDays) GetClosestOpeningHour(ZDateTime dateTime)
		{
			if (AddressIsOpenAtDateTime(dateTime))
			{
				return (dateTime, new List<ZDateTime>());
			}

			return GetNextAvailableDateTimeWithinOpeningHours(dateTime);
		}

		(ZDateTime ClosestOpeningHour, IEnumerable<ZDateTime> NonWorkingDays) GetClosestNonApplicableOpeningHour(ZDateTime dateTime)
		{
			// Not Applicable means that the address is always open 24/7 except for public holidays
			var nonWorkingDays = new List<ZDateTime>();
			while (calendarDayTypeProvider.IsPublicHoliday(address, pickupOrDeliveryType, dateTime.Date, logger))
			{
				dateTime = dateTime.Date;
				nonWorkingDays.Add(dateTime);
				dateTime = dateTime.AddDays(1);
			}
			return (dateTime, nonWorkingDays);
		}

		public (ZDateTime ClosestOpeningHour, List<ZDateTime> NonWorkingDays) FindNextWorkingDay(ZDateTime dateTime)
		{
			var nonWorkingDays = new List<ZDateTime>();
			var maximumTriesToFindNextWorkingDay = 100;
			var i = 0;
			var nextWorkingDay = dateTime;
			while (calendarDayTypeProvider.IsNonWorkingDay(address, pickupOrDeliveryType, nextWorkingDay.Date, logger) && i < maximumTriesToFindNextWorkingDay)
			{
				nonWorkingDays.Add(nextWorkingDay);
				nextWorkingDay = nextWorkingDay.AddDays(1);
				i++;
			}

			return (nextWorkingDay.Date, nonWorkingDays);
		}

		(ZDateTime ClosestOpeningHour, IEnumerable<ZDateTime> NonWorkingDays) GetNextAvailableDateTimeWithinOpeningHours(ZDateTime dateTime)
		{
			if (!(address is OrgAddress orgAddress))
			{
				return FindNextWorkingDay(dateTime);
			}

			if (orgAddress.Timetables.NotApplicable)
			{
				return GetClosestNonApplicableOpeningHour(dateTime);
			}

			var nonWorkingDays = new List<ZDateTime>();

			int failed = 0;

			for (var day = dateTime; failed < 100; day = day.Date.AddDays(1))
			{
				var (startDateTime, nonWorkingDay) = GetStartDateTimeOnDate(orgAddress, day);

				if (nonWorkingDay.IsValid)
				{
					nonWorkingDays.Add(nonWorkingDay);
				}
				if (startDateTime.IsValid)
				{
					return (startDateTime, nonWorkingDays);
				}

				failed++;
			}

			return (ZDateTime.Empty, nonWorkingDays);
		}

		ZBool AddressIsOpenAtDateTime(ZDateTime dateTime)
		{
			if (calendarDayTypeProvider.IsPublicHoliday(address, pickupOrDeliveryType, dateTime.Date, logger))
			{
				return false;
			}

			return !(address is OrgAddress orgAddress) || orgAddress.TimetablesRangeType == OrgTimeTableRangeType.NotApplicable || DeliveryDueDateCalculationHelper.GetMatchedTimetable(dateTime, orgAddress, pickupOrDeliveryType) != null;
		}

		(ZDateTime startDateTime, ZDateTime nonWorkingDay) GetStartDateTimeOnDate(OrgAddress orgAddress, ZDateTime start)
		{
			var nonWorkingDay = start.Date;

			if (calendarDayTypeProvider.IsPublicHoliday(address, pickupOrDeliveryType, start.Date, logger))
			{
				return (ZDateTime.Empty, nonWorkingDay);
			}

			ZDateTime CalculateDateTime(ZDate date, ZDateTime time) => date.AddHours(time.TimeOfDay.Hours).AddMinutes(time.TimeOfDay.Minutes);

			var result = ZDateTime.Empty;

			foreach (var dayTimetable in orgAddress.Timetables.Where(t => t.OTT_Type == pickupOrDeliveryType && t.IsAppliedToDayOfWeek(start.DayOfWeek)))
			{
				var openDateTime = CalculateDateTime(start.Date, dayTimetable.OTT_TimeFrom);
				var closeDateTime = CalculateDateTime(start.Date, dayTimetable.OTT_TimeTo);

				if (start < closeDateTime)
				{
					if (start >= openDateTime)
					{
						return (start, ZDateTime.Empty);
					}

					if (result.IsEmpty || openDateTime < result)
					{
						nonWorkingDay = ZDate.Empty;
						result = openDateTime;
					}
				}
				else
				{
					nonWorkingDay = ZDate.Empty;
				}
			}

			return (result, nonWorkingDay);
		}
	}
}
