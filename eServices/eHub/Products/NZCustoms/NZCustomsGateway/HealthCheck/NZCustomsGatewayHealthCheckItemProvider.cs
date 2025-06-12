using CargoWise.eServices.Monitoring.HealthCheck.API;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CargoWise.eHub.Products.NZCustoms.Gateway.HealthCheck
{
    public class NZCustomsGatewayHealthCheckItemProvider : IHealthCheckItemProvider
    {
        public string healthCheckError = string.Empty;

        public string Name { get { return "NZCustomsGatewayService"; } }

        public Task<HealthCheckItem> CheckHealthAsync()
        {
            try
            {
                if (!MessageGateway.Ping())
                {
                    healthCheckError += "NZCustomsGatewayService is not active. ";
                }

                if (healthCheckError != string.Empty)
                    return Task.FromResult(HealthCheckItem.Error(healthCheckError));
                else
                    return Task.FromResult(HealthCheckItem.Info("Service is alive."));
            }
            catch (Exception ex)
            {
                var errorDescription = string.Format(CultureInfo.InvariantCulture, "An exception '{0}' was thrown during health check.", ex.GetType().FullName);
                return Task.FromResult(HealthCheckItem.Error(errorDescription));
            }
        }

        public virtual IMessageGateway MessageGateway
        {
            get
            {
                if (messageGateway == null)
                {
                    messageGateway = new MessageGateway();
                }
                return messageGateway;
            }
        }

        MessageGateway messageGateway;
    }
}