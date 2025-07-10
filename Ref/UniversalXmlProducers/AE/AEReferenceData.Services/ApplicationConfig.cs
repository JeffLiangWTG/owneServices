using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.AEReferenceData.Services;

public static class ApplicationConfig
{
	const string JsonConfigFileName = "CargoWise.RefDbRepo.AEReferenceData.CmdLine.config.json";

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

	public static string InputFolder => Config["InputFolder"];

	public static string OutputFolder => Config["OutputFolder"];

	public static string DubaiRefDataInputFile => Config["DubaiRefDataInputFile"];
}
