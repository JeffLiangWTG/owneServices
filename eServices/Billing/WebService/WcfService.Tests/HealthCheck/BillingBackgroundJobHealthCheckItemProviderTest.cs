using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.eServices.Monitoring.HealthCheck.API;
using CargoWise.eServices.Monitoring.HealthCheck.API.Test;
using Castle.Windsor;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	public class BillingBackgroundJobHealthCheckItemProviderTest : HealthCheckItemProviderBaseClass<BillingBackgroundJobHealthCheckItemProvider>
	{
		[Test]
		public async Task TestBillingBackgroundJobHealthCheckItemProvider()
		{
			var mockProvider = new Mock<BillingBackgroundJobHealthCheckItemProvider>();
			mockProvider.CallBase = true;
			mockProvider.Setup(x => x.IsBackgroundJobServerRunning()).Returns(true);
			mockProvider.Setup(x => x.GetProcessingJobCount()).Returns(1);
			mockProvider.Setup(x => x.GetConsumerGroupId()).Returns("eServices-billing-transactions-consumer");
			var provider = mockProvider.Object;

			Assert.That(provider.Name, Is.EqualTo("BillingBackgroundJob"));

			var describeGroupsResult = new DescribeConsumerGroupsResult
			{
				ConsumerGroupDescriptions = new List<ConsumerGroupDescription>
				{
					new ConsumerGroupDescription
					{
						Members = new List<MemberDescription>
						{
							new MemberDescription
							{
								ClientId = "Client1",
								Assignment = new MemberAssignment
								{
									TopicPartitions = new List<TopicPartition>
									{
										new TopicPartition("test-billing", new Partition(0))
									}
								}
							},
							new MemberDescription
							{
								ClientId = "Client2",
								Assignment = new MemberAssignment
								{
									TopicPartitions = new List<TopicPartition>
									{
										new TopicPartition("test-billing", new Partition(1))
									}
								}
							}
						}
					}
				}
			};

			adminClientMock.Setup(x =>
				x.DescribeConsumerGroupsAsync(It.Is<IEnumerable<string>>(arr =>
					arr.First().Equals("eServices-billing-transactions-consumer")), null)).Returns(Task.FromResult(describeGroupsResult));

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.OK));
			Assert.That(checkItem.Description, Is.EqualTo("Healthy"));
		}

		[Test]
		public async Task TestBillingBackgroundJobHealthCheckItemProvider_Error_NoJobRunning()
		{
			var mockProvider = new Mock<BillingBackgroundJobHealthCheckItemProvider>();
			mockProvider.CallBase = true;
			mockProvider.Setup(x => x.IsBackgroundJobServerRunning()).Returns(true);
			mockProvider.Setup(x => x.GetProcessingJobCount()).Returns(0);
			var provider = mockProvider.Object;

			Assert.That(provider.Name, Is.EqualTo("BillingBackgroundJob"));

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
			Assert.That(checkItem.Description, Is.EqualTo("There is no billing consuming job running"));

			mockProvider.Setup(x => x.IsBackgroundJobServerRunning()).Returns(false);
			checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
			Assert.That(checkItem.Description, Is.EqualTo("Billing background job server stopped"));
		}

		[Test]
		public async Task TestBillingBackgroundJobHealthCheckItemProvider_Error_MultiplePartitionAssigned()
		{
			var mockProvider = new Mock<BillingBackgroundJobHealthCheckItemProvider>();
			mockProvider.CallBase = true;
			mockProvider.Setup(x => x.IsBackgroundJobServerRunning()).Returns(true);
			mockProvider.Setup(x => x.GetProcessingJobCount()).Returns(1);
			mockProvider.Setup(x => x.GetConsumerGroupId()).Returns("eServices-billing-transactions-consumer");
			var provider = mockProvider.Object;

			Assert.That(provider.Name, Is.EqualTo("BillingBackgroundJob"));

			var describeGroupsResult = new DescribeConsumerGroupsResult
			{
				ConsumerGroupDescriptions = new List<ConsumerGroupDescription>
				{
					new ConsumerGroupDescription
					{
						Members = new List<MemberDescription>
						{
							new MemberDescription
							{
								ClientId = "Client1",
								Assignment = new MemberAssignment
								{
									TopicPartitions = new List<TopicPartition>
									{
										new TopicPartition("test-billing", new Partition(0)),
										new TopicPartition("test-billing", new Partition(1))
									}
								}
							},
							new MemberDescription
							{
								ClientId = "Client2",
								Assignment = new MemberAssignment
								{
									TopicPartitions = new List<TopicPartition>
									{
										new TopicPartition("test-billing", new Partition(2)),
										new TopicPartition("test-billing", new Partition(3))
									}
								}
							}
						}
					}
				}
			};

			adminClientMock.Setup(x =>
				x.DescribeConsumerGroupsAsync(It.Is<IEnumerable<string>>(arr =>
					arr.First().Equals("eServices-billing-transactions-consumer")), null)).Returns(Task.FromResult(describeGroupsResult));

			var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
			Assert.That(checkItem.Description, Is.EqualTo("Multiple partitions assigned to single consumer. [JobId: Client1 - Partitions: 0,1],[JobId: Client2 - Partitions: 2,3]"));

			mockProvider.Setup(x => x.IsBackgroundJobServerRunning()).Returns(false);
			checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
			Assert.That(checkItem.Description, Is.EqualTo("Billing background job server stopped"));
		}

		[SetUp]
		public void Init()
		{
			configMock = new Mock<IConfigurationProvider>();
			configMock.Setup(x => x.BillingKafkaTopic).Returns("test-billing");
			containerMock = new Mock<IWindsorContainer>();
			adminClientMock = new Mock<IAdminClient>();
			adminClientMock.Setup(x => x.GetMetadata("test-billing", It.IsAny<TimeSpan>()))
				.Returns(new Metadata(new List<BrokerMetadata>()
				{
					new BrokerMetadata(0, "test.com", 1),
					new BrokerMetadata(1, "test.com", 2)
				}, new List<TopicMetadata>()
				{
					new TopicMetadata("test-billing", new List<PartitionMetadata>()
					{
						new PartitionMetadata(0, 0, new []{0, 1}, new []{2, 3}, ErrorCode.NoError)
					}, ErrorCode.NoError)
				}, 0, "test"));
			containerMock.Setup(x => x.Resolve<IAdminClient>()).Returns(adminClientMock.Object);
			containerMock.Setup(x => x.Resolve<IConfigurationProvider>()).Returns(configMock.Object);
			Global.WindsorContainer = containerMock.Object;
		}

		protected override List<string> GetAllKindsOfDescriptions()
		{
			var descriptions = new List<string>();

			var mockProvider = new Mock<BillingBackgroundJobHealthCheckItemProvider>();
			mockProvider.CallBase = true;
			var provider = mockProvider.Object;

			mockProvider.Setup(x => x.IsBackgroundJobServerRunning()).Returns(true).Verifiable();
			mockProvider.Setup(x => x.GetProcessingJobCount()).Returns(1).Verifiable();
			mockProvider.Setup(x => x.GetConsumerGroupId()).Returns("eServices-billing-transactions-consumer");
			var checkItem = provider.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			mockProvider.Setup(x => x.IsBackgroundJobServerRunning()).Throws(new InvalidOperationException());
			checkItem = provider.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			return descriptions;
		}

		Mock<IWindsorContainer> containerMock;
		Mock<IAdminClient> adminClientMock;
		Mock<IConfigurationProvider> configMock;
	}
}
