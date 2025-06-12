using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Configuration;
using CargoWise.eServices.Monitoring.HealthCheck.API;

using Common.Logging;

namespace CargoWise.eHub.Products.GBCustoms.CDS.CredentialWebService
{
    public class CredentialWebServiceHealthCheckItemProvider : IHealthCheckItemProvider
	{
		public static int ServiceErrorCount = 0;
		public static int DatabaseErrorCount = 0;

		public string Name { get { return "GBCustomsCredentialWebService"; } }

		public Func<Int32> Retries = () => Convert.ToInt32(WebConfigurationManager.AppSettings["HealthCheckRetryCount"]);
		
		public Task<HealthCheckItem> CheckHealthAsync()
        {
			try
			{
				var retries = Retries();
				var allErrors = string.Join(Environment.NewLine, CheckCreatingServiceInstance(), CheckConnectingToDatabase());

				return string.IsNullOrWhiteSpace(allErrors) ?
					Task.FromResult(HealthCheckItem.Info("Service is alive.")) :
					(ServiceErrorCount < retries && DatabaseErrorCount < retries) ?
						Task.FromResult(HealthCheckItem.Warning(allErrors)) :
						Task.FromResult(HealthCheckItem.Error(allErrors));
			}
			catch (Exception ex)
			{
				return Task.FromResult(HealthCheckItem.Error($"An exception '{ex.GetType().FullName}' was thrown during health check."));
			}
        }

		#region Implementation
		string CheckCreatingServiceInstance()
		{
			try
			{
				var instance = new CredentialWebService();

				if(instance == null)
                {
					ServiceErrorCount++;
					return "The instance of the service is null.";
				}

				ServiceErrorCount = 0;
				return string.Empty;
			}
			catch
			{
				ServiceErrorCount++;
				return "Could not create an instance of the service.";
			}
		}

		string CheckConnectingToDatabase()
		{
			try
			{
				using (var connection = DatabaseConnector.GeteHubTransactionsConnection())
				{
					connection.Open();
					DatabaseErrorCount = 0;
					return string.Empty;
				}
			}
			catch
			{
				DatabaseErrorCount++;
				return "Could not create a connection to database.";
			}
		}
		#endregion

		#region DatabaseConnector
		protected ILog logger;
		public virtual IConfigurationHelper DatabaseConnector
		{
			get { return databaseConnector ?? (databaseConnector = new ConfigurationHelper(logger)); }
		}

		IConfigurationHelper databaseConnector;
		#endregion

	}
}
