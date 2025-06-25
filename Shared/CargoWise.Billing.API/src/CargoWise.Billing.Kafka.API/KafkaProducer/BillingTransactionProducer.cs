using System;
using Confluent.Kafka;

namespace CargoWise.Billing.Kafka.API
{
	public class BillingTransactionProducer<TKey, TValue> : IBillingTransactionProducer<TKey, TValue>
	{
		public BillingTransactionProducer(ProducerConfig config, ISerializer<TValue> serializer = null)
		{
			if (config == null)
				throw new InvalidOperationException("Producer config is null");
			if (string.IsNullOrEmpty(config.BootstrapServers))
				throw new InvalidOperationException("billingKafkaHosts is empty");
			this.serializer = serializer;
			BuildProducer(config);
		}

		public void Produce(string topic, Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler)
		{
			producer.Produce(topic, message, deliveryHandler);
		}

		public void Flush() => producer?.Flush();

		public void Dispose()
		{
			producer?.Flush();
			producer?.Dispose();
			producer = null;
		}

		void BuildProducer(ProducerConfig config)
		{
			var producerBuilder =  new ProducerBuilder<TKey, TValue>(config);
			if (serializer != null)
			{
				producerBuilder.SetValueSerializer(serializer);
			}
			producer = producerBuilder.Build();
		}

		private IProducer<TKey, TValue> producer;
		private ISerializer<TValue> serializer;
	}
}
