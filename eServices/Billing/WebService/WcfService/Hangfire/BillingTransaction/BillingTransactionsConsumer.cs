using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Billing.Kafka.API;
using Common.Logging;
using Confluent.Kafka;
using API = CargoWise.Billing.API;

namespace CargoWise.eServices.Billing.WcfService.Hangfire
{
	public class BillingTransactionsConsumer : IBillingTransactionsConsumer
	{
		public BillingTransactionsConsumer(string clientId)
		{
			kafkaConsumer = BuildBillingConsumer(clientId);
		}

		internal virtual IConsumer<Ignore, API.BillingTransaction> BuildBillingConsumer(string clientId)
		{
			ConsumerConfiguration = KafkaConfigurationProvider.GetKafkaConfig<ConsumerConfig>();
			ConsumerConfiguration.ClientId = clientId;

			return new ConsumerBuilder<Ignore, API.BillingTransaction>(ConsumerConfiguration)
				.SetValueDeserializer(new IssuerReporterDeserializerProxy<API.BillingTransaction>(new CargoWise.Billing.Kafka.API.BillingTransactionsDeserializer()))
				.Build();
		}

		public void Subscribe(string topic) => kafkaConsumer?.Subscribe(topic);
		public void Commit() => kafkaConsumer?.Commit();

		public void Unsubscribe() => kafkaConsumer?.Unsubscribe();

		public ConsumeResult<Ignore, API.BillingTransaction> Consume(CancellationToken token)
		{
			try
			{
				var result = kafkaConsumer?.Consume(token);
				var logParams = result == null ? new[] {"0", string.Empty} : new[] {"1", $" Topic: {result.Topic}, Partition: {result.Partition}, Offset: {result.Offset.Value}"};
				Logger.TraceFormat("Consumed {0} message from kafka queue{1}", logParams);
				return result;
			}
			catch (OperationCanceledException)
			{
				Logger.TraceFormat("Cancellation requested");
				return null;
			}
		}

		public void CommitMessageOffset(ConsumeResult<Ignore, API.BillingTransaction> message) => kafkaConsumer?.Commit(message);
		public void CommitMessageOffsets(IEnumerable<TopicPartitionOffset> offsets) => kafkaConsumer?.Commit(offsets);
		public void StoreMessageOffsets(List<ConsumeResult<Ignore, CargoWise.Billing.API.BillingTransaction>> consumeResults)
		{
			if (kafkaConsumer != null && consumeResults != null)
			{
				foreach (var consumeResult in consumeResults)
				{
					StoreMessageOffset(consumeResult);
				}
			}
		}
		public void StoreMessageOffset(ConsumeResult<Ignore, CargoWise.Billing.API.BillingTransaction> consumeResult)
		{
			if (kafkaConsumer != null && consumeResult != null)
			{
				kafkaConsumer.StoreOffset(consumeResult);
			}
		}
		public WatermarkOffsets GetWatermarkOffsets(TopicPartition topicPartition) => kafkaConsumer.GetWatermarkOffsets(topicPartition);

		public virtual Offset Position(TopicPartition topicPartition) => kafkaConsumer.Position(topicPartition);

		public void Close()
		{
			kafkaConsumer?.Close();
		}

		public void Dispose()
		{
			kafkaConsumer?.Dispose();
		}

		private readonly IConsumer<Ignore, API.BillingTransaction> kafkaConsumer;
		internal static ILog Logger = LogManager.GetLogger(typeof(BillingTransactionsConsumer));
		public virtual  ConsumerConfig ConsumerConfiguration   { get;  private  set; }
	}
}
