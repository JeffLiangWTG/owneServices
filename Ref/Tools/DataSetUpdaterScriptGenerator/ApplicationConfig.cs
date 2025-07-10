using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator
{
	class ApplicationConfig
	{
		static IConfiguration Config =>
			new ConfigurationBuilder().AddJsonFile("CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator.config.json").Build();

		public static string ConnectionString = Config.GetConnectionString("connString");
	}
}
