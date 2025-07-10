using System;

namespace CargoWise.RefDbRepo.SharedReferenceData.Services.Common
{
	public interface IDateTimeProvider
	{
		DateTime UTCDateTime { get; }
		DateTime UTCHistoricalDate { get; }
	}
}
