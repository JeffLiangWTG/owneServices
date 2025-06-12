using System.Data.SqlClient;

namespace CargoWise.eServices.Authentication.WebService
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.Threading.Tasks;
	using System.Web.Configuration;
	using CargoWise.eServices.Monitoring.HealthCheck.API;

	public class AuthenticationServiceHealthCheckItemProvider : IHealthCheckItemProvider
	{
		public string Name { get { return "AuthenticationWebService"; } }

		private static DateTime? FirstFailedDBConnection = null;

		public Task<HealthCheckItem> CheckHealthAsync()
		{
			try
			{
				var ediProdSystemID = GetAppSettings("ediProdSystemID");

				if (string.IsNullOrWhiteSpace(ediProdSystemID))
				{
					return Task.FromResult(
						HealthCheckItem.Error("ediProd SystemID not found in AppSettings in Web.config."));
				}

				var systemLastEditTimeUTC = DatabaseHelper.ReadSystemLastEditUTC(ediProdSystemID);
				FirstFailedDBConnection = null;

				if (systemLastEditTimeUTC == null)
				{
					return Task.FromResult(HealthCheckItem.Error("ediProd SystemID not found in database."));
				}

				if (systemLastEditTimeUTC.Value.Add(new TimeSpan(1, 0, 0)) < DateTime.UtcNow)
				{
					return Task.FromResult(HealthCheckItem.Warning("Some records are too old."));
				}

				return Task.FromResult(HealthCheckItem.Info("Service is alive."));
			}
			catch (SqlException ex)
			{
				if (!int.TryParse(GetAppSettings("DatabaseResilienceTimeout"), out var databaseResilienceTimeout)) databaseResilienceTimeout = 300;

				var errorDescription = string.Format(CultureInfo.InvariantCulture, "A sql exception '{0}' was thrown during health check.", ex.GetType().FullName);
				if (!FirstFailedDBConnection.HasValue || (DateTime.Now - FirstFailedDBConnection.Value).TotalSeconds < databaseResilienceTimeout)
				{
					if (!FirstFailedDBConnection.HasValue) FirstFailedDBConnection = DateTime.Now;
					return Task.FromResult(HealthCheckItem.Warning(errorDescription));
				}
				else
				{
					return Task.FromResult(HealthCheckItem.Error(errorDescription));
				}
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

		protected virtual IDatabaseHelper DatabaseHelper
		{
			get
			{
				if (databaseHelper == null)
				{
					databaseHelper = new DatabaseHelper();
				}
				return databaseHelper;
			}
		}

		DatabaseHelper databaseHelper;
	}
}
