using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.DataSetServiceGenerator
{
	static class ApplicationConfig
	{
		static IConfiguration Config =>
			new ConfigurationBuilder().AddJsonFile("CargoWise.RefDbRepo.DataSetServiceGenerator.config.json").Build();

		public static string DataSetServicePath => Config[nameof(DataSetServicePath)];
		public static string SafeConnectionString => DbConnectionStringManager.SafeConnectionString;
	}
}
