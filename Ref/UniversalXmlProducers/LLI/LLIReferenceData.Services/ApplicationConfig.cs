using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.LLIReferenceData.Services;

public static class ApplicationConfig
{
	public static string VesselApiMainURL => Configuration[nameof(VesselApiMainURL)];

	public static string TokenUriPath => Configuration[nameof(TokenUriPath)];

	public static string VesselListUriPath => Configuration[nameof(VesselListUriPath)];

	public static string VesselBasicCharacteristicsUriPath => Configuration[nameof(VesselBasicCharacteristicsUriPath)];

	public static string VesselAdvancedCharacteristicsUriPath => Configuration[nameof(VesselAdvancedCharacteristicsUriPath)];

	public static string VesselApiUserName => Configuration[nameof(VesselApiUserName)];

	public static string VesselApiPassword => Configuration[nameof(VesselApiPassword)];

	public static string OutputPath => Configuration[nameof(OutputPath)];

	public static int MaxParallelHttpRequests => int.Parse(Configuration[nameof(MaxParallelHttpRequests)], CultureInfo.InvariantCulture);

	static IConfiguration configuration;

	static IConfiguration Configuration
	{
		get
		{
			if (configuration == null)
			{
				InitConfiguration();
			}

			return configuration;
		}
	}

	static void InitConfiguration()
	{
		var configurationBuilder = new ConfigurationBuilder();
		configurationBuilder.AddJsonFile(JsonConfigFile);
		configuration = configurationBuilder.Build();
	}

	static string JsonConfigFile = "CargoWise.RefDbRepo.LLIReferenceData.CmdLine.config.json";

	public static void SetJsonConfigFile(string fileName)
	{
		JsonConfigFile = fileName;
		InitConfiguration();
	}
}
