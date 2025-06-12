using CargoWise.eServices.Monitoring.HealthCheck.API;
using CargoWise.eServices.Monitoring.HealthCheck.API.Test;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite.Tests
{
    class CredentialWebSiteHealthCheckItemProviderTest : HealthCheckItemProviderBaseClass<GBCustomsCredentialWebSiteHealthCheckItemProvider>
    {
        [Test]
        public async Task TestCredentialWebServiceHealthCheckItemProvider()
        {
            var mockProvider = new Mock<GBCustomsCredentialWebSiteHealthCheckItemProvider>();
            mockProvider.CallBase = true;
            var provider = mockProvider.Object;

            Assert.That(provider.Name, Is.EqualTo("GBCustomsCredentialWebSite"));

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
            var mockProvider = new Mock<GBCustomsCredentialWebSiteHealthCheckItemProvider>();
            mockProvider.CallBase = true;
            var provider = mockProvider.Object;

            mockProvider.Setup<string>(_ => _.GetAppSettings(It.IsAny<string>())).Returns((string)null);
            var checkItem = provider.CheckHealthAsync().Result;
            descriptions.Add(checkItem.Description);
            mockProvider.Setup<string>(_ => _.GetAppSettings(It.IsAny<string>())).Returns("10");
            checkItem = provider.CheckHealthAsync().Result;
            descriptions.Add(checkItem.Description);

            return descriptions;
        }

    }
}
