using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.IHSReferenceData.Services
{
	public static class ApplicationConfig
	{
		public static string OutputPath => Configuration[nameof(OutputPath)];
		public static string VesselListFileName => Configuration[nameof(VesselListFileName)];
		public static string FlagCodesFileName => Configuration[nameof(FlagCodesFileName)];
		public static string VesselListFTPHost => Configuration[nameof(VesselListFTPHost)];
		public static string VesselListFTPUser => Configuration[nameof(VesselListFTPUser)];
		public static string VesselListFTPPassword => Configuration[nameof(VesselListFTPPassword)];

		static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configuration = configurationBuilder.AddJsonFile(JsonFileName).Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		const string JsonFileName = "CargoWise.RefDbRepo.IHSReferenceData.CmdLine.config.json";
	}
}
