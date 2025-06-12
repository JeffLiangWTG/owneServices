using CargoWise.eServices.Monitoring.HealthCheck.API;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite
{
    public class GBCustomsCredentialWebSiteHealthCheckHttpTaskAsyncHandler : HealthCheckHttpTaskAsyncHandler
    {

        public GBCustomsCredentialWebSiteHealthCheckHttpTaskAsyncHandler()
           : base(new IHealthCheckItemProvider[] { new GBCustomsCredentialWebSiteHealthCheckItemProvider() })
        { }
    }
}