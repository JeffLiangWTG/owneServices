using System.Configuration;

namespace Enterprise.Services.Scim.Api.Config
{
	public class AppSettings : IAppSettings
	{
		/// <summary>
		/// ServerName
		/// </summary>
		public string ServerName
		{
			get
			{
				if (serverName == null)
				{
					var settingValue = ConfigurationManager.AppSettings[ApplicationSettings.ServerName];
					serverName = settingValue ?? string.Empty;
				}

				return serverName;
			}
		}
		string serverName;

		/// <summary>
		/// DatabaseName
		/// </summary>
		public string DatabaseName
		{
			get
			{
				if (databaseName == null)
				{
					var settingValue = ConfigurationManager.AppSettings[ApplicationSettings.DatabaseName];
					databaseName = settingValue ?? string.Empty;
				}

				return databaseName;
			}
		}
		string databaseName;

		public string Issuer
		{
			get
			{
				if (issuer == null)
				{
					var settingValue = ConfigurationManager.AppSettings[ApplicationSettings.Issuer];
					issuer = settingValue ?? string.Empty;
				}

				return issuer;
			}
		}
		string issuer;

		public string AudienceId
		{
			get
			{
				if (audienceId == null)
				{
					var settingValue = ConfigurationManager.AppSettings[ApplicationSettings.AudienceId];
					audienceId = settingValue ?? string.Empty;
				}

				return audienceId;
			}
		}
		string audienceId;

		public string MaxResults
		{
			get
			{
				if (maxResults == null)
				{
					var settingValue = ConfigurationManager.AppSettings[ApplicationSettings.MaxResults];
					maxResults = settingValue ?? string.Empty;
				}

				return maxResults;
			}
		}
		string maxResults;

		public string SchemaPath
		{
			get
			{
				if (schemaPath == null)
				{
					var settingValue = ConfigurationManager.AppSettings[ApplicationSettings.SchemaPath];
					schemaPath = settingValue ?? string.Empty;
				}

				return schemaPath;
			}
		}
		string schemaPath;

		public string KnownEndpointPath
		{
			get
			{
				if (knownEndpointPath == null)
				{
					var settingValue = ConfigurationManager.AppSettings[ApplicationSettings.KnownEndpointPath];
					knownEndpointPath = settingValue ?? string.Empty;
				}

				return knownEndpointPath;
			}
		}
		string knownEndpointPath;

		public bool ShowPii
		{
			get
			{
				var settingValue = ConfigurationManager.AppSettings[ApplicationSettings.ShowPii];

				bool.TryParse(settingValue, out showPii);

				return showPii;
			}
		}
		bool showPii;

		public int ThrottleTimeInSeconds
		{
			get
			{
				if (throttleTimeInSeconds != null)
				{
					return throttleTimeInSeconds.Value;
				}
				var settingValue = ConfigurationManager.AppSettings[ApplicationSettings.ThrottleTimeInSeconds];

				throttleTimeInSeconds = int.TryParse(settingValue, out var tmp) ? tmp : null;

				return throttleTimeInSeconds ?? 1;
			}
		}
		int? throttleTimeInSeconds;

		public int ThrottleMaxRequestCount
		{
			get
			{
				if (maxRequestCount != null)
				{
					return maxRequestCount.Value;
				}
				var settingValue = ConfigurationManager.AppSettings[ApplicationSettings.ThrottleMaxRequestCount];

				maxRequestCount = int.TryParse(settingValue, out var tmp) ? tmp : null;

				return maxRequestCount ?? 30;
			}
		}
		int? maxRequestCount;

		public string ThrottleSkippedIps
		{
			get
			{
				if (throttleSkippedIps == null)
				{
					var settingValue = ConfigurationManager.AppSettings[ApplicationSettings.ThrottleSkippedIps];
					throttleSkippedIps = settingValue ?? "::1,127.0.0.1";
				}

				return throttleSkippedIps;
			}
		}
		string throttleSkippedIps;

		public string SafelistSkippedIps
		{
			get
			{
				if (safelistSkippedIps == null)
				{
					var settingValue = ConfigurationManager.AppSettings[ApplicationSettings.SafelistSkippedIps];
					safelistSkippedIps = settingValue ?? "::1,127.0.0.1";
				}

				return safelistSkippedIps;
			}
		}
		string safelistSkippedIps;
	}
}
