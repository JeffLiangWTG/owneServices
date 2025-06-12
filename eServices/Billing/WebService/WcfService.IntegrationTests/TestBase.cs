using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.eServices.Billing.Tests.Common;
using CargoWise.eServices.TestHelpers.Database.Common;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.WcfService.IntegrationTests
{
	public class TestBase
	{
		[SetUp]
		public void Setup()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.TruncateTable(con, "dbo.ELKResubmitTransaction");
				BillingDataTestHelper.AddDatabaseAndCompany(con, 1, 1, "MEL", serverCode: "MEL", hostedLocation: "SYD");
				BillingDataTestHelper.SetUpBillingRules(con);
			}
			JobStorage = new SqlServerStorage(SqlServerHelper.GetAdminConnectionString(Tests.Common.Deployments.IntegrationTestingDbName));
			RecurringJobManager = new RecurringJobManager(JobStorage);
			BackgroundJobClient = new BackgroundJobClient(JobStorage);
			BackgroundJobClient.DeleteProcessingJobs(JobStorage.GetMonitoringApi());
			AdminClient = new AdminClientBuilder(new ClientConfig(new Dictionary<string, string>()
			{
				{ "bootstrap.servers", WithBillingWcfServiceAttribute.Current.KafkaBrokers }
			})).Build();
		}

		[TearDown]
		public void TearDown()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.TruncateStagingTable(con);
				BillingDataTestHelper.TruncateTable(con, "edi.Chargeable");
				BillingDataTestHelper.TruncateTable(con, "edi.Usage");
				BillingDataTestHelper.ClearDatabaseAndCompanyList(con);
				BillingDataTestHelper.TruncateTable(con, "dbo.ELKResubmitTransaction");
				BillingDataTestHelper.TruncateTable(con, "edi.BillingRules");
				BillingDataTestHelper.TruncateTable(con, "[HangFire].[Hash]");
				BillingDataTestHelper.ExecuteNonQuery(con, @"DELETE FROM HangFire.Hash
DELETE FROM HangFire.State
DELETE FROM HangFire.JobParameter
DELETE FROM HangFire.Job");
			}
		}

		protected void CheckRecurringJobRunsAndCompleted(string id)
		{
			using (var connection = JobStorage.GetConnection())
			{
				var monthlyAggregationJob = connection.GetRecurringJobs().FirstOrDefault(x => x.Id == id);
				if (monthlyAggregationJob?.LastExecution != null && connection.GetStateData(monthlyAggregationJob.LastJobId).Name == "Succeeded")
				{
					return;
				}

				throw new InvalidOperationException($"{id} has not completed");
			}
		}

		protected void CheckKafkaBacklogProcessed(string groupName = "eServices-billing-transactions-consumer", string topicName = "billing-topic")
		{
			var metadata = AdminClient.GetMetadata(topicName, TimeSpan.FromMinutes(1));
			var topicMetadata = metadata.Topics.First();
			var eofCount = 0;
			var consumerGroupOffsetsResult = AdminClient.ListConsumerGroupOffsetsAsync(new List<ConsumerGroupTopicPartitions>
			{
				new ConsumerGroupTopicPartitions(groupName, topicMetadata.Partitions.Select(x => new TopicPartition(topicName, new Partition(x.PartitionId))).ToList())
			}).ConfigureAwait(false).GetAwaiter().GetResult();

			var listOffsetResult = AdminClient.ListOffsetsAsync(topicMetadata.Partitions.Select(x => new TopicPartitionOffsetSpec
			{
				OffsetSpec = OffsetSpec.Latest(),
				TopicPartition = new TopicPartition(topicName, new Partition(x.PartitionId))
			})).ConfigureAwait(false).GetAwaiter().GetResult();

			var groupResult = consumerGroupOffsetsResult.First();
			foreach (var partitionId in topicMetadata.Partitions.Select(x => x.PartitionId))
			{
				var groupOffset = groupResult.Partitions.Single(x => x.TopicPartition.Partition.Value == partitionId).Offset;
				var latestOffset = listOffsetResult.ResultInfos.Single(x => x.TopicPartitionOffsetError.TopicPartition.Partition.Value == partitionId).TopicPartitionOffsetError.Offset;
				if ((groupOffset < 0 ? 0 : groupOffset) == latestOffset)
				{
					eofCount++;
				}
			}

			if (eofCount == topicMetadata.Partitions.Count)
			{
				return;
			}

			throw new InvalidOperationException("Transaction backlog has not been processed");
		}

		protected JobStorage JobStorage { get; private set; }
		protected RecurringJobManager RecurringJobManager { get; private set; }
		IBackgroundJobClient BackgroundJobClient { get; set; }
		protected IAdminClient AdminClient { get; private set; }
	}
}
