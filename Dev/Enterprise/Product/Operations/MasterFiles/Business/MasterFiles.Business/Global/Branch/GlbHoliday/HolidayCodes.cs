using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.CalendarArithmetic;
using CargoWise.Definitions;

namespace Enterprise.MasterFiles.Business
{
	public static class HolidayCodes
	{
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static MonthOfYear? GetMonthFromCode(string monthCode)
		{
			switch (monthCode)
			{
				case CalendarCodes.Months.January:
					return MonthOfYear.January;
				case CalendarCodes.Months.February:
					return MonthOfYear.February;
				case CalendarCodes.Months.March:
					return MonthOfYear.March;
				case CalendarCodes.Months.April:
					return MonthOfYear.April;
				case CalendarCodes.Months.May:
					return MonthOfYear.May;
				case CalendarCodes.Months.June:
					return MonthOfYear.June;
				case CalendarCodes.Months.July:
					return MonthOfYear.July;
				case CalendarCodes.Months.August:
					return MonthOfYear.August;
				case CalendarCodes.Months.September:
					return MonthOfYear.September;
				case CalendarCodes.Months.October:
					return MonthOfYear.October;
				case CalendarCodes.Months.November:
					return MonthOfYear.November;
				case CalendarCodes.Months.December:
					return MonthOfYear.December;
				default:
					return null;
			}
		}

		public static DayOfWeek? GetDayFromCode(string dayCode)
		{
			switch (dayCode)
			{
				case CalendarCodes.Days.Sunday:
					return DayOfWeek.Sunday;
				case CalendarCodes.Days.Monday:
					return DayOfWeek.Monday;
				case CalendarCodes.Days.Tuesday:
					return DayOfWeek.Tuesday;
				case CalendarCodes.Days.Wednesday:
					return DayOfWeek.Wednesday;
				case CalendarCodes.Days.Thursday:
					return DayOfWeek.Thursday;
				case CalendarCodes.Days.Friday:
					return DayOfWeek.Friday;
				case CalendarCodes.Days.Saturday:
					return DayOfWeek.Saturday;
				default:
					return null;
			}
		}

		public static string GetCodeFromDay(DayOfWeek day)
		{
			switch (day)
			{
				case DayOfWeek.Sunday:
					return CalendarCodes.Days.Sunday;
				case DayOfWeek.Monday:
					return CalendarCodes.Days.Monday;
				case DayOfWeek.Tuesday:
					return CalendarCodes.Days.Tuesday;
				case DayOfWeek.Wednesday:
					return CalendarCodes.Days.Wednesday;
				case DayOfWeek.Thursday:
					return CalendarCodes.Days.Thursday;
				case DayOfWeek.Friday:
					return CalendarCodes.Days.Friday;
				case DayOfWeek.Saturday:
					return CalendarCodes.Days.Saturday;
				default:
					return string.Empty;
			}
		}

		public static HolidayType GetTypeFromCode(string typeCode, bool isRecuring)
		{
			switch (typeCode)
			{
				case HolidayRecurTypeCodes.Date:
					return isRecuring
						? HolidayType.RecuringDate
						: HolidayType.Date;
				case HolidayRecurTypeCodes.First:
					return HolidayType.RecuringFirst;
				case HolidayRecurTypeCodes.Second:
					return HolidayType.RecuringSecond;
				case HolidayRecurTypeCodes.Third:
					return HolidayType.RecuringThird;
				case HolidayRecurTypeCodes.Fourth:
					return HolidayType.RecuringFourth;
				case HolidayRecurTypeCodes.Last:
					return HolidayType.RecuringLast;
				case HolidayRecurTypeCodes.GoodFriday:
					return HolidayType.GoodFriday;
				case HolidayRecurTypeCodes.EasterMonday:
					return HolidayType.EasterMonday;
				default:
					return HolidayType.None;
			}
		}
	}
}
