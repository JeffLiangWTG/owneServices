using System;

namespace CargoWise.Billing.CollectorService.Plugin
{
	public interface IPluginScheduler
	{
		TimeSpan GetNextRunDueTime(IClock clock, PluginControllerSettings settings, PluginControllerState state);
		TimeSpan GetNextIntervalDueTime(IClock clock, PluginControllerSettings settings, PluginControllerState state);
		DateTime GetStartUtc(PluginControllerSettings settings, PluginControllerState state);
		DateTime GetEndUtc(PluginControllerSettings settings, PluginControllerState state);
	}
}
