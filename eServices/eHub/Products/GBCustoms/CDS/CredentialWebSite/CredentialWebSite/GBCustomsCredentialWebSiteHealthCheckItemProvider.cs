using CargoWise.eServices.Monitoring.HealthCheck.API;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebSite
{
    public class GBCustomsCredentialWebSiteHealthCheckItemProvider : IHealthCheckItemProvider
    {
        public string Name { get { return "GBCustomsCredentialWebSite"; } }

        public Task<HealthCheckItem> CheckHealthAsync()
        {
            try
            {
                var expirationTime = GetAppSettings("AuthorisationTokenExpirationMinute");

                if (string.IsNullOrWhiteSpace(expirationTime))
                {
                    return Task.FromResult(HealthCheckItem.Error("Authorisation Token Expiration Minute not found in AppSettings in Web.config."));
                }

                return Task.FromResult(HealthCheckItem.Info("Service is alive."));
            }
            catch (Exception ex)
            {
                var errorDescription = string.Format(CultureInfo.InvariantCulture, "An exception '{0}' was thrown during health check.", ex.GetType().FullName);
                return Task.FromResult(HealthCheckItem.Error(errorDescription));
            }
        }

        public virtual string GetAppSettings(string name)
        {
            return WebConfigurationManager.AppSettings[name];
        }
    }
}