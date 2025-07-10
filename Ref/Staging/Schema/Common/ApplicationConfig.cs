using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.Common
{
	public static class ApplicationConfig
	{
		static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					config = new ConfigurationBuilder().AddJsonFile("CargoWise.RefDbRepo.Staging.Common.config.json").Build();
				}
				return config;
			}
		}
		static IConfiguration config;

		public static string ErrorReportMaxCount => Config[nameof(ErrorReportMaxCount)];
	}
}
