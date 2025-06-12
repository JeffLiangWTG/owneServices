using System;
using System.Data;
using System.Threading;
using CargoWise.Billing.Kafka.API;
using CargoWise.eServices.Billing.DataAccess;
using Common.Logging;
using Confluent.Kafka;
using Hangfire.Server;
using Hangfire;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Hangfire.Storage;

namespace CargoWise.eServices.Billing.WcfService.Hangfire.ELKMessage
{
	public class ELKFailedMessagesProcessor : BillingJob
	{
		[DisableConcurrentExecution(0)]
		[AutomaticRetry(Attempts = 0, LogEvents = false, OnAttemptsExceeded = AttemptsExceededAction.Delete, OnlyOn = new[] { typeof(DistributedLockTimeoutException) })]
		public static void RecurringProcess(PerformContext context, CancellationToken token) => new ELKFailedMessagesProcessor().StartProcess(token, context);

		public override void Processing(CancellationToken token, params object[] args)
		{
			using (var producer = GetProducer())
			{
				var repository = CreateRepository();
				while (!token.IsCancellationRequested)
				{
					successCount = 0;
					var transactionDataTable = repository.SelectELKTransaction(Configuration.ELKResubmissionBatchSize);
					if (transactionDataTable == null || transactionDataTable.Rows.Count == 0)
						break;

					Logger.Info($"Retrieved {transactionDataTable.Rows.Count} transactions");
					foreach (DataRow row in transactionDataTable.Rows)
					{
						var transaction = JObject.Parse(row["RT_JsonData"].ToString());
						var topic = GetELKTopic(transaction, Configuration);
						if (string.IsNullOrEmpty(topic))
						{
							Logger.Warn($"Invalid message: {transaction.ToString(Formatting.None)}");
							continue;
						}

						if (transaction.ContainsKey("SubmitToELKTime"))
						{
							transaction["SubmitToELKTime"] = DateTimeProvider.DateTimeNow;
						}
						else
						{
							transaction.Add(new JProperty("SubmitToELKTime", DateTimeProvider.DateTimeNow));
						}
						var message = new Message<string, string>()
						{
							Key = row["RT_PK"].ToString(),
							Value = transaction.ToString(Formatting.None)
						};

						producer.Produce(topic, message, HandleDeliveryReport);
					}

					producer.Flush();
					Logger.Info($"{successCount} submitted successfully. {transactionDataTable.Rows.Count - successCount} in error");
				}
			}
		}

		public void HandleDeliveryReport(DeliveryReport<string, string> report)
		{
			if (report.Error.Code == ErrorCode.NoError)
			{
				CreateRepository().DeleteELKTransaction(new Guid(report.Message.Key));
				Interlocked.Increment(ref successCount);
			}
			else
			{
				Logger.Error($"Failed to submit transaction to ELK. Error: {report.Error.Reason}");
			}
		}

		static string GetELKTopic(JObject jsonData, IConfigurationProvider configProvider)
		{
			if (jsonData.ContainsKey("BillableCount"))
				return configProvider.BilledELKKafkaTopic;
			if (jsonData.ContainsKey("UsageCount"))
				return configProvider.UsageELKKafkaTopic;
			return string.Empty;
		}

		internal virtual IBillingTransactionProducer<string, string> GetProducer()
		{
			var config = KafkaConfigurationProvider.GetKafkaConfig<ProducerConfig>();
			return new BillingTransactionProducer<string, string>(config);
		}

		internal virtual IBillingRepository CreateRepository() => new BillingRepository();
		public override ILog Logger { get; } = LogManager.GetLogger(typeof(ELKFailedMessagesProcessor));
		protected internal override IDateTimeProvider DateTimeProvider { get; } = new DateTimeProvider();
		int successCount;
		public override string Name => "ELKMessagesProcessor";
		protected override bool ExitWhenClosed => true;
		public override string ItemName => "failed ELK messages";
	}
}
