using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using CargoWise.eHub.Products.JPCustoms.Client;
using CargoWise.eHub.Products.JPCustoms.Gateway;
using CargoWise.eHub.Products.JPCustoms.HealthCheckService;
using CargoWise.eServices.Monitoring.HealthCheck.API;
using Common.Logging;
using Moq;

namespace CargoWise.eHub.Products.JPCustoms.Tests.MockClasses
{
    [TestClass]
    public class TestJPGatewayServiceHealthCheckItemProvider : TestBase
    {
        //This is integration test. Smtp server should be installed
        //[TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public async Task TestJPGatewayServiceHealthCheckItemProvider_Integration()
        {
            var provider = new JPGatewayServiceHealthCheckItemProvider();
            var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
            Assert.AreEqual(HealthCheckStatus.OK, checkItem.Status);
            Assert.AreEqual(checkItem.Description, "Service is alive.");
        }

        [TestMethod]
        public async Task TestJPCustomsGatewayServiceHealthCheckItemProvider()
        {
            var mockProvider = new Mock<JPGatewayServiceHealthCheckItemProvider> { CallBase = true };
            var provider = mockProvider.Object;
            Assert.AreEqual(provider.Name, "JPCustomsGatewayWebService");
            var sendClient = new Mock<IJPCustomsSendClient>();
            var checkItem = await provider.CheckHealthAsync(sendClient.Object).ConfigureAwait(false);
            Assert.AreEqual(HealthCheckStatus.OK, checkItem.Status);
            Assert.AreEqual(checkItem.Description, "Service is alive.");
        }

        [TestMethod]
        public async Task TestJPCustomsGatewayServiceHealthCheckItemProviderThrowException()
        {
            var mockProvider = new Mock<JPGatewayServiceHealthCheckItemProvider>() { CallBase = true };
            var errorClient = new JPCustomsSendClient(new SmtpMailClientConfiguration(new Mock<ILog>().Object));
            var provider = mockProvider.Object;
            Assert.AreEqual(provider.Name, "JPCustomsGatewayWebService");
            var exceptionMessage = "An exception 'System.ArgumentNullException' was thrown during health check.";
            var checkItem = await provider.CheckHealthAsync(errorClient).ConfigureAwait(false);
            Assert.AreEqual(checkItem.Status, HealthCheckStatus.Error);
            Assert.AreEqual(checkItem.Description, exceptionMessage);
        }
    }
}
