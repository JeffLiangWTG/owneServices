using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using CargoWise.Billing.API;
using CargoWise.Billing.Kafka.API;
using CargoWise.Billing.Client;
using Confluent.Kafka;
using log4net.Appender;
using log4net.Core;
using Microsoft.Extensions.Logging;
using Moq;
using WTG.ErrorReporting;
using ErrorCode = Confluent.Kafka.ErrorCode;

namespace CargoWise.Billing.CollectorService.Plugin.Tests
{
	[TestFixture]
    public class PluginControllerTest
    {
		[Test]
		public void TestIntervalsAndSettings()
		{
			var pluginType = typeof(DummyBillingTransactionsPlugin);
			var now = DateTime.UtcNow;
			var lastTransactionTimestamp = now.AddMinutes(-16);
			var lastSuccessfulRun = now.AddMinutes(-11);
			var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, true);
			var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun, lastTransactionTimestamp);

			var timeStamp1 = now.AddMinutes(-5);
			var timeStamp2 = now.AddMinutes(-6);
			var timeStamp3 = now.AddMinutes(-7);
			DummyBillingTransactionsPlugin.Transactions = new[]
			{
				new TimeStampedTransaction(timeStamp3, new BillingTransaction()),
				new TimeStampedTransaction(timeStamp1, new BillingTransaction()),
				new TimeStampedTransaction(timeStamp2, new BillingTransaction()),
			};

			using (var controller = CreatePluginController(pluginType.Assembly, settings, state, new TestClock { UtcNow = now }))
			{
				var eventRaised = false;
				controller.LastSuccessfulRunChanged += delegate
				{ eventRaised = true; };
				controller.Start();

				Assert.IsTrue(eventRaised, "LastSuccessfulRunChanged event was raised.");
				var currentState = controller.CurrentState;
				Assert.That(currentState.LastTransactionTimestamp, Is.EqualTo(timeStamp1));
				Assert.That(currentState.LastSuccessfulRun, Is.EqualTo(lastSuccessfulRun.AddMinutes(10)));
				var plugin = DummyBillingTransactionsPlugin.LastInstance;
				Assert.That(plugin.LastStart, Is.EqualTo(lastSuccessfulRun));
				Assert.That(plugin.LastEnd, Is.EqualTo(lastSuccessfulRun.AddMinutes(10)));
			}
		}

		[Test]
		public void TestIntervalEdges()
		{
			var pluginType = typeof(DummyBillingTransactionsPlugin);
			var now = DateTime.UtcNow;
			var lastTransactionTimestamp = now.AddMinutes(-10);
			var lastSuccessfulRun = now.AddMinutes(-10);
			var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, true);
			var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun, lastTransactionTimestamp);

			var timeStamp1 = now.AddMilliseconds(-1);
			var timeStamp2 = now;
			var timeStamp3 = now.AddMilliseconds(1);

			var transaction1 = new TimeStampedTransaction(timeStamp1, new BillingTransaction());
			var transaction2 = new TimeStampedTransaction(timeStamp2, new BillingTransaction());
			var transaction3 = new TimeStampedTransaction(timeStamp3, new BillingTransaction());

			DummyBillingTransactionsPlugin.Transactions = new[]
			{
				transaction1,
				transaction2,
				transaction3
			};

			using (var controller = CreatePluginController(pluginType.Assembly, settings, state, new TestClock { UtcNow = now.AddHours(1) }))
			{
				controller.Start();
				var plugin = DummyBillingTransactionsPlugin.LastInstance;
				Assert.AreEqual(3, plugin.FoundTransactions.Count());
			}
		}

		[Test]
        public void TestIntervalEdgesRoundingToWholeSecond()
        {
            var pluginType = typeof(DummyBillingTransactionsPlugin);

            var now = DateTime.UtcNow;
            var lastTransactionTimestamp = now.AddMinutes(-10).AddTicks(4961);
            var lastSuccessfulRun = now.AddMinutes(-10).AddTicks(4961);

            var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, true);
            var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun, lastTransactionTimestamp);

            DummyBillingTransactionsPlugin.Transactions = new[]
            {
                new TimeStampedTransaction(now.AddMinutes(-6), new BillingTransaction()),
                new TimeStampedTransaction(now.AddMinutes(-5), new BillingTransaction()),
                new TimeStampedTransaction(now.AddMinutes(-7), new BillingTransaction()),
            };

            var clock = new TestClock() { UtcNow = now };
            using (var controller = CreatePluginController(pluginType.Assembly, settings, state, clock))
            {
                controller.Start();
                memoryAppender = GetMemoryAppender();
				var plugin = DummyBillingTransactionsPlugin.LastInstance;
                Assert.AreEqual(0, plugin.FoundTransactions.Count());

                var infoLogMessagesList = memoryAppender.GetEvents().Where(e => e.Level == Level.Info).ToList();
                Assert.That(infoLogMessagesList[0].MessageObject.ToString(), Is.EqualTo("Next run scheduled in 00:00:00:01"));

                clock.UtcNow = now.AddSeconds(1);
                Assert.AreEqual(3, plugin.FoundTransactions.Count());

                var end = lastSuccessfulRun + settings.Interval;
                var infoLogMessagesList2 = memoryAppender.GetEvents().Where(e => e.Level == Level.Info).ToList();
                Assert.That(infoLogMessagesList2.Count, Is.EqualTo(4));
                Assert.That(infoLogMessagesList2[0].MessageObject.ToString(), Is.EqualTo("Next run scheduled in 00:00:00:01"));
                Assert.That(infoLogMessagesList2[1].MessageObject.ToString(), Is.EqualTo("Transactions being retrieved for period '" + lastSuccessfulRun + "' (exclusive) to '" + end + "'(inclusive)"));
                Assert.That(infoLogMessagesList2[2].MessageObject.ToString(), Is.EqualTo("3 billing transactions found. 3 submitted successfully. 0 in error. "));
                Assert.That(infoLogMessagesList2[3].MessageObject.ToString(), Is.EqualTo("Next run scheduled in 00:00:09:59"));
            }
        }

		[Test]
		public void TestTransactionPluginLogger_GeneralException_ShouldNotReportToIssueManagerIfRetrySuccessfully()
		{
			var now = DateTime.UtcNow;
			var lastTransactionTimestamp = now.AddMinutes(-25);
			var lastSuccessfulRun = now.AddMinutes(-11);
			var pluginType = typeof(DummyBillingTransactionsPlugin);
			var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, true);
			var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun, lastTransactionTimestamp);
			var errorBillingTransaction = new BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ_",
				PriceItemCode = "TST",
				ServiceOccuredUTC = now.AddMinutes(-6),
				ReportingSource = "TST"
			};

			var exception = new Exception("GeneralException");
			clientMock.Setup(_ => _.AddTransactionRange(It.IsAny<IEnumerable<BillingTransaction>>())).Throws(exception);

			DummyBillingTransactionsPlugin.Transactions = new[]
			{
				new TimeStampedTransaction(now.AddMinutes(-6), errorBillingTransaction),
				new TimeStampedTransaction(now.AddMinutes(-5), new BillingTransaction()),
				new TimeStampedTransaction(now.AddMinutes(-7), new BillingTransaction()),
			};

			using (var controller = CreatePluginController(pluginType.Assembly, settings, state, new TestClock { UtcNow = now }))
			{
				controller.Start();
			}

			var end = lastSuccessfulRun + settings.Interval;
			memoryAppender = GetMemoryAppender();
			var infoLogMessagesList = memoryAppender.GetEvents().Where(e => e.Level == Level.Info).ToList();
			Assert.That(infoLogMessagesList.Count, Is.EqualTo(3));
			Assert.That(infoLogMessagesList[0].MessageObject.ToString(), Is.EqualTo("Next run scheduled in 00:00:00:00"));
			Assert.That(infoLogMessagesList[1].MessageObject.ToString(), Is.EqualTo("Transactions being retrieved for period '" + lastSuccessfulRun + "' (exclusive) to '" + end + "'(inclusive)"));
			Assert.That(infoLogMessagesList[2].MessageObject.ToString(), Is.EqualTo("Next run scheduled in 00:00:00:10"));

			clientMock.VerifyAll();
		}

		[Test]
		public void TestTransactionPluginLogger_GeneralException_ShouldReportToIssueManagerIfRetryFailed()
		{
			var now = DateTime.UtcNow;
			var lastTransactionTimestamp = now.AddMinutes(-25);
			var lastSuccessfulRun = now.AddMinutes(-11);
			var pluginType = typeof(DummyBillingTransactionsPlugin);
			var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, true);
			settings.MaxRetryAttempts = 0;
			var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun, lastTransactionTimestamp);
			var errorBillingTransaction = new BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ_",
				PriceItemCode = "TST",
				ServiceOccuredUTC = now.AddMinutes(-6),
				ReportingSource = "TST"
			};

			var exception = new Exception("GeneralException");
			clientMock.Setup(_ => _.AddTransactionRange(It.IsAny<IEnumerable<BillingTransaction>>())).Throws(exception);

			DummyBillingTransactionsPlugin.Transactions = new[]
			{
						new TimeStampedTransaction(now.AddMinutes(-6), errorBillingTransaction),
						new TimeStampedTransaction(now.AddMinutes(-5), new BillingTransaction()),
						new TimeStampedTransaction(now.AddMinutes(-7), new BillingTransaction()),
					};

			using (var controller = CreatePluginController(pluginType.Assembly, settings, state, new TestClock { UtcNow = now }))
			{
				controller.Start();
			}

			var end = lastSuccessfulRun + settings.Interval;

			memoryAppender = GetMemoryAppender();
			var infoLogMessagesList = memoryAppender.GetEvents().Where(e => e.Level == Level.Info).ToList();
			Assert.That(infoLogMessagesList.Count, Is.EqualTo(3));
			Assert.That(infoLogMessagesList[0].MessageObject.ToString(), Is.EqualTo("Next run scheduled in 00:00:00:00"));
			Assert.That(infoLogMessagesList[1].MessageObject.ToString(), Is.EqualTo("Transactions being retrieved for period '" + lastSuccessfulRun + "' (exclusive) to '" + end + "'(inclusive)"));
			Assert.That(infoLogMessagesList[2].MessageObject.ToString(), Is.EqualTo("Next run scheduled in 00:00:09:00"));

			var errorLogMessage = memoryAppender.GetEvents().Where(e => e.Level == Level.Error).ToList().Single();
			Assert.That(errorLogMessage.MessageObject.ToString(), Is.EqualTo("Error happened while collecting and sending transactions."));
			Assert.That(errorLogMessage.ExceptionObject.Message, Is.EqualTo("GeneralException"));

			var warningLogMessage = memoryAppender.GetEvents().Where(e => e.Level == Level.Warn).ToList().Single();
			Assert.That(warningLogMessage.MessageObject.ToString(), Is.EqualTo("Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'"));

			clientMock.VerifyAll();
		}

		[Test]
		public void TestProcessBatch_CountTransactionsInBatchDoesNotReIterateTransactions()
		{
			var now = DateTime.UtcNow;
			DummyBillingTransactionsPlugin.Transactions = new[]
			{
						new TimeStampedTransaction(now.AddMinutes(-6), new BillingTransaction()),
						new TimeStampedTransaction(now.AddMinutes(-5), new BillingTransaction())
					};

			var start = now.AddMinutes(-25);
			var lastSuccessfulRun = now.AddMinutes(-11);
			var lastTransactionTimestamp = now.AddMinutes(-11);
			var pluginType = typeof(DummyBillingTransactionsPlugin);
			var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, true);
			var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun, lastTransactionTimestamp);

			using (var controller = CreatePluginController(pluginType.Assembly, settings, state, new TestClock { UtcNow = now }))
			{
				controller.Start();
			}

			var end = lastSuccessfulRun + settings.Interval;

			memoryAppender = GetMemoryAppender();
			var infoLogMessagesList = memoryAppender.GetEvents().Where(e => e.Level == Level.Info).ToList();
			Assert.That(infoLogMessagesList.Count, Is.EqualTo(4));
			Assert.That(infoLogMessagesList[0].MessageObject.ToString(), Is.EqualTo("Next run scheduled in 00:00:00:00"));
			Assert.That(infoLogMessagesList[1].MessageObject.ToString(), Is.EqualTo("Transactions being retrieved for period '" + lastSuccessfulRun + "' (exclusive) to '" + end + "'(inclusive)"));
			Assert.That(infoLogMessagesList[2].MessageObject.ToString(), Is.EqualTo("2 billing transactions found. 2 submitted successfully. 0 in error. "));
			Assert.That(infoLogMessagesList[3].MessageObject.ToString(), Is.EqualTo("Next run scheduled in 00:00:09:00"));
			var traceLogMessageList = memoryAppender.GetEvents().Where(e => e.Level == Level.Debug).ToList();
			Assert.That(traceLogMessageList.Count, Is.EqualTo(1));
			Assert.That(traceLogMessageList[0].MessageObject.ToString(), Is.EqualTo("Iterated transactions 1 times"));
		}

		[Test]
		public void TestTransactionPluginLogger_ValidationException_ShouldReportToIssueManager()
		{
			var pluginType = typeof(DummyBillingTransactionsPlugin);
			var now = DateTime.UtcNow;
			var lastTransactionTimestamp = now.AddMinutes(-16);
			var lastSuccessfulRun = now.AddMinutes(-11);
			var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, true);
			settings.MaxRetryAttempts = 0;
			var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun, lastTransactionTimestamp);
			var billingTransaction = new BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ_",
				PriceItemCode = "TST",
				ServiceOccuredUTC = now.AddMinutes(-6),
				ReportingSource = "TST"
			};
			DummyBillingTransactionsPlugin.Transactions = new[]
			{
						new TimeStampedTransaction(now.AddMinutes(-5), billingTransaction)
					};

			var validationException = new ValidationException("", new[] { "The ClientID field is required.", "The Reference1 field is required." });
			clientMock.Setup(_ => _.AddTransaction(billingTransaction)).Throws(validationException);

			using (var controller = CreatePluginController(pluginType.Assembly, settings, state, new TestClock { UtcNow = now }))
			{
				controller.Start();
			}

			memoryAppender = GetMemoryAppender();
			var errorMessage = string.Format("Validation failed for transaction [{0}]:\r\n{1}\r\n{2}",
				billingTransaction,
				"  The ClientID field is required.\r\n  The Reference1 field is required.",
				validationException);
			var errorLogMessage = memoryAppender.GetEvents().Where(e => e.Level == Level.Error).ToList().Single();
			Assert.That(errorLogMessage.MessageObject.ToString(), Is.EqualTo("Error happened while collecting and sending transactions."));

			var exceptionObject = errorLogMessage.ExceptionObject as ValidationException;
			Assert.That(exceptionObject.Message, Is.EqualTo("Validation failed for transactions"));
			Assert.That(exceptionObject.Errors.Single(), Is.EqualTo(errorMessage));

			var warningLogMessagesList = memoryAppender.GetEvents().Where(e => e.Level == Level.Warn).ToList();
			Assert.That(warningLogMessagesList.Count, Is.EqualTo(1));
			Assert.That(warningLogMessagesList[0].MessageObject.ToString(), Is.EqualTo("Did not log to Issue Manager due to missing app setting: 'IssueManagerUri'"));

			clientMock.VerifyAll();
		}

		[TestCaseSource(nameof(KnownExceptions))]
		public void TestTransactionPluginLogger_KnownException_ShouldNotReportToIssueManager(Exception ex)
		{
			var pluginType = typeof(DummyBillingTransactionsPlugin);
			var now = DateTime.UtcNow;
			var lastTransactionTimestamp = now.AddMinutes(-16);
			var lastSuccessfulRun = now.AddMinutes(-11);
			var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, true);
			settings.MaxRetryAttempts = 0;
			var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun, lastTransactionTimestamp);
			var billingTransaction = new BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ_",
				PriceItemCode = "TST",
				ServiceOccuredUTC = now.AddMinutes(-6),
				ReportingSource = "TST"
			};
			DummyBillingTransactionsPlugin.Transactions = new[]
			{
				new TimeStampedTransaction(now.AddMinutes(-5), billingTransaction)
			};

			clientMock.Setup(_ => _.AddTransaction(billingTransaction)).Throws(ex);

			var knownExceptions = new Dictionary<string, string[]>()
			{
				{ex.GetType().FullName, null}
			};
			using (var controller = CreatePluginController(pluginType.Assembly, settings, state, new TestClock { UtcNow = now }, knownExceptions))
			{
				controller.Start();
			}

			memoryAppender = GetMemoryAppender();
			var errorLogMessage = memoryAppender.GetEvents().Where(e => e.Level == Level.Error).FirstOrDefault();
			Assert.IsNull(errorLogMessage);

			var warningLogMessagesList = memoryAppender.GetEvents().Where(e => e.Level == Level.Warn).ToList();
			Assert.That(warningLogMessagesList.Count, Is.EqualTo(1));
			Assert.That(warningLogMessagesList[0].MessageObject.ToString(), Does.StartWith("Error happened while collecting and sending transactions."));
			clientMock.VerifyAll();
		}

		[TestCaseSource(nameof(KnownExceptionRetryTestCases))]
		public string TestTransactionPluginLogger_KnownException_Retry(DateTime now, DateTime lastSuccessfulRun)
		{
			var pluginType = typeof(DummyBillingTransactionsPlugin);
			var lastTransactionTimestamp = lastSuccessfulRun.AddSeconds(10);
			var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, true);
			settings.MaxRetryAttempts = 0;
			settings.IntervalMinutes = 1;
			settings.RetryIntervalSeconds = 5;
			var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun, lastTransactionTimestamp);
			var billingTransaction = new BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ_",
				PriceItemCode = "TST",
				ServiceOccuredUTC = lastSuccessfulRun.AddSeconds(59),
				ReportingSource = "TST"
			};
			DummyBillingTransactionsPlugin.Transactions = new[]
			{
				new TimeStampedTransaction(now.AddMinutes(-1), billingTransaction)
			};

			clientMock.Setup(_ => _.AddTransaction(billingTransaction)).Throws(new TimeoutException());

			var knownExceptions = new Dictionary<string, string[]>()
			{
				{typeof(TimeoutException).FullName, null}
			};
			var clock = new TestClock { UtcNow = now };
			using (var controller = CreatePluginController(pluginType.Assembly, settings, state, clock, knownExceptions))
			{
				controller.Start();
			}

			memoryAppender = GetMemoryAppender();
			var errorLogMessage = memoryAppender.GetEvents().Where(e => e.Level == Level.Error).FirstOrDefault();
			Assert.IsNull(errorLogMessage);

			var messages = memoryAppender.GetEvents().Select(x => x.MessageObject).ToList();
			Assert.That(messages.Count, Is.EqualTo(5));
			Assert.That(messages[0].ToString(), Does.StartWith("Next run scheduled in 00:00:00:00"));
			Assert.That(messages[1].ToString(), Does.StartWith($"Transactions being retrieved for period '{lastSuccessfulRun}' (exclusive) to '{lastSuccessfulRun.AddMinutes(1)}'(inclusive)"));
			Assert.That(messages[2].ToString(), Does.StartWith("Error happened while collecting and sending transactions."));
			return messages[3].ToString();
		}

		[Test]
		public void TestActive()
		{
			var pluginType = typeof(DummyBillingTransactionsPlugin);
			var now = DateTime.UtcNow;
			var lastTransactionTimestamp = now.AddMinutes(-16);
			var lastSuccessfulRun = now.AddMinutes(-11);
			var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, false);
			var state = PluginControllerTestHelper.PluginControllerState(pluginType, lastSuccessfulRun, lastTransactionTimestamp);

			Assert.That(DummyBillingTransactionsPlugin.LastInstance, Is.Null);
			using (var controller = CreatePluginController(pluginType.Assembly, settings, state, new TestClock { UtcNow = now }))
			{
				Assert.That(DummyBillingTransactionsPlugin.LastInstance, Is.Not.Null);
				controller.Start();
				Assert.That(DummyBillingTransactionsPlugin.LastInstance.GetTransactionsCallsCount, Is.EqualTo(0));
			}
		}

		[Test]
		public void TestProcessBatch()
		{
			var pluginType = typeof(DummyBillingTransactionsPlugin);
			var now = DateTime.UtcNow;
			var lastTransactionTimestamp = now.AddMinutes(-16);
			var lastSuccessfulRun = now.AddMinutes(-11);
			var settings = new PluginControllerSettings
			{
				Key = pluginType.FullName + ".Key",
				Active = true,
				TypeName = pluginType.FullName,
				IntervalMinutes = 10,
				RetryIntervalSeconds = 10,
				MaxRetryAttempts = 10,
				SendUsageTransaction = true
			};

			var state = new PluginControllerState
			{
				Key = pluginType.FullName + ".Key",
				LastSuccessfulRun = lastSuccessfulRun,
				LastTransactionTimestamp = lastTransactionTimestamp
			};

			var sendCount = PluginController.BatchSize + 5;
			var toSend = new List<BillingTransaction>(sendCount);
			var toSendTimeStamped = new List<TimeStampedTransaction>(sendCount);
			for (int i = 0; i < sendCount; ++i)
			{
				var billingTransaction = new BillingTransaction
				{
					BillableCount = 2,
					ClientID = "ABCDEFXYZ_",
					PriceItemCode = "TST",
					ServiceOccuredUTC = now.AddMinutes(-6),
					ReportingSource = "TST",
					Reference1 = "",
					MessageTrackingID = (i + 1).ToString()
				};
				var usage = new UsageTransaction()
				{
					UsageCount = 1,
					EnterpriseCode = "ABC",
					CompanyCode = "DEF",
					ServerCode = "XYZ",
					UsageCode = "TST",
					ServiceOccuredUTC = now.AddMinutes(-6),
					AdditionalRefs = $@"{{""ReportingSource"":""TST"",""MessageTrackingID"": ""{i + 1}""}}"
				};
				toSend.Add(billingTransaction);
				toSendTimeStamped.Add(new TimeStampedTransaction(now.AddMinutes(-5), billingTransaction, usage));
			}

			DummyBillingTransactionsPlugin.Transactions = toSendTimeStamped.ToArray();

			var transactions = new List<BillingTransaction>();
			var usageTransactions = new List<ELKTransaction>();
			Expression<Action<IBillingKafkaClient>> sendUsageAction = client =>
				client.SendUsageInfoToELK(It.Is<string>(topic => topic.Equals("TestTopic")), It.IsAny<string>(),
					It.IsAny<UsageTransaction>(), It.IsAny<Action<DeliveryReport<string, ELKTransaction>>>());
			clientMock.Setup(_ => _.AddTransactionRange(It.IsAny<IEnumerable<BillingTransaction>>()))
				.Callback<IEnumerable<BillingTransaction>>(t => transactions.AddRange(t));
			kafkaClientMock.Setup(sendUsageAction).Callback<string, string, UsageTransaction, Action<DeliveryReport<string, ELKTransaction>>>(
				(topic, id, trn, report) => usageTransactions.Add(trn));

			using (var controller = CreatePluginController(pluginType.Assembly, settings, state, new TestClock { UtcNow = now }))
			{
				controller.Start();
			}

			Assert.That(transactions.Count, Is.EqualTo(sendCount));
			Assert.That(usageTransactions.Count, Is.EqualTo(sendCount));
			for (int i = 0; i < sendCount; ++i)
			{
				Assert.AreEqual((i + 1).ToString(), transactions[i].MessageTrackingID);
				Assert.AreEqual($@"{{""ReportingSource"":""TST"",""MessageTrackingID"": ""{i + 1}""}}", usageTransactions[i].AdditionalRefs);
			}

			clientMock.VerifyAll();
			kafkaClientMock.VerifyAll();
			kafkaClientMock.Verify(sendUsageAction, Times.Exactly(usageTransactions.Count));
		}

		[Test]
		public void TestProcessBatch_FailUsageTransaction()
		{
			var pluginType = typeof(DummyBillingTransactionsPlugin);
			var now = DateTime.UtcNow;
			var lastTransactionTimestamp = now.AddMinutes(-16);
			var lastSuccessfulRun = now.AddMinutes(-11);
			var settings = new PluginControllerSettings
			{
				Key = pluginType.FullName + ".Key",
				Active = true,
				TypeName = pluginType.FullName,
				IntervalMinutes = 10,
				RetryIntervalSeconds = 10,
				MaxRetryAttempts = 10,
				SendUsageTransaction = true,
				SendBillingTransaction = false
			};

			var state = new PluginControllerState
			{
				Key = pluginType.FullName + ".Key",
				LastSuccessfulRun = lastSuccessfulRun,
				LastTransactionTimestamp = lastTransactionTimestamp
			};

			var sendCount = 10;
			var toSendTimeStamped = new List<TimeStampedTransaction>(sendCount);
			for (int i = 0; i < sendCount; ++i)
			{
				var usage = new UsageTransaction()
				{
					UsageCount = 1,
					EnterpriseCode = "ABC",
					CompanyCode = i % 3 == 0 ? "BR" + i : "MSG",
					ServerCode = "XYZ",
					UsageCode = "TST",
					ServiceOccuredUTC = new DateTime(2024, 5, 1, 0, 0, 0, DateTimeKind.Utc),
					AdditionalRefs = $@"{{""ReportingSource"":""TST"",""MessageTrackingID"": ""{i + 1}""}}"
				};
				toSendTimeStamped.Add(new TimeStampedTransaction(now.AddMinutes(-5), null, usage));
			}

			DummyBillingTransactionsPlugin.Transactions = toSendTimeStamped.ToArray();

			var usageTransactions = new List<ELKTransaction>();
			Expression<Action<IBillingKafkaClient>> sendUsageAction = client =>
				client.SendUsageInfoToELK(It.Is<string>(topic => topic.Equals("TestTopic")), It.IsAny<string>(),
					It.IsAny<UsageTransaction>(), It.IsAny<Action<DeliveryReport<string, ELKTransaction>>>());
			clientMock.Setup(_ => _.AddUsageTransaction(It.IsAny<UsageTransaction>())).Callback<UsageTransaction>(trn =>
			{
				if (trn.CompanyCode != "BR0")
				{
					throw new Exception("Test exception message");
				}

				usageTransactions.Add(trn);
			});
			kafkaClientMock.Setup(sendUsageAction).Callback<string, string, UsageTransaction, Action<DeliveryReport<string, ELKTransaction>>>(
				(topic, id, trn, report) =>
				{
					if (trn.CompanyCode.Equals("MSG"))
					{
						report(new DeliveryReport<string, ELKTransaction>()
						{
							Message = new Message<string, ELKTransaction> { Key = id, Value = trn },
							Error = new Error(ErrorCode.Local_MsgTimedOut)
						});
					}
					else
					{
						var result = new DeliveryReport<string, ELKTransaction>()
						{
							Message = new Message<string, ELKTransaction> { Key = id, Value = trn },
							Error = new Error(ErrorCode.BrokerNotAvailable)
						};
						report(result);
					}
				});

			using (var controller = CreatePluginController(pluginType.Assembly, settings, state, new TestClock { UtcNow = now }))
			{
				controller.Start();
			}

			memoryAppender = GetMemoryAppender();
			var warnLogMessage = memoryAppender.GetEvents().Where(e => e.Level == Level.Warn).FirstOrDefault();
			Assert.That(warnLogMessage.MessageObject.ToString(), Is.EqualTo(@"Error happened while collecting and sending transactions."));
			Assert.That(warnLogMessage.ExceptionObject.ToString(), Does.StartWith(@"System.InvalidOperationException: Kafka Error: Local: Message timed out. Billing service exception message: Test exception message
Count: 1, ServiceOccuredUTC: 2024-05-01T00:00:00, AdditionalRefs: {""ReportingSource"":""TST"",""MessageTrackingID"": ""2""}, EnterpriseCode: ABC, ServerCode: XYZ, Environment: , CompanyCode: MSG, CompanyName: , BranchCode: , UsageCode: TST
Count: 1, ServiceOccuredUTC: 2024-05-01T00:00:00, AdditionalRefs: {""ReportingSource"":""TST"",""MessageTrackingID"": ""3""}, EnterpriseCode: ABC, ServerCode: XYZ, Environment: , CompanyCode: MSG, CompanyName: , BranchCode: , UsageCode: TST
Count: 1, ServiceOccuredUTC: 2024-05-01T00:00:00, AdditionalRefs: {""ReportingSource"":""TST"",""MessageTrackingID"": ""5""}, EnterpriseCode: ABC, ServerCode: XYZ, Environment: , CompanyCode: MSG, CompanyName: , BranchCode: , UsageCode: TST
Count: 1, ServiceOccuredUTC: 2024-05-01T00:00:00, AdditionalRefs: {""ReportingSource"":""TST"",""MessageTrackingID"": ""6""}, EnterpriseCode: ABC, ServerCode: XYZ, Environment: , CompanyCode: MSG, CompanyName: , BranchCode: , UsageCode: TST
Count: 1, ServiceOccuredUTC: 2024-05-01T00:00:00, AdditionalRefs: {""ReportingSource"":""TST"",""MessageTrackingID"": ""8""}, EnterpriseCode: ABC, ServerCode: XYZ, Environment: , CompanyCode: MSG, CompanyName: , BranchCode: , UsageCode: TST
Count: 1, ServiceOccuredUTC: 2024-05-01T00:00:00, AdditionalRefs: {""ReportingSource"":""TST"",""MessageTrackingID"": ""9""}, EnterpriseCode: ABC, ServerCode: XYZ, Environment: , CompanyCode: MSG, CompanyName: , BranchCode: , UsageCode: TST
Kafka Error: Broker: Broker not available. Billing service exception message: Test exception message
Count: 1, ServiceOccuredUTC: 2024-05-01T00:00:00, AdditionalRefs: {""ReportingSource"":""TST"",""MessageTrackingID"": ""4""}, EnterpriseCode: ABC, ServerCode: XYZ, Environment: , CompanyCode: BR3, CompanyName: , BranchCode: , UsageCode: TST
Count: 1, ServiceOccuredUTC: 2024-05-01T00:00:00, AdditionalRefs: {""ReportingSource"":""TST"",""MessageTrackingID"": ""7""}, EnterpriseCode: ABC, ServerCode: XYZ, Environment: , CompanyCode: BR6, CompanyName: , BranchCode: , UsageCode: TST
Count: 1, ServiceOccuredUTC: 2024-05-01T00:00:00, AdditionalRefs: {""ReportingSource"":""TST"",""MessageTrackingID"": ""10""}, EnterpriseCode: ABC, ServerCode: XYZ, Environment: , CompanyCode: BR9, CompanyName: , BranchCode: , UsageCode: TST"));

			Assert.That(1, Is.EqualTo(usageTransactions.Count));
			Assert.AreEqual($@"{{""ReportingSource"":""TST"",""MessageTrackingID"": ""1""}}", usageTransactions[0].AdditionalRefs);

			clientMock.VerifyAll();
			kafkaClientMock.VerifyAll();
			kafkaClientMock.Verify(sendUsageAction, Times.Exactly(sendCount));
			kafkaClientMock.Verify(x => x.FlushProducers(), Times.Once);
		}

		[Test]
		public void TestProcessBatch_ValidationException_ShouldReportToIssueManager()
		{
			var pluginType = typeof(DummyBillingTransactionsPlugin);
			var now = DateTime.UtcNow;
			var lastTransactionTimestamp = now.AddMinutes(-16);
			var lastSuccessfulRun = now.AddMinutes(-11);
			var settings = PluginControllerTestHelper.PluginControllerSettings(pluginType, true);
			settings.MaxRetryAttempts = 0;
			var state = new PluginControllerState
			{
				Key = pluginType.FullName + ".Key",
				LastSuccessfulRun = lastSuccessfulRun,
				LastTransactionTimestamp = lastTransactionTimestamp
			};

			var validCount = PluginController.BatchSize - 1;
			var toSendTimeStamped = new List<TimeStampedTransaction>();
			for (int i = 0; i < validCount; ++i)
			{
				var billingTransaction = new BillingTransaction
				{
					BillableCount = 2,
					ClientID = "ABCDEFXYZ_",
					PriceItemCode = "TST",
					ServiceOccuredUTC = now.AddMinutes(-6),
					ReportingSource = "TST",
					Reference1 = "",
					MessageTrackingID = (i + 1).ToString()
				};
				toSendTimeStamped.Add(new TimeStampedTransaction(now.AddMinutes(-5), billingTransaction));
			}

			var invalidTransaction = new BillingTransaction
			{
				BillableCount = 2,
				ClientID = "ABCDEFXYZ_",
				PriceItemCode = "TST",
				ServiceOccuredUTC = now.AddMinutes(-6),
				ReportingSource = "TST",
				Reference1 = "",
				MessageTrackingID = "99999"
			};
			toSendTimeStamped.Insert(1, new TimeStampedTransaction(now.AddMinutes(-5), invalidTransaction));

			DummyBillingTransactionsPlugin.Transactions = toSendTimeStamped.ToArray();

			var transactions = new List<BillingTransaction>();
			var validationException = new ValidationException("", new[] { "The ClientID field is required.", "The Reference1 field is required." });
			clientMock.Setup(_ => _.AddTransactionRange(It.IsAny<IEnumerable<BillingTransaction>>()))
				.Throws(validationException);
			clientMock.Setup(_ => _.AddTransaction(It.Is<BillingTransaction>(x => x.MessageTrackingID == "99999")))
				.Throws(validationException);
			clientMock.Setup(_ => _.AddTransaction(It.Is<BillingTransaction>(x => x.MessageTrackingID != "99999")))
				.Callback<BillingTransaction>(t => transactions.Add(t));

			string issueDescription = "";

			errorReportingClientMock.Setup(x => x.ServiceUri).Returns(new Uri("http://abc.com"));
			errorReportingClientMock.Setup(_ => _.PostCrashReportAsync(It.IsAny<IOpaqueErrorReport>(), It.IsAny<CancellationToken>()))
							.Callback((IOpaqueErrorReport errBuilder, CancellationToken cancellationToken) =>
							{
								var reportBuilder = errBuilder as EnterpriseErrorReportBuilder;
								using (var memStream = new MemoryStream())
								{
									var task = reportBuilder.WriteToAsync(memStream);
									task.ConfigureAwait(false);
									task.Wait();
									memStream.Seek(0, SeekOrigin.Begin);
									using (var reader = new StreamReader(memStream))
									{
										var t2 = reader.ReadToEndAsync();
										t2.ConfigureAwait(false);
										t2.Wait();
										issueDescription = t2.Result;
									}
								}
							}).Returns(Task.CompletedTask);

			using (var controller = CreatePluginController(pluginType.Assembly, settings, state, new TestClock { UtcNow = now }))
			{
				controller.Start();
			}

			Assert.That(transactions.Count, Is.EqualTo(validCount));
			for (int i = 0; i < validCount; ++i)
			{
				Assert.AreEqual((i + 1).ToString(), transactions[i].MessageTrackingID);
			}

			memoryAppender = GetMemoryAppender();
			var errorMessage = string.Format("Validation failed for transaction [{0}]:\r\n{1}\r\n{2}",
				invalidTransaction,
				"  The ClientID field is required.\r\n  The Reference1 field is required.",
				validationException);
			var errorLogMessage = memoryAppender.GetEvents().Where(e => e.Level == Level.Error).ToList().Single();
			Assert.That(errorLogMessage.MessageObject.ToString(), Is.EqualTo("Error happened while collecting and sending transactions."));

			var exceptionObject = errorLogMessage.ExceptionObject as ValidationException;
			Assert.That(exceptionObject.Message, Is.EqualTo("Validation failed for transactions"));
			Assert.That(exceptionObject.Errors.Single(), Is.EqualTo(errorMessage));

			Assert.IsTrue(issueDescription.Contains("The ClientID field is required."));
			Assert.IsTrue(issueDescription.Contains("The Reference1 field is required."));

			clientMock.VerifyAll();
		}

		static Exception[] KnownExceptions = { new TimeoutException() };

		static IEnumerable KnownExceptionRetryTestCases
		{
			get
			{
				yield return new TestCaseData(new DateTime(2024, 9, 1, 1, 2, 0, DateTimeKind.Utc), new DateTime(2024, 9, 1, 1, 0, 57, DateTimeKind.Utc))
					.SetName("TestTransactionPluginLogger_KnownException_RetryInterval")
					.Returns("Next run scheduled in 00:00:00:05");
				yield return new TestCaseData(new DateTime(2024, 9, 1, 1, 2, 0, DateTimeKind.Utc), new DateTime(2024, 9, 1, 1, 0, 03, DateTimeKind.Utc))
					.SetName("TestTransactionPluginLogger_KnownException_PluginInterval")
					.Returns("Next run scheduled in 00:00:00:03");
			}
		}

		[SetUp]
        public void SetUp()
        {
            clientMock = new Mock<IBillingServiceClient>();
            kafkaClientMock = new Mock<IBillingKafkaClient>();
            errorReportingClientMock = new Mock<IErrorReportingClient>();
            errorReportingClientMock.Setup(x => x.ServiceUri).Returns(new Uri("", UriKind.Relative));
            loggerFactory = new LoggerFactory().AddLog4Net();
        }

        [TearDown]
        public void TearDown()
        {
            DummyBillingTransactionsPlugin.Reset();
        }

        static MemoryAppender GetMemoryAppender() => log4net.LogManager.GetRepository().GetAppenders().OfType<MemoryAppender>().Single();

        PluginController CreatePluginController(Assembly assembly, PluginControllerSettings settings, PluginControllerState state, TestClock clock, Dictionary<string, string[]> knownExcepions = null) => new PluginController(assembly, settings, state,
	        globalSettings, clientMock.Object, kafkaClientMock.Object, clock, new object(), loggerFactory, errorReportingClientMock.Object, knownExcepions);

		MemoryAppender memoryAppender;
        Mock<IBillingServiceClient> clientMock;
        Mock<IBillingKafkaClient> kafkaClientMock;
        Dictionary<string, string> globalSettings = new()
        {
	        { "UsageELKKafkaTopic", "TestTopic" }
        };
		ILoggerFactory loggerFactory;
		Mock<IErrorReportingClient> errorReportingClientMock;
    }
}
