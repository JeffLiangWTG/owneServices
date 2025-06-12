using CargoWise.eServices.Monitoring.HealthCheck.API;

namespace CargoWise.eHub.Products.GBCustoms.CDS.InboundMessageWebService
{
    public class InboundMessageWebServiceHealthCheckHttpTaskAsyncHandler : HealthCheckHttpTaskAsyncHandler
    {
        public InboundMessageWebServiceHealthCheckHttpTaskAsyncHandler()
			: base(new IHealthCheckItemProvider[] { new InboundMessageWebServiceHealthCheckItemProvider() })
		{
        }
    }
}