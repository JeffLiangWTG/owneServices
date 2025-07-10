using System.Configuration;

namespace CargoWise.RefDbRepo.PLReferenceData.Services
{
	public static class AppConfigHelper
	{
		public static string GetAppSettingsValue(string settingsKey, ISettingsIndexer configuration = null)
		{
			var result = (configuration ?? ApplicationConfig.Instance.Settings)[settingsKey] ?? string.Empty;

			return string.IsNullOrEmpty(result)
				? throw new ConfigurationErrorsException($"{settingsKey} is not defined in config")
				: result;
		}
	}
}
