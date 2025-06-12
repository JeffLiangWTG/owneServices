using CargoWise.eServices.Monitoring.HealthCheck.API;

namespace CargoWise.eHub.Products.GBCustoms.CSP.OutboundWebService
{
    public class OutboundMessageWebServiceHealthCheckHttpTaskAsyncHandler : HealthCheckHttpTaskAsyncHandler
    {
        public OutboundMessageWebServiceHealthCheckHttpTaskAsyncHandler()
			: base(new IHealthCheckItemProvider[] { new OutboundMessageWebServiceHealthCheckItemProvider() })
		{
        }
    }
}
