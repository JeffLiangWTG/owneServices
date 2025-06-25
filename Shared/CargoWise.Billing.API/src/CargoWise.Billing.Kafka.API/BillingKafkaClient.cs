using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using CargoWise.Billing.API;
using Confluent.Kafka;

namespace CargoWise.Billing.Kafka.API
{
	public interface IBillingKafkaClient : IDisposable
	{
		void SendBillingInfoToKafka(string kafkaTopic, string trackingId, BillingTransaction transaction, Action<DeliveryReport<string, BillingTransaction>> deliveryHandler);
		void SendBillingInfoToELK(string kafkaTopic, IEnumerable<BillingTransaction> transactions, Action<DeliveryReport<string, string>> deliveryHandler, bool validationTransaction = true);
		void SendBillingInfoToELK(string kafkaTopic, string trackingId, BillingTransaction transaction, Action<DeliveryReport<string, string>> deliveryHandler, bool validationTransaction = true);
		void SendUsageInfoToELK(string kafkaTopic, string trackingId, UsageTransaction transaction, Action<DeliveryReport<string, string>> deliveryHandler, bool validationTransaction = true);
		void SendUsageInfoToELK(string kafkaTopic, string trackingId, UsageTransaction transaction, Action<DeliveryReport<string, ELKTransaction>> deliveryHandler);
		void FlushProducers();
	}
	public class BillingKafkaClient : IBillingKafkaClient
	{
		public BillingKafkaClient(string kafkaHosts, IDateTimeProvider dateTimeProvider = null)
		{
			this.dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();

			InitializeProducers(CreateDefaultProducerConfig(kafkaHosts));
		}

		public BillingKafkaClient(ProducerConfig producerConfig, IDateTimeProvider dateTimeProvider = null)
		{
			this.dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();

			InitializeProducers(producerConfig);
		}

		public void SendBillingInfoToKafka(string kafkaTopic, string trackingId, BillingTransaction transaction, Action<DeliveryReport<string, BillingTransaction>> deliveryHandler)
		{
			BillingTransactionValidator.ValidateTransaction(transaction);
			var message = new Message<string, BillingTransaction> { Key = trackingId, Value = transaction };
			_billingProducer.Value.Produce(kafkaTopic, message, deliveryHandler);
		}

		public void SendBillingInfoToELK(string kafkaTopic, IEnumerable<BillingTransaction> transactions, Action<DeliveryReport<string, string>> deliveryHandler, bool validationTransaction = true)
		{
			foreach (var transaction in transactions)
			{
				SendBillingInfoToELK(kafkaTopic, transaction.MessageTrackingID, transaction, deliveryHandler, validationTransaction);
			}
		}

		public void SendBillingInfoToELK(string kafkaTopic, string trackingId, BillingTransaction transaction, Action<DeliveryReport<string, string>> deliveryHandler, bool validationTransaction = true)
		{
			if (validationTransaction) BillingTransactionValidator.ValidateTransaction(transaction);
			
			string usageTransaction = TransactionHelper.ConvertBillingInfoToJson(transaction, dateTimeProvider.DateTimeNow);
			var message = new Message<string, string> { Key = trackingId, Value = usageTransaction };
			_jsonProducer.Value.Produce(kafkaTopic, message, deliveryHandler);
		}

		public virtual void Dispose()
		{
			FlushAndDisposeProducers();
		}

		public void FlushProducers()
		{
			if (_billingProducer.IsValueCreated)
				_billingProducer.Value.Flush();
			if (_jsonProducer.IsValueCreated)
				_jsonProducer.Value.Flush();
			if (_elkProducer.IsValueCreated)
				_elkProducer.Value.Flush();
		}

		public static ProducerConfig CreateDefaultProducerConfig(string kafkaHosts)
		{ 
			return new ProducerConfig
			{
				BootstrapServers = kafkaHosts,
				EnableSslCertificateVerification = true,
				SecurityProtocol = SecurityProtocol.Ssl,
				LingerMs = 50
			};
		}

		void InitializeProducers(ProducerConfig config)
		{
			_billingProducer = CreateProducer(() => new BillingTransactionProducer<string, BillingTransaction>(config, new BillingTransactionsSerializer<BillingTransaction>()));
			_jsonProducer = CreateProducer(() => new BillingTransactionProducer<string, string>(config, new BillingTransactionsSerializer<string>()));
			_elkProducer = CreateProducer(() => new BillingTransactionProducer<string, ELKTransaction>(config, new ELKSerializer<ELKTransaction>(dateTimeProvider)));
		}

		public void FlushAndDisposeProducers()
		{
			FlushProducers();
			if (_billingProducer.IsValueCreated)
			{
				_billingProducer.Value.Dispose();
			}

			if (_elkProducer.IsValueCreated)
			{
				_elkProducer.Value.Dispose();
			}

			if (_jsonProducer.IsValueCreated)
			{
				_jsonProducer.Value.Dispose();
			}
		}

		public void SendUsageInfoToELK(string kafkaTopic, string trackingId, UsageTransaction transaction, Action<DeliveryReport<string, string>> deliveryHandler, bool validationTransaction = true)
		{
			string usageTransaction = TransactionHelper.ConvertUsageInfoToJson(transaction, dateTimeProvider.DateTimeNow);
			var message = new Message<string, string> { Key = trackingId, Value = usageTransaction };
			_jsonProducer.Value.Produce(kafkaTopic, message, deliveryHandler);
		}

		public void SendUsageInfoToELK(string kafkaTopic, string trackingId, UsageTransaction transaction, Action<DeliveryReport<string, ELKTransaction>> deliveryHandler)
		{
			var message = new Message<string, ELKTransaction> { Key = trackingId, Value = transaction };
			_elkProducer.Value.Produce(kafkaTopic, message, deliveryHandler);
		}

		public virtual Lazy<IBillingTransactionProducer<string, T>> CreateProducer<T>(Func<IBillingTransactionProducer<string, T>> valueFactory) => new Lazy<IBillingTransactionProducer<string, T>>(valueFactory);
		Lazy<IBillingTransactionProducer<string, BillingTransaction>> _billingProducer;
		Lazy<IBillingTransactionProducer<string, ELKTransaction>> _elkProducer;
		Lazy<IBillingTransactionProducer<string, string>> _jsonProducer;
		readonly IDateTimeProvider dateTimeProvider;

		public static T GetKafkaConfig<T>(Dictionary<string, string> baseConfig, Dictionary<string, string> producerDic = null, Dictionary<string, string> consumerDic = null) where T : ClientConfig
		{
			var config = new Dictionary<string, string>(baseConfig);
			switch (typeof(T).Name)
			{
				case nameof(ProducerConfig):
					if (producerDic != null)
					{
						MergeDictionaries(config, producerDic);
					}
					return new ProducerConfig(config) as T;

				case nameof(ConsumerConfig):
					if (consumerDic != null)
					{
						MergeDictionaries(config, consumerDic);
					}
					return new ConsumerConfig(config) as T;

				case nameof(AdminClientConfig):
					return new AdminClientConfig(baseConfig) as T;

				default:
					return baseConfig as T;
			}
		}
		private static void MergeDictionaries(
			Dictionary<string, string> target,
			Dictionary<string, string> source)
		{
			foreach (var setting in source.Where(setting => !target.ContainsKey(setting.Key)))
			{
				target[setting.Key] = setting.Value;
			}
		}
	}
}
