using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using CargoWise.eHub.Products.JPCustoms.Client;
using CargoWise.eHub.Products.JPCustoms.Common;
using CargoWise.eHub.Products.JPCustoms.Gateway;
using CargoWise.eServices.Monitoring.HealthCheck.API;
using Common.Logging;

namespace CargoWise.eHub.Products.JPCustoms.HealthCheckService
{
    public class JPGatewayServiceHealthCheckItemProvider : IHealthCheckItemProvider
    {
        public Task<HealthCheckItem> CheckHealthAsync()
        {
            var sendClient = new JPCustomsSendClient(new SmtpMailClientConfiguration(LogManager.GetLogger(typeof(JPGatewayServiceHealthCheckItemProvider))));
            return CheckHealthAsync(sendClient);
        }

        public string Name
        {
            get { return "JPCustomsGatewayWebService"; }
        }


        #region CheckHealthAsync

        public virtual Task<HealthCheckItem> CheckHealthAsync(IJPCustomsSendClient sendClient) 
        {
            try
            {
                sendClient.Ping();
                return Task.FromResult(HealthCheckItem.Info("Service is alive."));
            }
            catch (Exception ex)
            {
                var errorDescription = string.Format(CultureInfo.InvariantCulture, "An exception '{0}' was thrown during health check.", ex.GetType().FullName);
                return Task.FromResult(HealthCheckItem.Error(errorDescription));
            }
       }

        #endregion
    }
}
