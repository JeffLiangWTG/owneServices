using CargoWise.RefDbRepo.ComplianceListReferenceData.Business;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.ComplianceListReferenceData.CmdLine
{
	public static class AppConfigurationProvider
	{
		public static AppConfiguration AppConfiguration
		{
			get
			{
				if (appConfiguration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					var configuration = configurationBuilder.AddJsonFile("CargoWise.RefDbRepo.ComplianceListReferenceData.CmdLine.config.json")
						.Build();
					appConfiguration = configuration.Get<AppConfiguration>();
				}
				return appConfiguration;
			}
		}

		static AppConfiguration appConfiguration;
	}
}
