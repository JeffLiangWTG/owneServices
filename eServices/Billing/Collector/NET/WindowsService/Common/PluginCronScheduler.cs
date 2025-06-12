using System;
using CargoWise.Billing.CollectorService.Plugin;
using Cronos;

namespace CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common
{
	public class PluginCronScheduler : IPluginScheduler
	{
		public TimeSpan GetNextRunDueTime(IClock clock, PluginControllerSettings settings, PluginControllerState state)
		{
			if (!settings.Active)
			{
				return InfiniteTimeSpan;
			}

			TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById(settings.CronSettings.TimezoneId);
			CronExpression expression = CronExpression.Parse(settings.CronSettings.Expression);
			var nextOccurrenceDelayInMinutes = int.TryParse(settings.CronSettings?.NextOccurrenceDelayInMinutes, out var result) ? result : 0;
			var nexRunFromUtcNow = expression.GetNextOccurrence(clock.UtcNow, timezone);
			var nexRunFromLastSuccessfulRun = expression.GetNextOccurrence(DateTime.SpecifyKind(state.LastSuccessfulRun, DateTimeKind.Utc), timezone);

			if (nexRunFromUtcNow > nexRunFromLastSuccessfulRun)
			{
				return TimeSpan.Zero;
			}

			return nexRunFromUtcNow!.Value + TimeSpan.FromMinutes(nextOccurrenceDelayInMinutes) - clock.UtcNow;
		}

		public TimeSpan GetNextIntervalDueTime(IClock clock, PluginControllerSettings settings, PluginControllerState state)
		{
			TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById(settings.CronSettings.TimezoneId);
			CronExpression expression = CronExpression.Parse(settings.CronSettings.Expression);
			var nextOccurrenceDelayInMinutes = int.TryParse(settings.CronSettings?.NextOccurrenceDelayInMinutes, out var result) ? result : 0;
			var nexRunFromUtcNow = expression.GetNextOccurrence(clock.UtcNow.AddMinutes(-nextOccurrenceDelayInMinutes), timezone);

			return nexRunFromUtcNow!.Value + TimeSpan.FromMinutes(nextOccurrenceDelayInMinutes) - clock.UtcNow;
		}

		public DateTime GetStartUtc(PluginControllerSettings settings, PluginControllerState state)
		{
			TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById(settings.CronSettings.TimezoneId);
			CronExpression expression = CronExpression.Parse(settings.CronSettings.Expression);

			if (settings.CronSettings.CollectFromPreviousOccurrence)
			{
				var from = state.LastSuccessfulRun.AddMonths(-6);
				return expression.GetOccurrences(DateTime.SpecifyKind(from, DateTimeKind.Utc), DateTime.SpecifyKind(state.LastSuccessfulRun, DateTimeKind.Utc), timezone, true, true).Last();
			}

			return state.LastSuccessfulRun;
		}

		public DateTime GetEndUtc(PluginControllerSettings settings, PluginControllerState state)
		{
			TimeZoneInfo timezone = TimeZoneInfo.FindSystemTimeZoneById(settings.CronSettings.TimezoneId);
			CronExpression expression = CronExpression.Parse(settings.CronSettings.Expression);
			return expression.GetNextOccurrence(DateTime.SpecifyKind(state.LastSuccessfulRun, DateTimeKind.Utc), timezone)!.Value;
		}

		public static readonly TimeSpan InfiniteTimeSpan = new(0, 0, 0, 0, -1);
	}
}
