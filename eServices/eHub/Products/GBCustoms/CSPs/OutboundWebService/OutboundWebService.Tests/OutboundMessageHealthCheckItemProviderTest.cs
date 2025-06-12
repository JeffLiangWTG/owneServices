using System.Collections.Generic;
using System.Threading.Tasks;

using CargoWise.eServices.Monitoring.HealthCheck.API;
using CargoWise.eServices.Monitoring.HealthCheck.API.Test;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

namespace CargoWise.eHub.Products.GBCustoms.CSP.OutboundWebService.Tests
{
	[TestClass]
	public class OutboundMessageHealthCheckItemProviderTest : HealthCheckItemProviderBaseClass<OutboundMessageWebServiceHealthCheckItemProvider>
    {
		[TestMethod]
		public async Task TestOutboundMessageHealthCheckItemProvider()
		{
			var mockProvider = new Mock<OutboundMessageWebServiceHealthCheckItemProvider> { CallBase = true };
			var provider = mockProvider.Object;

			Assert.AreEqual(provider.Name, "GBCustomsCSPOutboundNotificationWebService");

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.AreEqual(checkItem.Status, HealthCheckStatus.OK);
			Assert.AreEqual(checkItem.Description, "Service is alive.");
		}

		protected override List<string> GetAllKindsOfDescriptions()
        {
			return new List<string>();
		}
	}
}
