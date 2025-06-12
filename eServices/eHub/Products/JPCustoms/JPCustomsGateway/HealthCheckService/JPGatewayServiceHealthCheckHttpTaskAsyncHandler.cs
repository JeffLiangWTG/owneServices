using CargoWise.eHub.Products.JPCustoms.HealthCheckService;
using CargoWise.eServices.Monitoring.HealthCheck.API;


namespace CargoWise.eHub.Products.JPCustoms.Gateway.HealthCheckService
{
    public class JPGatewayServiceHealthCheckHttpTaskAsyncHandler : HealthCheckHttpTaskAsyncHandler
    {
        public JPGatewayServiceHealthCheckHttpTaskAsyncHandler() 
            : base(new IHealthCheckItemProvider[] {new JPGatewayServiceHealthCheckItemProvider()})
        {
        }
    }
}

