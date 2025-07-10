
namespace CargoWise.RefDbRepo.PLReferenceData.Tests
{
	public static class Configuration
	{
		public static class Cus {
			public const int Day = 23;
			public const int Month = 12;
			public const int Year = 2020;
		}

		public static class Taric {
			public const int StartYear = 2020;
			public const int StartMonth = 08;
			public const int StartDay = 20;
			public const int StartHour = 0; // 24 hour clock [0-23]
			public const int StartMinute = 0;
			public const int EndYear = 2020;
			public const int EndMonth = 08;
			public const int EndDay = 20;
			public const int EndHour = 23; // 24 hour clock [0-23]
			public const int EndMinute = 59;
		}
	}
}
