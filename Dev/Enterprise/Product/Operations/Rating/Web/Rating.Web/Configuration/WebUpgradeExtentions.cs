using System.Web.Http;

namespace Enterprise.Rating.Web.Configuration
{
	static class WebUpgradeExtentions
	{
		public static void ConfigureWebApplicationUpgrade(this HttpConfiguration config)
		{
			const int IntervalsInMillisecondToRetryWebUpgradeManagerCreation = 5000;
			new WebApplicationUpgradeConfigurator(IntervalsInMillisecondToRetryWebUpgradeManagerCreation).ConfigureWebUpgradeManager(config);
		}
	}
}
