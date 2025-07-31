using CargoWise.Billing.CollectorService.Plugin;
using CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Plugins.WGReport;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.MSearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Serilog;

namespace CargoWise.eServices.Billing.Collector.NET.ElasticSearch.WorkerService.Tests
{
	[TestFixture]
	class WGReportPluginTest
	{
		private Mock<ElasticsearchClient> mockClient;
		private PluginSettings _settings = new PluginSettings();
		private Plugins.WGReport.Plugin plugin;
		Mock<ILoggerFactory> mockLoggerFactory;
		Mock<Microsoft.Extensions.Logging.ILogger> mockLogger;
		private Dictionary<string, string[]> testDefectiveQueryHashesDictionary;
		private string enterpriseCode;
		private string serverCode;

		[SetUp]
		public void Setup()
		{
			mockClient = new Mock<ElasticsearchClient>();
			plugin = new  Plugins.WGReport.Plugin();
			mockLoggerFactory = new Mock<ILoggerFactory>();
			mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger>();
			mockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
			plugin.SetLoggerFactory(mockLoggerFactory.Object);
			testDefectiveQueryHashesDictionary = new Dictionary<string, string[]>
			{
				{ "glowwriter", new[] { "5728260986491023210", "1123" } },
				{ "cw1writer", new[] { "9876543210123456789", "2213" } }
			};
			enterpriseCode = "XYZ";
			serverCode = "789";
		}

		[Test]
		public void TestUpdateSettings()
		{
			testDefectiveQueryHashesDictionary = new Dictionary<string, string[]>
			{
				{
					"unKnown",
					new[]
					{
						"4459270267907445875", "7636026590593691186", "13433326112627462770", "8748920740649423001",
						"13082343669470270252", "628620786089426297", "9118391940311386632", "5728260986491023210",
						"16115785089201480695", "975782326017290567", "1130196169699715416", "5088376827700630000",
						"503032482394790140", "2796027762694531528", "17290807452547785650", "5330059344077040864",
						"5728260986491023210", "9350526159082613110", "6143522232945328674", "7778293997177632117",
						"8450459544285921314", "244988531137190284", "615189408138390377", "9369227185114107812",
						"1264112813600996262", "7007334953061862845", "13693833851925598624", "5088376827700638977",
						"11646560601095277755", "0xAE516DB1858172AA", "17483974037930588235", "4567284591508213910",
						"6135068121755414305"
					}
				},
				{ "Archiving", new[] { "0x045A8C8E49D9016C" } },
				{ "Accounting", new[] { "15864644139553466995" } },
				{
					"Warehouse",
					new[]
					{
						"16115785089201480695", "9202538955058083800", "3198311722474669987", "4415314494590265533",
					}
				},
				{ "Pave", new[] { "0x9E54804C57F16301" } },
				{ "CDC", new[] { "10478365439202353342" } },
				{ "StmALog", null },
				{ "Forwarding", null },
				{
					"Customs",
					new[] { "6176400199857466201", "3738139362575558473", "16922104566503100626", "0x079057ED87C3DFEA" }
				},
				{
					"Glow",
					new[]
					{
						"17483974037930588235", "8316699205152117529", "2269298059545940954", "6301341131417706541"
					}
				},
				{ "Core", new[] { "10478365439202353342" } },
				{ "Master Data", null }
			};
			string defectiveQueryHashesJson = JsonConvert.SerializeObject(testDefectiveQueryHashesDictionary);

			_settings = new PluginSettings()
			{
				Parameters = new PluginParameter[]
				{
					new PluginParameter("ElasticEndpoint", "https://r.test-1.es.wtg.ws:443"),
					new PluginParameter("ElasticApiKey", "k++HhEyGTc2m1oj0+YUCAGzS43thuQnQDJqwNkeCH45ceU8sGwy+6ZzpSAwk/i8mLDMJBJcZd9feTPjq3dJhYW8wI5x0ocuuWlFwa1Vo8UG8rGHwszb/6foHBxPoJ8Wm2COM/0IO8bW4P6JKy+DN5PNFlU8fb29asLNsRmu0U4s="),
					new PluginParameter("CpuMonitoringIndex", "idx-*-prod-sqlcpumonitoring-prod*"),
					new PluginParameter("GatewayNonBilledUsage", "idx-*-*-ehubgatewaynonbilledusage*"),
					new PluginParameter("DefectiveQueryHashesDictionary", defectiveQueryHashesJson),
					new PluginParameter("BatchSize", "20"),
					new PluginParameter("ElasticRetryMaxAttempts", "1440"),
					new PluginParameter("ElasticRetryDelayInSecond", "60")
				}
			};
			plugin.UpdateSettings(_settings);
			Assert.That(plugin.ElasticEndpoint, Is.EqualTo("https://r.test-1.es.wtg.ws:443"));
			Assert.That(plugin.CpuMonitoringIndex, Is.EqualTo("idx-*-prod-sqlcpumonitoring-prod*"));
			Assert.That(plugin.GatewayNonBilledUsage, Is.EqualTo("idx-*-*-ehubgatewaynonbilledusage*"));
			Assert.That(plugin.DefectiveQueryHashesDictionary, Is.EqualTo(testDefectiveQueryHashesDictionary));
			Assert.That(plugin.BatchSize, Is.EqualTo(20));
			Assert.That(plugin.ElasticRetryMaxAttempts, Is.EqualTo(1440));
			Assert.That(plugin.ElasticRetryDelayInSecond, Is.EqualTo(60));
		}
		[Test]
		public async Task TestRetryLoggingOnTransientFailure()
		{
			mockClient
				.Setup(x => x.SearchAsync<DynamicResponse>(
					It.IsAny<Action<SearchRequestDescriptor<DynamicResponse>>>(),
					It.IsAny<CancellationToken>()))
				.ThrowsAsync(new Elastic.Transport.TransportException("test error"));

			DateTime startUtc = DateTime.UtcNow.AddHours(-1);
			DateTime endUtc = DateTime.UtcNow;

			try
			{
				await plugin.RetryAsync(() =>
					plugin.GetUserCount(enterpriseCode, serverCode, startUtc, endUtc, mockClient.Object));
			}
			catch
			{
				mockLogger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(),It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retry attempt")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
			}
		}

		[Test]
		public void TestGetClientSystems()
		{
			var client1 = new CompositeBucket
			{
				Key = new Dictionary<string, FieldValue>
				{
					{ "EnterpriseCode", "XYZ" }, { "ServerCode", "789" }
				},
				DocCount = 10
			};
			var client2 = new CompositeBucket
			{
				Key = new Dictionary<string, FieldValue>
				{
					{ "EnterpriseCode", "ABC" }, { "ServerCode", "123" }
				},
				DocCount = 15
			};
			var mockAggregation = new AggregateDictionary(new Dictionary<string, IAggregate>
			{
				{
					"system_group", new CompositeAggregate { Buckets = new List<CompositeBucket> { client1, client2 } }
				}
			});
			var mockSearchResponse = new SearchResponse<ValueTuple<string, string>> { Aggregations = mockAggregation };
			var searchResponse = TestableResponseFactory.CreateSuccessfulResponse(mockSearchResponse, 200);

			mockClient
				.Setup(x => x.SearchAsync<(string, string)>(
					It.IsAny<Action<SearchRequestDescriptor<(string, string)>>>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(searchResponse);
			var start = new DateTime(2024, 5, 30, 14, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2024, 6, 30, 14, 0, 0, DateTimeKind.Utc);
			var systems = plugin.GetClientSystems(start, end, mockClient.Object).ToList();
			mockLogger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(),It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrieving systems")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
			mockLogger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(),It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Found 2 systems")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
			Assert.AreEqual(2, systems.Count);
		}

		[Test]
		public async Task TestGetWGRTransactions()
		{
			var mockClient = new Mock<ElasticsearchClient>();
			var mockOpenPitResponse = new OpenPointInTimeResponse { Id = "1" };
			mockClient
				.Setup(x => x.OpenPointInTimeAsync<OpenPointInTimeResponse>(
					It.IsAny<Action<OpenPointInTimeRequestDescriptor<OpenPointInTimeResponse>>>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(mockOpenPitResponse);

			var mockWGRTransaction1 = new WGRUsageTransaction
			{
				Reference1 = "Ref1",
				Reference2 = "Ref2",
				SubmitToELKTime = DateTime.UtcNow
			};

			var mockWGRTransaction2 = new WGRUsageTransaction
			{
				Reference1 = "RefA",
				Reference2 = "RefB",
				SubmitToELKTime = DateTime.UtcNow.AddMinutes(-5)
			};

			var mockSearchResponse = new SearchResponse<WGRUsageTransaction>
			{
				HitsMetadata = new()
				{
					Hits = new List<Hit<WGRUsageTransaction>>
					{
						new Hit<WGRUsageTransaction>
						{
							Id = "hit_1",
							Index = "idx-prod-sqlcpumonitoring-prod-2024.05.29-000123",
							Source = mockWGRTransaction1
						},
						new Hit<WGRUsageTransaction>
						{
							Id = "hit_2",
							Index = "idx-prod-sqlcpumonitoring-prod-2024.05.29-000456",
							Source = mockWGRTransaction2
						}
					}
				},
				PitId = "1"
			};

			var mockSearchResponse2 = new SearchResponse<WGRUsageTransaction>
			{
				HitsMetadata = new() { Hits = new List<Hit<WGRUsageTransaction>>() },
				PitId = null
			};
			var searchResponse = TestableResponseFactory.CreateSuccessfulResponse(mockSearchResponse, 200);
			var searchCallCount = 0;
			mockClient
				.Setup(x => x.SearchAsync<WGRUsageTransaction>(
					It.IsAny<SearchRequestDescriptor<WGRUsageTransaction>>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(() =>
				{
					if (searchCallCount == 0)
					{
						searchCallCount++;
						return TestableResponseFactory.CreateSuccessfulResponse(searchResponse, 200);
					}

					return TestableResponseFactory.CreateSuccessfulResponse(mockSearchResponse2, 200);
				});

			var startUtc = new DateTime(2024, 5, 30, 14, 0, 0, DateTimeKind.Utc);
			var endUtc = new DateTime(2024, 6, 30, 14, 0, 0, DateTimeKind.Utc);

			var transactions = await plugin.GetWGRUsageTransactions(serverCode, enterpriseCode, startUtc, endUtc, testDefectiveQueryHashesDictionary,mockClient.Object);

			Assert.AreEqual(2, transactions.Count(), "Expected transaction count to be 2 but got a different value.");
			Assert.AreEqual("Ref1", transactions.First().Reference1, "First transaction Reference1 does not match.");
			Assert.AreEqual("RefA", transactions.Last().Reference1, "Last transaction Reference1 does not match.");
			mockLogger.Verify(
				x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(
						$"Searching defective queries for 2 glowwriter,cw1writer usage documents"
					)),
					null,
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once
			);
		}

		[Test]
		public async Task TestMultiSearchForDefectiveQueries()
		{
			var mockTransactions = new List<WGRUsageTransaction>
			{
				new WGRUsageTransaction
				{
					Id = "txn_1",
					Reference1 = "10",
					Reference2 = "glowwriter",
					ServiceOccuredUTC = DateTime.UtcNow.AddMinutes(-30)
				},
				new WGRUsageTransaction
				{
					Id = "txn_2",
					Reference1 = "20",
					Reference2 = "cw1writer",
					ServiceOccuredUTC = DateTime.UtcNow.AddMinutes(-60)
				}
			};
			var mockSqlMonitoringElasticClient = new Mock<ElasticsearchClient>();
			var mockMultiSearchResponse = new MultiSearchResponse<DynamicResponse>
			{
				Responses = new List<MultiSearchResponseItem<DynamicResponse>>
				{
					new MultiSearchResponseItem<DynamicResponse>(
						new MultiSearchItem<DynamicResponse>
						{
							HitsMetadata = new()
							{
								Total = new TotalHits { Value = 2 },
								Hits = new List<Hit<DynamicResponse>>
								{
									new Hit<DynamicResponse>
									{
										Id = "hit_1",
										Index = "idx-prod-sqlcpumonitoring-prod-2024.05.29-000123",
									},
									new Hit<DynamicResponse>
									{
										Id = "hit_2",
										Index = "idx-prod-sqlcpumonitoring-prod-2024.05.29-000456",
									}
								}
							}
						}
					),
				}
			};

			mockSqlMonitoringElasticClient
				.Setup(x => x.MultiSearchAsync<DynamicResponse>(
					It.IsAny<MultiSearchRequest>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(mockMultiSearchResponse);

			var defectiveTransactionIds = await plugin.FilterDefectiveTransactionIds(enterpriseCode,
				serverCode, testDefectiveQueryHashesDictionary, mockTransactions, mockSqlMonitoringElasticClient.Object);

			mockSqlMonitoringElasticClient.Verify(
				x => x.MultiSearchAsync<DynamicResponse>(
					It.IsAny<MultiSearchRequest>(),
					It.IsAny<CancellationToken>()),
				Times.Once);

			Assert.AreEqual(1, defectiveTransactionIds.Count(),
				"Expected only 1 transaction to have defects, but got a different count.");

			mockLogger.Verify(
				x => x.Log(LogLevel.Information, It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(
						$"Found 1 glowwriter,cw1writer usage having defect associated with for EnterpriseCode=XYZ, ServerCode=789"
					)),
					null,
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once
			);
		}

		[Test]
		public async Task TestGetHighestCpuUsage()
		{
		    var mockTransaction = new WGRUsageTransaction
		    {
		        EnterpriseCode = "XYZ",
		        ServerCode = "789",
		        UsageCode = "WGR",
		        Category = "WGR",
		        Reference1 = "100",
		        UsageCount = 500
		    };
		    var mockSearchResponse = new SearchResponse<WGRUsageTransaction>
		    {
		        HitsMetadata = new()
		        {
		            Hits = new List<Hit<WGRUsageTransaction>>
		            {
		                new Hit<WGRUsageTransaction>
		                {
		                    Id = "txn_1",
		                    Source = mockTransaction
		                }
		            }
		        }
		    };
		    var searchResponse = TestableResponseFactory.CreateSuccessfulResponse(mockSearchResponse, 200);
		    mockClient

			    .Setup(x => x.SearchAsync<WGRUsageTransaction>(
				    It.IsAny<Action<SearchRequestDescriptor<WGRUsageTransaction>>>(),
				    It.IsAny<CancellationToken>()))
			    .ReturnsAsync(searchResponse);

		    var startUtc = new DateTime(2024, 5, 30, 14, 0, 0, DateTimeKind.Utc);
		    var endUtc = new DateTime(2024, 6, 30, 14, 0, 0, DateTimeKind.Utc);
		    var excludingIds = new List<string> { "txn_99", "txn_100" };

		    var highestCpuUsage = await plugin.GetHighestCpuUsage(enterpriseCode, serverCode, startUtc, endUtc, excludingIds, mockClient.Object);

		    Assert.IsNotNull(highestCpuUsage, "Expected highestCpuUsage to be not null");
		    Assert.AreEqual("XYZ", highestCpuUsage.EnterpriseCode, "EnterpriseCode mismatch");
		    Assert.AreEqual("789", highestCpuUsage.ServerCode, "ServerCode mismatch");
		    Assert.AreEqual("WGR", highestCpuUsage.UsageCode, "UsageCode mismatch");
		    Assert.AreEqual("WGR", highestCpuUsage.Category, "Category mismatch");
		}

		[Test]
		public async Task TestGetHighestCpuCores()
		{

		    var mockAggregation = new AggregateDictionary(new Dictionary<string, IAggregate>
		    {
		        {
		            "group_usage", new CompositeAggregate
		            {
		                Buckets = new List<CompositeBucket>
		                {
		                    new CompositeBucket
		                    {
		                        Key = new Dictionary<string, Elastic.Clients.Elasticsearch.FieldValue>
		                        {
		                            { "EnterpriseCode", "XYZ" },
		                            { "ServerCode", "789" },
		                            { "@timestamp", "2024-06-30T14:00:00Z" }
		                        },
		                        Aggregations = new AggregateDictionary(new Dictionary<string, IAggregate>
		                        {
		                            { "sum_usage", new SumAggregate { Value = 12.3 } } 
		                        })
		                    }
		                }
		            }
		        }
		    });

		    var mockSearchResponse = new SearchResponse<WGRUsageTransaction>
		    {
		        Aggregations = mockAggregation
		    };

		    var searchResponse = TestableResponseFactory.CreateSuccessfulResponse(mockSearchResponse, 200);

		    mockClient

			    .Setup(x => x.SearchAsync<WGRUsageTransaction>(
				    It.IsAny<Action<SearchRequestDescriptor<WGRUsageTransaction>>>(),
				    It.IsAny<CancellationToken>()))
			    .ReturnsAsync(searchResponse);

		    var startUtc = new DateTime(2024, 5, 30, 14, 0, 0, DateTimeKind.Utc);
		    var endUtc = new DateTime(2024, 6, 30, 14, 0, 0, DateTimeKind.Utc);
		    var excludingIds = new List<string> { "txn_123", "txn_456" };

		    var highestCpuCores = await plugin.GetHighestCpuCores(
		        enterpriseCode, serverCode, startUtc, endUtc, excludingIds, mockClient.Object);
		    Assert.IsNotNull(highestCpuCores, "Expected a valid CPU core value.");
		    Assert.AreEqual(12.3, highestCpuCores, "Expected highest CPU core usage to be 12.3 but got a different value.");
		}

		[Test]
		public async Task TestGetUserCount()
		{
			var mockAggregation = new AggregateDictionary(new Dictionary<string, IAggregate>
			{
				{
					"user_count", new SumAggregate { Value = 42 }
				}
			});
			var mockSearchResponse = new SearchResponse<DynamicResponse>
			{
				Aggregations = mockAggregation
			};
			var searchResponse = TestableResponseFactory.CreateSuccessfulResponse(mockSearchResponse, 200);

			mockClient
				.Setup(x => x.SearchAsync<DynamicResponse>(
					It.IsAny<Action<SearchRequestDescriptor<DynamicResponse>>>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(searchResponse);
			var startUtc = new DateTime(2024, 5, 30, 14, 0, 0, DateTimeKind.Utc);
			var endUtc = new DateTime(2024, 6, 30, 14, 0, 0, DateTimeKind.Utc);

			var userCount = await plugin.GetUserCount(enterpriseCode, serverCode, startUtc, endUtc, mockClient.Object);
			Assert.AreEqual(42, userCount, "Expected user count to be 42, but got a different value.");
		}

		[Test]
		public void TestGetTransaction()
		{
			string defectiveQueryHashesJson = JsonConvert.SerializeObject(testDefectiveQueryHashesDictionary);
			var testSettings = new PluginSettings()
			{
				Parameters = new PluginParameter[]
				{
					new PluginParameter("ElasticEndpoint", "https://r.test-1.es.wtg.ws:443"),
					new PluginParameter("ElasticApiKey", "k++HhEyGTc2m1oj0+YUCAGzS43thuQnQDJqwNkeCH45ceU8sGwy+6ZzpSAwk/i8mLDMJBJcZd9feTPjq3dJhYW8wI5x0ocuuWlFwa1Vo8UG8rGHwszb/6foHBxPoJ8Wm2COM/0IO8bW4P6JKy+DN5PNFlU8fb29asLNsRmu0U4s="),
					new PluginParameter("CpuMonitoringIndex", "idx-*-sqlcpumonitoring-test-ro*"),
					new PluginParameter("GatewayNonBilledUsage", "idx-*-*-ehubgatewaynonbilledusage*"),
					new PluginParameter("DefectiveQueryHashesDictionary", defectiveQueryHashesJson)
				}
			};

			//var prodsettings = new PluginSettings()
			//{
			//	Parameters = new PluginParameter[]
			//	{
			//		new PluginParameter("ElasticEndpoint", "https://r.prod-1.es.wtg.ws:443"),
			//		new PluginParameter("ElasticApiKey", "GuphOosrYRrRoWF9Bs3RJtzWi+TwhjYvzZkXgC1Wn2umapfvhqwyL2pkQ5lVnOtMvqVNTuOm2mdFPwVtWigtq9Gc/QIfofzn2Ohmee8hT52H91dadB8hmfqte4Z8RGi+mIb6rmmIcGH/ApYCOf+ZoRXzVUuO/H3oRJwNE0zgmc0="),
			//		new PluginParameter("CpuMonitoringIndex", "idx-*-prod-sqlcpumonitoring-prod*"),
			//		new PluginParameter("GatewayNonBilledUsage", "idx-*-*-ehubgatewaynonbilledusage*"),
			//		new PluginParameter("BatchSize", "20"),
			//		new PluginParameter("DefectiveQueryHashesDictionary", defectiveQueryHashesJson),
			//		new PluginParameter("ElasticRetryMaxAttempts", "1440"),
			//		new PluginParameter("ElasticRetryDelayInSecond", "60")
			//	}
			//};

			var plugin = new Plugins.WGReport.Plugin();
			plugin.UpdateSettings(testSettings);
			var logger = new LoggerConfiguration().WriteTo.File("C:\\TestElasticSearchSearchHalfYear.log", rollOnFileSizeLimit: true, retainedFileCountLimit: 10).CreateLogger();
			plugin.SetLoggerFactory(new LoggerFactory().AddSerilog(logger));
			var start = new DateTime(2025, 7, 01, 00, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2025, 7, 30, 01, 0, 0, DateTimeKind.Utc);
			var timeStampedTransactions = plugin.GetTransactions(start, end).ToList();
		}


		[Test]
		public void TestGeneratedTransaction()
		{
			var plugin = new Plugins.Test.Plugin();
			var start = DateTime.Today;
			var end = start.AddDays(1);
			var transaction = plugin.GetTransactions(start, end).Single();
			Assert.That(transaction.TimeStamp, Is.EqualTo(end));
			Assert.That(transaction.BillingTransaction.BillableCount, Is.EqualTo(1));
			Assert.That(transaction.BillingTransaction.Branch, Is.Null);
			Assert.That(transaction.BillingTransaction.Category, Is.EqualTo("TST"));
			Assert.That(transaction.BillingTransaction.ClientID, Is.EqualTo("TSTTSTTST"));
			Assert.That(transaction.BillingTransaction.ClientNumber, Is.Null);
			Assert.That(transaction.BillingTransaction.ClientStaffCode, Is.Null);
			Assert.That(transaction.BillingTransaction.PriceItemCode, Is.EqualTo("TST"));
			Assert.That(transaction.BillingTransaction.Reference1, Is.EqualTo("TEST"));
			Assert.That(transaction.BillingTransaction.Reference2, Is.EqualTo("TEST"));
			Assert.That(transaction.BillingTransaction.Reference3, Is.EqualTo("TEST"));
			Assert.That(transaction.BillingTransaction.Reference4, Is.EqualTo("TEST"));
			Assert.That(transaction.BillingTransaction.Reference5, Is.Null);
			Assert.That(transaction.BillingTransaction.ReportingSource, Is.EqualTo("MSC"));
			Assert.That(transaction.BillingTransaction.ServiceOccuredUTC, Is.EqualTo(start));
			Assert.That(transaction.BillingTransaction.Version, Is.EqualTo(0));
			Assert.That(transaction.BillingTransaction.MessageTrackingID, Is.EqualTo("C3D5AA7A-0FCA-42C5-A164-28757C25D0FA"));
		}
	}
}
