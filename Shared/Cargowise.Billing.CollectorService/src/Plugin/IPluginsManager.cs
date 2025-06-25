using System;

namespace CargoWise.Billing.CollectorService.Plugin
{
	public interface IPluginsManager : IDisposable
	{
		event EventHandler PluginsLoadFailed;
		event EventHandler PluginsLoadSettingsFileFailed;
		event EventHandler PluginsLoadStateFileFailed;
		void Start();
	}
}
