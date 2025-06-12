using CargoWise.eServices.Monitoring.HealthCheck.API;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService.HealthCheck
{
    public class RoutingRuleValidationHealthCheckItemProvider : IHealthCheckItemProvider
    {
        #region Name

        public string Name => "Routing Rule Validation WebService";

        #endregion

        #region CheckHealthAsync

        public Task<HealthCheckItem> CheckHealthAsync()
        {
            try
            {
	            var allErrors = Regex.Replace(string.Concat(ErrorsOnCreatingServiceInstance(),ErrorsOnConnectingToDatabase()), "[\r\n]", "");
				return string.IsNullOrWhiteSpace(allErrors) ? 
                    Task.FromResult(HealthCheckItem.Info("Service is alive.")) :
                    Task.FromResult(HealthCheckItem.Error(allErrors));
            }
            catch (Exception ex)
            {
                var errorDescription = string.Format(CultureInfo.InvariantCulture, "An exception '{0}' was thrown during health check.", ex.GetType().FullName);
                return Task.FromResult(HealthCheckItem.Error(errorDescription));
            }
        }

		#endregion

		#region Implementation

		public virtual string ErrorsOnCreatingServiceInstance()
        {
            try
            {
                var instance = new RoutingRuleValidationWebService();
                return instance != null ? string.Empty : "The instance of the service is Null.";
            }
            catch
            {
                return "Could not create an instance of the service.";
            }
        }

		public virtual string ErrorsOnConnectingToDatabase()
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["eHubTransactionsContext"].ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    return string.Empty;
                }
            }
            catch
            {
                return "Could not create a connection to database.";
            }
        }

        #endregion
    }
}
