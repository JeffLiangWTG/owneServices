using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class OverlappingCalculator : IOverlappingCalculator
	{
		bool inclusiveEndDate;
		public OverlappingCalculator(bool inclusiveEndDate)
		{
			this.inclusiveEndDate = inclusiveEndDate;
		}

		public DateTime GetEndDate(DateTime expireTime)
		{
			return inclusiveEndDate ? expireTime.AddMinutes(-1) : expireTime;
		}

		public DateTimeOffset GetEndDate(DateTimeOffset expireTime)
		{
			return inclusiveEndDate ? expireTime.AddMinutes(-1) : expireTime;
		}

		public bool IsOverlapped(IEnumerable<DateTimeRange> dateRanges, DateTimeRange dateRangeToCheck)
		{
			return inclusiveEndDate ? dateRanges.Any(x => x.StartDate <= dateRangeToCheck.EndDate && x.EndDate >= dateRangeToCheck.StartDate)
				: dateRanges.Any(x => x.StartDate < dateRangeToCheck.EndDate && x.EndDate > dateRangeToCheck.StartDate);
		}

		public bool IsOverlapped(IEnumerable<DateTimeRange> dateRanges)
		{
			if (dateRanges.Count() < 2)
			{
				return false;
			}
			var sorted = dateRanges.OrderBy(x => x.StartDate).ToArray();

			if (inclusiveEndDate)
			{
				for (int i = 1; i < sorted.Length; i++)
				{
					if (sorted[i - 1].EndDate >= sorted[i].StartDate)
					{
						return true;
					}
				}
			}
			else
			{
				for (int i = 1; i < sorted.Length; i++)
				{
					if (sorted[i - 1].EndDate > sorted[i].StartDate)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
