using System;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public static class ExchangeRatesHelper
	{
		public static DateTime GetFirstDayOfNextMonth(this DateTime currentDateTime) => new DateTime(currentDateTime.Year, currentDateTime.Month, 1).AddMonths(1);

		public static DateTime GetLastDayOfNextMonth(this DateTime currentDateTime)
		{
			var firstOfNextMonth = currentDateTime.GetFirstDayOfNextMonth();
			var year = firstOfNextMonth.Year;
			var month = firstOfNextMonth.Month;
			return new DateTime(year, month, DateTime.DaysInMonth(year, month));
		}

		public static DateTime GetLatestValidPenultimateWednesday(this DateTime currentDateTime)
		{
			var result = GetPenultimateWednesday(currentDateTime);
			if (result > currentDateTime)
			{
				result = GetPenultimateWednesday(currentDateTime.AddMonths(-1));
			}
			return result;
		}

		static DateTime GetPenultimateWednesday(this DateTime dateTime)
		{
			var year = dateTime.Year;
			var month = dateTime.Month;
			var daysInMonth = DateTime.DaysInMonth(year, month);
			var differenceToLastWednesday = (new DateTime(year, month, daysInMonth).DayOfWeek - DayOfWeek.Wednesday + 7) % 7;
			return new DateTime(year, month, daysInMonth - differenceToLastWednesday - 7);
		}
	}
}
