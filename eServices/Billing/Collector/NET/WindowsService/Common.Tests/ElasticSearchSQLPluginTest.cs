using System.Text;
using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.Billing.CollectorService.Plugin.Tests;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Serialization;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using WTG.ErrorReporting;

namespace CargoWise.eServices.Billing.Collector.NET.BackgroundService.Common.Tests
{
	[TestFixture]
	public abstract class ElasticSearchSQLPluginTest<TPlugin> : AbstractPluginTest<TPlugin>
		where TPlugin : ElasticSearchSQLPlugin, new()
	{
		[Test]
		public void TestGetTransactions()
		{
			AssertTransactions(mockPlugin.Object.GetTransactions(start, end).ToArray());
			AssertLogs();

			mockPlugin.Verify(_ => _.Client, Times.AtLeastOnce);
		}

		protected abstract TestQueryResponse[] QuerySqlResponses { get; }

		protected virtual int QuerySqlResponseStatusCode => 200;

		protected abstract void AssertTransactions(TimeStampedTransaction[] transactions);

		protected virtual void AssertLogs()
		{
		}

		protected void AssertTransaction(TimeStampedTransaction transaction,
			DateTime timeStamp,
			int billableCount,
			string clientID,
			string clientNumber,
			string clientStaffCode,
			string category,
			string priceItemCode,
			string reference1,
			string reference2,
			string reference3,
			string reference4,
			string reference5,
			string reportingSource,
			DateTime serviceOccuredUTC,
			string MessageTrackingID)
		{
			Assert.That(transaction.BillingTransaction.BillableCount, Is.EqualTo(billableCount));
			Assert.That(transaction.BillingTransaction.ClientID, Is.EqualTo(clientID));
			Assert.That(transaction.BillingTransaction.ClientNumber, Is.EqualTo(clientNumber));
			Assert.That(transaction.BillingTransaction.ClientStaffCode, Is.EqualTo(clientStaffCode));
			Assert.That(transaction.BillingTransaction.Category, Is.EqualTo(category));
			Assert.That(transaction.BillingTransaction.PriceItemCode, Is.EqualTo(priceItemCode));
			Assert.That(transaction.BillingTransaction.Reference1, Is.EqualTo(reference1));
			Assert.That(transaction.BillingTransaction.Reference2, Is.EqualTo(reference2));
			Assert.That(transaction.BillingTransaction.Reference3, Is.EqualTo(reference3));
			Assert.That(transaction.BillingTransaction.Reference4, Is.EqualTo(reference4));
			Assert.That(transaction.BillingTransaction.Reference5, Is.EqualTo(reference5));
			Assert.That(transaction.BillingTransaction.ReportingSource, Is.EqualTo(reportingSource));
			Assert.That(transaction.BillingTransaction.ServiceOccuredUTC, Is.EqualTo(serviceOccuredUTC));
			Assert.That(transaction.BillingTransaction.MessageTrackingID, Is.EqualTo(MessageTrackingID));
			Assert.That(transaction.TimeStamp, Is.EqualTo(timeStamp));
		}

		[SetUp]
		public void Setup() => SetUpInternal();

		protected internal virtual void SetUpInternal()
		{
			mockLogger = new Mock<ILogger>();
			start = new DateTime(2014, 9, 30, 23, 9, 16);
			end = new DateTime(2014, 9, 8, 13, 37, 10);

			var clientQueue = new Queue<ElasticsearchClient>();
			foreach (var querySqlResponse in QuerySqlResponses)
			{
				var responseBytes = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(querySqlResponse));
				var connection = new InMemoryRequestInvoker(responseBytes, QuerySqlResponseStatusCode);
				var connectionSettings = new ElasticsearchClientSettings(new SingleNodePool(new Uri("http://localhost:9200")), connection, sourceSerializer: (builtin, settings) => new DefaultSourceSerializer(settings, options => { }));
				var elasticClient = new ElasticsearchClient(connectionSettings);
				clientQueue.Enqueue(elasticClient);
			}
			mockPlugin = new Mock<TPlugin> { CallBase = true };
			mockPlugin.Setup(_ => _.QueryAssembly).Returns(typeof(TPlugin).Assembly);
			SetLogger(mockPlugin, mockLogger.Object);
			mockPlugin.Setup(x => x.Client).Returns(clientQueue.Dequeue);

			var mockErrorReportingClient = new Mock<IErrorReportingClient>();
			mockErrorReportingClient.Setup(_ => _.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()))
							.Callback((IOpaqueErrorReport errBuilder, CancellationToken cancellationToken) =>
							{
								var reportBuilder = errBuilder as EnterpriseErrorReportBuilder;
								using (var memStream = new MemoryStream())
								{
									var task = reportBuilder.WriteToAsync(memStream);
									task.ConfigureAwait(false);
									task.Wait();
									memStream.Seek(0, SeekOrigin.Begin);
									using (var str = new StreamReader(memStream))
									{
										var t2 = str.ReadToEndAsync();
										t2.ConfigureAwait(false);
										t2.Wait();
										issueDescriptions.Add(t2.Result);
									}
								}
							}).Returns(Task.CompletedTask);

			mockPlugin.Object.SetErrorReportingClient(mockErrorReportingClient.Object);

			mockPlugin.Object.UpdateSettings(new PluginSettings {
				Parameters = new[]
				{
					new PluginParameter("ElasticEndpoint", "test.endpoint"),
					new PluginParameter("ElasticApiKey", "TestApiKey")
				}
			});
		}

		[TearDown]
		public void TearDown() => TearDownInternal();

		protected internal virtual void TearDownInternal()
		{
			issueDescriptions.Clear();
		}

		protected DateTime start;
		protected DateTime end;
		protected Mock<TPlugin> mockPlugin;
		protected Mock<ILogger> mockLogger;
		protected List<string> issueDescriptions = new ();
	}
}
