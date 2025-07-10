using System;

namespace Enterprise.Services.ServiceHost
{
	public interface IEditableMilestone
	{
		Guid PK { get; }
		DateTimeOffset? ScheduledDate { get; }
		DateTimeOffset? ActualDate { get; }
		string EventCode { get; }
		DateTimeOffset? NewScheduledDate { get; set; }
		DateTimeOffset? NewActualDate { get; set; }
	}
}
