using System;
using System.Threading;

namespace CargoWise.eHub.Nudge
{
	public class RepeatableTask : IObserver
	{
		public event EventHandler TaskHandler = delegate { };

		public RepeatableTask(int runInterval, EventHandler taskHandler)
		{
			this.runInterval = TimeSpan.FromMilliseconds(runInterval);
			this.timer = new Timer(TimerCallback);
			this.TaskHandler = taskHandler;
		}

		public void Dispose()
		{
			timer.Dispose();
		}

		public virtual void Start()
		{
			timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
		}

		public virtual void Start(TimeSpan delayBeforeStart)
		{
			timer.Change(delayBeforeStart, Timeout.InfiniteTimeSpan);
		}

		void TimerCallback(object state)
		{
			DateTime startTime = DateTime.UtcNow;
			TaskHandler(this, EventArgs.Empty);
			DateTime endTime = DateTime.UtcNow;
			timer.Change(CalNextRunInterval(startTime, endTime), Timeout.InfiniteTimeSpan);
		}

		TimeSpan CalNextRunInterval(DateTime lastRunTime, DateTime nowTime)
		{
			TimeSpan elapsedTime = nowTime - lastRunTime;
			return elapsedTime >= runInterval ? TimeSpan.Zero : runInterval - elapsedTime;
		}

		public void SetRunInterval(int newInterval)
		{
			runInterval = TimeSpan.FromMilliseconds(newInterval);
		}

		public void SubscribeTo(IObservable topic)
		{
			topic.RegisterSubscriber(this);
		}

		public void OnNotify(IObservable topic)
		{
			SetRunInterval(GetRunIntervalFromCurrentSettingFunc((NudgeSettings)topic));
		}

		public Func<NudgeSettings, int> GetRunIntervalFromCurrentSettingFunc { get; set; }

		readonly Timer timer;
		TimeSpan runInterval;
	}
}