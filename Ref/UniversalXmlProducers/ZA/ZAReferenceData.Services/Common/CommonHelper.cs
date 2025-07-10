using System;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Common
{
	public static class CommonHelper
	{
		public static DateTime MinimumDateTime => new DateTime(1900, 1, 1);
		public static DateTime MaximumDateTime => new DateTime(2079, 6, 6, 23, 59, 0);

		public static DateTime CalcMinDate(DateTime? dateTime)
		{
			var dt = dateTime.IsNullOrDefault() ? MinimumDateTime : dateTime.Value;

			dt = dt < MinimumDateTime ? MinimumDateTime : dt;

			return DropSeconds(dt);
		}

		public static DateTime CalcMaxDate(DateTime? dateTime)
		{
			var dt = dateTime.IsNullOrDefault() ? MaximumDateTime : dateTime.Value;

			if (dt.IsAtMidnight() && dt.Date != DateTime.MaxValue.Date)
			{
				dt = dt.AddDays(1);
			}
			dt = dt > MaximumDateTime ? MaximumDateTime : dt;

			return BackInAMinute(DropSeconds(dt));
		}

		public static DateTime GetMinDate(DateTime dateTime1, DateTime dateTime2) => dateTime1 < dateTime2 ? dateTime1 : dateTime2;

		public static DateTime GetMaxDate(DateTime dateTime1, DateTime dateTime2) => dateTime1 > dateTime2 ? dateTime1 : dateTime2;

		public static DateTime DropSeconds(DateTime dateTime) => dateTime.AddSeconds(-dateTime.Second);

		public static DateTime BackInAMinute(DateTime dateTime) => dateTime.IsAtMidnight() ? dateTime.AddMinutes(-1) : dateTime;

		static bool IsNullOrDefault(this DateTime? dateTime) => dateTime == null || dateTime == DateTime.MinValue;

		static bool IsAtMidnight(this DateTime dateTime) => dateTime.Hour == 0 && dateTime.Minute == 0 && dateTime.Second == 0;
	}
}
