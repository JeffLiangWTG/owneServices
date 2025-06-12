namespace CargoWise.eServices.Billing.WcfService
{
	using System;
	using System.Configuration;
	using System.Globalization;
	using System.Threading.Tasks;
	using CargoWise.eServices.Billing.DataAccess;
	using CargoWise.eServices.Monitoring.HealthCheck.API;

	class ELKResubmissionHealthCheckItemProvider : IHealthCheckItemProvider
	{
        public string Name { get { return "ELKResubmission"; } }

		public Task<HealthCheckItem> CheckHealthAsync()
		{
			try
			{
				return CreateRepository().DoesELKBacklogExceedThreshold(ELKFailedTransactionThreshold) ? Task.FromResult(HealthCheckItem.Error($"Backlog has exceeded its threshold ({ELKFailedTransactionThreshold})")) : Task.FromResult(HealthCheckItem.Info("Healthy"));
			}
			catch (Exception ex)
			{
				var errorDescription = string.Format(CultureInfo.InvariantCulture, "An exception '{0}' was thrown during health check. Message: {1}", ex.GetType().FullName, ex.Message);
				return Task.FromResult(HealthCheckItem.Error(errorDescription));
			}
		}

		static readonly int ELKFailedTransactionThreshold = int.Parse(ConfigurationManager.AppSettings["ELKFailedTransactionThreshold"]);
		internal virtual IBillingRepository CreateRepository() => new BillingRepository();
	}
}
