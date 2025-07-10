using System;
using System.Diagnostics;
using System.Threading;

namespace CargoWise.Blazor.SessionBroker
{
	public class RequestTracker
	{
		ulong activeActions;
		readonly Stopwatch stopwatch = Stopwatch.StartNew();

		public ulong ActiveActions { get => activeActions; set => activeActions = value; }
		public TimeSpan TimeElapsedSinceLastActionCompleted() => stopwatch.Elapsed;

		public void Increment()
		{
			Interlocked.Increment(ref activeActions);
		}

		public void Decrement()
		{
			Interlocked.Decrement(ref activeActions);
			stopwatch.Restart();
		}
	}
}
