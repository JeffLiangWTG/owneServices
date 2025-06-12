using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace eHub.DatImplementation.Deployment
{
	public class eHubDeploymentConfig
	{
		#region Variables

		const string KeyValuePattern = "(?<key>.*)=(?<value>.*)";
		readonly Dictionary<string, string> configurationSettings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public class Keys
		{
			public const string Server = "Server";
			public const string Database = "Database";
			public const string UserId = "User Id";
			public const string Password = "Password";
			public const string Project = "Project";
			public const string Profile = "Profile";
			public const string SourcePath = "SourcePath";
			public const string BinPath = "BinPath";
			public const string ConnectionString = "ConnectionString";
			public const string BAT = "BAT";
			public const string BATNuGet = "BATNuGet";
		}

		public class Prefix
		{
			public const string Property = "p:";
			public const string Target = "t:";
		}

		public class Target
		{
			public const string BatDeploy = "bat:deploy";
			public const string DatPackage = "dat:package";
			public const string DatDeploy = "dat:deploy";
			public const string DatBackup = "dat:backup";
			public const string DatInstall = "dat:install";
			public const string DatRollback = "dat:rollback";
			public const string DatRemove = "dat:remove";
		}

		#endregion

		#region Properties

		public Dictionary<string, string> Settings
		{
			get { return configurationSettings; }
		}

		#endregion

		#region Methods
		public void AddKeyValue(string key, string value)
		{
			configurationSettings[key] = value;
		}

		public Dictionary<string, string> SettingsByPrefix(string prefix)
		{
			return Settings.Where(x =>
				x.Key.StartsWith(prefix, System.StringComparison.InvariantCultureIgnoreCase))
					.Select(item => new { k = item.Key.Substring(prefix.Length), v = item.Value })
					.ToDictionary(x => x.k, x => x.v);
		}

		#endregion

		#region Constructos

		public static eHubDeploymentConfig Parse(string deploymentConfiguration)
		{
			var instance = new eHubDeploymentConfig();

			var configs = deploymentConfiguration.Split(';');
			foreach (var config in configs)
			{
				var match = Regex.Match(config, KeyValuePattern);
				if (match.Success)
				{
					instance.AddKeyValue(match.Groups["key"].Value.Trim(), match.Groups["value"].Value.Trim());
				}
				else
				{
					instance.AddKeyValue(config, string.Empty);
				}
			}

			return instance;
		}

		#endregion
	}
}
