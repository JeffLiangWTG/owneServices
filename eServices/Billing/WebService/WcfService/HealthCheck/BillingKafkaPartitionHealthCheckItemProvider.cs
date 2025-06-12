using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CargoWise.eServices.Monitoring.HealthCheck.API;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace CargoWise.eServices.Billing.WcfService.HealthCheck
{
	public class BillingKafkaPartitionHealthCheckItemProvider : IHealthCheckItemProvider
	{
		public BillingKafkaPartitionHealthCheckItemProvider() : this(new TopicPartitionError(null, Partition.Any, new Error(ErrorCode.NoError)))
		{
		}

		public BillingKafkaPartitionHealthCheckItemProvider(TopicPartitionError topicPartitionError)
		{
			TopicPartitionError = topicPartitionError;
		}

		public async Task<HealthCheckItem> CheckHealthAsync()
		{
			try
			{
				if (TopicPartitionError.Error != ErrorCode.NoError)
				{
					return HealthCheckItem.Error(TopicPartitionError.Error.ToString());
				}

				var adminClient = Global.WindsorContainer.Resolve<IAdminClient>();
				var configuration = Global.WindsorContainer.Resolve<IConfigurationProvider>();
				var groupId = GetConsumerGroupId();
				var consumerGroupOffsetsResult = await adminClient.ListConsumerGroupOffsetsAsync(new List<ConsumerGroupTopicPartitions>
				{
					new ConsumerGroupTopicPartitions(groupId, new List<TopicPartition>
					{
						TopicPartitionError.TopicPartition
					})
				}).ConfigureAwait(false);

				var listOffsetResult = await ListOffsetsAsync(adminClient, new []
				{
					new TopicPartitionOffsetSpec
					{
						OffsetSpec = OffsetSpec.Latest(),
						TopicPartition = TopicPartitionError.TopicPartition
					}
				}).ConfigureAwait(false);

				var consumerGroupPartitionOffset = Math.Max(consumerGroupOffsetsResult[0].Partitions[0].Offset.Value, 0);
				var latestPartitionOffset = Math.Max(listOffsetResult.ResultInfos[0].TopicPartitionOffsetError.Offset.Value, 0);
				var lag = latestPartitionOffset - consumerGroupPartitionOffset;
				var resultPostfix = $"Current: {consumerGroupOffsetsResult[0].Partitions[0].Offset.Value}, Latest: {listOffsetResult.ResultInfos[0].TopicPartitionOffsetError.Offset.Value}, Lag: {lag}";
				return lag > configuration.PartitionLagThreshold ? HealthCheckItem.Error($"Partition lag has exceeded threshold ({configuration.PartitionLagThreshold}). {resultPostfix}") : HealthCheckItem.Info($"Healthy. {resultPostfix}");
			}
			catch (Exception ex)
			{
				var errorDescription = string.Format(CultureInfo.InvariantCulture, "An exception '{0}' was thrown during health check. Message: {1}", ex.GetType().FullName, ex.Message);
				return HealthCheckItem.Error(errorDescription);
			}
		}

		internal virtual Task<ListOffsetsResult> ListOffsetsAsync(IAdminClient adminClient,
			IEnumerable<TopicPartitionOffsetSpec> topicPartitionOffsets, ListOffsetsOptions options = null) =>
			adminClient.ListOffsetsAsync(topicPartitionOffsets, options);
		internal virtual string GetConsumerGroupId() => KafkaConfigurationProvider.GetKafkaConfig<ConsumerConfig>()?.GroupId;

		public string Name => $"BillingKafkaPartition{(TopicPartitionError.Error == ErrorCode.NoError ? TopicPartitionError?.Partition.Value.ToString() : string.Empty)}";
		TopicPartitionError TopicPartitionError { get; }
	}
}
