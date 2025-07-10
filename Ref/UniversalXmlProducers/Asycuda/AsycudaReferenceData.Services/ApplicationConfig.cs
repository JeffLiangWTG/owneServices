using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Services;

public static class ApplicationConfig
{
	const string JsonConfigFileName = "CargoWise.RefDbRepo.AsycudaReferenceData.CmdLine.config.json";

	public static IConfiguration Config
	{
		get
		{
			if (config == null)
			{
				var configurationBuilder = new ConfigurationBuilder();
				config = configurationBuilder.AddJsonFile(JsonConfigFileName).Build();
			}
			return config;
		}
	}
	static IConfiguration config;

	public static string OutputPath => Config["OutputPath"];
}
