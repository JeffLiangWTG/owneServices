using Microsoft.VisualStudio.TestTools.UnitTesting;
using CargoWise.eHub.Products.JPCustoms.Gateway.HealthCheckService;

namespace CargoWise.eHub.Products.JPCustoms.Tests.MockClasses
{
    [TestClass]
    public class TestJPGatewayServiceHealthCheckHttpTaskAsyncHandler
    {
        [TestMethod]
        public void TestJPCustomsGatewayServiceHealthCheckHttpTaskAsyncHandler()
        {
            var healthCheckHttpTaskAsyncHandler = new JPGatewayServiceHealthCheckHttpTaskAsyncHandler();
            Assert.IsTrue(healthCheckHttpTaskAsyncHandler != null);
        }
    }
}
