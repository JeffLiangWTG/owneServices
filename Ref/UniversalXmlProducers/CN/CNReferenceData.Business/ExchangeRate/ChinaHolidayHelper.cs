using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public static class ChinaHolidayHelper
	{
		static readonly List<DateTime> ThirdWednesdayHolidays = new List<DateTime>()
		{
			new DateTime(2018, 2, 21),
			new DateTime(2021, 2, 17)
		};

		public static bool IsHoliday(DateTime date)
		{
			return ThirdWednesdayHolidays.Contains(date);
		}

		public static int AcceptableYear => 2025;
	}
}
