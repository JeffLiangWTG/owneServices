using System;
using CargoWise.RefDbRepo.Common.SafeDataClient;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class DateTimeRange : IEquatable<DateTimeRange>
	{
		public DateTimeRange() { }
		public DateTimeRange(DateTime startDate, DateTime endDate)
		{
			StartDate = startDate.ToUTCDateTimeOffset();
			EndDate = endDate.ToUTCDateTimeOffset();
		}
		public DateTimeRange(DateTimeOffset startDateOffset, DateTimeOffset endDateOffset)
		{
			StartDate = startDateOffset;
			EndDate = endDateOffset;
		}
		public DateTimeOffset StartDate { get; set; }
		public DateTimeOffset EndDate { get; set; }

		public bool Equals(DateTimeRange other)
		{
			return other != null && ((StartDate == other.StartDate) && (EndDate == other.EndDate));
		}

		public override bool Equals(object obj)
		{
			return obj is DateTimeRange dateTimeRange && Equals(dateTimeRange);
		}

		public override int GetHashCode()
		{
			return StartDate.GetHashCode() ^ EndDate.GetHashCode();
		}
	}
}
