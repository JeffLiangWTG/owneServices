using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.CmdLine
{
	public static class AppConfigurationProvider
	{
		const string JsonConfigFileName = "CargoWise.RefDbRepo.RefUNLOCORelatedPortReferenceData.CmdLine.config.json";

		public static RelatedPortAppConfiguration AppConfiguration
		{
			get
			{
				if (appConfiguration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					var configuration = configurationBuilder.AddJsonFile(JsonConfigFileName).Build();
					appConfiguration = configuration.Get<RelatedPortAppConfiguration>();
				}

				return appConfiguration;
			}
		}

		static RelatedPortAppConfiguration appConfiguration;
	}
}
