using CargoWise.eHub.Share.eHubServices.eHubSender.ServiceProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Share.eHubServices.Tests.eHubSender
{
	[TestClass]
	public class ServiceProviderFactoryTests : BaseTest
	{
		[TestMethod]
		public void GetServiceProvider_APOMELTSH_ELM()
		{
			var serviceProvider = ServiceProviderFactory.Create(Logger, "Sender1", "APOMELTSH_ELM", "");
			Assert.IsInstanceOfType(serviceProvider, typeof(eLMSServiceProvider));
		}

		[TestMethod]
		public void GetServiceProvider_USDIS()
		{
			var serviceProvider = ServiceProviderFactory.Create(Logger, "Sender1", "USDIS", "");
			Assert.IsInstanceOfType(serviceProvider, typeof(USDISServiceProvider));
		}

		[TestMethod]
		public void GetServiceProvider_OceanInsights()
		{
			var serviceProvider = ServiceProviderFactory.Create(Logger, "Sender1", "OceanInsights_CSS", "");
			Assert.IsInstanceOfType(serviceProvider, typeof(OceanInsightsServiceProvider));
		}
	}
}
