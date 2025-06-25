using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using CargoWise.Billing.Kafka.API;
using CargoWise.Billing.Tests.Common;
using Confluent.Kafka;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CargoWise.Billing.API.Tests
{
	[TestFixture]
	public class BillingKafkaClientTest
	{
		[Test]
		public void TestBillingKafkaClientTest_SendBillingInfoToKafka()
		{
			var transactions = new List<BillingTransaction>();
			var mockKafkaProducer = new Mock<IBillingTransactionProducer<string, BillingTransaction>>();
			mockKafkaProducer
				.Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<Message<string, BillingTransaction>>(), It.IsAny<Action<DeliveryReport<string, BillingTransaction>>>()))
				.Callback<string, Message<string, BillingTransaction>, Action<DeliveryReport<string, BillingTransaction>>>((t, m, d) => transactions.Add(m.Value));

			var mockKafkaClient = new Mock<BillingKafkaClient>("hosts", null);
			mockKafkaClient.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, BillingTransaction>>>())).Returns(new Lazy<IBillingTransactionProducer<string, BillingTransaction>>(() => mockKafkaProducer.Object));

			var transaction1 = GetValidTransaction();
			transaction1.Reference1 = "Transaction 1";
			mockKafkaClient.Object.SendBillingInfoToKafka("topic", Guid.NewGuid().ToString(), transaction1, null);
			var transaction2 = GetValidTransaction();
			transaction2.Reference1 = "Transaction 2";
			mockKafkaClient.Object.SendBillingInfoToKafka("topic", Guid.NewGuid().ToString(), transaction2, null);

			Assert.That(transactions.Count, Is.EqualTo(2));
			ObjectComparer.AssertPropertiesAreEqual(transaction1, transactions[0]);
			ObjectComparer.AssertPropertiesAreEqual(transaction2, transactions[1]);
		}

		[Test]
		public void TestBillingKafkaClientTest_SendBillingInfoToELK()
		{
			var transactions = new List<string>();
			var mockDateTimeProvider = new Mock<IDateTimeProvider>();
			mockDateTimeProvider.Setup(x => x.DateTimeNow).Returns(new DateTime(2022, 3, 31, 9, 30, 0));

			var mockKafkaProducer = new Mock<IBillingTransactionProducer<string, string>>();
			mockKafkaProducer
				.Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()))
				.Callback<string, Message<string, string>, Action<DeliveryReport<string, string>>>((t, m, d) => transactions.Add(m.Value));

			var mockKafkaClient = new Mock<BillingKafkaClient>("hosts", mockDateTimeProvider.Object);
			mockKafkaClient.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, string>>>())).Returns(new Lazy<IBillingTransactionProducer<string, string>>(() => mockKafkaProducer.Object));

			var transaction1 = GetValidTransaction();
			transaction1.Reference1 = "Transaction 1";
			transaction1.AdditionalRefs = TestHelper.GetEmbeddedResourceAsString("TestFiles.BillingKafka.AdditionalRef_Input.txt");
			mockKafkaClient.Object.SendBillingInfoToELK("topic", Guid.NewGuid().ToString(), transaction1, null);
			var expected = JObject.Parse(TestHelper.GetEmbeddedResourceAsString("TestFiles.BillingKafka.BillingToELK_Output.txt")).ToString(Formatting.None);
			var expectedDateTime = $"{transaction1.ServiceOccuredUTC.Year}-12-01T01:00:00Z";

			Assert.That(transactions.Count, Is.EqualTo(1));
			expected = expected.Replace("[TRANSACTION_DATETIME]", expectedDateTime);
			Assert.AreEqual(expected, transactions[0]);
		}

		[Test]
		public void TestBillingKafkaClientTest_SendBillingInfoToELK_EmptyAdditionalRef()
		{
			var transactions = new List<string>();
			var mockDateTimeProvider = new Mock<IDateTimeProvider>();
			mockDateTimeProvider.Setup(x => x.DateTimeNow).Returns(new DateTime(2022, 3, 31, 9, 30, 0));

			var mockKafkaProducer = new Mock<IBillingTransactionProducer<string, string>>();
			mockKafkaProducer
				.Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<Message<string, string>>(), It.IsAny<Action<DeliveryReport<string, string>>>()))
				.Callback<string, Message<string, string>, Action<DeliveryReport<string, string>>>((t, m, d) => transactions.Add(m.Value));

			var mockKafkaClient = new Mock<BillingKafkaClient>("hosts", mockDateTimeProvider.Object);
			mockKafkaClient.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, string>>>())).Returns(new Lazy<IBillingTransactionProducer<string, string>>(() => mockKafkaProducer.Object));

			var transaction1 = GetValidTransaction();
			transaction1.Reference1 = "Transaction 1";
			mockKafkaClient.Object.SendBillingInfoToELK("topic", Guid.NewGuid().ToString(), transaction1, null);
			var expected = JObject.Parse(TestHelper.GetEmbeddedResourceAsString("TestFiles.BillingKafka.BillingToELK_EmptyRef_Output.txt")).ToString(Formatting.None);
			var expectedDateTime = $"{transaction1.ServiceOccuredUTC.Year}-12-01T01:00:00Z";

			Assert.That(transactions.Count, Is.EqualTo(1));
			expected = expected.Replace("[TRANSACTION_DATETIME]", expectedDateTime);
			Assert.AreEqual(expected, transactions[0]);
		}

		[Test]
		public void TestBillingKafkaClientTest_ValidationFault()
		{
			var errors = new[] 
			{ 
				"The field ServiceOccuredUTC is required and must be no more than 5 years in the past and no more than one month in the future.",
				"Field PriceItemCode has invalid characters. It must contain only ASCII letters, digits, fullstop, hyphen, hash or underscore."
			};

			var transaction = GetValidTransaction();
			transaction.ServiceOccuredUTC = transaction.ServiceOccuredUTC.AddYears(-5);
			transaction.PriceItemCode = "())";

			var kafkaClient = new BillingKafkaClient("hosts");

			var exception = Assert.Throws<ValidationException>(delegate { kafkaClient.SendBillingInfoToKafka("topic", Guid.NewGuid().ToString(), transaction, null); });
			Assert.That(exception.Message, Is.EqualTo("Transaction validation failed."));
			Assert.That(exception.Errors, Is.EquivalentTo(errors));
		}

		[Test]
		public void TestBillingKafkaClientTest_InvalidJson()
		{
			var kafkaClient = new BillingKafkaClient("hosts");

			var transaction = GetValidTransaction();
			transaction.Reference1 = "Transaction 1";
			transaction.AdditionalRefs = "{ \"OrganisationName\": &lt;XMLNode&gt; }";
			var exception = Assert.Throws<ValidationException>(delegate { kafkaClient.SendBillingInfoToELK("topic", Guid.NewGuid().ToString(), transaction, null); ; });
			Assert.That(exception.Message, Is.EqualTo("Transaction validation failed."));
			Assert.IsTrue(exception.InnerException is JsonReaderException);
			Assert.That(exception.InnerException.Message, Does.StartWith("Unexpected character encountered while parsing value"));
		}

		[Test]
		public void TestSendUsageJsonToELK()
		{
			AssertSendUsageInfoToELK<string>("TestFiles.BillingKafka.AdditionalRef_Input.txt", SendUsageAsString, typeof(BillingTransactionsSerializer<>));
		}

		[Test]
		public void TestSendUsageJsonToELK_OverlappingButEqualAdditionalRefProperties()
		{
			AssertSendUsageInfoToELK<string>("TestFiles.BillingKafka.AdditionalRef_OverlappingUsageProperties.txt", SendUsageAsString, typeof(BillingTransactionsSerializer<>));
		}

		[Test]
		public void TestSendUsageJsonToELK_OverlappingButDifferentAdditionalRefProperties()
		{
			AssertSendUsageInfoToELK<string>("TestFiles.BillingKafka.AdditionalRef_OverlappingDifferentUsageProperties.txt", SendUsageAsString, typeof(BillingTransactionsSerializer<>), expectException: true);
		}

		[Test]
		public void TestSendUsageTransactionToELK()
		{
			AssertSendUsageInfoToELK<ELKTransaction>("TestFiles.BillingKafka.AdditionalRef_Input.txt", SendUsageAsTransaction, typeof(ELKSerializer<>));
		}

		[Test]
		public void TestSendUsageTransactionToELK_OverlappingButEqualAdditionalRefProperties()
		{
			AssertSendUsageInfoToELK<ELKTransaction>("TestFiles.BillingKafka.AdditionalRef_OverlappingUsageProperties.txt", SendUsageAsTransaction, typeof(ELKSerializer<>));
		}

		[Test]
		public void TestSendUsageTransactionToELK_OverlappingButDifferentAdditionalRefProperties()
		{
			AssertSendUsageInfoToELK<ELKTransaction>("TestFiles.BillingKafka.AdditionalRef_OverlappingDifferentUsageProperties.txt", SendUsageAsTransaction, typeof(ELKSerializer<>), expectException: true);
		}

		void AssertSendUsageInfoToELK<T>(string additionalRefsFile, Action<Mock<BillingKafkaClient>, UsageTransaction> sendAction, Type serializerType, bool expectException = false)
		{
			var transactions = new List<T>();
			byte[] serializedResult = null;
			var mockDateTimeProvider = new Mock<IDateTimeProvider>();
			mockDateTimeProvider.Setup(x => x.DateTimeNow).Returns(new DateTime(2022, 3, 31, 9, 30, 0));
			var kafkaSerializerType = serializerType.MakeGenericType(typeof(T));
			var serializerParams = kafkaSerializerType.GetConstructors().Any(c =>
				c.GetParameters().Any(p => p.ParameterType == typeof(IDateTimeProvider)))
					? new object[] { mockDateTimeProvider.Object } : null;
			var serializer = (ISerializer<T>)Activator.CreateInstance(kafkaSerializerType, serializerParams);
			var mockKafkaProducer = new Mock<IBillingTransactionProducer<string, T>>();
			mockKafkaProducer
				.Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<Message<string, T>>(), It.IsAny<Action<DeliveryReport<string, T>>>()))
				.Callback<string, Message<string, T>, Action<DeliveryReport<string, T>>>((t, m, d) =>
				{
					transactions.Add(m.Value);
					serializedResult = serializer.Serialize(m.Value, SerializationContext.Empty);
				});

			var mockKafkaClient = new Mock<BillingKafkaClient>("hosts", mockDateTimeProvider.Object);
			mockKafkaClient.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, T>>>())).Returns(new Lazy<IBillingTransactionProducer<string, T>>(() => mockKafkaProducer.Object));

			var transaction1 = GetUsageTransaction();
			transaction1.AdditionalRefs = TestHelper.GetEmbeddedResourceAsString(additionalRefsFile);
			if (expectException)
			{
				Assert.Throws(typeof(ValidationException), () => sendAction(mockKafkaClient, transaction1));
			}
			else
			{
				sendAction(mockKafkaClient, transaction1);
				var expected = JObject.Parse(TestHelper.GetEmbeddedResourceAsString("TestFiles.BillingKafka.UsageTransactionToELK_Output.txt")).ToString(Formatting.None);

				using (var ms = new MemoryStream(serializedResult))
				using (var reader = new StreamReader(ms))
				{
					var actual = JObject.Parse(reader.ReadToEnd()).ToString(Formatting.None);
					var expectedDateTime = $"{transaction1.ServiceOccuredUTC.Year}-12-01T01:00:00Z";
					Assert.That(transactions.Count, Is.EqualTo(1));
					expected = expected.Replace("[TRANSACTION_DATETIME]", expectedDateTime);
					Assert.AreEqual(expected, actual);
				}
			}
		}

		void SendUsageAsString(Mock<BillingKafkaClient> mockClient, UsageTransaction transaction) =>
			mockClient.Object.SendUsageInfoToELK("topic", Guid.NewGuid().ToString(), transaction, (DeliveryReport<string, string> report) => { });

		void SendUsageAsTransaction(Mock<BillingKafkaClient> mockClient, UsageTransaction transaction) =>
			mockClient.Object.SendUsageInfoToELK("topic", Guid.NewGuid().ToString(), transaction, (report) => { });

		[Test]
		public void TestBillingKafkaClientTest_WhenDisposed_CreatedProducer()
		{
			var transactions = new List<BillingTransaction>();
			var mockKafkaProducer = new Mock<IBillingTransactionProducer<string, BillingTransaction>>();
			var mockJsonKafkaProducer = new Mock<IBillingTransactionProducer<string, string>>();
			var mockElkProducer = new Mock<IBillingTransactionProducer<string, ELKTransaction>>();
			var billingProducerLazy = new Lazy<IBillingTransactionProducer<string, BillingTransaction>>(() => mockKafkaProducer.Object);
			var jsonProducerLazy = new Lazy<IBillingTransactionProducer<string, string>>(() => mockJsonKafkaProducer.Object);
			var elkProducerLazy = new Lazy<IBillingTransactionProducer<string, ELKTransaction>>(() => mockElkProducer.Object);

			var mockKafkaClient = new Mock<BillingKafkaClient>("hosts", null);
			mockKafkaClient.CallBase = true;
			mockKafkaClient.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, BillingTransaction>>>())).Returns(billingProducerLazy);
			mockKafkaClient.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, string>>>())).Returns(jsonProducerLazy);
			mockKafkaClient.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, ELKTransaction>>>())).Returns(elkProducerLazy);
			mockKafkaClient.Object.SendBillingInfoToELK("testTopic", transactions, report => { });
			mockKafkaClient.Object.SendBillingInfoToKafka("testTopic", "trackingId", GetValidTransaction(), report => { });
			mockKafkaClient.Object.SendUsageInfoToELK("testTopic", "trackingId", new UsageTransaction(), report => { }, true);
			mockKafkaClient.Object.SendUsageInfoToELK("testTopic", "trackingId", new UsageTransaction(), report => { });

			mockKafkaClient.Object.Dispose();
			Assert.IsTrue(billingProducerLazy.IsValueCreated, "billingProducerLazy is not created");
			Assert.IsTrue(jsonProducerLazy.IsValueCreated, "jsonProducerLazy is not created");
			Assert.IsTrue(elkProducerLazy.IsValueCreated, "elkProducerLazy is not created");

			mockJsonKafkaProducer.Verify(x => x.Flush(), Times.Once());
			mockKafkaProducer.Verify(x => x.Flush(), Times.Once());
			mockElkProducer.Verify(x => x.Flush(), Times.Once());
			mockJsonKafkaProducer.Verify(x => x.Dispose(), Times.Once());
			mockKafkaProducer.Verify(x => x.Dispose(), Times.Once());
			mockElkProducer.Verify(x => x.Dispose(), Times.Once());
		}

		[Test]
		public void TestBillingKafkaClientTest_WhenDisposed_NotCreatedProducers()
		{
			var transactions = new List<BillingTransaction>();
			var mockKafkaProducer = new Mock<IBillingTransactionProducer<string, BillingTransaction>>();
			var mockJsonKafkaProducer = new Mock<IBillingTransactionProducer<string, string>>();
			var mockElkProducer = new Mock<IBillingTransactionProducer<string, UsageTransaction>>();
			var billingProducerLazy = new Lazy<IBillingTransactionProducer<string, BillingTransaction>>(() => mockKafkaProducer.Object);
			var jsonProducerLazy = new Lazy<IBillingTransactionProducer<string, string>>(() => mockJsonKafkaProducer.Object);
			var elkProducerLazy = new Lazy<IBillingTransactionProducer<string, UsageTransaction>>(() => mockElkProducer.Object);

			var mockKafkaClient = new Mock<BillingKafkaClient>("hosts", null);
			mockKafkaClient.CallBase = true;
			mockKafkaClient.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, BillingTransaction>>>())).Returns(billingProducerLazy);
			mockKafkaClient.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, string>>>())).Returns(jsonProducerLazy);
			mockKafkaClient.Setup(x => x.CreateProducer(It.IsAny<Func<IBillingTransactionProducer<string, UsageTransaction>>>())).Returns(elkProducerLazy);

			mockKafkaClient.Object.Dispose();
			Assert.IsFalse(billingProducerLazy.IsValueCreated, "billingProducerLazy is not created");
			Assert.IsFalse(jsonProducerLazy.IsValueCreated, "jsonProducerLazy is not created");
			Assert.IsFalse(elkProducerLazy.IsValueCreated, "elkProducerLazy is not created");

			mockJsonKafkaProducer.Verify(x => x.Flush(), Times.Never());
			mockKafkaProducer.Verify(x => x.Flush(), Times.Never());
			mockElkProducer.Verify(x => x.Flush(), Times.Never());
			mockJsonKafkaProducer.Verify(x => x.Dispose(), Times.Never());
			mockKafkaProducer.Verify(x => x.Dispose(), Times.Never());
			mockElkProducer.Verify(x => x.Dispose(), Times.Never());
		}
		[Test]
		public void TestGetProducerConfig()
		{
			var baseConfig = new Dictionary<string, string>
			{
				{ "bootstrap.servers", "localhost:9092" },
				{ "security.protocol", "SaslSsl" },
				{ "enable.ssl.certificate.verification", "true" },
				{ "sasl.mechanism", "GSSAPI" },
				{ "sasl.username", "testUsername" },
				{ "sasl.password", "testpassword" },
				{ "ssl.ca.certificate.stores", "ROOT" },
			};

			var producerSettings = new Dictionary<string, string>
			{
				{ "linger.ms", "50" },
				{ "message.timeout.ms", "5000" }
			};
			var ConsumerSettings = new Dictionary<string, string>
			{
				{ "group.id", "eServices-billing-transactions-consumer" },
				{ "auto.offset.reset", "1" },
				{ "enable.auto.commit", "false" },
				{ "enable.partition.eof", "true" }
			};

			var producerConfig = BillingKafkaClient.GetKafkaConfig<ProducerConfig>(baseConfig, producerSettings);
			Assert.IsInstanceOf<ProducerConfig>(producerConfig);
			Assert.AreEqual("localhost:9092", producerConfig.BootstrapServers);
			Assert.AreEqual(50, producerConfig.LingerMs);
			Assert.AreEqual(5000, producerConfig.MessageTimeoutMs);
			Assert.AreEqual(true, producerConfig.EnableSslCertificateVerification);
			Assert.AreEqual(SecurityProtocol.SaslSsl, producerConfig.SecurityProtocol);
			Assert.AreEqual("testUsername", producerConfig.SaslUsername);
			Assert.AreEqual( "testpassword", producerConfig.SaslPassword);
			Assert.AreEqual(SaslMechanism.Gssapi, producerConfig.SaslMechanism);
			Assert.AreEqual("ROOT", producerConfig.SslCaCertificateStores);
			var consumerConfig = BillingKafkaClient.GetKafkaConfig<ConsumerConfig>(baseConfig, null, ConsumerSettings);
			Assert.IsInstanceOf<ConsumerConfig>(consumerConfig);
			Assert.AreEqual("eServices-billing-transactions-consumer", consumerConfig.GroupId);
			Assert.AreEqual(AutoOffsetReset.Earliest, consumerConfig.AutoOffsetReset);
			Assert.AreEqual(false, consumerConfig.EnableAutoCommit);
			Assert.AreEqual(true, consumerConfig.EnablePartitionEof);
		}
		static API.BillingTransaction GetValidTransaction()
		{
			return new API.BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ",
				ClientNumber = "98765432100123456789",
				ClientStaffCode = "ABC",
				Category = "TST",
				PriceItemCode = "DEF",
				Reference1 = "REFERENCE 1",
				Reference2 = "REFERENCE 2",
				Reference3 = "REFERENCE 3",
				ReportingSource = "XYZ",
				ServiceOccuredUTC = new DateTime(DateTime.Now.Year, 12, 1, 1, 0, 0).AddYears(-1),
				Version = 4,
				MessageTrackingID = "AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA",
			};
		}

		static API.UsageTransaction GetUsageTransaction()
		{
			return new API.UsageTransaction
			{
				UsageCount = 2,
				ServiceOccuredUTC = new DateTime(DateTime.Now.Year, 12, 1, 1, 0, 0).AddYears(-1),
				EnterpriseCode = "WUT",
				ServerCode = "AAA",
				Environment = "TST",
				CompanyCode = "DAU",
				CompanyName = "Your Australia Corp",
				BranchCode = "BRC",
				UsageCode = "USS"
			};
		}
	}
}
