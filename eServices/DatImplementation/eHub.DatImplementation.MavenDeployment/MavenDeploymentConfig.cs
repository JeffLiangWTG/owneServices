using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace eHub.DatImplementation.MavenDeployment
{
	public class MavenDeploymentConfig
	{
		private readonly Dictionary<string, string> settingsFromDeploymentConfiguration = new Dictionary<string, string>();

		private void AddKeyValue(string key, string value)
		{
			settingsFromDeploymentConfiguration[key] = value;
		}

		public string SettingsByPrefix(string prefix)
		{
			return settingsFromDeploymentConfiguration.FirstOrDefault(dictionaryEntry =>
				dictionaryEntry.Key.StartsWith(prefix, System.StringComparison.InvariantCultureIgnoreCase)).
				Value;
		}

		public static MavenDeploymentConfig Parse(string deploymentConfiguration)
		{
			var instance = new MavenDeploymentConfig();
			var configStrings = deploymentConfiguration.Split(';');
			foreach (var configString in configStrings)
			{
				var indexEqual = configString.IndexOf('=');
				if (indexEqual > 0)
				{
					instance.AddKeyValue(configString.Substring(0, indexEqual).Trim(), configString.Substring(indexEqual + 1).Trim());
				}
				else
				{
					instance.AddKeyValue(configString, string.Empty);
				}
			}
			return instance;
		}
	}
}
