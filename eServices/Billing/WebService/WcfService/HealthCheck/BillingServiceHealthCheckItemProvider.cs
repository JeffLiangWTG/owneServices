using CargoWise.Billing.Service;

namespace CargoWise.eServices.Billing.WcfService
{
	using System;
	using System.Configuration;
	using System.Globalization;
	using System.Threading.Tasks;
	using CargoWise.eServices.Billing.DataAccess;
	using CargoWise.Billing.Kafka.API;
	using CargoWise.eServices.Monitoring.HealthCheck.API;
	using Common.Logging;
	using Confluent.Kafka;

	class BillingServiceHealthCheckItemProvider : IHealthCheckItemProvider
	{
        public string Name { get { return "BillingWebService"; } }

		public Task<HealthCheckItem> CheckHealthAsync()
		{
			var transaction = new BillingTransaction
			{
				BillableCount = 1,
				ClientID = "TSTTSTTST",
				Category = "TST",
				PriceItemCode = "TST",
				Reference1 = "TEST",
				Reference2 = "TEST",
				Reference3 = "TEST",
				Reference4 = "TEST",
				ReportingSource = "CHK",
				ServiceOccuredUTC = DateTime.UtcNow,
				MessageTrackingID = "55B2D0DA-8230-43BE-83BF-5C7D5766343E",
			};

			try
			{
				AddTransaction(transaction);

				return Task.FromResult(HealthCheckItem.Info("Service is alive."));
			}
			catch (Exception ex)
			{
				var errorDescription = string.Format(CultureInfo.InvariantCulture, "An exception '{0}' was thrown during health check. Message: {1}", ex.GetType().FullName, ex.Message);
				return Task.FromResult(HealthCheckItem.Error(errorDescription));
			}
		}

		protected virtual void AddTransaction(BillingTransaction transaction)
		{
			var config = KafkaConfigurationProvider.GetKafkaConfig<ProducerConfig>();
			config.MessageTimeoutMs = 5000;

			using (var kafkaClient = new BillingKafkaClient(config))
			{
				var handler = new BillingHandler(kafkaClient, Logger);
				new BillingService(handler).AddTransaction(transaction);
			}
		}
		static readonly ILog Logger = LogManager.GetLogger(typeof(BillingService));
	}
}
