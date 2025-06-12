using CargoWise.eHub.Portal.HealthCheck;
using CargoWise.eServices.Monitoring.HealthCheck.API;
using CargoWise.eServices.Monitoring.HealthCheck.API.Test;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoWise.eHub.Portal.Tests.HealthCheck
{
    class PortalHealthCheckItemProviderTest : HealthCheckItemProviderBaseClass<PortalHealthCheckItemProvider>
    {
        [Test]
        public async Task TestPortalHealthCheckItemProvider()
        {
            var mockProvider = new Mock<PortalHealthCheckItemProvider> { CallBase = true };
            var provider = mockProvider.Object;

            Assert.AreEqual(provider.Name, "eHubPortalService");

   
            mockProvider.Setup<string>(_ => _.GetAppSettings(It.IsAny<string>())).Returns((string)null);
            var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);

            Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
            Assert.That(checkItem.Description, Is.EqualTo("Authorisation Token Expiration Minute not found in AppSettings in Web.config."));

            mockProvider.Setup<string>(_ => _.GetAppSettings(It.IsAny<string>())).Returns("10");
            checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);

            Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.OK));
            Assert.That(checkItem.Description, Is.EqualTo("Service is alive."));
        }

        protected override List<string> GetAllKindsOfDescriptions()
        {
            var descriptions = new List<string>();

            var mockProvider = new Mock<PortalHealthCheckItemProvider> { CallBase = true };
            var provider = mockProvider.Object;

            var checkItem = provider.CheckHealthAsync().Result;
            descriptions.Add(checkItem.Description);
            checkItem = provider.CheckHealthAsync().Result;
            descriptions.Add(checkItem.Description);

            return descriptions;
        }
    }
}
