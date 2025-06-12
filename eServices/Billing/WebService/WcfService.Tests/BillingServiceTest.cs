using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using CargoWise.eServices.Billing.DataAccess;
using CargoWise.Billing.Kafka.API;
using Common.Logging;
using Confluent.Kafka;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using API = CargoWise.Billing.API;
using CargoWise.Billing.Tests.Common;
using System.Reflection;
using CargoWise.eServices.Billing.Tests.Common;
using CargoWise.Billing.Service;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	[TestFixture]
	public class BillingServiceTest
	{
		[SetUp]
		public void Setup()
		{
			configurationMock = new Mock<IConfigurationProvider>();
			configurationMock.Setup(x => x.UsageELKKafkaTopic).Returns("usage-topic");
			configurationMock.Setup(x => x.BilledELKKafkaTopic).Returns("billed-topic");
			configurationMock.Setup(x => x.NonBilledCodes).Returns(new[] { "ZZZ" });
			configurationMock.Setup(x => x.NotSendToELKBillingCodes).Returns(new[] { "AAA", "BBB" });
			configurationMock.Setup(x => x.SendToELKBillingCategories).Returns(new[] { "TST" });
			toELKTrackingIDs = new List<string>();
			jsonBillingProducerMock = new Mock<IBillingTransactionProducer<string, string>>();
		}

		[Test]
		public void TestAddTransaction()
		{
			var transactions = new List<API.BillingTransaction>();
			var mockLog = new Mock<ILog>();
			var mockRepository = new Mock<IBillingRepository>();
			mockRepository
				.Setup(_ => _.Add(It.IsAny<API.BillingTransaction>()))
				.Callback<API.BillingTransaction>(t => transactions.Add(t));

			var transaction1 = GenerateValidTransaction();
			var transaction2 = GenerateValidTransaction();
			transaction2.PriceItemCode = "ZZZ";
			transaction2.Category = "STL";
			var transaction3 = GenerateValidTransaction();
			transaction3.PriceItemCode = "AAA";
			var transaction4 = GenerateValidTransaction();
			transaction4.PriceItemCode = "BBB";
			var transaction5 = GenerateValidTransaction();
			transaction5.PriceItemCode = "ZZZ";

			var mockKafkaClient = CreateKafkaClient(jsonBillingProducerMock, toELKTrackingIDs, null, null);
			var mockHandler = new Mock<BillingHandler>(mockKafkaClient.Object, mockLog.Object, configurationMock.Object);
			mockHandler.Setup(x => x.CreateRepository()).Returns(mockRepository.Object);

			var service = new BillingService(mockHandler.Object);
			service.AddTransaction(transaction1);
			service.AddTransaction(transaction2);
			service.AddTransaction(transaction3);
			service.AddTransaction(transaction4);
			service.AddTransaction(transaction5);

			Assert.That(transactions.Count, Is.EqualTo(4));
			ObjectComparer.AssertPropertiesAreEqual(transaction1, transactions[0]);
			ObjectComparer.AssertPropertiesAreEqual(transaction3, transactions[1]);
			ObjectComparer.AssertPropertiesAreEqual(transaction4, transactions[2]);
			ObjectComparer.AssertPropertiesAreEqual(transaction5, transactions[3]);
			Assert.That(toELKTrackingIDs.Contains(transaction1.MessageTrackingID) && toELKTrackingIDs.Contains(transaction2.MessageTrackingID) && toELKTrackingIDs.Contains(transaction5.MessageTrackingID));
			jsonBillingProducerMock.Verify(x => x.Produce(It.Is<string>(s => s.Equals("billed-topic")), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()), Times.Exactly(3));
		}

		[Test]
		public void TestAddUsageTransaction()
		{
			string actualTransaction = "";
			var mockLog = new Mock<ILog>();
			var mockRepository = new Mock<IBillingRepository>();
			mockRepository
				.Setup(_ => _.InsertELKResubmitTransaction(It.IsAny<string>()))
				.Callback<string>(t => actualTransaction = t);

			var transaction = GenerateUsageTransaction(occurredUtc:DateTimeOffset.Parse("2024-01-01T00:00:00").DateTime);

			var mockKafkaClient = CreateKafkaClient(jsonBillingProducerMock, toELKTrackingIDs, null, null);
			var mockHandler = new Mock<BillingHandler>(mockKafkaClient.Object, mockLog.Object, configurationMock.Object);
			mockHandler.Setup(x => x.CreateRepository()).Returns(mockRepository.Object);

			var service = new BillingService(mockHandler.Object);
			service.AddUsageTransaction(transaction);

			var expected = "{\"@timestamp\":\"2024-01-01T00:00:00Z\",\"UsageCount\":2,\"ServiceOccured\":\"2024-01-01T00:00:00Z\",\"EnterpriseCode\":\"TST\",\"ServerCode\":\"TST\",\"Environment\":\"TST\",\"CompanyCode\":\"ABC\",\"CompanyName\":\"Test\",\"BranchCode\":\"KLM\",\"UsageCode\":\"USG\"}";
			Assert.That(actualTransaction, Is.EqualTo(expected));
		}

		[Test]
		public void TestAddTransactionValidationFail()
		{
			const string validationFailedMessage = "Transaction validation failed.";
			var validationErrors = new[] { "First validation error", "Second validation error" };
			var mockLog = new Mock<ILog>();
			var mockRepository = new Mock<IBillingRepository>();
			mockRepository
				.Setup(_ => _.Add(It.IsAny<API.BillingTransaction>()))
				.Throws(new API.ValidationException(validationFailedMessage, validationErrors));

			var mockKafkaClient = CreateKafkaClient(jsonBillingProducerMock, toELKTrackingIDs, null, null);
			var mockHandler = new Mock<BillingHandler>(mockKafkaClient.Object, mockLog.Object, configurationMock.Object);
			mockHandler.Setup(x => x.CreateRepository()).Returns(mockRepository.Object);

			var service = new BillingService(mockHandler.Object);
			try
			{
				service.AddTransaction(new BillingTransaction());
				Assert.Fail("FaultException must be thrown.");
			}
			catch (FaultException<ValidationFault> e)
			{
				Assert.That(e.Message, Is.EqualTo(validationFailedMessage));
				Assert.That(e.Detail.Errors, Is.EqualTo(validationErrors));
			}
		}

		[Test]
		public void TestAddTransactionRange()
		{
			configurationMock.Setup(x => x.SendToELKBillingCategories).Returns(new[] { "ALL" });
			var transactions = new List<API.BillingTransaction>();
			var toSend = new List<BillingTransaction>();
			var mockLog = new Mock<ILog>();
			var mockRepository = new Mock<IBillingRepository>();
			mockRepository
				.Setup(_ => _.AddRange(It.IsAny<IEnumerable<API.BillingTransaction>>()))
				.Callback<IEnumerable<API.BillingTransaction>>(t => transactions.AddRange(t));

			for (int i = 0; i < 10; i++)
			{
				toSend.Add(GenerateValidTransaction());
			}
			toSend[1].Category = "STL";
			toSend[1].PriceItemCode = "ZZZ";
			toSend[2].PriceItemCode = "AAA";
			toSend[3].PriceItemCode = "BBB";

			var mockKafkaClient = CreateKafkaClient(jsonBillingProducerMock, toELKTrackingIDs, null, null);
			var mockHandler = new Mock<BillingHandler>(mockKafkaClient.Object, mockLog.Object, configurationMock.Object);
			mockHandler.Setup(x => x.CreateRepository()).Returns(mockRepository.Object);

			var service = new BillingService(mockHandler.Object);
			service.AddTransactionRange(toSend.ToArray());

			Assert.That(toELKTrackingIDs.Contains(toSend[0].MessageTrackingID) && toELKTrackingIDs.Contains(toSend[1].MessageTrackingID));
			Assert.That(transactions.Count, Is.EqualTo(9));
			jsonBillingProducerMock.Verify(x => x.Produce(It.Is<string>(s => s.Equals("billed-topic")), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()), Times.Exactly(8));

			toSend.RemoveAt(1);
			for (int i = 0; i < 9; ++i)
			{
				ObjectComparer.AssertPropertiesAreEqual(toSend[i], transactions[i]);
			}
		}

		[Test]
		public void TestAddUsageTransactionRange()
		{
			var transactions = new List<string>();
			var toSend = new List<UsageTransaction>();
			var mockLog = new Mock<ILog>();
			var mockRepository = new Mock<IBillingRepository>();
			mockRepository
				.Setup(_ => _.InsertELKResubmitTransactions(It.IsAny<IEnumerable<string>>()))
				.Callback<IEnumerable<string>>(t => transactions.AddRange(t));

			for (int i = 0; i < 10; ++i)
			{
				toSend.Add(GenerateUsageTransaction($"Test{i}", DateTimeOffset.Parse("2024-01-01T00:00:00").DateTime));
			}

			var mockKafkaClient = CreateKafkaClient(jsonBillingProducerMock, toELKTrackingIDs, null, null);
			var mockHandler = new Mock<BillingHandler>(mockKafkaClient.Object, mockLog.Object, configurationMock.Object);
			mockHandler.Setup(x => x.CreateRepository()).Returns(mockRepository.Object);

			var service = new BillingService(mockHandler.Object);
			service.AddUsageTransactionRange(toSend.ToArray());

			Assert.That(transactions.Count, Is.EqualTo(10));
			var expected = "{{\"@timestamp\":\"2024-01-01T00:00:00Z\",\"UsageCount\":2,\"ServiceOccured\":\"2024-01-01T00:00:00Z\",\"EnterpriseCode\":\"TST\",\"ServerCode\":\"TST\",\"Environment\":\"TST\",\"CompanyCode\":\"ABC\",\"CompanyName\":\"Test{0}\",\"BranchCode\":\"KLM\",\"UsageCode\":\"USG\"}}";

			Assert.Multiple(() =>
			{
				for (int i = 0; i < 10; ++i)
				{
					Assert.That(transactions[i], Is.EqualTo(string.Format(expected, i)));
				}
			});
		}

		[Test]
		public void TestAddTransactionRangeValidationFail()
		{
			const string validationFailedMessage = "Transaction validation failed.";
			var validationErrors = new[] { "First validation error", "Second validation error" };
			var mockLog = new Mock<ILog>();
			var mockRepository = new Mock<IBillingRepository>();
			mockRepository
				.Setup(_ => _.AddRange(It.IsAny<IEnumerable<API.BillingTransaction>>()))
				.Throws(new API.ValidationException(validationFailedMessage, validationErrors));

			const int SendCount = 10;
			var toSend = new List<BillingTransaction>(SendCount);
			for (int i = 0; i < SendCount; ++i)
			{
				toSend.Add(GenerateValidTransaction());
			}

			var mockKafkaClient = CreateKafkaClient(jsonBillingProducerMock, toELKTrackingIDs, null, null);
			var mockHandler = new Mock<BillingHandler>(mockKafkaClient.Object, mockLog.Object, configurationMock.Object);
			mockHandler.Setup(x => x.CreateRepository()).Returns(mockRepository.Object);

			var service = new BillingService(mockHandler.Object);
			try
			{
				service.AddTransactionRange(toSend.ToArray());
				Assert.Fail("FaultException must be thrown.");
			}
			catch (FaultException<ValidationFault> e)
			{
				Assert.That(e.Message, Is.EqualTo(validationFailedMessage));
				Assert.That(e.Detail.Errors, Is.EqualTo(validationErrors));
			}
		}

		[Test]
		public void TestAddTransaction_BillingUsageJson()
		{
			var billingDateTime = new DateTime(DateTime.Now.Year, 12, 1, 11, 0, 0, DateTimeKind.Utc).AddYears(-1);
			Dictionary<string, string> expectedOutputs = new Dictionary<string, string>();
			var expectedDateTime = $"{billingDateTime.Year}-12-01T11:00:00Z";
			var guids = new Queue<Guid>(new[]
			{
					new Guid("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"), new Guid("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB"),
					new Guid("CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC")
				});

			var toSend = new List<BillingTransaction>();
			var mockLog = new Mock<ILog>();
			var type = typeof(BillingServiceTest);

			for (int i = 0; i < 3; i++)
			{
				var transaction = GenerateValidTransaction();
				transaction.Category = "STL";
				transaction.PriceItemCode = "ZZZ";
				transaction.MessageTrackingID = guids.Dequeue().ToString().ToUpper();
				transaction.ServiceOccuredUTC = billingDateTime;
				var expectedOutput = "";
				if (i == 0)
				{
					transaction.AdditionalRefs = TestHelper.GetEmbeddedResourceAsString("TestFiles.BillingUsage.NonBilledUsage_Input.txt", type);
					expectedOutput = JObject.Parse(TestHelper.GetEmbeddedResourceAsString("TestFiles.BillingUsage.NonBilledUsage_Output.txt", type)).ToString(Formatting.None);
				}
				else if (i == 1)
				{
					transaction.AdditionalRefs = TestHelper.GetEmbeddedResourceAsString("TestFiles.BillingUsage.NonBilledUsage_Input.txt", type);
					transaction.Reference2 = null;
					transaction.Reference3 = null;
					transaction.Reference4 = null;
					expectedOutput = JObject.Parse(TestHelper.GetEmbeddedResourceAsString("TestFiles.BillingUsage.NonBilledUsage_Output_NullValue.txt", type)).ToString(Formatting.None);
				}
				else
				{
					transaction.AdditionalRefs = "";
					transaction.Reference2 = "";
					transaction.Reference3 = "";
					transaction.Reference4 = "";
					transaction.Reference5 = "";
					transaction.ClientNumber = "";
					transaction.ClientStaffCode = "";
					expectedOutput = JObject.Parse(TestHelper.GetEmbeddedResourceAsString("TestFiles.BillingUsage.NonBilledUsage_Output_EmptyReference.txt", type)).ToString(Formatting.None);
				}
				toSend.Add(transaction);
				expectedOutputs.Add(transaction.MessageTrackingID, expectedOutput.Replace("[TRANSACTION_DATETIME]", expectedDateTime));
			}

			var mockKafkaClient = CreateKafkaClient(jsonBillingProducerMock, toELKTrackingIDs, null, expectedOutputs);
			var mockHandler = new Mock<BillingHandler>(mockKafkaClient.Object, mockLog.Object, configurationMock.Object);

			var service = new BillingService(mockHandler.Object);
			service.AddTransactionRange(toSend.ToArray());

			jsonBillingProducerMock.Verify(x => x.Produce(It.Is<string>(s => s.Equals("billed-topic")), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()), Times.Exactly(3));
		}

		[Test]
		public void TestBillingTransactionMessageV4_ErrorReport_Exception()
		{
			var transactions = new List<API.BillingTransaction>();
			var mockLog = new Mock<ILog>();
			var mockRepository = new Mock<IBillingRepository>();
			mockRepository
				.Setup(_ => _.Add(It.IsAny<API.BillingTransaction>()))
				.Callback<API.BillingTransaction>(t => transactions.Add(t));

			var transaction1 = GenerateValidTransaction();
			transaction1.PriceItemCode = "ZZZ";
			var deliveryErrors = new Dictionary<string, Error>() { { transaction1.MessageTrackingID, new Error(ErrorCode.Local_MsgTimedOut) } };
			var mockKafkaClient = CreateKafkaClient(jsonBillingProducerMock, toELKTrackingIDs, deliveryErrors, null);
			var mockHandler = new Mock<BillingHandler>(mockKafkaClient.Object, mockLog.Object, configurationMock.Object);
			mockHandler.Setup(x => x.CreateRepository()).Returns(mockRepository.Object);

			var service = new BillingService(mockHandler.Object);
			service.AddTransaction(transaction1);
			mockRepository.Verify(x => x.InsertELKResubmitTransaction(It.Is<string>(y => y.Contains(transaction1.MessageTrackingID))), Times.Once);
		}

		[Test]
		public void TestBillingTransactions_KafkaClientNull()
		{
			var transactions = new List<API.BillingTransaction>();
			var mockLog = new Mock<ILog>();
			var mockRepository = new Mock<IBillingRepository>();
			mockRepository
				.Setup(_ => _.Add(It.IsAny<API.BillingTransaction>()))
				.Callback<API.BillingTransaction>(t => transactions.Add(t));
			mockRepository
				.Setup(_ => _.AddRange(It.IsAny<IEnumerable<API.BillingTransaction>>()))
				.Callback<IEnumerable<API.BillingTransaction>>(t => transactions.AddRange(t));

			var transaction1 = GenerateValidTransaction();
			transaction1.PriceItemCode = "ZZZ";
			var mockHandler = new Mock<BillingHandler>(null, mockLog.Object, configurationMock.Object);
			mockHandler.Setup(x => x.CreateRepository()).Returns(mockRepository.Object);

			var service = new BillingService(mockHandler.Object);
			service.AddTransaction(transaction1);

			Assert.That(transactions.Count, Is.EqualTo(1));
			ObjectComparer.AssertPropertiesAreEqual(transaction1, transactions[0]);
			jsonBillingProducerMock.Verify(x => x.Produce(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()), Times.Never);
			mockRepository.Verify(x => x.InsertELKResubmitTransaction(It.Is<string>(y => y.Contains(transaction1.MessageTrackingID))), Times.Once);

			var transaction2 = GenerateValidTransaction();
			transaction2.PriceItemCode = "ZZZ";
			var transaction3 = GenerateValidTransaction();
			transaction3.PriceItemCode = "ZZZ";

			service.AddTransactionRange(new[]{transaction1, transaction2, transaction3});
			Assert.That(transactions.Count, Is.EqualTo(4));
			ObjectComparer.AssertPropertiesAreEqual(transaction1, transactions[0]);
			ObjectComparer.AssertPropertiesAreEqual(transaction1, transactions[1]);
			ObjectComparer.AssertPropertiesAreEqual(transaction2, transactions[2]);
			ObjectComparer.AssertPropertiesAreEqual(transaction3, transactions[3]);
			jsonBillingProducerMock.Verify(x => x.Produce(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()), Times.Never);
			Func<IEnumerable<string>, bool> isExpectedTransactions = ts =>
			{
				return ts.Any(t => t.Contains(transaction1.MessageTrackingID))
				       && ts.Any(t => t.Contains(transaction2.MessageTrackingID))
				       && ts.Any(t => t.Contains(transaction3.MessageTrackingID));
			};
			mockRepository.Verify(x => x.InsertELKResubmitTransactions(It.Is<IEnumerable<string>>(ts => isExpectedTransactions(ts))), Times.Exactly(1));
		}

		[Test]
		public void TestBillingTransactionHasSamePropertiesAsAPITransaction()
		{
			Assert.That(
				typeof(BillingTransaction).GetProperties().Select(p => new { p.Name, p.PropertyType }),
				Is.EquivalentTo(typeof(API.BillingTransaction).GetProperties().Select(p => new { p.Name, p.PropertyType })));
		}

		internal static Mock<BillingKafkaClient> CreateKafkaClient(Mock<IBillingTransactionProducer<string, string>> jsonProducerMock, List<string> toELKTrackingIDs, Dictionary<string, Error> deliveryErrors, Dictionary<string, string> expectedJsons)
		{
			var dateTimeProviderMock = new Mock<CargoWise.Billing.Kafka.API.IDateTimeProvider>();
			dateTimeProviderMock.Setup(x => x.DateTimeNow).Returns(new DateTime(2022, 3, 31, 9, 30, 0));
			jsonProducerMock
				.Setup(x => x.Produce(It.Is<string>(s => s.Equals("billed-topic")), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()))
				.Callback<string, Message<string, string>, Action<DeliveryReport<string, string>>>(
					(topic, message, deliveryHandler) =>
					{

						var transaction = message.Value;
						var trackingID = JObject.Parse(transaction)["MessageTrackingID"].ToString();
						toELKTrackingIDs.Add(trackingID);
						if (expectedJsons != null && expectedJsons.Count != 0)
						{
							Assert.AreEqual(expectedJsons[trackingID], transaction);
						}
						var topicPartitionOffset = new TopicPartitionOffset(topic, new Partition(0), Offset.Unset);
						var report = new DeliveryReport<string, string>
						{
							Error = deliveryErrors == null ? new Error(ErrorCode.NoError) : deliveryErrors[trackingID],
							Message = new Message<string, string> { Key = message.Key, Value = transaction },
							TopicPartitionOffset = topicPartitionOffset
						};
						deliveryHandler(report);
					});
			jsonProducerMock.Setup(x => x.Flush());
			jsonProducerMock.Setup(x => x.Dispose());
			var kafkClientMock = new Mock<BillingKafkaClient>("billing-hosts", dateTimeProviderMock.Object);
			kafkClientMock.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, string>>>())).Returns(new Lazy<IBillingTransactionProducer<string, string>>(() => jsonProducerMock.Object));

			return kafkClientMock;
		}

		public static BillingTransaction GenerateValidTransaction()
		{
			return new BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ",
				Branch = "KLM",
				Category = "TST",
				MessageTrackingID = Guid.NewGuid().ToString(),
				ClientStaffCode = "ABC",
				ClientNumber = "98765432100123456789",
				PriceItemCode = "TST",
				Reference1 = "REFERENCE 1",
				Reference2 = "REFERENCE 2",
				Reference3 = "REFERENCE 3",
				Reference4 = "REFERENCE 4",
				Reference5 = "REFERENCE 5",
				ReportingSource = "XYZ",
				Version = 4,
				ServiceOccuredUTC = DateTime.UtcNow,
				AdditionalRefs = ""
			};
		}

		public static UsageTransaction GenerateUsageTransaction(string companyName = "Test", DateTime? occurredUtc = null)
		{
			return new UsageTransaction
			{
				BranchCode = "KLM",
				CompanyCode = "ABC",
				CompanyName = companyName,
				EnterpriseCode = "TST",
				Environment = "TST",
				ServerCode = "TST",
				UsageCode = "USG",
				UsageCount = 2,
				ServiceOccuredUTC = occurredUtc ?? DateTime.UtcNow,
				AdditionalRefs = ""
			};
		}

		List<string> toELKTrackingIDs;
		private Mock<IBillingTransactionProducer<string, string>> jsonBillingProducerMock;
		private Mock<IConfigurationProvider> configurationMock;
	}
}
