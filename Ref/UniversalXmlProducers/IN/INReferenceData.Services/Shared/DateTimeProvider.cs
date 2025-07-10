using System;

namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTime GetDateTimeNow()
		{
			return DateTime.Now;
		}

		public DateTime GetIndiaToday()
		{
			return GetIndiaTime().Date;
		}

		public DateTime GetIndiaTime()
		{
			return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(IndiaTimeZoneID));
		}

		const string IndiaTimeZoneID = "India Standard Time";
	}
}
