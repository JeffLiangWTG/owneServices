using System;

namespace CargoWise.Blazor.SessionBroker
{
	public class ShutdownOptions
	{
		public TimeSpan IdleWatcherStartDueTime { get; set; } = TimeSpan.FromMinutes(1);
		public TimeSpan IdleWatcherCheckPeriod { get; set; } = TimeSpan.FromSeconds(1);
		public TimeSpan ShutdownThresholdSinceLastActiveAction { get; set; } = TimeSpan.FromMinutes(3);
	}
}
