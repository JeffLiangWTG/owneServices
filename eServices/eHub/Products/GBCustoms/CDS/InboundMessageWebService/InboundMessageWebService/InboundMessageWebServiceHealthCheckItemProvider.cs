using CargoWise.eHub.Products.GBCustoms.CDS.InboundMessageWebService.Controllers;
using CargoWise.eHub.Products.GBCustoms.Core.InboundMessageWebService;
using CargoWise.eServices.Monitoring.HealthCheck.API;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace CargoWise.eHub.Products.GBCustoms.CDS.InboundMessageWebService
{
    public class InboundMessageWebServiceHealthCheckItemProvider : IHealthCheckItemProvider
    {
        public string Name { get { return "GBCustomsInboundMessageWebService"; } }

		public static int ServiceErrorCount = 0;
		public static int DatabaseErrorCount = 0;
		public Func<Int32> Retries = () => Convert.ToInt32(WebConfigurationManager.AppSettings["HealthCheckRetryCount"]);

		public Task<HealthCheckItem> CheckHealthAsync()
        {
            try
            {
				var retries = Retries();
				var allErrors = string.Join("|", new string[] { CheckAppSettings(), CheckCreatingServiceInstance(), CheckConnectingToDatabase() }.Where(s => !string.IsNullOrEmpty(s)));

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
		string CheckAppSettings()
		{
			var regType = GetAppSettings("GBCustomsRegistrationType");

			return string.IsNullOrWhiteSpace(regType)
				? "GBCustomsRegistrationType not found in AppSettings in Web.config."
				: string.Empty;
		}

		public virtual string GetAppSettings(string name)
		{
			return WebConfigurationManager.AppSettings[name];
		}

		string CheckCreatingServiceInstance()
		{
			try
			{
				var instance = new InboundMessageController();

				if (instance == null)
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
				using (var connection = DatabaseConnector.GeteHubTransactionsConnection("eHubTransactionsContext"))
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
		public virtual IDataConnector DatabaseConnector
		{
			get { return dataConnector ?? (dataConnector = new DataConnector()); }
		}

		IDataConnector dataConnector;
		#endregion
	}
}
