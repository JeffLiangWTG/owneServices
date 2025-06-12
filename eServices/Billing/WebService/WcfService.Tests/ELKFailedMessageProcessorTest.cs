using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Billing.Kafka.API;
using CargoWise.eServices.Billing.DataAccess;
using CargoWise.eServices.Billing.Tests.Common;
using CargoWise.eServices.Billing.WcfService.Hangfire.ELKMessage;
using Castle.Windsor;
using Common.Logging;
using Confluent.Kafka;
using Hangfire.Server;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	[TestFixture]
	public class ELKFailedMessageProcessorTest
	{
		[Test]
		public void ResubmitFailedMessages_Success()
		{
			var handleReportTasks = new List<Task>();
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				InsertTestTransactions();
				BillingDataTestHelper.AddELKTransaction("{ \"Test\" : \"Test\", \"UsageCount\" : 1 }");
				producerMock.Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()))
					.Callback<string, Message<string, string>, Action<DeliveryReport<string, string>>>((t, m, handlerDrAction) => 
					{
						var report = new DeliveryReport<string, string>() { Message = m, Error = ErrorCode.NoError };
						handleReportTasks.Add(new Task(() => handlerDrAction(report)));
					});
				producerMock.Setup(x => x.Flush()).Callback(() => 
				{
					handleReportTasks.ForEach(x => x.Start());
					Task.WaitAll(handleReportTasks.ToArray());
					handleReportTasks.Clear();
				});

				var dt = BillingDataTestHelper.SelectELKTransactions();
				Assert.AreEqual(11, dt.Rows.Count);

				var processor = GetProcessor();
				processor.Object.StartProcess(CancellationToken.None, null);

				dt = BillingDataTestHelper.SelectELKTransactions();
				Assert.AreEqual(0, dt.Rows.Count);

				producerMock.Verify(x => x.Produce("billed-topic", It.Is<Message<string, string>>(message => message.Value.Equals(ExpectedData)), It.IsAny<Action<DeliveryReport<string, string>>>()), Times.Exactly(10));
				producerMock.Verify(x => x.Produce("usage-topic", It.Is<Message<string, string>>(message => message.Value.Equals(ExpectedUsageData)), It.IsAny<Action<DeliveryReport<string, string>>>()), Times.Once);
				loggerMock.Verify(x => x.Info("Retrieved 3 transactions"), Times.Exactly(3));
				loggerMock.Verify(x => x.Info("Retrieved 2 transactions"), Times.Exactly(1));
				loggerMock.Verify(x => x.Info("3 submitted successfully. 0 in error"), Times.Exactly(3));
				loggerMock.Verify(x => x.Info("2 submitted successfully. 0 in error"), Times.Exactly(1));
			}
		}

		[Test]
		public void ResubmitFailedMessages_KafkaError()
		{
			var handleReportTasks = new List<Task>();
			var count = 1;
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				InsertTestTransactions();
				producerMock.Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()))
					.Callback<string, Message<string, string>, Action<DeliveryReport<string, string>>>((t, m, handlerDrAction) =>
					{
						var report = new DeliveryReport<string, string>() { Message = m, Error = count % 3 != 0 ? ErrorCode.NoError : ErrorCode.Local_MsgTimedOut };
						handleReportTasks.Add(new Task(() => handlerDrAction(report)));
						count++;
					});
				producerMock.Setup(x => x.Flush()).Callback(() =>
				{
					handleReportTasks.ForEach(x => x.Start());
					Task.WaitAll(handleReportTasks.ToArray());
					handleReportTasks.Clear();
					count = 1;
				});
				producerMock.Setup(x => x.Dispose());

				var dt = BillingDataTestHelper.SelectELKTransactions();
				Assert.AreEqual(10, dt.Rows.Count);

				var processor = GetProcessor();
				processor.Object.StartProcess(CancellationToken.None, null);

				dt = BillingDataTestHelper.SelectELKTransactions();
				Assert.AreEqual(0, dt.Rows.Count);

				producerMock.Verify(x => x.Produce("billed-topic", It.Is<Message<string, string>>(message => message.Value.Equals(ExpectedData)), It.IsAny<Action<DeliveryReport<string, string>>>()), Times.Exactly(14));
				loggerMock.Verify(x => x.Info("Retrieved 3 transactions"), Times.Exactly(4));
				loggerMock.Verify(x => x.Info("Retrieved 2 transactions"), Times.Exactly(1));
				loggerMock.Verify(x => x.Info("2 submitted successfully. 1 in error"), Times.Exactly(4));
				loggerMock.Verify(x => x.Info("2 submitted successfully. 0 in error"), Times.Exactly(1));
				loggerMock.Verify(x => x.Error("Failed to submit transaction to ELK. Error: Local: Message timed out"), Times.Exactly(4));
			}
		}

		[Test]
		public void ResubmitFailedMessages_Exception()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				InsertTestTransactions();
				producerMock.Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()))
					.Throws(new Exception("Test error"));
				producerMock.Setup(x => x.Dispose());

				var dt = BillingDataTestHelper.SelectELKTransactions();
				Assert.AreEqual(10, dt.Rows.Count);

				var processor = GetProcessor();
				var ex = Assert.Throws<Exception>(() => processor.Object.StartProcess(CancellationToken.None, null));
				Assert.AreEqual("Test error", ex?.Message);

				dt = BillingDataTestHelper.SelectELKTransactions();
				Assert.AreEqual(10, dt.Rows.Count);

				producerMock.Verify(x => x.Produce("billed-topic", It.Is<Message<string, string>>(message => message.Value.Equals(ExpectedData)), It.IsAny<Action<DeliveryReport<string, string>>>()), Times.Exactly(4));
				loggerMock.Verify(x => x.Info("Retrieved 3 transactions"), Times.Exactly(4));
				loggerMock.Verify(x => x.Error("Reached maximum retries. Stop processing."), Times.Exactly(1));
			}
		}

		void InsertTestTransactions()
		{
			BillingDataTestHelper.AddELKTransaction("{ \"Test\" : \"Test\", \"BillableCount\" : 1, \"SubmitToELKTime\": \"1900-01-01T00:00:00\" }");
			for (int i = 0; i < 9; i++)
			{
				BillingDataTestHelper.AddELKTransaction("{ \"Test\" : \"Test\", \"BillableCount\" : 1 }");
			}
		}

		[SetUp]
		public void Setup()
		{
			loggerMock = new Mock<ILog>();
			dateTimeMock = new Mock<IDateTimeProvider>();
			dateTimeMock.Setup(x => x.DateTimeNow).Returns(new DateTime(2024, 1, 1, 2, 2, 2));
			producerMock = new Mock<IBillingTransactionProducer<string, string>>();
			producerMock.Setup(x => x.Dispose());
			configurationMock = new Mock<IConfigurationProvider>();
			configurationMock.Setup(x => x.BilledELKKafkaTopic).Returns("billed-topic");
			configurationMock.Setup(x => x.UsageELKKafkaTopic).Returns("usage-topic");
			configurationMock.Setup(x => x.MaxRetries).Returns(3);
			configurationMock.Setup(x => x.RetryTimeoutInMilliseconds).Returns(10);
			configurationMock.Setup(x => x.ELKResubmissionBatchSize).Returns(3);
			var containerMock = new Mock<IWindsorContainer>();
			containerMock.Setup(x => x.Resolve<IConfigurationProvider>()).Returns(configurationMock.Object);
			Global.WindsorContainer = containerMock.Object;
			repository = BillingDataTestHelper.GetBillingRepository();
		}

		Mock<ELKFailedMessagesProcessor> GetProcessor()
		{
			var mock = new Mock<ELKFailedMessagesProcessor>() { CallBase = true };
			mock.Setup(x => x.CreateRepository()).Returns(repository);
			mock.Setup(x => x.DateTimeProvider).Returns(dateTimeMock.Object);
			mock.Setup(x => x.GetProducer()).Returns(producerMock.Object);
			mock.Setup(x => x.Logger).Returns(loggerMock.Object);
			mock.Setup(x => x.GetBackgroundJobId(It.IsAny<PerformContext>())).Returns(string.Empty);
			return mock;
		}

		[TearDown]
		public void TearDown()
		{
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
				BillingDataTestHelper.TruncateTable(con, "dbo.ELKResubmitTransaction");
		}

		Mock<IBillingTransactionProducer<string, string>> producerMock;
		Mock<IDateTimeProvider> dateTimeMock;
		Mock<IConfigurationProvider> configurationMock;
		Mock<ILog> loggerMock;
		IBillingRepository repository;
		const string ExpectedData = "{\"Test\":\"Test\",\"BillableCount\":1,\"SubmitToELKTime\":\"2024-01-01T02:02:02\"}";
		const string ExpectedUsageData = "{\"Test\":\"Test\",\"UsageCount\":1,\"SubmitToELKTime\":\"2024-01-01T02:02:02\"}";
	}
}
