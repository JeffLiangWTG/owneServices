using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Enterprise.Freight.Forwarding.Logging;
using static System.FormattableString;

namespace Enterprise.Freight.Forwarding.PerformanceMonitor
{
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer")]
	public class PerformanceTracker
	{
		#region Instance

		internal PerformanceTracker()
		{
			trackedEvents = new ConcurrentDictionary<string, DateTimeOffset>();
		}

		[field: SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static PerformanceTracker Instance { get; } = new PerformanceTracker();

		#endregion

		public void StartTrack(string operation, string objectTypeName, string pk, int numberOfRecords)
		{
			var timeNow = DateTimeOffset.UtcNow;
			var key = OperationKey(operation, pk);

			PerformanceLogger.Info("start", operation, objectTypeName, pk, numberOfRecords, TimeSpan.Zero);
			trackedEvents[key] = timeNow;
		}

		public void EndTrack(string operation, string objectTypeName, string pk, int numberOfRecords, string message = "completed")
		{
			var timeNow = DateTimeOffset.UtcNow;
			var key = OperationKey(operation, pk);

			if (trackedEvents.TryRemove(key, out var startTime))
			{
				var elapsed = timeNow.Subtract(startTime);
				PerformanceLogger.Info(message, operation, objectTypeName, pk, numberOfRecords, elapsed);
			}
			else
			{
				PerformanceLogger.Info(message, operation, objectTypeName, pk, numberOfRecords, TimeSpan.Zero);
			}
		}

		static string OperationKey(string operation, string pk) => Invariant($"{operation}-{pk}");

		readonly ConcurrentDictionary<string, DateTimeOffset> trackedEvents;
	}
}
