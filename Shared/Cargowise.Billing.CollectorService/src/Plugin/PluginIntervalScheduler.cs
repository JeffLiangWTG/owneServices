using System;

namespace CargoWise.Billing.CollectorService.Plugin
{
	public sealed class PluginIntervalScheduler : IPluginScheduler
	{
		public TimeSpan GetNextRunDueTime(IClock clock, PluginControllerSettings settings, PluginControllerState state)
		{
			if (!settings.Active)
			{
				return InfiniteTimeSpan;
			}

			return clock.UtcNow - state.LastSuccessfulRun > settings.Interval
				? TimeSpan.Zero
				: GetNextIntervalDueTime(clock, settings, state);
		}

		public TimeSpan GetNextIntervalDueTime(IClock clock, PluginControllerSettings settings, PluginControllerState state)
		{
			Math.DivRem((clock.UtcNow - state.LastSuccessfulRun).Ticks, settings.Interval.Ticks, out var ticksAfterLastInterval);
			return settings.Interval - TimeSpan.FromTicks(ticksAfterLastInterval) < TimeSpan.FromSeconds(1)
				? TimeSpan.FromSeconds(1)
				: TimeSpan.FromSeconds(settings.Interval.TotalSeconds - TimeSpan.FromTicks(ticksAfterLastInterval).TotalSeconds);
		}

		public DateTime GetStartUtc(PluginControllerSettings settings, PluginControllerState state) => state.LastSuccessfulRun;

		public DateTime GetEndUtc(PluginControllerSettings settings, PluginControllerState state) => state.LastSuccessfulRun + settings.Interval;

		public static readonly TimeSpan InfiniteTimeSpan = new(0, 0, 0, 0, -1);
	}
}
