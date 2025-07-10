using System;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public static class SystemContext
	{
		[ThreadStatic]
		private static Func<DateTime> now;

		[ThreadStatic]
		private static Func<DateTime> utcNow;

		public static Func<DateTime> Now
		{
			get { return now ?? (now = () => DateTime.Now); }
			set
			{
				now = value;
				utcNow = () => TimeZoneInfo.ConvertTimeToUtc(now());
			}
		}

		public static Func<DateTime> UtcNow
		{
			get { return utcNow ?? (utcNow = () => DateTime.UtcNow); }
			set { utcNow = value; }
		}
	}
}
