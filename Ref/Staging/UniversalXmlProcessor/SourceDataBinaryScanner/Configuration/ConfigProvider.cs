using System;
using System.Globalization;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.SourceDataBinaryScanner
{
	public class ConfigProvider : IConfigProvider
	{
		public const string JsonConfigFile = "CargoWise.RefDbRepo.SourceDataBinaryScanner.config.json";
		public int ExecutionTimeOutInHours => Convert.ToInt32(Config[nameof(ExecutionTimeOutInHours)], CultureInfo.InvariantCulture);
		public string ConnectionString => DbConnectionStringManager.StagingConnectionString;

		static IConfiguration Config =>
			new ConfigurationBuilder().AddJsonFile(JsonConfigFile).Build();
	}
}
