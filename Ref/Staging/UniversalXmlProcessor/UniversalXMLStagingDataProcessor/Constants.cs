using System.Globalization;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public static class Constants
	{
		public static string SafeUpdateServiceUri => Config["safeUpdateServiceUri"];
		public static string StagingServiceUri => Config["stagingServiceUri"];
		public static int TimeOutHours => int.Parse(Config["TimeOutHours"], CultureInfo.InvariantCulture);
		public static int TimeToProcessMinutes => int.Parse(Config["TimeToProcessMinutes"], CultureInfo.InvariantCulture);
		public static double HeartbeatIntervalInMinutes => double.Parse(Config["HeartbeatIntervalInMinutes"], CultureInfo.InvariantCulture);
		public static string CloneBlackList => Config[nameof(CloneBlackList)];
		public static string DescriptionAsMainContentList => Config[nameof(DescriptionAsMainContentList)];
		public static string ConnectionStrings => DbConnectionStringManager.StagingConnectionString;
		public static string TenantId => Config[nameof(TenantId)];
		public static string ClientId => Config[nameof(ClientId)];
		public static string ServiceId => Config[nameof(ServiceId)];
		public static string PrivateKeyFileName => Config[nameof(PrivateKeyFileName)];
		public static string CertificateFileName => Config[nameof(CertificateFileName)];
		public static int RefreshAdvanceInMinutes =>  int.Parse(Config[nameof(RefreshAdvanceInMinutes)], CultureInfo.InvariantCulture);
		public static int SQLExecutionTimeoutInSecondsInvolvesDPR => int.Parse(Config[nameof(SQLExecutionTimeoutInSecondsInvolvesDPR)], CultureInfo.InvariantCulture);
		public static int SQLExecutionTimeoutInSeconds => int.Parse(Config[nameof(SQLExecutionTimeoutInSeconds)], CultureInfo.InvariantCulture);
		public static bool EnableCleanDPRPerBatch => bool.Parse(Config["EnableCleanDPRPerBatch"] ?? "False");
		public static int ExpiredBatchSize => int.Parse(Config[nameof(ExpiredBatchSize)], CultureInfo.InvariantCulture);

		public const string StartDatePropertySuffix = "StartDate";
		public const string EndDatePropertySuffix = "EndDate";
		public const string UserOverrideSuffix = "UserOverride";
		public const string IAmUniqueSuffix = "IAMUnique";
		public const string PublishedDate = "PublishedDate";

		static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					config = new ConfigurationBuilder().AddJsonFile(JsonConfigFile).Build();
				}
				return config;
			}
		}

		public const string JsonConfigFile = "CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor.config.json";
		static IConfiguration config;

#if DEBUG
		public static void SetConfigFileForTest(string path)
		{
			config = new ConfigurationBuilder().AddJsonFile(path).Build();
		}
#endif
	}

	public enum DataProcessingStatus
	{
		QUE,
		PRS,
		ERR,
		IGR
	}

	public enum IdenticalLevel
	{
		HasChange = 0,
		MainContentIdentical = 1,
		Identical = 2
	}
}
