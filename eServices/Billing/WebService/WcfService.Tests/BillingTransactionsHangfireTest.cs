using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Billing.API;
using CargoWise.eServices.Billing.DataAccess;
using CargoWise.Billing.Kafka.API;
using CargoWise.eServices.Billing.WcfService.Hangfire;
using CargoWise.eServices.Billing.WcfService.Hangfire.BillingTransaction;
using Castle.Windsor;
using Common.Logging;
using Confluent.Kafka;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Moq;
using ProtoBuf;
using NUnit.Framework;
using API = CargoWise.Billing.API;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	[TestFixture]
	public class BillingTransactionsHangfireTest
	{
		[Test]
		public void TestBillingTransactionsHangfire_Success()
		{
			var messageOffset = 1;
			Task consumerTask = null;

			var tokenSource = new CancellationTokenSource();
			var topicParition = new TopicPartition("billing-topic", new Partition(0));
			List<API.BillingTransaction> transactions = new List<API.BillingTransaction>();

			var invalidTransaction = CreateBillingTransaction();
			invalidTransaction.ClientNumber = "5";
			invalidTransaction.MessageTrackingID = "44444444-4444-4444-4444-444444444444";
			invalidTransaction.ServiceOccuredUTC = DateTime.UtcNow.AddYears(-6);
			messages.Add(BillingTransactionProtoBufSerializerForTest.SerializeTransaction(invalidTransaction));


			var kafkaConsumerMock = new Mock<IConsumer<Ignore, API.BillingTransaction>>();
			kafkaConsumerMock.Setup(x => x.Subscribe(It.IsAny<string>()));
			kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>())).Returns(() =>
			{
				if (messages.Count > 0)
				{
					var bytesResult = new ReadOnlySpan<byte>(messages.First());
					var transaction = new CargoWise.Billing.Kafka.API.BillingTransactionsDeserializer().Deserialize(bytesResult, false, new Confluent.Kafka.SerializationContext());
					var messageResult = new Message<Ignore, API.BillingTransaction> { Value = transaction };
					var consumeResult = new ConsumeResult<Ignore, API.BillingTransaction> { Message = messageResult, TopicPartitionOffset = new TopicPartitionOffset(topicParition, new Offset(messageOffset++)) };
					messages.Remove(messages.First(message => BillingTransactionProtoBufSerializerForTest.DeserializeTransaction(message).ClientNumber == consumeResult.Message.Value.ClientNumber));
					return consumeResult;
				}

				return null;
			});

			kafkaConsumerMock.Setup(x => x.Commit(It.IsAny<ConsumeResult<Ignore, API.BillingTransaction>>()));
			kafkaConsumerMock.Setup(x => x.GetWatermarkOffsets(topicParition)).Returns(new WatermarkOffsets(new Offset(0), new Offset(messages.Count)));

			var billingConsumerMock = new Mock<BillingTransactionsConsumer>("TestBillingJob-1");
			billingConsumerMock.Setup(x => x.BuildBillingConsumer( "TestBillingJob-1")).Returns(kafkaConsumerMock.Object);
			billingConsumerMock.Setup(x => x.ConsumerConfiguration).Returns(new ConsumerConfig()
			{
				EnableAutoCommit = false,
				EnableAutoOffsetStore = false
			});

			var billingRepoMock = new Mock<IBillingRepository>();
			billingRepoMock.Setup(x => x.AddRange(It.IsAny<IEnumerable<API.BillingTransaction>>())).Callback<IEnumerable<API.BillingTransaction>>((transactionsList) =>
			{
				var billingTransactions = transactionsList.ToList();
				BillingTransactionValidator.ValidateTransactions(billingTransactions);
				transactions.AddRange(billingTransactions);
			});

			var kafkaClientMock = BillingServiceTest.CreateKafkaClient(jsonBillingProducerMock, toELKTrackingIDs, null, null);
			var handlerMock = new Mock<BillingHandler>(kafkaClientMock.Object, processorLoggerMock.Object, configurationMock.Object);
			handlerMock.Setup(x => x.CreateRepository()).Returns(billingRepoMock.Object);
			var billingProcessor = GetBillingTransactionProcessorMock(configurationMock, handlerMock, billingConsumerMock, processorLoggerMock);

			jobWrapperMock.Setup(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default")).Callback(billingJobManager.EnqueueOrDeleteJobs);
			jobWrapperMock.Setup(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>())).Returns(() =>
			{
				consumerTask = Task.Run(() => billingProcessor.Object.StartProcess(tokenSource.Token, null));
				return billingProcessor.Object.Name;
			});
			jobWrapperMock.Setup(x => x.TriggerRecurringJob("BillingJobManager"));
			billingJobManager.ResetJobs();

			while (messages.Count > 0)
				Task.Delay(10).Wait();

			tokenSource.Cancel();
			consumerTask?.Wait();

			Assert.AreEqual(3, transactions.Count);
			Assert.AreEqual("0", transactions[0].ClientNumber);
			Assert.AreEqual("1", transactions[1].ClientNumber);
			Assert.AreEqual("2", transactions[2].ClientNumber);
			Assert.AreEqual(3, toELKTrackingIDs.Count);
			Assert.AreEqual("00000000-0000-0000-0000-000000000000", toELKTrackingIDs[0]);
			Assert.AreEqual("11111111-1111-1111-1111-111111111111", toELKTrackingIDs[1]);
			Assert.AreEqual("33333333-3333-3333-3333-333333333333", toELKTrackingIDs[2]);

			jobWrapperMock.Verify(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default"), Times.Once);
			jobWrapperMock.Verify(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>()), Times.Once);
			jobWrapperMock.Verify(x => x.TriggerRecurringJob("BillingJobManager"), Times.Once);
			jobWrapperMock.Verify(x => x.DeleteBackgroundJob(It.IsAny<string>()), Times.Never);
			kafkaConsumerMock.Verify(x => x.Subscribe("billing-topic"), Times.Once);
			kafkaConsumerMock.Verify(x => x.StoreOffset(It.IsAny<ConsumeResult<Ignore, API.BillingTransaction>>()), Times.Exactly(5));
			processorLoggerMock.Verify(x => x.Info("Start processing billing transactions"), Times.Once);
			processorLoggerMock.Verify(x => x.InfoFormat("{0} running in process {1}", "TestBillingJob-1", It.IsAny<int>()), Times.Once);
			processorLoggerMock.Verify(x => x.InfoFormat("{0} stopping in process {1}", "TestBillingJob-1", It.IsAny<int>()), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", transactions[0]), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", transactions[1]), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", transactions[2]), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Batch transaction processed for {0} transactions", transactions.Count), Times.Once);
			processorLoggerMock.Verify(x => x.DebugFormat("Checked CountOfStaging and throttlingThresholdOfStaging - Count: {0} Threshold: {1}", 0, Int32.MaxValue), Times.AtLeastOnce());
			billingRepoMock.Verify(x => x.AddRange(It.Is<List<API.BillingTransaction>>(y => y.SequenceEqual(transactions.Take(3)))), Times.Once);
			jsonBillingProducerMock.Verify(x => x.Produce(It.Is<string>(s => s.Equals("billed-topic")), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()), Times.Exactly(3));
		}

		[Test]
		public void TestBillingTransactionsHangfire_IsThrottlingThresholdReached()
		{
			var messageOffset = 1;
			Task consumerTask = null;

			var tokenSource = new CancellationTokenSource();
			var topicParition = new TopicPartition("billing-topic", new Partition(0));
			List<API.BillingTransaction> transactions = new List<API.BillingTransaction>();

			var invalidTransaction = CreateBillingTransaction();
			invalidTransaction.ClientNumber = "5";
			invalidTransaction.MessageTrackingID = "44444444-4444-4444-4444-444444444444";
			invalidTransaction.ServiceOccuredUTC = DateTime.UtcNow.AddYears(-6);
			messages.Add(BillingTransactionProtoBufSerializerForTest.SerializeTransaction(invalidTransaction));

			var kafkaConsumerMock = new Mock<IConsumer<Ignore, API.BillingTransaction>>();
			kafkaConsumerMock.Setup(x => x.Subscribe(It.IsAny<string>()));
			kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>())).Returns(() =>
			{
				if (messages.Count > 0)
				{
					var bytesResult = new ReadOnlySpan<byte>(messages.First());
					var transaction = new CargoWise.Billing.Kafka.API.BillingTransactionsDeserializer().Deserialize(bytesResult, false, new Confluent.Kafka.SerializationContext());
					var messageResult = new Message<Ignore, API.BillingTransaction> { Value = transaction };
					var consumeResult = new ConsumeResult<Ignore, API.BillingTransaction> { Message = messageResult, TopicPartitionOffset = new TopicPartitionOffset(topicParition, new Offset(messageOffset++)) };
					messages.Remove(messages.First(message => BillingTransactionProtoBufSerializerForTest.DeserializeTransaction(message).ClientNumber == consumeResult.Message.Value.ClientNumber));
					return consumeResult;
				}

				return null;
			});

			kafkaConsumerMock.Setup(x => x.Commit(It.IsAny<ConsumeResult<Ignore, API.BillingTransaction>>()));
			kafkaConsumerMock.Setup(x => x.GetWatermarkOffsets(topicParition)).Returns(new WatermarkOffsets(new Offset(0), new Offset(messages.Count)));

			var billingConsumerMock = new Mock<BillingTransactionsConsumer>("TestBillingJob-1");
			billingConsumerMock.Setup(x => x.BuildBillingConsumer("TestBillingJob-1")).Returns(kafkaConsumerMock.Object);
			billingConsumerMock.Setup(x => x.ConsumerConfiguration).Returns(new ConsumerConfig()
			{
				EnableAutoCommit = false,
				EnableAutoOffsetStore = false
			});

			var billingRepoMock = new Mock<IBillingRepository>();
			billingRepoMock.Setup(x => x.AddRange(It.IsAny<IEnumerable<API.BillingTransaction>>())).Callback<IEnumerable<API.BillingTransaction>>((transactionsList) =>
			{
				var billingTransactions = transactionsList.ToList();
				BillingTransactionValidator.ValidateTransactions(billingTransactions);
				transactions.AddRange(billingTransactions);
			});

			var kafkaClientMock = BillingServiceTest.CreateKafkaClient(jsonBillingProducerMock, toELKTrackingIDs, null, null);
			var handlerMock = new Mock<BillingHandler>(kafkaClientMock.Object, processorLoggerMock.Object, configurationMock.Object);
			handlerMock.Setup(x => x.CreateRepository()).Returns(billingRepoMock.Object);

			configurationMock.Setup(x => x.ThrottlingThresholdOfStaging).Returns(10);
			var billingProcessor = GetBillingTransactionProcessorMock(configurationMock, handlerMock, billingConsumerMock, processorLoggerMock);
			billingProcessor.SetupSequence(_ => _.CountStaging()).Returns(0).Returns(3).Returns(11).Returns(11);

			jobWrapperMock.Setup(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default")).Callback(billingJobManager.EnqueueOrDeleteJobs);
			jobWrapperMock.Setup(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>())).Returns(() =>
			{
				consumerTask = Task.Run(() => billingProcessor.Object.StartProcess(tokenSource.Token, null));
				return billingProcessor.Object.Name;
			});
			jobWrapperMock.Setup(x => x.TriggerRecurringJob("BillingJobManager"));
			billingJobManager.ResetJobs();

			while (messages.Count > 0)
				Task.Delay(10).Wait();

			consumerTask?.Wait();

			Assert.AreEqual(3, transactions.Count);
			Assert.AreEqual("0", transactions[0].ClientNumber);
			Assert.AreEqual("1", transactions[1].ClientNumber);
			Assert.AreEqual("2", transactions[2].ClientNumber);
			Assert.AreEqual(3, toELKTrackingIDs.Count);
			Assert.AreEqual("00000000-0000-0000-0000-000000000000", toELKTrackingIDs[0]);
			Assert.AreEqual("11111111-1111-1111-1111-111111111111", toELKTrackingIDs[1]);
			Assert.AreEqual("33333333-3333-3333-3333-333333333333", toELKTrackingIDs[2]);

			jobWrapperMock.Verify(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default"), Times.Once);
			jobWrapperMock.Verify(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>()), Times.Once);
			jobWrapperMock.Verify(x => x.TriggerRecurringJob("BillingJobManager"), Times.Once);
			jobWrapperMock.Verify(x => x.DeleteBackgroundJob(It.IsAny<string>()), Times.Never);
			kafkaConsumerMock.Verify(x => x.Subscribe("billing-topic"), Times.Once);
			kafkaConsumerMock.Verify(x => x.StoreOffset(It.IsAny<ConsumeResult<Ignore, API.BillingTransaction>>()), Times.Exactly(5));
			processorLoggerMock.Verify(x => x.Info("Start processing billing transactions"), Times.Once);
			processorLoggerMock.Verify(x => x.InfoFormat("{0} running in process {1}", "TestBillingJob-1", It.IsAny<int>()), Times.Once);
			processorLoggerMock.Verify(x => x.InfoFormat("{0} exiting in process {1}", "TestBillingJob-1", It.IsAny<int>()), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", transactions[0]), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", transactions[1]), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", transactions[2]), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Batch transaction processed for {0} transactions", transactions.Count), Times.Once);
			processorLoggerMock.Verify(x => x.DebugFormat("Checked CountOfStaging and throttlingThresholdOfStaging - Count: {0} Threshold: {1}", 0, 10), Times.Once());
			processorLoggerMock.Verify(x => x.DebugFormat("Checked CountOfStaging and throttlingThresholdOfStaging - Count: {0} Threshold: {1}", 3, 10), Times.Once());
			processorLoggerMock.Verify(x => x.WarnFormat("CountOfStaging reached throttlingThresholdOfStaging - Count: {0} Threshold: {1}", 11, 10), Times.Exactly(2));
			billingRepoMock.Verify(x => x.AddRange(It.Is<List<API.BillingTransaction>>(y => y.SequenceEqual(transactions.Take(3)))), Times.Once);
			jsonBillingProducerMock.Verify(x => x.Produce(It.Is<string>(s => s.Equals("billed-topic")), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()), Times.Exactly(3));
		}

		private ConsumeResult<Ignore, BillingTransaction> ConsumeResult(ref int messageOffset, TopicPartition topicParition)
		{
			var bytesResult = new ReadOnlySpan<byte>(messages.First());
			var transaction = new CargoWise.Billing.Kafka.API.BillingTransactionsDeserializer().Deserialize(bytesResult, false, new Confluent.Kafka.SerializationContext());
			var messageResult = new Message<Ignore, API.BillingTransaction> { Value = transaction };
			var consumeResult = new ConsumeResult<Ignore, API.BillingTransaction> { Message = messageResult, TopicPartitionOffset = new TopicPartitionOffset(topicParition, new Offset(messageOffset++)) };
			return consumeResult;
		}

		[Test]
		public void TestBillingTransactionsHangfire_CommitMessageEOF()
		{
			var messageOffset = 1;
			Task consumerTask = null;
			var call = 0;

			var tokenSource = new CancellationTokenSource();
			var topicParition = new TopicPartition("billing-topic", new Partition(0));
			List<API.BillingTransaction> transactions = new List<API.BillingTransaction>();

			var kafkaConsumerMock = new Mock<IConsumer<Ignore, API.BillingTransaction>>();
			kafkaConsumerMock.Setup(x => x.Subscribe(It.IsAny<string>()));
			kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>())).Returns(() =>
			{
				call++;
				if (call < 3 && messages.Count > 0)
				{
					var bytesResult = new ReadOnlySpan<byte>(messages.First());
					var transaction = new CargoWise.Billing.Kafka.API.BillingTransactionsDeserializer().Deserialize(bytesResult, false, new Confluent.Kafka.SerializationContext());
					var messageResult = new Message<Ignore, API.BillingTransaction> { Value = transaction };
					var consumeResult = new ConsumeResult<Ignore, API.BillingTransaction> { Message = messageResult, TopicPartitionOffset = new TopicPartitionOffset(topicParition, new Offset(messageOffset++)) };
					messages.Remove(messages.First(message => BillingTransactionProtoBufSerializerForTest.DeserializeTransaction(message).ClientNumber == consumeResult.Message.Value.ClientNumber));
					return consumeResult;
				}
				return null;
			});
			kafkaConsumerMock.Setup(x => x.Commit(It.IsAny<ConsumeResult<Ignore, API.BillingTransaction>>()));
			kafkaConsumerMock.Setup(x => x.GetWatermarkOffsets(topicParition)).Returns(new WatermarkOffsets(new Offset(0), new Offset(messages.Count)));

			var billingConsumerMock = new Mock<BillingTransactionsConsumer>("TestBillingJob-1");
			billingConsumerMock.Setup(x => x.BuildBillingConsumer("TestBillingJob-1")).Returns(kafkaConsumerMock.Object);
			billingConsumerMock.Setup(x => x.ConsumerConfiguration).Returns(new ConsumerConfig()
			{
				EnableAutoCommit = false,
				EnableAutoOffsetStore = false
			});

			var billingRepoMock = new Mock<IBillingRepository>();
			billingRepoMock.Setup(x => x.AddRange(It.IsAny<IEnumerable<API.BillingTransaction>>())).Callback<IEnumerable<API.BillingTransaction>>((transactionsList) =>
			{
				var billingTransactions = transactionsList.ToList();
				BillingTransactionValidator.ValidateTransactions(billingTransactions);
				transactions.AddRange(billingTransactions);
			});

			var kafkaClientMock = BillingServiceTest.CreateKafkaClient(jsonBillingProducerMock, toELKTrackingIDs, null, null);
			var handlerMock = new Mock<BillingHandler>(kafkaClientMock.Object, processorLoggerMock.Object, configurationMock.Object);
			handlerMock.Setup(x => x.CreateRepository()).Returns(billingRepoMock.Object);
			var billingProcessor = GetBillingTransactionProcessorMock(configurationMock, handlerMock, billingConsumerMock, processorLoggerMock);

			jobWrapperMock.Setup(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default")).Callback(billingJobManager.EnqueueOrDeleteJobs);
			jobWrapperMock.Setup(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>())).Returns(() =>
			{
				consumerTask = Task.Run(() => billingProcessor.Object.StartProcess(tokenSource.Token, null));
				return billingProcessor.Object.Name;
			});
			jobWrapperMock.Setup(x => x.TriggerRecurringJob("BillingJobManager"));
			billingJobManager.ResetJobs();

			while (messages.Count > 2)
				Task.Delay(10).Wait();

			consumerTask?.Wait(21000);
			tokenSource.Cancel();


			Assert.AreEqual(2, transactions.Count);
			Assert.AreEqual("0", transactions[0].ClientNumber);
			Assert.AreEqual("1", transactions[1].ClientNumber);
			Assert.AreEqual(2, toELKTrackingIDs.Count);
			Assert.AreEqual("00000000-0000-0000-0000-000000000000", toELKTrackingIDs[0]);
			Assert.AreEqual("11111111-1111-1111-1111-111111111111", toELKTrackingIDs[1]);

			jobWrapperMock.Verify(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default"), Times.Once);
			jobWrapperMock.Verify(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>()), Times.Once);
			jobWrapperMock.Verify(x => x.TriggerRecurringJob("BillingJobManager"), Times.Once);
			jobWrapperMock.Verify(x => x.DeleteBackgroundJob(It.IsAny<string>()), Times.Never);
			kafkaConsumerMock.Verify(x => x.Subscribe("billing-topic"), Times.Once);
			kafkaConsumerMock.Verify(x => x.StoreOffset(It.IsAny<ConsumeResult<Ignore, API.BillingTransaction>>()), Times.Exactly(2));
			processorLoggerMock.Verify(x => x.Info("Start processing billing transactions"), Times.Once);
			processorLoggerMock.Verify(x => x.InfoFormat("{0} running in process {1}", "TestBillingJob-1", It.IsAny<int>()), Times.Once);
			processorLoggerMock.Verify(x => x.InfoFormat("{0} stopping in process {1}", "TestBillingJob-1", It.IsAny<int>()), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", transactions[0]), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", transactions[1]), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Batch transaction processed for {0} transactions", transactions.Count), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Batch transaction processed for {0} transactions", 0), Times.AtLeast(1));
			billingRepoMock.Verify(x => x.AddRange(It.Is<List<API.BillingTransaction>>(y => y.SequenceEqual(transactions.Take(3)))), Times.Once);
			jsonBillingProducerMock.Verify(x => x.Produce(It.Is<string>(s => s.Equals("billed-topic")), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()), Times.Exactly(2));
		}

		[Test]
		public void TestBillingTransactionsHangfire_Retry_ReachedMaxRetries()
		{
			Task consumerTask = null;

			var tokenSource = new CancellationTokenSource();
			var capturedTransactions = new List<API.BillingTransaction>();

			var kafkaConsumerMock = new Mock<IConsumer<Ignore, API.BillingTransaction>>();
			kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>())).Returns(() =>
			{
				if (messages.Count > 0)
				{
					var bytesResult = new ReadOnlySpan<byte>(messages.First());
					var transaction = new CargoWise.Billing.Kafka.API.BillingTransactionsDeserializer().Deserialize(bytesResult, false, new Confluent.Kafka.SerializationContext());
					var messageResult = new Message<Ignore, API.BillingTransaction>() { Value = transaction };
					var consumeResult = new ConsumeResult<Ignore, API.BillingTransaction> { Message = messageResult };
					messages.Remove(messages.First(message => BillingTransactionProtoBufSerializerForTest.DeserializeTransaction(message).ClientNumber == consumeResult.Message.Value.ClientNumber));
					capturedTransactions.Add(transaction);
					return consumeResult;
				}

				return null;
			});

			kafkaConsumerMock.Setup(x => x.Commit(It.IsAny<ConsumeResult<Ignore, API.BillingTransaction>>()))
				.Callback<ConsumeResult<Ignore, API.BillingTransaction>>(x =>
				{
					capturedTransactions.Add(BillingTransactionProtoBufSerializerForTest.DeserializeTransaction(messages.First()));
				});
			var billingConsumerMock = new Mock<BillingTransactionsConsumer>("TestBillingJob-1");
			billingConsumerMock.Setup(x => x.BuildBillingConsumer("TestBillingJob-1")).Returns(kafkaConsumerMock.Object);
			billingConsumerMock.Setup(x => x.ConsumerConfiguration).Returns(new ConsumerConfig()
			{
				EnableAutoCommit = false,
				EnableAutoOffsetStore = false
			});

			var billingRepoMock = new Mock<IBillingRepository>();
			var exception = new Exception("Cannot connect to SQL server");
			billingRepoMock.Setup(x => x.AddRange(It.IsAny<IEnumerable<API.BillingTransaction>>()))
				.Callback<IEnumerable<API.BillingTransaction>>(transactions =>
				{
					foreach (var message in capturedTransactions)
					{
						messages.Insert(0, BillingTransactionProtoBufSerializerForTest.SerializeTransaction(message));
					}
					capturedTransactions.Clear();
				})
				.Throws(exception);

			var kafkaClientMock = BillingServiceTest.CreateKafkaClient(jsonBillingProducerMock, toELKTrackingIDs, null, null);
			var handlerMock = new Mock<BillingHandler>(kafkaClientMock.Object, processorLoggerMock.Object, configurationMock.Object);
			handlerMock.Setup(x => x.CreateRepository()).Returns(billingRepoMock.Object);
			var billingProcessor = GetBillingTransactionProcessorMock(configurationMock, handlerMock, billingConsumerMock, processorLoggerMock);

			jobWrapperMock.Setup(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default")).Callback(billingJobManager.EnqueueOrDeleteJobs);
			jobWrapperMock.Setup(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>())).Returns(() =>
			{
				consumerTask = Task.Run(() => billingProcessor.Object.StartProcess(tokenSource.Token, null));
				return billingProcessor.Object.Name;
			});
			jobWrapperMock.Setup(x => x.TriggerRecurringJob("BillingJobManager"));
			billingJobManager.ResetJobs();

			var ex = Assert.Throws<AggregateException>(() => consumerTask?.Wait());
			Assert.AreEqual("Cannot connect to SQL server", ex?.InnerExceptions.First().Message);

			kafkaConsumerMock.Verify(x => x.Consume(It.IsAny<CancellationToken>()), Times.Exactly(12));
			kafkaConsumerMock.Verify(x => x.Commit(It.IsAny<IEnumerable<TopicPartitionOffset>>()), Times.Never);
			kafkaConsumerMock.Verify(x => x.Subscribe("billing-topic"), Times.Exactly(4));
			processorLoggerMock.Verify(x => x.Info("Start processing billing transactions"), Times.Once);
			processorLoggerMock.Verify(x => x.Error("Reached maximum retries. Stop processing."), Times.Once);
			processorLoggerMock.Verify(x => x.ErrorFormat("Error processing billing transactions. Retry", exception), Times.Exactly(4));
		}

		[Test]
		public void TestBillingTransactionsHangfire_Retry_ResourceRecover()
		{
			var messageOffset = 1;
			var call = 0;
			Task consumerTask = null;

			var tokenSource = new CancellationTokenSource();
			var topicParition = new TopicPartition("billing-topic", new Partition(0));
			List<API.BillingTransaction> transactions = new List<API.BillingTransaction>();
			var capturedTransactions = new List<API.BillingTransaction>();

			var kafkaConsumerMock = new Mock<IConsumer<Ignore, API.BillingTransaction>>();
			kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>())).Returns(() =>
			{
				if (messages.Count > 0)
				{
					var bytesResult = new ReadOnlySpan<byte>(messages.First());
					var transaction = new CargoWise.Billing.Kafka.API.BillingTransactionsDeserializer().Deserialize(bytesResult, false, new Confluent.Kafka.SerializationContext());
					var messageResult = new Message<Ignore, API.BillingTransaction>() { Value = transaction };
					var consumeResult = new ConsumeResult<Ignore, API.BillingTransaction>
					{
						Message = messageResult,
						TopicPartitionOffset = new TopicPartitionOffset(topicParition, new Offset(messageOffset++))
					};
					messages.Remove(messages.First(message => BillingTransactionProtoBufSerializerForTest.DeserializeTransaction(message).ClientNumber == consumeResult.Message.Value.ClientNumber));
					capturedTransactions.Add(transaction);
					return consumeResult;
				}

				return null;
			});

			kafkaConsumerMock.Setup(x => x.Commit(It.IsAny<ConsumeResult<Ignore, API.BillingTransaction>>()));
			kafkaConsumerMock.Setup(x => x.GetWatermarkOffsets(topicParition)).Returns(new WatermarkOffsets(new Offset(0), new Offset(messages.Count)));

			var billingConsumerMock = new Mock<BillingTransactionsConsumer>("BillingConsumer-1");
			billingConsumerMock.Setup(x => x.BuildBillingConsumer("BillingConsumer-1")).Returns(kafkaConsumerMock.Object);
			billingConsumerMock.Setup(x => x.ConsumerConfiguration).Returns(new ConsumerConfig()
			{
				EnableAutoCommit = false,
				EnableAutoOffsetStore = false
			});

			var exception = new Exception("Cannot connect to SQL server.");
			var billingRepoMock = new Mock<IBillingRepository>();
			billingRepoMock.Setup(x => x.AddRange(It.IsAny<IEnumerable<API.BillingTransaction>>())).Callback<IEnumerable<API.BillingTransaction>>((transactionsList) =>
			{
				var billingTransactions = transactionsList.ToList();
				BillingTransactionValidator.ValidateTransactions(billingTransactions);
				call++;
				if (call == 1)
				{
					foreach (var transaction in capturedTransactions)
					{
						messages.Add(BillingTransactionProtoBufSerializerForTest.SerializeTransaction(transaction));
					}
					throw exception;
				}
				transactions.AddRange(billingTransactions);
			});

			var kafkaClientMock = BillingServiceTest.CreateKafkaClient(jsonBillingProducerMock, toELKTrackingIDs, null, null);
			var handlerMock = new Mock<BillingHandler>(kafkaClientMock.Object, processorLoggerMock.Object, configurationMock.Object);
			handlerMock.Setup(x => x.CreateRepository()).Returns(billingRepoMock.Object);
			var billingProcessor = GetBillingTransactionProcessorMock(configurationMock, handlerMock, billingConsumerMock, processorLoggerMock);

			jobWrapperMock.Setup(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default")).Callback(billingJobManager.EnqueueOrDeleteJobs);
			jobWrapperMock.Setup(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>())).Returns(() =>
			{
				consumerTask = Task.Run(() => billingProcessor.Object.StartProcess(tokenSource.Token, null));
				return billingProcessor.Object.Name;
			});
			jobWrapperMock.Setup(x => x.TriggerRecurringJob("BillingJobManager"));
			billingJobManager.ResetJobs();

			while (messages.Count > 0)
				Task.Delay(10).Wait();

			tokenSource.Cancel();
			consumerTask?.Wait();

			Assert.AreEqual(3, transactions.Count);
			Assert.AreEqual("0", transactions[0].ClientNumber);
			Assert.AreEqual("1", transactions[1].ClientNumber);
			Assert.AreEqual("2", transactions[2].ClientNumber);

			processorLoggerMock.Verify(x => x.Info("Start processing billing transactions"), Times.Once);

			processorLoggerMock.Verify(x => x.InfoFormat("{0} running in process {1}", "TestBillingJob-1", It.IsAny<int>()), Times.Once);
			processorLoggerMock.Verify(x => x.InfoFormat("{0} stopping in process {1}", "TestBillingJob-1", It.IsAny<int>()), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", It.IsAny<object>()), Times.Exactly(7));
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", transactions[0]), Times.Exactly(2));
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", transactions[1]), Times.Exactly(2));
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", transactions[2]), Times.Exactly(2));
			kafkaConsumerMock.Verify(x => x.Subscribe("billing-topic"), Times.Exactly(2));
			kafkaConsumerMock.Verify(x => x.StoreOffset(It.IsAny<ConsumeResult<Ignore, API.BillingTransaction>>()), Times.Exactly(4));
			billingRepoMock.Verify(x => x.AddRange(It.Is<List<API.BillingTransaction>>(y => y.SequenceEqual(transactions.Take(3)))), Times.Once);
			processorLoggerMock.Verify(x => x.ErrorFormat("Error processing billing transactions. Retry", exception), Times.Once);
		}

		[Test]
		public void TestBillingTransactionsHangfire_CancellationRequest()
		{
			Task consumerTask = null;

			var tokenSource = new CancellationTokenSource();
			List<API.BillingTransaction> transactions = new List<API.BillingTransaction>();

			var consumerLoggerMock = new Mock<ILog>();
			consumerLoggerMock.Setup(x => x.TraceFormat(It.IsAny<string>(), It.IsAny<object[]>()));

			var kafkaConsumerMock = new Mock<IConsumer<Ignore, API.BillingTransaction>>();
			var billingConsumerMock = new Mock<BillingTransactionsConsumer>("TestBillingJob-1");
			kafkaConsumerMock.Setup(x => x.Subscribe(It.IsAny<string>()));
			kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>())).Callback<CancellationToken>(token =>
			{
				while (true)
				{
					token.ThrowIfCancellationRequested();
				}
			});
			billingConsumerMock.Setup(x => x.BuildBillingConsumer("TestBillingJob-1")).Returns(kafkaConsumerMock.Object);
			billingConsumerMock.Setup(x => x.ConsumerConfiguration).Returns(new ConsumerConfig()
			{
				EnableAutoCommit = false,
				EnableAutoOffsetStore = false
			});
			BillingTransactionsConsumer.Logger = consumerLoggerMock.Object;

			var billingRepoMock = new Mock<IBillingRepository>();

			var kafkaClientMock = BillingServiceTest.CreateKafkaClient(jsonBillingProducerMock, toELKTrackingIDs, null, null);
			var handlerMock = new Mock<BillingHandler>(kafkaClientMock.Object, processorLoggerMock.Object, configurationMock.Object);
			handlerMock.Setup(x => x.CreateRepository()).Returns(billingRepoMock.Object);
			var billingProcessor = GetBillingTransactionProcessorMock(configurationMock, handlerMock, billingConsumerMock, processorLoggerMock);

			jobWrapperMock.Setup(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default")).Callback(billingJobManager.EnqueueOrDeleteJobs);
			jobWrapperMock.Setup(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>())).Returns(() =>
			{
				consumerTask = Task.Run(() => billingProcessor.Object.StartProcess(tokenSource.Token, null));
				return billingProcessor.Object.Name;
			});
			jobWrapperMock.Setup(x => x.TriggerRecurringJob("BillingJobManager"));
			billingJobManager.ResetJobs();
			Task.Delay(2000).Wait();

			tokenSource.Cancel();
			consumerTask?.Wait();

			Assert.AreEqual(0, transactions.Count);

			jobWrapperMock.Verify(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default"), Times.Once);
			jobWrapperMock.Verify(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>()), Times.Once);
			jobWrapperMock.Verify(x => x.TriggerRecurringJob("BillingJobManager"), Times.Once);
			jobWrapperMock.Verify(x => x.DeleteBackgroundJob(It.IsAny<string>()), Times.Never);
			kafkaConsumerMock.Verify(x => x.Subscribe("billing-topic"), Times.Once);
			kafkaConsumerMock.Verify(x => x.Commit(It.IsAny<IEnumerable<TopicPartitionOffset>>()), Times.Never);
			processorLoggerMock.Verify(x => x.Info("Start processing billing transactions"), Times.Once);
			processorLoggerMock.Verify(x => x.InfoFormat("{0} running in process {1}", "TestBillingJob-1", It.IsAny<int>()), Times.Once);
			processorLoggerMock.Verify(x => x.InfoFormat("{0} stopping in process {1}", "TestBillingJob-1", It.IsAny<int>()), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", It.IsAny<object[]>()), Times.Never);
			consumerLoggerMock.Verify(x => x.TraceFormat("Cancellation requested"), Times.Once);
			billingRepoMock.Verify(x => x.AddRange(It.Is<List<API.BillingTransaction>>(y => y.SequenceEqual(transactions.Take(3)))), Times.Never);
		}

		[Test]
		public void TestBillingTransactionsHangfire_ProcessingEnqueuedJobs_EqualConsumerCount()
		{
			processingJobList.Add(new KeyValuePair<string, ProcessingJobDto>("BillingConsuming1", new ProcessingJobDto { Job = CreateHangfireJob() }));
			processingJobList.Add(new KeyValuePair<string, ProcessingJobDto>("BillingConsuming2", new ProcessingJobDto { Job = CreateHangfireJob() }));
			processingJobList.Add(new KeyValuePair<string, ProcessingJobDto>("BillingConsuming3", new ProcessingJobDto { Job = CreateHangfireJob() }));
			partitionMetadata.Add(new PartitionMetadata(1, 1, null, null, new Error(ErrorCode.NoError)));
			partitionMetadata.Add(new PartitionMetadata(2, 1, null, null, new Error(ErrorCode.NoError)));

			jobWrapperMock.Setup(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default")).Callback(billingJobManager.EnqueueOrDeleteJobs);
			jobWrapperMock.Setup(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action>>()));
			jobWrapperMock.Setup(x => x.TriggerRecurringJob("BillingJobManager"));
			billingJobManager.ResetJobs();

			jobWrapperMock.Verify(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default"), Times.Once());
			jobWrapperMock.Verify(x => x.TriggerRecurringJob("BillingJobManager"), Times.Once());
			jobWrapperMock.Verify(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action>>()), Times.Never);
		}

		[Test]
		public void TestBillingTransactionsHangfire_ProcessingEnqueuedJobs_LessThanConsumerCount()
		{
			var processingJobCount = 2;
			for (int i = 1; i <= processingJobCount; i++)
			{
				var jobId = $"TestBillingJob{i}";
				processingJobList.Add(new KeyValuePair<string, ProcessingJobDto>(jobId, new ProcessingJobDto { Job = CreateHangfireJob() }));
			}

			partitionMetadata.Add(new PartitionMetadata(1, 1, null, null, new Error(ErrorCode.NoError)));
			partitionMetadata.Add(new PartitionMetadata(2, 1, null, null, new Error(ErrorCode.NoError)));
			partitionMetadata.Add(new PartitionMetadata(3, 1, null, null, new Error(ErrorCode.NoError)));

			jobWrapperMock.Setup(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default")).Callback(billingJobManager.EnqueueOrDeleteJobs);
			jobWrapperMock.Setup(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>())).Returns(() => $"TestBillingJob{++processingJobCount}");
			jobWrapperMock.Setup(x => x.TriggerRecurringJob("BillingJobManager"));
			billingJobManager.ResetJobs();

			jobWrapperMock.Verify(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default"), Times.Once());
			jobWrapperMock.Verify(x => x.TriggerRecurringJob("BillingJobManager"), Times.Once());
			jobWrapperMock.Verify(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>()), Times.Exactly(2));
		}

		[Test]
		public void TestBillingTransactionsHangfire_ProcessingEnqueuedJobs_GreaterThanConsumerCount()
		{
			var processingJobCount = 4;
			for (int i = processingJobCount; i >= 1; i--)
			{
				var jobId = $"TestBillingJob{i}";
				processingJobList.Add(new KeyValuePair<string, ProcessingJobDto>(jobId, new ProcessingJobDto { Job = CreateHangfireJob() }));
			}

			partitionMetadata.Add(new PartitionMetadata(1, 1, null, null, new Error(ErrorCode.NoError)));

			jobWrapperMock.Setup(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default")).Callback(billingJobManager.EnqueueOrDeleteJobs);
			jobWrapperMock.Setup(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>()));
			jobWrapperMock.Setup(x => x.TriggerRecurringJob("BillingJobManager"));
			jobWrapperMock.Setup(x => x.DeleteBackgroundJob("TestBillingJob2"));
			jobWrapperMock.Setup(x => x.DeleteBackgroundJob("TestBillingJob1"));
			billingJobManager.ResetJobs();

			jobWrapperMock.Verify(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default"), Times.Once());
			jobWrapperMock.Verify(x => x.TriggerRecurringJob("BillingJobManager"), Times.Once());
			jobWrapperMock.Verify(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action>>()), Times.Never);
			jobWrapperMock.Verify(x => x.DeleteBackgroundJob("TestBillingJob2"), Times.Once);
			jobWrapperMock.Verify(x => x.DeleteBackgroundJob("TestBillingJob1"), Times.Once);
		}

		[Test]
		public void TestBillingTransactionsHangfire_StopProcessingOnInvalidData()
		{
			var messageOffset = 1;
			Task consumerTask = null;

			var tokenSource = new CancellationTokenSource();
			var topicParition = new TopicPartition("billing-topic", new Partition(0));
			List<API.BillingTransaction> transactions = new List<API.BillingTransaction>();

			var maxRetries = 3;
			int count = 0;
			var exceptions = new List<Exception>();
			var capturedTransactions = new List<API.BillingTransaction>();
			messages.Add(Encoding.UTF8.GetBytes("Invalid data content"));
			var kafkaConsumerMock = new Mock<IConsumer<Ignore, API.BillingTransaction>>();
			kafkaConsumerMock.Setup(x => x.Subscribe(It.IsAny<string>()));
			kafkaConsumerMock.Setup(x => x.Consume(It.IsAny<CancellationToken>())).Returns(() =>
			{
				if (messages.Count > 0)
				{
					try
					{
						var bytesResult = new ReadOnlySpan<byte>(messages.First());
						var transaction = new IssuerReporterDeserializerProxy<API.BillingTransaction>(new CargoWise.Billing.Kafka.API.BillingTransactionsDeserializer()).Deserialize(bytesResult, false, new Confluent.Kafka.SerializationContext());
						var messageResult = new Message<Ignore, API.BillingTransaction> { Value = transaction };
						var consumeResult = new ConsumeResult<Ignore, API.BillingTransaction> { Message = messageResult, TopicPartitionOffset = new TopicPartitionOffset(topicParition, new Offset(messageOffset++)) };
						if (consumeResult.Message.Value != null)
						{
							messages.Remove(messages.First(message => BillingTransactionProtoBufSerializerForTest.DeserializeTransaction(message).ClientNumber == consumeResult.Message.Value.ClientNumber));
						}
						else
						{
							messages.Remove(messages.First());
						}
						capturedTransactions.Add(transaction);
						return consumeResult;
					}
					catch (ProtoException ex)
					{
						if (count >= maxRetries)
						{
							messages.Remove(messages.First());
						}
						exceptions.Add(ex);
						count++;
						throw;
					}
				}
				return null;
			});

			kafkaConsumerMock.Setup(x => x.Commit(It.IsAny<ConsumeResult<Ignore, API.BillingTransaction>>()));
			kafkaConsumerMock.Setup(x => x.GetWatermarkOffsets(topicParition)).Returns(new WatermarkOffsets(new Offset(0), new Offset(messages.Count)));

			var billingConsumerMock = new Mock<BillingTransactionsConsumer>("TestBillingJob-1");
			billingConsumerMock.Setup(x => x.BuildBillingConsumer("TestBillingJob-1")).Returns(kafkaConsumerMock.Object);
			billingConsumerMock.Setup(x => x.ConsumerConfiguration).Returns(new ConsumerConfig()
			{
				EnableAutoCommit = false,
				EnableAutoOffsetStore = false
			});
			var billingRepoMock = new Mock<IBillingRepository>();
			billingRepoMock.Setup(x => x.AddRange(It.IsAny<IEnumerable<API.BillingTransaction>>())).Callback<IEnumerable<API.BillingTransaction>>((transactionsList) =>
			{
				var billingTransactions = transactionsList.ToList();
				BillingTransactionValidator.ValidateTransactions(billingTransactions);
				transactions.AddRange(billingTransactions);
			});

			var kafkaClientMock = BillingServiceTest.CreateKafkaClient(jsonBillingProducerMock, toELKTrackingIDs, null, null);
			var handlerMock = new Mock<BillingHandler>(kafkaClientMock.Object, processorLoggerMock.Object, configurationMock.Object);
			handlerMock.Setup(x => x.CreateRepository()).Returns(billingRepoMock.Object);
			var billingProcessor = GetBillingTransactionProcessorMock(configurationMock, handlerMock, billingConsumerMock, processorLoggerMock);

			jobWrapperMock.Setup(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default")).Callback(billingJobManager.EnqueueOrDeleteJobs);
			jobWrapperMock.Setup(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>())).Returns(() =>
			{
				consumerTask = Task.Run(() => billingProcessor.Object.StartProcess(tokenSource.Token, null));
				return billingProcessor.Object.Name;
			});
			jobWrapperMock.Setup(x => x.TriggerRecurringJob("BillingJobManager"));
			billingJobManager.ResetJobs();

			while (messages.Count > 0)
			{
				Task.Delay(10).Wait();
			}

			var jobEx = Assert.Throws<AggregateException>(() => consumerTask?.Wait());
			Assert.That(jobEx?.InnerExceptions.First().Message, Does.StartWith("Deserialization error occurred during proto buffer processing."));

			Assert.AreEqual(3, transactions.Count);
			Assert.AreEqual("0", transactions[0].ClientNumber);
			Assert.AreEqual("1", transactions[1].ClientNumber);
			Assert.AreEqual("2", transactions[2].ClientNumber);
			Assert.AreEqual(2, toELKTrackingIDs.Count);
			Assert.AreEqual("00000000-0000-0000-0000-000000000000", toELKTrackingIDs[0]);
			Assert.AreEqual("11111111-1111-1111-1111-111111111111", toELKTrackingIDs[1]);

			jobWrapperMock.Verify(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default"), Times.Once);
			jobWrapperMock.Verify(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>()), Times.Once);
			jobWrapperMock.Verify(x => x.TriggerRecurringJob("BillingJobManager"), Times.Once);
			jobWrapperMock.Verify(x => x.DeleteBackgroundJob(It.IsAny<string>()), Times.Never);
			kafkaConsumerMock.Verify(x => x.Subscribe("billing-topic"), Times.Exactly(4));
			kafkaConsumerMock.Verify(x => x.StoreOffset(It.IsAny<ConsumeResult<Ignore, API.BillingTransaction>>()), Times.Exactly(3));

			processorLoggerMock.Verify(x => x.InfoFormat("{0} running in process {1}", "TestBillingJob-1", It.IsAny<int>()), Times.Once);
			Assert.AreEqual(4, exceptions.Count);
			processorLoggerMock.Verify(x => x.ErrorFormat("Error processing billing transactions. Retry", exceptions[0]), Times.Once);
			processorLoggerMock.Verify(x => x.ErrorFormat("Error processing billing transactions. Retry", exceptions[1]), Times.Once);
			processorLoggerMock.Verify(x => x.ErrorFormat("Error processing billing transactions. Retry", exceptions[2]), Times.Once);
			processorLoggerMock.Verify(x => x.ErrorFormat("Error processing billing transactions. Retry", exceptions[3]), Times.Once);
			processorLoggerMock.Verify(x => x.Error("Reached maximum retries. Stop processing."), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", transactions[0]), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", transactions[1]), Times.Once);
			processorLoggerMock.Verify(x => x.TraceFormat("Adding transaction [{0}]", transactions[2]), Times.Once);
			billingRepoMock.Verify(x => x.AddRange(It.Is<List<API.BillingTransaction>>(y => y.SequenceEqual(transactions.Take(3)))), Times.Once);
			jsonBillingProducerMock.Verify(x => x.Produce(It.Is<string>(s => s.Equals("billed-topic")), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()), Times.Exactly(2));
		}

		[Test]
		public void TestBillingTransactionsHangfire_SkipHangfireJobReset()
		{
			configurationMock.Setup(x => x.SkipHangfireJobReset).Returns(true);

			var processingJobCount = 2;
			for (int i = 1; i <= processingJobCount; i++)
			{
				var jobId = $"TestBillingJob{i}";
				processingJobList.Add(new KeyValuePair<string, ProcessingJobDto>(jobId, new ProcessingJobDto { Job = CreateHangfireJob() }));
			}

			partitionMetadata.Add(new PartitionMetadata(1, 1, null, null, new Error(ErrorCode.NoError)));
			partitionMetadata.Add(new PartitionMetadata(2, 1, null, null, new Error(ErrorCode.NoError)));
			partitionMetadata.Add(new PartitionMetadata(3, 1, null, null, new Error(ErrorCode.NoError)));

			jobWrapperMock.Setup(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default")).Callback(billingJobManager.EnqueueOrDeleteJobs);
			jobWrapperMock.Setup(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>())).Returns(() => $"TestBillingJob{++processingJobCount}");
			jobWrapperMock.Setup(x => x.TriggerRecurringJob("BillingJobManager"));
			billingJobManager.ResetJobs();

			jobWrapperMock.Verify(x => x.AddOrUpdateRecurringJob("BillingJobManager", It.IsAny<Expression<Action>>(), Cron.Minutely(), It.IsAny<TimeZoneInfo>(), "default"), Times.Never());
			jobWrapperMock.Verify(x => x.TriggerRecurringJob("BillingJobManager"), Times.Never());
			jobWrapperMock.Verify(x => x.EnqueueBackgroundJob(It.IsAny<Expression<Action<BillingServiceProcessor>>>()), Times.Never());
		}

		public Job CreateHangfireJob() => new Job(typeof(BillingServiceProcessor), typeof(BillingServiceProcessor).GetMethod("StartProcess"), CancellationToken.None, null, null);

		Mock<IConsumer<Ignore, API.BillingTransaction>> CreateConsumerMock(string jobId)
		{
			var kafkaConsumerMock = new Mock<IConsumer<Ignore, API.BillingTransaction>>();
			kafkaConsumerMock.Setup(x => x.MemberId).Returns($"TestBillingJob-{jobId}-{Guid.NewGuid()}");
			kafkaConsumerMock.Setup(x => x.Subscribe(It.IsAny<string>()));
			kafkaConsumerMock.Setup(x => x.Commit(It.IsAny<ConsumeResult<Ignore, API.BillingTransaction>>()));
			kafkaConsumerMock.Setup(x => x.GetWatermarkOffsets(It.IsAny<TopicPartition>())).Returns(new WatermarkOffsets(new Offset(0), new Offset(1000)));
			kafkaConsumerMock.Setup(x => x.Unsubscribe());
			kafkaConsumerMock.Setup(x => x.Close());

			return kafkaConsumerMock;
		}

		[SetUp]
		public void Init()
		{
			jobIds.Enqueue("1");
			var guids = new Queue<Guid>(new[]
			{
				new Guid("00000000-0000-0000-0000-000000000000"), new Guid("11111111-1111-1111-1111-111111111111"),
				new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333333")
			});
			toELKTrackingIDs = new List<string>();
			jsonBillingProducerMock = new Mock<IBillingTransactionProducer<string, string>>();
			processingJobList = new List<KeyValuePair<string, ProcessingJobDto>>();
			messages = new List<byte[]>();
			for (int i = 0; i < 4; i++)
			{
				var transaction = CreateBillingTransaction();
				transaction.ClientNumber = i.ToString();
				transaction.MessageTrackingID = guids.Dequeue().ToString();
				if (i == 2)
					transaction.PriceItemCode = "AAA";
				if (i == 3)
				{
					transaction.Category = "STL";
					transaction.PriceItemCode = "ZZZ";
				}
				messages.Add(BillingTransactionProtoBufSerializerForTest.SerializeTransaction(transaction));
			}

			monitoringApiMock = new Mock<IMonitoringApi>();
			monitoringApiMock.Setup(x => x.ProcessingCount()).Returns(() => processingJobList.Count);
			monitoringApiMock.Setup(x => x.ProcessingJobs(0, It.Is<int>(count => count == processingJobList.Count))).Returns(() => new JobList<ProcessingJobDto>(processingJobList));
			configurationMock = new Mock<IConfigurationProvider>();
			configurationMock.Setup(x => x.BillingKafkaTopic).Returns("billing-topic");
			configurationMock.Setup(x => x.MaxRetries).Returns(3);
			configurationMock.Setup(x => x.RetryTimeoutInMilliseconds).Returns(2000);
			configurationMock.Setup(x => x.BillingConnectionString).Returns("data source=localhost;initial catalog=CargoWise.eServices.Billing;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework");
			configurationMock.Setup(x => x.UsageELKKafkaTopic).Returns("usage-topic");
			configurationMock.Setup(x => x.BilledELKKafkaTopic).Returns("billed-topic");
			configurationMock.Setup(x => x.NonBilledCodes).Returns(new[] { "ZZZ" });
			configurationMock.Setup(x => x.SendToELKBillingCategories).Returns(new[] { "TST" });
			configurationMock.Setup(x => x.NotSendToELKBillingCodes).Returns(new[] { "AAA" });
			configurationMock.Setup(x => x.BillingKafkaBatchSize).Returns(3);
			configurationMock.Setup(x => x.ThrottlingThresholdOfStaging).Returns(Int32.MaxValue);
			configurationMock.Setup(x => x.SkipHangfireJobReset).Returns(false);

			partitionMetadata = new List<PartitionMetadata>()
			{
				new PartitionMetadata(0, 1, null, null, new Error(ErrorCode.NoError)),
			};
			var topicMetadata = new List<TopicMetadata>()
			{
				new TopicMetadata("billing-topic", partitionMetadata, new Error(ErrorCode.NoError))
			};
			adminClientMock = new Mock<IAdminClient>();
			adminClientMock.Setup(x => x.GetMetadata("billing-topic", TimeSpan.FromSeconds(5))).Returns(new Metadata(null, topicMetadata, 0, "0"));
			jobWrapperMock = new Mock<IHangfireJobWrapper>();

			billingJobManager = new BillingJobManager();
			var containerMock = new Mock<IWindsorContainer>();
			containerMock.Setup(x => x.Resolve<IConfigurationProvider>()).Returns(configurationMock.Object);
			containerMock.Setup(x => x.Resolve<IMonitoringApi>()).Returns(monitoringApiMock.Object);
			containerMock.Setup(x => x.Resolve<IAdminClient>()).Returns(adminClientMock.Object);
			containerMock.Setup(x => x.Resolve<IHangfireJobWrapper>()).Returns(jobWrapperMock.Object);
			containerMock.Setup(x => x.Resolve<IBillingJobManager>()).Returns(billingJobManager);
			Global.WindsorContainer = containerMock.Object;
			processorLoggerMock = new Mock<ILog>();
		}

		Mock<BillingServiceProcessor> GetBillingTransactionProcessorMock(Mock<IConfigurationProvider> configProviderMock, Mock<BillingHandler> handlerMock, Mock<BillingTransactionsConsumer> consumerMock, Mock<ILog> loggerMock)
		{
			var mock = new Mock<BillingServiceProcessor>() { CallBase = true };
			mock.Setup(x => x.Name).Returns("TestBillingJob");
			mock.Setup(x => x.GetBackgroundJobId(It.IsAny<PerformContext>())).Returns(jobIds.Dequeue());
			mock.Setup(x => x.Logger).Returns(loggerMock.Object);
			mock.Setup(x => x.GetBillingHandler(configProviderMock.Object)).Returns(handlerMock.Object);
			mock.Setup(x => x.GetConsumer(configProviderMock.Object)).Returns(consumerMock.Object);
			return mock;
		}

		List<byte[]> messages;
		private Mock<ILog> processorLoggerMock;
		private Mock<IConfigurationProvider> configurationMock;
		private Mock<IMonitoringApi> monitoringApiMock;
		private Mock<IAdminClient> adminClientMock;
		private Mock<IHangfireJobWrapper> jobWrapperMock;
		private IBillingJobManager billingJobManager;
		List<KeyValuePair<string, ProcessingJobDto>> processingJobList;
		List<PartitionMetadata> partitionMetadata;
		public static BillingTransaction CreateBillingTransaction()
		{
			return new BillingTransaction()
			{
				BillableCount = 2,
				ClientID = "WTLEDINPN",
				ClientNumber = "98765432100123456789",
				ClientStaffCode = "TST",
				PriceItemCode = "TST",
				Reference1 = "TEST",
				Reference2 = "TEST",
				Reference3 = "TEST",
				Reference4 = "TEST",
				ReportingSource = "HUB",
				Category = "TST",
				ServiceOccuredUTC = DateTime.UtcNow.AddYears(-4)
			};
		}

		List<string> toELKTrackingIDs;
		Queue<string> jobIds = new Queue<string>();
		private Mock<IBillingTransactionProducer<string, string>> jsonBillingProducerMock;
	}
}
