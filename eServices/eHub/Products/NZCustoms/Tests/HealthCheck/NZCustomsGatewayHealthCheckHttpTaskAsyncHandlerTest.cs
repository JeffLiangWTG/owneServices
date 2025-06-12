using CargoWise.eHub.Products.NZCustoms.Gateway.HealthCheck;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace CargoWise.eHub.Products.NZCustoms.Tests.HealthCheck
{
    [TestClass]
    class NZCustomsGatewayHealthCheckHttpTaskAsyncHandlerTest
    {
        [TestMethod]
        public void TestNZCustomsGatewayHealthCheckHttpTaskAsyncHandler()
        {
            var healthCheckHttpTaskAsyncHandler = new NZCustomsGatewayHealthCheckHttpTaskAsyncHandler();
            Assert.IsTrue(healthCheckHttpTaskAsyncHandler != null);
        }
    }
}
