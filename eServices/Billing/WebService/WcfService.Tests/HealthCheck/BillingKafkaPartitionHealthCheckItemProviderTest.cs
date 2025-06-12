using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.eServices.Billing.WcfService.HealthCheck;
using CargoWise.eServices.Monitoring.HealthCheck.API;
using CargoWise.eServices.Monitoring.HealthCheck.API.Test;
using Castle.Windsor;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	public class BillingKafkaPartitionHealthCheckItemProviderTest : HealthCheckItemProviderBaseClass<BillingKafkaPartitionHealthCheckItemProvider>
	{
		[Test]
		public async Task TestKafkaPartitionHealthCheckItemProvider()
		{
			var topicPartition = new TopicPartition("test-billing", new Partition(0));
			var topicPartitionError = new TopicPartitionError(topicPartition, new Error(ErrorCode.NoError));
			var mockProvider = new Mock<BillingKafkaPartitionHealthCheckItemProvider>(topicPartitionError)
			{
				CallBase = true
			};
			mockProvider.Setup(x => x.GetConsumerGroupId()).Returns("eServices-billing-transactions-consumer");

			var listGroupOffsetResults = new List<ListConsumerGroupOffsetsResult>
			{
				new ListConsumerGroupOffsetsResult
				{
					Group = "eServices-billing-transactions-consumer",
					Partitions = new List<TopicPartitionOffsetError>
					{
						new TopicPartitionOffsetError(new TopicPartitionOffset(topicPartitionError.TopicPartition, new Offset(0)), new Error(ErrorCode.NoError))
					}
				}
			};

			var listPartitionOffsetsResult = new ListOffsetsResult
			{
				ResultInfos = new List<ListOffsetsResultInfo>
				{
					new ListOffsetsResultInfo()
					{
						TopicPartitionOffsetError = new TopicPartitionOffsetError(
							new TopicPartitionOffset(topicPartitionError.TopicPartition, new Offset(500)),
							new Error(ErrorCode.NoError))
					}
				}
			};

			adminClientMock.Setup(x =>
				x.ListConsumerGroupOffsetsAsync(It.Is<List<ConsumerGroupTopicPartitions>>(arr =>
					arr.First().Group.Equals("eServices-billing-transactions-consumer") &&
					arr.First().TopicPartitions.First() == topicPartitionError.TopicPartition), null)).Returns(Task.FromResult(listGroupOffsetResults));

			mockProvider.Setup(x => x.ListOffsetsAsync(It.IsAny<IAdminClient>(), It.Is<IEnumerable<TopicPartitionOffsetSpec>>(arr =>
				arr.First().OffsetSpec == OffsetSpec.Latest() &&
				arr.First().TopicPartition == topicPartitionError.TopicPartition), null)).Returns(Task.FromResult(listPartitionOffsetsResult));

			var provider = mockProvider.Object;

			Assert.That(provider.Name, Is.EqualTo("BillingKafkaPartition0"));

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(provider.Name, Is.EqualTo("BillingKafkaPartition0"));
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.OK));
			Assert.That(checkItem.Description, Is.EqualTo("Healthy. Current: 0, Latest: 500, Lag: 500"));
		}

		[Test]
		public async Task TestKafkaPartitionHealthCheckItemProvider_Error_LagExceedingThreshold()
		{
			var topicPartition = new TopicPartition("test-billing", new Partition(0));
			var topicPartitionError = new TopicPartitionError(topicPartition, new Error(ErrorCode.NoError));
			var mockProvider = new Mock<BillingKafkaPartitionHealthCheckItemProvider>(topicPartitionError) { CallBase = true };
			mockProvider.Setup(x => x.GetConsumerGroupId()).Returns("eServices-billing-transactions-consumer");

			var listGroupOffsetResults = new List<ListConsumerGroupOffsetsResult>
			{
				new ListConsumerGroupOffsetsResult
				{
					Group = "eServices-billing-transactions-consumer",
					Partitions = new List<TopicPartitionOffsetError>
					{
						new TopicPartitionOffsetError(new TopicPartitionOffset(topicPartitionError.TopicPartition, new Offset(499)), new Error(ErrorCode.NoError))
					}
				}
			};

			var listPartitionOffsetsResult = new ListOffsetsResult
			{
				ResultInfos = new List<ListOffsetsResultInfo>
				{
					new ListOffsetsResultInfo
					{
						TopicPartitionOffsetError = new TopicPartitionOffsetError(
							new TopicPartitionOffset(topicPartitionError.TopicPartition, new Offset(1000)),
							new Error(ErrorCode.NoError))
					}
				}
			};

			adminClientMock.Setup(x =>
				x.ListConsumerGroupOffsetsAsync(It.Is<List<ConsumerGroupTopicPartitions>>(arr =>
					arr.First().Group.Equals("eServices-billing-transactions-consumer") &&
					arr.First().TopicPartitions.First() == topicPartitionError.TopicPartition), null)).Returns(Task.FromResult(listGroupOffsetResults));

			mockProvider.Setup(x => x.ListOffsetsAsync(It.IsAny<IAdminClient>(), It.Is<IEnumerable<TopicPartitionOffsetSpec>>(arr =>
				arr.First().OffsetSpec == OffsetSpec.Latest() &&
				arr.First().TopicPartition == topicPartitionError.TopicPartition), null)).Returns(Task.FromResult(listPartitionOffsetsResult));

			var provider = mockProvider.Object;
			Assert.That(provider.Name, Is.EqualTo("BillingKafkaPartition0"));

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
			Assert.That(checkItem.Description, Is.EqualTo("Partition lag has exceeded threshold (500). Current: 499, Latest: 1000, Lag: 501"));
		}

		[SetUp]
		public void Init()
		{
			configurationMock = new Mock<IConfigurationProvider>();
			configurationMock.Setup(x => x.PartitionLagThreshold).Returns(500);
			containerMock = new Mock<IWindsorContainer>();
			adminClientMock = new Mock<IAdminClient>();
			containerMock.Setup(x => x.Resolve<IAdminClient>()).Returns(adminClientMock.Object);
			containerMock.Setup(x => x.Resolve<IConfigurationProvider>()).Returns(configurationMock.Object);
			Global.WindsorContainer = containerMock.Object;
		}

		protected override List<string> GetAllKindsOfDescriptions()
		{
			var descriptions = new List<string>();

			var topicPartitionError = new TopicPartitionError("test-billing", 0, new Error(ErrorCode.NoError));
			var mockProvider = new Mock<BillingKafkaPartitionHealthCheckItemProvider>(topicPartitionError)
			{
				CallBase = true
			};
			mockProvider.Setup(x => x.GetConsumerGroupId()).Returns("eServices-billing-transactions-consumer");
			var provider = mockProvider.Object;

			var checkItem = provider.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			adminClientMock.Setup(x => x.ListConsumerGroupOffsetsAsync(It.IsAny<List<ConsumerGroupTopicPartitions>>(), null)).Throws(new InvalidOperationException());
			checkItem = provider.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			return descriptions;
		}

		Mock<IWindsorContainer> containerMock;
		Mock<IAdminClient> adminClientMock;
		Mock<IConfigurationProvider> configurationMock;
	}
}
