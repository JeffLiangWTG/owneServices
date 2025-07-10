using System;
using System.Diagnostics;

namespace Enterprise.Telematics.ServiceTasks
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used in WI00228055")]
	class TimerProgressLogger : IProgressLogger
	{
		public TimerProgressLogger()
		{
			stopwatch = Stopwatch.StartNew();
			LogTimeSpan = TimeSpan.FromMinutes(5);
		}

		public TimeSpan LogTimeSpan { get; internal set; }

		public void Initialize()
		{
			stopwatch.Restart();
		}

		public bool ShouldLog()
		{
			if (stopwatch.Elapsed < LogTimeSpan)
			{
				return false;
			}

			stopwatch.Restart();
			return true;
		}

		readonly Stopwatch stopwatch;
	}
}
