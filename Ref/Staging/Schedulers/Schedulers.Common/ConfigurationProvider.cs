using System.Collections.Specialized;
using System.Globalization;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	public static class ConfigurationProvider
	{
		const string JsonConfigFile = "CargoWise.RefDbRepo.Staging.NewSchedulers.config.json";
		static IConfiguration configuration;
		static NameValueCollection quartzProps;

		public static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configurationBuilder.AddJsonFile(JsonConfigFile);
					configuration = configurationBuilder.Build();
				}
				return configuration;
			}
		}

		public static NameValueCollection QuartzProps
		{
			get
			{
				if (quartzProps == null)
				{
					quartzProps = new NameValueCollection();
					var quartzSection = Configuration.GetSection("quartz");
					foreach (var config in quartzSection.GetChildren())
					{
						var key = config.Key;
						var value = config.Value;
						quartzProps.Add(key, value);
					}
				}
				return quartzProps;
			}
		}

		public static string QuartzPanelPort => Configuration[nameof(QuartzPanelPort)];
		public static string SafeUpdateServiceUri => Configuration[nameof(SafeUpdateServiceUri)];
		public static string StagingConnectionString
		{
			get => stagingConnectionString;
#if DEBUG
			set => stagingConnectionString = value;
#endif
		}

		static string stagingConnectionString = DbConnectionStringManager.StagingConnectionString;

		public static bool CheckAuthorization => bool.Parse(Configuration["CheckAuthorization"]);
		public static string SchedulerName => QuartzProps["quartz.scheduler.instanceName"];
		public static string QuartzDefaultConnectionString
		{
			get => Configuration["quartz:quartz.dataSource.default.connectionString"];
#if DEBUG
			set => Configuration["quartz:quartz.dataSource.default.connectionString"] = value;
#endif
		}

		public static string TenantId => Configuration[nameof(TenantId)];
		public static string ClientId => Configuration[nameof(ClientId)];
		public static string ServiceId => Configuration[nameof(ServiceId)];
		public static string PrivateKeyFileName => Configuration[nameof(PrivateKeyFileName)];
		public static string CertificateFileName => Configuration[nameof(CertificateFileName)];
		public static int RefreshAdvanceInMinutes =>  int.Parse(Configuration[nameof(RefreshAdvanceInMinutes)], CultureInfo.InvariantCulture);

		public static int MemoryMonitorIntervalInSeconds => int.Parse(Configuration[nameof(MemoryMonitorIntervalInSeconds)], CultureInfo.InvariantCulture);
	}
}
