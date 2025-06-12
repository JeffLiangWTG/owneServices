using System;
using System.Collections.Generic;
using System.Threading;
using Confluent.Kafka;

namespace CargoWise.eServices.Billing.WcfService.Hangfire
{
	public interface IBillingTransactionsConsumer : IDisposable
	{
		ConsumerConfig ConsumerConfiguration { get;}
		void Subscribe(string topic);
		void Unsubscribe();
		ConsumeResult<Ignore, CargoWise.Billing.API.BillingTransaction> Consume(CancellationToken token);
		void StoreMessageOffsets(List<ConsumeResult<Ignore, CargoWise.Billing.API.BillingTransaction>> consumeResults);
		WatermarkOffsets GetWatermarkOffsets(TopicPartition topicPartition);
		Offset Position(TopicPartition topicPartition);
		void Close();
		void Commit();
	}
}
