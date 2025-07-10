using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.GVMSReferenceData.Config
{
	public sealed class GvmsUnlocoLookupConfig
	{
		public static GvmsUnlocoLookupConfig Instance => instance.Value;
		static readonly Lazy<GvmsUnlocoLookupConfig> instance = new Lazy<GvmsUnlocoLookupConfig>(() => new GvmsUnlocoLookupConfig());

		GvmsUnlocoLookupConfig()
		{
			ConfigEnvironment();
		}

		public Dictionary<string, List<string>> GvmsCodeUnlocoLookup { get; set; }

		void ConfigEnvironment()
		{
			GvmsCodeUnlocoLookup = new Dictionary<string, List<string>>();

			ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
			var baseDir = AppDomain.CurrentDomain.BaseDirectory;
			configFileMap.ExeConfigFilename = baseDir + @"\GVMSReferenceData\Config\GvmsUnlocoLookup.xml";
			Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

			AppSettingsSection section = (AppSettingsSection)config.GetSection("appSettings");
			foreach (var key in section.Settings.AllKeys)
			{
				GvmsCodeUnlocoLookup.Add(key, section.Settings[key].Value.Trim().Split(',').ToList());
			}
		}
	}
}
