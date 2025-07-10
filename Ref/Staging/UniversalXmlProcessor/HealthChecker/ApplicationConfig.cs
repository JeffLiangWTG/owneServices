using System.Globalization;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.HealthChecker
{
	static class ApplicationConfig
	{
		public const string SOURCEDATA = "SOURCEDATA";
		const string JsonFile = "CargoWise.RefDbRepo.Staging.HealthChecker.config.json";
		static IConfiguration Config => new ConfigurationBuilder().AddJsonFile(JsonFile).Build();

		public static int CheckPeriodInHours => int.Parse(Config["SourceDataCheckPeriodInHours"], CultureInfo.InvariantCulture);
		public static double SourceDataFailedRatio => double.Parse(Config["SourceDataFailedRatio"], CultureInfo.InvariantCulture);
		public static string StagingConnectionString => DbConnectionStringManager.StagingConnectionString;
	}
}
