using System;

namespace CargoWise.Billing.CollectorService.Plugin
{
	public interface IClock : IDisposable
	{
		event EventHandler Notify;
		void SetNotificationDueTime(TimeSpan dueTime);
		DateTime UtcNow { get; }
	}
}
