using CargoWise.Types;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.Forwarding.Routing.S8.Business
{
	public class S8MatchResult : IS8MatchResult
	{
		public ZString ScheduleStatus { get; set; }
		public ScheduleInfo MatchedSchedule { get; set; }
		public ZString MatchErrorMessage { get; set; }

		public IS8MatchResult Clone()
		{
			return (S8MatchResult)MemberwiseClone();
		}
	}
}
