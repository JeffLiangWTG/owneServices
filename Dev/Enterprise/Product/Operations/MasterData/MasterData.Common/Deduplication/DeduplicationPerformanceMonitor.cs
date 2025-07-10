using System;
using System.Diagnostics;

namespace Enterprise.MasterData.Common
{
	public sealed class DeduplicationPerformanceMonitor : IDisposable
	{
		readonly Stopwatch watch = new Stopwatch();
		readonly Action<TimeSpan> callback;

		public DeduplicationPerformanceMonitor()
		{
			watch.Start();
		}

		public DeduplicationPerformanceMonitor(Action<TimeSpan> callback)
			: this()
		{
			this.callback = callback;
		}

		public TimeSpan ElapsedDuration
		{
			get { return watch.Elapsed; }
		}

		public void Dispose()
		{
			watch.Stop();

			callback?.Invoke(ElapsedDuration);
		}
	}
}
