using CargoWise.eServices.Monitoring.HealthCheck.API;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService.HealthCheck
{
    public class RoutingRuleValidationHealthCheckHttpTaskAsyncHandler : HealthCheckHttpTaskAsyncHandler
    {
        public RoutingRuleValidationHealthCheckHttpTaskAsyncHandler()
            : base(new IHealthCheckItemProvider[] { new RoutingRuleValidationHealthCheckItemProvider() })
        {
        }
    }
}