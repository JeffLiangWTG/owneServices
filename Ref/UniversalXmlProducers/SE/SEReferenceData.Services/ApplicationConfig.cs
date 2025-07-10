#if NET6_0_OR_GREATER
using Microsoft.Extensions.Configuration;
#endif
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.RefDbRepo.SEReferenceData.Services
{
	public static class ApplicationConfig
	{
		public static IDictionary<string, string> Config => _config = _config ?? ReadConfig();
		private static IDictionary<string, string> _config;
		public static IDictionary<string, string> ReadConfig()
		{
			const string JSON_FILE = "CargoWise.RefDbRepo.SEReferenceData.CmdLine.config.json";
			return new ConfigurationBuilder().AddJsonFile(JSON_FILE).Build().GetChildren().ToDictionary(x => x.Key, x => x.Value);
		}

		public static string OutputDirectory => Config["OutputPath"];
		public static string CompleteMonthlyRepositoryUrl => Config["CompleteMonthlyRepositoryUrl"];
		public static string IncrementalDailyRepositoryUrl => Config["IncrementalDailyRepositoryUrl"];
		public static string FilePrefix_MeasureType => Config["FilePrefix_MeasureType"];
		public static string FilePrefix_GeographicalArea => Config["FilePrefix_GeographicalArea"];
	}
}
