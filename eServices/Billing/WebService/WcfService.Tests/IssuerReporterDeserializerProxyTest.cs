using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Billing.Kafka.API;
using CargoWise.eServices.Billing.WcfService.Hangfire;
using Confluent.Kafka;
using Moq;
using NUnit.Framework;
using ProtoBuf;
using WTG.ErrorReporting;
using API = CargoWise.Billing.API;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	public class IssuerReporterDeserializerProxyTest
	{
		private IssuerReporterDeserializerProxy<API.BillingTransaction> proxy;
		private Mock<IErrorReportingClient> errorReportClientMock;
		private Func<IErrorReportingClient> originalGetReportClient;

		[SetUp]
		public void SetUp()
		{
			var serializer = new BillingTransactionsDeserializer();
			proxy = new IssuerReporterDeserializerProxy<API.BillingTransaction>(serializer);

			errorReportClientMock = new Mock<IErrorReportingClient>();
			originalGetReportClient = IssueReporter.GetErrorReportingClient;
			IssueReporter.GetErrorReportingClient = () => errorReportClientMock.Object;
		}

		[TearDown]
		public void TearDown()
		{
			IssueReporter.GetErrorReportingClient = originalGetReportClient;
		}

		[Test]
		public void TestDeserializer_Success()
		{
			var transaction = new API.BillingTransaction
			{
				Version = 3,
				AdditionalRefs = nameof(API.BillingTransaction.AdditionalRefs),
				BillableCount = 1
			};
			var data = new ReadOnlySpan<byte>(BillingTransactionProtoBufSerializerForTest.SerializeTransaction(transaction));
			var result = proxy.Deserialize(data, false, Confluent.Kafka.SerializationContext.Empty);
			Assert.That(result.ToString(), Is.EqualTo(transaction.ToString()));
		}

		[Test]
		public void TestDeserializer_ExceptionReportToIssueReporter()
		{
			EnterpriseErrorReportBuilder builder = null;
			errorReportClientMock
				.Setup(x => x.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()))
				.Callback<IOpaqueErrorReport, CancellationToken>((b, _) => builder = (EnterpriseErrorReportBuilder)b)
				.Returns(Task.CompletedTask);

			var content = Encoding.UTF8.GetBytes("Random string");

			Assert.Throws<ProtoException>(() =>
			{
				var invalidData = new ReadOnlySpan<byte>(content);
				var result = proxy.Deserialize(invalidData, false,  Confluent.Kafka.SerializationContext.Empty);
			});

			Assert.That(builder, Is.Not.Null);
			errorReportClientMock.Verify(x => x.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()), Times.Once);

			var reportContent = builder.ToStringAsync(Encoding.UTF8).GetAwaiter().GetResult();
			Assert.That(reportContent, Is.Not.Null);
			Assert.That(reportContent, Is.Not.Empty);
			Assert.IsTrue(reportContent.ToUpper().Contains(BitConverter.ToString(content).ToUpper()));
		}

		[Test]
		public void TestDeserializer_ThrowSerializationException()
		{
			var content = Encoding.UTF8.GetBytes("Random string");

			var exception = Assert.Throws<ProtoException>(() =>
			{
				var invalidData = new ReadOnlySpan<byte>(content);
				var result = proxy.Deserialize(invalidData, false, Confluent.Kafka.SerializationContext.Empty);
			});

			Assert.AreEqual("ProtoBuf.ProtoException", exception.GetType().FullName);
			Assert.AreEqual("Deserialization error occurred during proto buffer processing.", exception.Message);
			Assert.IsTrue(exception.StackTrace.Contains("BillingTransactionsDeserializer.Deserialize"));
		}
	}
}
