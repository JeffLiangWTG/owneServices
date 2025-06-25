using System;
using Confluent.Kafka;

namespace CargoWise.Billing.Kafka.API
{
	public interface IBillingTransactionProducer<TKey, TValue> : IDisposable
	{
		void Produce(string topic, Message<TKey, TValue> message, Action<DeliveryReport<TKey, TValue>> deliveryHandler);
		void Flush();
	}
}
