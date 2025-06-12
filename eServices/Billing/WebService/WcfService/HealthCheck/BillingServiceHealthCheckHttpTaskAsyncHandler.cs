using System.Collections.Generic;
using CargoWise.eServices.Billing.WcfService.HealthCheck;
using Confluent.Kafka;

namespace CargoWise.eServices.Billing.WcfService
{
	using System;
	using System.Runtime.Remoting.Messaging;
	using CargoWise.eServices.Monitoring.HealthCheck.API;

	public class BillingServiceHealthCheckHttpTaskAsyncHandler : HealthCheckHttpTaskAsyncHandler
	{
		public BillingServiceHealthCheckHttpTaskAsyncHandler() : base(GetProviders())
		{
		}

		static IHealthCheckItemProvider[] GetProviders()
		{
			var providers = new List<IHealthCheckItemProvider>
			{
				new BillingServiceHealthCheckItemProvider(),
				new BillingBackgroundJobHealthCheckItemProvider(),
				new StagingHealthCheckItemProvider(),
				new ProcessingBillingDatabaseMonthlyAggregationHealthCheckItemProvider(),
				new ProcessingBillingDatabaseUpdateBillingCubePartialHealthCheckItemProvider(),
				new ELKResubmissionHealthCheckItemProvider()
			};

			try
			{
				var adminClient = Global.WindsorContainer.Resolve<IAdminClient>();
				var configuration = Global.WindsorContainer.Resolve<IConfigurationProvider>();
				var topicMetadata = adminClient.GetMetadata(configuration.BillingKafkaTopic, TimeSpan.FromSeconds(5));
				if (topicMetadata?.Topics[0]?.Partitions?.Count == 0)
				{
					providers.Add(new BillingKafkaPartitionHealthCheckItemProvider(new TopicPartitionError(configuration.BillingKafkaTopic, Partition.Any, new Error(ErrorCode.Local_UnknownTopic))));
				}
				else
				{
					foreach (var partitionMetadata in topicMetadata.Topics[0].Partitions)
					{
						providers.Add(new BillingKafkaPartitionHealthCheckItemProvider(new TopicPartitionError(configuration.BillingKafkaTopic, new Partition(partitionMetadata.PartitionId), new Error(ErrorCode.NoError))));
					}
				}
			}
			catch (Exception e)
			{
				providers.Add(new BillingKafkaPartitionHealthCheckItemProvider(new TopicPartitionError(null, Partition.Any, new Error(ErrorCode.Unknown, e.Message))));
			}
			return providers.ToArray();
		}
	}
}
