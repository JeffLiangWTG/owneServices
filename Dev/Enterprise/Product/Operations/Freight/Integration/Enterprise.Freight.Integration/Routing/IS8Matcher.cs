namespace Enterprise.Freight.Integration
{
	public interface IS8Matcher
	{
		IS8MatchResult Match(ScheduleInfo scheduleToMatch);

		IServiceRequestManager ServiceRequestManager { get; set; }
	}
}
