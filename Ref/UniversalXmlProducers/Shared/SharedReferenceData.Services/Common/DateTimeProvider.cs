using System;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Common
{
	public class DateTimeProvider : IDateTimeProvider
	{
		public DateTimeProvider(int historicalMonths)
		{
			months = historicalMonths;
		}
		readonly int months;

		public DateTime UTCDateTime => DateTime.UtcNow;

		public DateTime UTCHistoricalDate => UTCDateTime.Date.AddMonths(-months);
	}
}
