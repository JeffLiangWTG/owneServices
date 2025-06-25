using System;
using System.Threading;

namespace CargoWise.Billing.CollectorService.Plugin
{
	class Clock : IClock
	{
		public event EventHandler Notify = delegate { };

		public Clock(TimeSpan delay)
		{
			this.delay = delay;
			timer = new Timer(TimerCallback);
		}

		public DateTime UtcNow
		{
			get { return DateTime.UtcNow.Add(-delay); }
		}

		public void SetNotificationDueTime(TimeSpan dueTime)
		{
			timer.Change(dueTime, Timeout.InfiniteTimeSpan);
		}

		public void Dispose()
		{
			timer.Dispose();
		}

		void TimerCallback(object state)
		{
			Notify(this, EventArgs.Empty);
		}

		readonly Timer timer;
		readonly TimeSpan delay;
	}
}
