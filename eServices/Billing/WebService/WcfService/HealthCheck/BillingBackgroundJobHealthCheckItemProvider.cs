using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.eServices.Billing.WcfService.Hangfire;
using CargoWise.eServices.Billing.WcfService.Hangfire.BillingTransaction;
using CargoWise.eServices.Monitoring.HealthCheck.API;
using Confluent.Kafka;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace CargoWise.eServices.Billing.WcfService
{
	public class BillingBackgroundJobHealthCheckItemProvider : IHealthCheckItemProvider
	{
		public string Name => "BillingBackgroundJob";

		public async Task<HealthCheckItem> CheckHealthAsync()
		{
			try
			{
				if (!IsBackgroundJobServerRunning())
				{
					return HealthCheckItem.Error("Billing background job server stopped");
				}

				var jobCount = GetProcessingJobCount();
				if (jobCount == 0)
				{
					return HealthCheckItem.Error("There is no billing consuming job running");
				}

				var metadata = AdminClient.Value.GetMetadata(Configuration.Value.BillingKafkaTopic, TimeSpan.FromMinutes(1));
				if (metadata.Brokers.All(x => x.Host == "127.0.0.1"))
				{
					if (jobCount < metadata.Topics[0].Partitions.Count)
					{
						return HealthCheckItem.Error($"Multiple partitions assigned to single consumer. Job count: {jobCount}, partition count: {metadata.Topics[0].Partitions.Count}");
					}
				}
				else
				{
					var groupId = GetConsumerGroupId();
					var describeGroupsResult = await AdminClient.Value
						.DescribeConsumerGroupsAsync(new[] { groupId })
						.ConfigureAwait(false);

					var description = describeGroupsResult.ConsumerGroupDescriptions.First();
					var multiPartitionsAssignedMembers = description.Members.Where(member => member.Assignment.TopicPartitions.Count > 1).ToList();

					if (multiPartitionsAssignedMembers.Any())
					{
						return HealthCheckItem.Error($"Multiple partitions assigned to single consumer. {string.Join(",", multiPartitionsAssignedMembers.Select(m => $"[JobId: {m.ClientId} - Partitions: {string.Join(",", m.Assignment.TopicPartitions.Select(tp => tp.Partition.Value))}]"))}");
					}
				}

				return HealthCheckItem.Info("Healthy");
			}
			catch (Exception ex)
			{
				var errorDescription = string.Format(CultureInfo.InvariantCulture, "An exception '{0}' was thrown during health check. Message: {1}", ex.GetType().FullName, ex.Message);
				return HealthCheckItem.Error(errorDescription);
			}
		}


		internal virtual int GetProcessingJobCount() => BillingJobManager.GetBillingJobs<ProcessingJobDto>(MonitoringApi.Value, typeof(BillingServiceProcessor), "StartProcess").Count();

		internal virtual bool IsBackgroundJobServerRunning() => BillingJobManager.IsBackgroundJobServerRunning(MonitoringApi.Value);

		readonly Lazy<IConfigurationProvider> Configuration = new Lazy<IConfigurationProvider>(() => Global.WindsorContainer.Resolve<IConfigurationProvider>());
		readonly Lazy<IAdminClient> AdminClient = new Lazy<IAdminClient>(() => Global.WindsorContainer.Resolve<IAdminClient>());
		readonly Lazy<IMonitoringApi> MonitoringApi = new Lazy<IMonitoringApi>(() => Global.WindsorContainer.Resolve<IMonitoringApi>());
		internal virtual string GetConsumerGroupId() => KafkaConfigurationProvider.GetKafkaConfig<ConsumerConfig>()?.GroupId;
	}
}
