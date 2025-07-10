using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public interface IOverlappingCalculator
	{
		DateTime GetEndDate(DateTime expireTime);
		DateTimeOffset GetEndDate(DateTimeOffset expireTime);
		bool IsOverlapped(IEnumerable<DateTimeRange> dateRanges);
		bool IsOverlapped(IEnumerable<DateTimeRange> dateRanges, DateTimeRange dateRangeToCheck);
	}
}
