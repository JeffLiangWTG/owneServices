using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.MetaDataGenerator;

static class ApplicationConfig
{
	static readonly IConfigurationRoot Config = new ConfigurationBuilder().AddJsonFile("CargoWise.RefDbRepo.MetaDataGenerator.config.json").Build();
	internal static string ConnectionString => Config.GetConnectionString("Default")!;
}
