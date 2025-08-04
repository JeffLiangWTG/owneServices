using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using CargoWise.eServices.Billing.DataAccess;
using CargoWise.Billing.Kafka.API;
using Common.Logging;
using Confluent.Kafka;
using API = CargoWise.Billing.API;

namespace CargoWise.eServices.Billing.WcfService
{
	public class BillingHandler
	{
		internal BillingHandler(IBillingKafkaClient kafkaClient, ILog logger, IConfigurationProvider configProvider = null)
		{
			this.logger = logger;
			this.kafkaClient = kafkaClient;
			this.repository = CreateRepository();
			this.configProvider = configProvider ?? new ConfigurationProvider();
		}

		public void SubmitTransaction(API.BillingTransaction transaction)
		{
			HandleBillingTransaction(transaction,
					(ts) => repository.Add(ts),
					(ts) => SendBillingToELK(kafkaClient, ts, false),
					(ts) => SendBillingToELK(kafkaClient, ts, true));
		}

		public void SubmitTransactions(IEnumerable<API.BillingTransaction> transactions)
		{
			var billingTransactions = new List<API.BillingTransaction>();
			var usageTransactions = new List<API.BillingTransaction>();

			foreach (var transaction in transactions)
			{
				HandleBillingTransaction(transaction, 
					(ts) => billingTransactions.Add(ts), 
					(ts) => usageTransactions.Add(ts),
					(ts) => SendBillingToELK(kafkaClient, ts, true));
			}

			if (billingTransactions.Count > 0) 
				repository.AddRange(billingTransactions);

			if (usageTransactions.Count == 0) return;
			if (kafkaClient != null)
			{
				kafkaClient.SendBillingInfoToELK(configProvider.BilledELKKafkaTopic, usageTransactions, HandleBillingUsageDeliveryReport, false);
			}
			else
			{
				repository.InsertELKResubmitTransactions(usageTransactions.Select(transaction => TransactionHelper.ConvertBillingInfoToJson(transaction)));
			}
		}

		public void AddUsageTransaction(API.UsageTransaction transaction) =>
			repository.InsertELKResubmitTransaction(TransactionHelper.ConvertUsageInfoToJson(transaction));

                public void AddUsageTransactions(IEnumerable<API.UsageTransaction> transactions) =>
                        repository.InsertELKResubmitTransactions(transactions.Select(transaction => TransactionHelper.ConvertUsageInfoToJson(transaction)));

                public IEnumerable<API.LicenseInfo> GetLatestLicenses() => repository.GetLatestLicenses();

		void HandleBillingTransaction(API.BillingTransaction transaction, Action<API.BillingTransaction> handleTransactionAction, Action<API.BillingTransaction> handleUsageTransactionAction, Action<API.BillingTransaction> handleNonBilledTransactionAction)
		{
			if (transaction.Category == "STL" && configProvider.NonBilledCodes.Contains(transaction.PriceItemCode))
			{
				handleNonBilledTransactionAction(transaction);
			}
			else
			{
				handleTransactionAction(transaction);

				if ((configProvider.SendToELKBillingCategories.Contains(transaction.Category) || configProvider.SendToELKBillingCategories.Contains("ALL")) && !configProvider.NotSendToELKBillingCodes.Contains(transaction.PriceItemCode))
				{
					handleUsageTransactionAction(transaction);
				}
			}
		}

		void SendBillingToELK(IBillingKafkaClient kafkaClient, API.BillingTransaction transaction, bool validateTransaction)
		{
			if (kafkaClient != null)
			{
				kafkaClient.SendBillingInfoToELK(configProvider.BilledELKKafkaTopic, transaction.MessageTrackingID, transaction, HandleBillingUsageDeliveryReport, validateTransaction);
			}
			else
			{
				repository.InsertELKResubmitTransaction(TransactionHelper.ConvertBillingInfoToJson(transaction));
			}
		}

		private void HandleBillingUsageDeliveryReport(DeliveryReport<string, string> report)
		{
			if (report.Error.Code != ErrorCode.NoError)
			{
				CreateRepository().InsertELKResubmitTransaction(report.Value);
			}
			else
			{
				logger.Debug("Submitted billing info to ELK stack.");
			}
		}

		internal virtual IBillingRepository CreateRepository() => new BillingRepository();

		internal void Close() => kafkaClient?.Dispose();

		readonly IBillingRepository repository;
		readonly ILog logger;
		readonly IBillingKafkaClient kafkaClient;
		readonly IConfigurationProvider configProvider;
	}
}
