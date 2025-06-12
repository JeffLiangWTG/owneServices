using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Billing.Kafka.API;
using Common.Logging;
using Confluent.Kafka;

namespace CargoWise.eServices.Billing.WcfService.Hangfire.BillingTransaction
{
	public class BillingServiceProcessor : BillingJob
	{
		public override void Processing(CancellationToken token, params object[] args)
		{
			if (IsThrottlingThresholdReached())
			{
				exitNow = true;
				return;
			}

			Logger.Info("Initialize billing consumer");
			var handler = GetBillingHandler(Configuration);
			var transactionsBatch = new List<CargoWise.Billing.API.BillingTransaction>();
			var consumeResults = new List<ConsumeResult<Ignore, CargoWise.Billing.API.BillingTransaction>>();
			using (billingConsumer = GetConsumer(Configuration))
			{
				billingConsumer.Subscribe(Configuration.BillingKafkaTopic);
				while (!token.IsCancellationRequested)
				{
					var currentConsumeResult = billingConsumer.Consume(token);
					if (currentConsumeResult?.Message?.Value != null)
					{
						transactionsBatch.Add(currentConsumeResult.Message.Value);
						consumeResults.Add(currentConsumeResult);
						Logger.TraceFormat("Adding transaction [{0}]", currentConsumeResult.Message.Value);
					}
					if (ShouldProcessBatch(transactionsBatch, currentConsumeResult))
					{
						SubmitTransactionByBatch(transactionsBatch, consumeResults, handler);
						if (IsThrottlingThresholdReached()) break;
					}
				}
				if (transactionsBatch.Any())
				{
					SubmitTransactionByBatch(transactionsBatch, consumeResults, handler);
				}

				billingConsumer.Unsubscribe();
				billingConsumer.Close();
				handler.Close();
				Logger.Info("Stopped consuming message");
			}
		}
		bool IsThrottlingThresholdReached()
		{
			var throttlingThresholdOfStaging = Configuration.ThrottlingThresholdOfStaging;
			var countOfStaging = CountStaging();

			if (countOfStaging >= throttlingThresholdOfStaging)
			{
				Logger.WarnFormat("CountOfStaging reached throttlingThresholdOfStaging - Count: {0} Threshold: {1}", countOfStaging, throttlingThresholdOfStaging);
				return true;
			}

			Logger.DebugFormat("Checked CountOfStaging and throttlingThresholdOfStaging - Count: {0} Threshold: {1}", countOfStaging, throttlingThresholdOfStaging);
			return false;
		}
		private void SubmitTransactionByBatch(List<CargoWise.Billing.API.BillingTransaction> transactionsBatch, List<ConsumeResult<Ignore, CargoWise.Billing.API.BillingTransaction>> consumeResults, BillingHandler handler)
		{
			try
			{
				handler.SubmitTransactions(transactionsBatch);
				Logger.TraceFormat("Batch transaction processed for {0} transactions", transactionsBatch.Count);
			}
			catch (CargoWise.Billing.API.ValidationException e)
			{
				var errorsList = e.Errors.ToList();
				Logger.Debug(
				  format => format(
					"Billing transaction is invalid:{0}:{1}{2}",
					e.Message,
					Environment.NewLine,
					string.Join(Environment.NewLine, errorsList.Select(error => "  - " + error))),
				  e);
			}
			if (consumeResults.Any())
			{
				var consumerConfig = billingConsumer.ConsumerConfiguration;
				if (billingConsumer.ConsumerConfiguration?.EnableAutoOffsetStore == false)
				{
					billingConsumer.StoreMessageOffsets(consumeResults);
					Logger.TraceFormat("stored {0} message offsets locally. AutoOffsetStore: {1}, AutoCommit: {2}.", consumeResults.Count, consumerConfig?.EnableAutoOffsetStore, consumerConfig?.EnableAutoCommit);
				}
				if (consumerConfig?.EnableAutoCommit == false)
				{
					billingConsumer.Commit();
					Logger.TraceFormat("AutoCommit is disabled, Please update the configuration to enable auto-commit. AutoOffsetStore: {0}, AutoCommit: {1}.", consumerConfig?.EnableAutoOffsetStore, consumerConfig?.EnableAutoCommit);
				}
				transactionsBatch.Clear();
				consumeResults.Clear();
			}
		}

		private bool ShouldProcessBatch(List<CargoWise.Billing.API.BillingTransaction> transactionsBatch, ConsumeResult<Ignore, CargoWise.Billing.API.BillingTransaction> currentResult)
		{
			var isLastMessage = currentResult?.IsPartitionEOF ?? true;
			return isLastMessage || transactionsBatch.Count >= Configuration.BillingKafkaBatchSize;
		}

		internal virtual BillingHandler GetBillingHandler(IConfigurationProvider configProvider)
		{
			var config =  KafkaConfigurationProvider.GetKafkaConfig<ProducerConfig>();

			return new BillingHandler(new BillingKafkaClient(new ProducerConfig(config)), Logger, Configuration);
		}

		internal virtual IBillingTransactionsConsumer GetConsumer(IConfigurationProvider configProvider)
		{
			return new BillingTransactionsConsumer(JobName);
		}

		internal virtual int CountStaging() => StagingCounter.Singleton.Count;
		public override ILog Logger { get; } = LogManager.GetLogger(typeof(BillingServiceProcessor));
		bool exitNow = false;
		protected override bool ExitWhenClosed => exitNow;
		IBillingTransactionsConsumer billingConsumer;
		public override string Name => "BillingConsumer";
		public override string ItemName => "billing transactions";
	}
}
