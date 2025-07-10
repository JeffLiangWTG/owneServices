using System;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.SharedReferenceData.Business.Common.Tests
{
	public static class CommonHelpers
	{
		public class DateTimeProvider : IDateTimeProvider
		{
			public DateTime? TestDateTime { get; set; }
			public DateTime? TestHistoricalDateTime { get; set; }

			public DateTime UTCDateTime => TestDateTime.HasValue ? TestDateTime.Value : DateTime.UtcNow;
			public DateTime UTCHistoricalDate => TestHistoricalDateTime.HasValue ? TestHistoricalDateTime.Value : UTCDateTime.Date.AddYears(-1);
		}
	}
}
