namespace CargoWise.Billing.CollectorService.Plugin.Tests
{
	public class TestClock : IClock
	{
		public event EventHandler Notify = delegate { };

		public DateTime UtcNow
		{
			get { return utcNow; }
			set
			{
				utcNow = value;
				if (notificationScheduled)
				{
					if (utcNow == notificationTime)
					{
						RaiseNotify();
					}
					else if (utcNow > notificationTime)
					{
						throw new ArgumentOutOfRangeException("value", value, "UtcNow should not be greater than next notification time scheduled. Otherwise Notify event won't be raised.");
					}
				}
			}
		}

		public void SetNotificationDueTime(TimeSpan dueTime)
		{
			if (dueTime == TimeSpan.Zero)
			{
				RaiseNotify();
			}
			else
			{
				notificationScheduled = true;
				notificationTime = UtcNow + dueTime;
			}
		}

		public void Dispose()
		{
		}

		void RaiseNotify()
		{
			notificationScheduled = false;
			Notify(this, EventArgs.Empty);
		}

		bool notificationScheduled;
		DateTime notificationTime;
		DateTime utcNow;
	}
}
