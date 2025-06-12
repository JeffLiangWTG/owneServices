using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.eHub.Products.NZCustoms.Gateway.HealthCheck;
using CargoWise.eServices.Monitoring.HealthCheck.API;
using CargoWise.eServices.Monitoring.HealthCheck.API.Test;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.Products.NZCustoms.Tests.HealthCheck
{
    class NZCustomsHealthCheckItemProviderTest : HealthCheckItemProviderBaseClass<NZCustomsGatewayHealthCheckItemProvider>
    {
        [Test]
        public async Task TestGatewayServiceHealthCheckItemProvider()
        {
            var mockProvider = new Mock<NZCustomsGatewayHealthCheckItemProvider> { CallBase = true };
            mockProvider.Setup<bool>(_ => _.MessageGateway.Ping()).Returns(true);
            var provider = mockProvider.Object;

            Assert.AreEqual(provider.Name, "NZCustomsGatewayService");

            var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
            Assert.AreEqual(checkItem.Status, HealthCheckStatus.OK);
            Assert.AreEqual(checkItem.Description, "Service is alive.");
        }

        [Test]
        public async Task TestGatewayServiceHealthCheckItemProviderThrowException()
        {
            var mockProvider = new Mock<NZCustomsGatewayHealthCheckItemProvider> { CallBase = true };
            mockProvider.Setup<bool>(_ => _.MessageGateway.Ping()).Returns(false);
            var provider = mockProvider.Object;

            Assert.AreEqual(provider.Name, "NZCustomsGatewayService");

            var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
            var exceptionMessage = "NZCustomsGatewayService is not active. ";
            Assert.AreEqual(checkItem.Status, HealthCheckStatus.Error);
            Assert.AreEqual(checkItem.Description, exceptionMessage);
        }

        protected override List<string> GetAllKindsOfDescriptions()
        {
            var descriptions = new List<string>();

            var mockProvider = new Mock<NZCustomsGatewayHealthCheckItemProvider> { CallBase = true };
            var provider = mockProvider.Object;

            mockProvider.Setup<bool>(_ => _.MessageGateway.Ping()).Returns(true);
            var checkItem = provider.CheckHealthAsync().Result;
            descriptions.Add(checkItem.Description);

            mockProvider.Setup<bool>(_ => _.MessageGateway.Ping()).Returns(false);
            checkItem = provider.CheckHealthAsync().Result;
            descriptions.Add(checkItem.Description);

            return descriptions;
        }
    }
}
