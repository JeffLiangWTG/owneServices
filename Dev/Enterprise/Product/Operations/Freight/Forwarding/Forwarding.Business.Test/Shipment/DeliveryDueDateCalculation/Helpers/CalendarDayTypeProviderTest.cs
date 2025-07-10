using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class CalendarDayTypeProviderTest : CalendarDayTypeProvider
	{
		public IEnumerable<DateTime> Holidays { get; set; }

		public CalendarDayTypeProviderTest(IEnumerable<DateTime> holidays)
		{
			Holidays = holidays;
		}

		protected override DateWithCalendarDayType[] GetDatesWithCalendarDayTypes(IDocAddress address, ZString pickupOrDeliveryType, DateTime fromLocalTime, DateTime toLocalTime, ZStringBuilder logger)
		{
			var result = new List<DateWithCalendarDayType>();

			var date = fromLocalTime.Date;
			while (date <= toLocalTime.Date)
			{
				bool weekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
				bool publicHoliday = Holidays.Any(holiday => holiday.Month == date.Month && holiday.Day == date.Day);

				result.Add(new DateWithCalendarDayType(date, CalendarDayType.Get(weekend, publicHoliday)));

				date = date.AddDays(1);
			}

			return result.ToArray();
		}
	}
}
