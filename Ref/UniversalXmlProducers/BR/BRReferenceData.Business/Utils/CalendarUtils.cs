using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public static class CalendarUtils
	{
		static Dictionary<int, List<DateTime>> holidaysByYear = new Dictionary<int, List<DateTime>>();

		public static DateTime GetBRLastButOneWorkDay(DateTime day)
		{
			var date = day.AddDays(-1);
			while (!IsWorkDay(date))
			{
				date = date.AddDays(-1);
			}

			return date;
		}

		public static DateTime GetBRNextWorkDay(DateTime date)
		{
			date = date.AddDays(1);
			while (!IsWorkDay(date))
			{
				date = date.AddDays(1);
			}
			return date;
		}

		public static bool IsWorkDay(DateTime date)
		{
			return !IsWeekend(date) && !IsBrazilianHolliday(date);
		}

		static bool IsWeekend(DateTime date)
		{
			return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
		}

		static bool IsBrazilianHolliday(DateTime date)
		{
			List<DateTime> holidays = GetHolidays(date.Year);
			return holidays.Any(holiday => holiday.Equals(date));
		}

		public static List<DateTime> GetHolidays(int year)
		{
			if (!holidaysByYear.ContainsKey(year))
			{
				var holidays = new List<DateTime>
				{
					// new year
					CreateDateTime(year, 1, 1),

					// carnival
					Easter(year).AddDays(-48),
					Easter(year).AddDays(-47),

					// tiradentes
					CreateDateTime(year, 4, 21),

					// labour
					CreateDateTime(year, 5, 1),

					// corpus christi
					Easter(year).AddDays(60),

					// independence
					CreateDateTime(year, 9, 7),

					// aparecida
					CreateDateTime(year, 10, 12),

					// dead
					CreateDateTime(year, 11, 2),

					// republic
					CreateDateTime(year, 11, 15),

					// black consciousness
					CreateDateTime(year, 11, 20),

					// christmas
					CreateDateTime(year, 12, 25)
				};
				holidaysByYear.Add(year, holidays);
			}

			holidaysByYear.TryGetValue(year, out var dates);
			return dates;
		}

		static DateTime Easter(int year)
		{
			int a = year % 19;

			int b = year / 100;

			int c = year % 100;

			int d = b / 4;

			int e = b % 4;

			int f = (b + 8) / 25;

			int g = (b - f + 1) / 3;

			int h = (19 * a + b - d - g + 15) % 30;

			int i = c / 4;

			int k = c % 4;

			int l = (32 + 2 * e + 2 * i - h - k) % 7;

			int m = (a + 11 * h + 22 * l) / 451;

			int month = (h + l - 7 * m + 114) / 31;

			int day = ((h + l - 7 * m + 114) % 31) + 1;

			return CreateDateTime(year, month, day);
		}

		static DateTime CreateDateTime(int year, int month, int day)
		{
			return new DateTime(year, month, day);
		}
	}
}
