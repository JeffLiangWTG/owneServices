using System;

namespace CargoWise.RefDbRepo.Common.SafeDataClient
{
	public static class DateTimeExtensions
	{
		public static DateTimeOffset ToUTCDateTimeOffset(this DateTime value)
		{
			return new DateTimeOffset(value, TimeSpan.Zero);
		}

		public static DateTimeOffset ToUTCDateTimeOffset(this DateTimeOffset value)
		{
			return new DateTimeOffset(value.DateTime, TimeSpan.Zero);
		}

		public static DateTime MidnightToEndOfDay(this DateTime value)
		{
			if (value.TimeOfDay == TimeSpan.Zero)
			{
				return value.AddDays(1).AddMinutes(-1);
			}

			return value;
		}
	}
}
