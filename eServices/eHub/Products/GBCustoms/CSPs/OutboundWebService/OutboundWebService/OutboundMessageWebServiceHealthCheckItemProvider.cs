using CargoWise.eServices.Monitoring.HealthCheck.API;

using System.Threading.Tasks;

namespace CargoWise.eHub.Products.GBCustoms.CSP.OutboundWebService
{
    public class OutboundMessageWebServiceHealthCheckItemProvider : IHealthCheckItemProvider
	{
		public string Name => "GBCustomsCSPOutboundNotificationWebService";

		public Task<HealthCheckItem> CheckHealthAsync()
		{
			return Task.FromResult(HealthCheckItem.Info("Service is alive."));
		}
	}
}
