using CargoWise.Types;

namespace Enterprise.Freight.Integration
{
	public interface IS8MatchResult
	{
		ZString ScheduleStatus { get; }
		ScheduleInfo MatchedSchedule { get; }
		ZString MatchErrorMessage { get; }
	}
}
