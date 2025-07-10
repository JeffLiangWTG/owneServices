using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Configuration
{
	public static class ConfigurationProvider
	{
		public static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configuration = configurationBuilder.AddJsonFile("CargoWise.RefDbRepo.ZAReferenceData.CmdLine.config.json").Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		public static string InputFolder => Configuration["InputFolder"];
		public static string InputFile => Configuration["InputFile"];
		public static string OutputFolder => Configuration["OutputFolder"];
		public static bool IsRefDbServiceSecure => bool.Parse(Configuration["IsRefDbServiceSecure"]);
		public static string CarrierBaseUrl => Configuration["CarrierBaseUrl"];
		public static string RefDbServiceURI => Configuration["RefDbServiceURI"];
		public static string CountryMatchingURI => Configuration["CountryMatchingURI"];
	}
}
