using CargoWise.eServices.Monitoring.HealthCheck.API;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService
{
    public class CredentialWebServiceHealthCheckHttpTaskAsyncHandler : HealthCheckHttpTaskAsyncHandler
    {
        public CredentialWebServiceHealthCheckHttpTaskAsyncHandler()
            : base(new IHealthCheckItemProvider[] { new CredentialWebServiceHealthCheckItemProvider() })
        { }
    }
}