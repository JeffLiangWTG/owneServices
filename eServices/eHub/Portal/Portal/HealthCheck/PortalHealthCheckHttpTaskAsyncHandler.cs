using CargoWise.eServices.Monitoring.HealthCheck.API;

namespace CargoWise.eHub.Portal.HealthCheck
{
    public class PortalHealthCheckHttpTaskAsyncHandler : HealthCheckHttpTaskAsyncHandler
    {

        public PortalHealthCheckHttpTaskAsyncHandler()
           : base(new IHealthCheckItemProvider[] { new PortalHealthCheckItemProvider() })
        { }
    }
}
