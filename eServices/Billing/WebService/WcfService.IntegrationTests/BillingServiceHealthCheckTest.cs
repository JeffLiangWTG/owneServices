using System;
using System.Linq;
using Cronos;
using Hangfire;
using Hangfire.States;
using Hangfire.SqlServer;
using Hangfire.Storage;

namespace CargoWise.eServices.Billing.WcfService.IntegrationTests
{
	using System.Collections.Generic;
	using System.IO;
	using System.Net;
	using System.Threading;
	using CargoWise.Billing.API;
	using CargoWise.Billing.Kafka.API;
	using CargoWise.eServices.Billing.Tests.Common;
	using CargoWise.eServices.TestHelpers.Database.Common;
	using Confluent.Kafka;
	using NUnit.Framework;

	[TestFixture]
	[WithBillingWcfService]
	public class BillingServiceHealthCheckTest : TestBase
	{
		[Test]
		public void TestBillingServiceHealthCheck()
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			using (var client = WithBillingWcfServiceAttribute.Current.CreateClient())
			{
				client.Ping();
			}

			var recurringMonthlyAggregationJob = JobStorage.GetConnection().GetRecurringJobs().First(x => x.Id == "ProcessBillingDatabaseMonthlyAggregation");
			var recurringUpdateBillingCubePartialJob = JobStorage.GetConnection().GetRecurringJobs().First(x => x.Id == "ProcessBillingDatabaseUpdateBillingCubePartial");
			RecurringJobManager.Trigger("BillingJobManager");
			RecurringJobManager.Trigger("ProcessBillingDatabase");
			TestHelper.DoWithRetry(() => CheckRecurringJobRunsAndCompleted("ProcessBillingDatabase"), "AwaitProcessBillingDatabaseFinished", TimeSpan.FromSeconds(5));
			// 1st monthly aggregation has not been triggered
			using (var statusResponse = (HttpWebResponse)GetRequest("wtg/status").GetResponse())
			{
				Assert.That(statusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
				Assert.IsFalse(statusResponse.IsFromCache);
				Assert.That(statusResponse.ContentType, Is.EqualTo("text/plain;charset=utf-8"));

				using (var responseStream = statusResponse.GetResponseStream())
				{
					using (var reader = new StreamReader(responseStream))
					{
						var result = reader.ReadToEnd();
						var items = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
						Assert.AreEqual("INFO(BillingWebService): Service is alive.", items[0]);
						Assert.AreEqual("INFO(BillingBackgroundJob): Healthy", items[1]);
						Assert.That(items[2], Does.StartWith("INFO(Staging): Healthy. The time difference in hours "));
						Assert.That(items[3], Does.StartWith($"INFO(ProcessBillingDatabaseMonthlyAggregation): Healthy. The job will run first time at {DateTime.SpecifyKind(recurringMonthlyAggregationJob.NextExecution.Value, DateTimeKind.Utc).ToLocalTime():yyyy-MM-dd HH:mm:sszzz}"));
						Assert.That(items[4], Does.StartWith($"INFO(ProcessBillingDatabaseUpdateBillingCubePartial): Healthy. The job will run first time at {DateTime.SpecifyKind(recurringUpdateBillingCubePartialJob.NextExecution.Value, DateTimeKind.Utc).ToLocalTime():yyyy-MM-dd HH:mm:sszzz}"));
						Assert.AreEqual("INFO(ELKResubmission): Healthy", items[5]);
						Assert.AreEqual("INFO(BillingKafkaPartition0): Healthy. Current: -1001, Latest: 0, Lag: 0", items[6]);
						Assert.AreEqual("INFO(BillingKafkaPartition1): Healthy. Current: -1001, Latest: 0, Lag: 0", items[7]);
						Assert.AreEqual("INFO(BillingKafkaPartition2): Healthy. Current: -1001, Latest: 0, Lag: 0", items[8]);
						Assert.AreEqual("INFO(BillingKafkaPartition3): Healthy. Current: -1001, Latest: 0, Lag: 0", items[9]);
					}
				}
			}

			var config = new Dictionary<string, string>()
			{
				{ "bootstrap.servers", WithBillingWcfServiceAttribute.Current.KafkaBrokers},
				{ "linger.ms", "50" },
				{ "message.timeout.ms", "10000"}
			};

			var producerConfig = new ProducerConfig(config);

			using (var kafkaClient = new BillingKafkaClient(producerConfig))
			{
				var dt = new DateTime(2024, 8, 1, 0, 0, 0, DateTimeKind.Utc);
				for (int i = 0; i < 500; i++)
				{
					var transaction = new BillingTransaction
					{
						BillableCount = 2,
						Category = "TST",
						ClientID = "ABCDEFXYZ",
						ClientNumber = "98765432100123456789",
						ClientStaffCode = "ABC",
						PriceItemCode = "DEF",
						Reference1 = "REFERENCE 1",
						Reference2 = "REFERENCE 2",
						Reference3 = "REFERENCE 3",
						ReportingSource = "XYZ",
						ServiceOccuredUTC = dt.AddSeconds(i),
						MessageTrackingID = "0",
					};

					kafkaClient.SendBillingInfoToKafka("billing-topic", transaction.MessageTrackingID, transaction,
						report =>
						{
							Assert.AreEqual(ErrorCode.NoError, report.Error.Code);
						});
				}
			}

			TestHelper.DoWithRetry(() => CheckKafkaBacklogProcessed(), "AwaitKafkaBacklogProcessed", TimeSpan.FromSeconds(5));
			using (var statusResponse = (HttpWebResponse)GetRequest("wtg/status").GetResponse())
			{
				Assert.That(statusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
				Assert.IsFalse(statusResponse.IsFromCache);
				Assert.That(statusResponse.ContentType, Is.EqualTo("text/plain;charset=utf-8"));

				using (var responseStream = statusResponse.GetResponseStream())
				{
					using (var reader = new StreamReader(responseStream))
					{
						var result = reader.ReadToEnd();
						Assert.That(result, Does.Contain("Healthy. Current: 500, Latest: 500, Lag: 0"));
					}
				}
			}
		}

		[Test]
		public void TestBillingServiceHealthCheck_ELKResubmission()
		{
			//Exceeding ELK resubmission threshold
			int sendCount = 11;
			var toSend = new List<UsageTransaction>();
			for (int i = 1; i <= sendCount; ++i)
			{
				var transaction = new UsageTransaction
				{
					BranchCode = "KLM",
					CompanyCode = "ABC",
					CompanyName = $"Test{i}",
					EnterpriseCode = "TST",
					Environment = "TST",
					ServerCode = "TST",
					UsageCode = "USG",
					UsageCount = i,
					ServiceOccuredUTC = DateTime.Now
				};

				toSend.Add(transaction);
			}

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				using (var client = WithBillingWcfServiceAttribute.Current.CreateClient())
				{
					client.AddUsageTransactionRange(toSend);
				}

				var actual = BillingDataTestHelper.SelectELKTransactions();
				Assert.That(actual.Rows.Count, Is.EqualTo(sendCount));
			}

			using (var failedElkStatusResponse = (HttpWebResponse)GetRequest("wtg/status").GetResponse())
			{
				Assert.That(failedElkStatusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
				Assert.IsFalse(failedElkStatusResponse.IsFromCache);
				Assert.That(failedElkStatusResponse.ContentType, Is.EqualTo("text/plain;charset=utf-8"));

				using (var responseStream = failedElkStatusResponse.GetResponseStream())
				{
					using (var reader = new StreamReader(responseStream))
					{
						var result = reader.ReadToEnd();
						var items = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
						Assert.AreEqual("ERROR(ELKResubmission): Backlog has exceeded its threshold (10)", items[5]);
					}
				}
			}

			// Healthy
			RecurringJobManager.Trigger("ProcessFailedELKMessages");
			TestHelper.DoWithRetry(() => CheckRecurringJobRunsAndCompleted("ProcessFailedELKMessages"), "AwaitProcessFailedELKMessagesFinished", TimeSpan.FromSeconds(5));
			Assert.That(BillingDataTestHelper.SelectELKTransactions().Rows.Count, Is.EqualTo(0), message: "Failed ELK messages should be processed");

			using (var failedElkStatusResponse = (HttpWebResponse)GetRequest("wtg/status").GetResponse())
			{
				Assert.That(failedElkStatusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
				Assert.IsFalse(failedElkStatusResponse.IsFromCache);
				Assert.That(failedElkStatusResponse.ContentType, Is.EqualTo("text/plain;charset=utf-8"));

				using (var responseStream = failedElkStatusResponse.GetResponseStream())
				{
					using (var reader = new StreamReader(responseStream))
					{
						var result = reader.ReadToEnd();
						var items = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
						Assert.AreEqual("INFO(ELKResubmission): Healthy", items[5]);
					}
				}
			}
		}

		[Test]
		public void TestBillingServiceHealthCheck_ProcessingBillingJobs()
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			using (var client = WithBillingWcfServiceAttribute.Current.CreateClient())
			{
				client.Ping();
			}

			RecurringJobManager.Trigger("ProcessBillingDatabase");
			TestHelper.DoWithRetry(() => CheckRecurringJobRunsAndCompleted("ProcessBillingDatabase"), "AwaitProcessBillingDatabaseFinished", TimeSpan.FromSeconds(5));

			var recurringMonthlyAggregationJob = JobStorage.GetConnection().GetRecurringJobs().First(x => x.Id == "ProcessBillingDatabaseMonthlyAggregation");
			Assert.AreEqual("0 6 1 * *", recurringMonthlyAggregationJob.Cron);
			Assert.IsNull(recurringMonthlyAggregationJob.LastExecution);
			Assert.That(recurringMonthlyAggregationJob.LastJobId, Is.Null.Or.Empty);

			var recurringUpdateBillingCubePartialJob = JobStorage.GetConnection().GetRecurringJobs().First(x => x.Id == "ProcessBillingDatabaseUpdateBillingCubePartial");
			Assert.AreEqual("30 9 * * *", recurringUpdateBillingCubePartialJob.Cron);
			Assert.IsNull(recurringUpdateBillingCubePartialJob.LastExecution);
			Assert.That(recurringUpdateBillingCubePartialJob.LastJobId, Is.Null.Or.Empty);

			RecurringJobManager.Trigger("ProcessBillingDatabaseMonthlyAggregation");
			RecurringJobManager.Trigger("ProcessBillingDatabaseUpdateBillingCubePartial");
			TestHelper.DoWithRetry(() => CheckRecurringJobRunsAndCompleted("ProcessBillingDatabaseMonthlyAggregation"), "AwaitProcessBillingDatabaseMonthlyAggregationFinished", TimeSpan.FromSeconds(5));
			TestHelper.DoWithRetry(() => CheckRecurringJobRunsAndCompleted("ProcessBillingDatabaseUpdateBillingCubePartial"), "AwaitProcessBillingDatabaseUpdateBillingCubePartialFinished", TimeSpan.FromSeconds(5));

			recurringMonthlyAggregationJob = JobStorage.GetConnection().GetRecurringJobs().First(x => x.Id == "ProcessBillingDatabaseMonthlyAggregation");
			Assert.IsNotNull(recurringMonthlyAggregationJob.LastExecution);
			Assert.That(recurringMonthlyAggregationJob.LastJobId, Is.Not.Null.And.Not.Empty);

			recurringUpdateBillingCubePartialJob = JobStorage.GetConnection().GetRecurringJobs().First(x => x.Id == "ProcessBillingDatabaseUpdateBillingCubePartial");
			Assert.IsNotNull(recurringUpdateBillingCubePartialJob.LastExecution);
			Assert.That(recurringUpdateBillingCubePartialJob.LastJobId, Is.Not.Null.And.Not.Empty);

			var monthlyAggregationJobDateTime = CronExpression.Parse(recurringMonthlyAggregationJob.Cron).GetOccurrences(DateTime.UtcNow.AddYears(-1), DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(recurringMonthlyAggregationJob.TimeZoneId)).Last();
			var updateBillingCubeJobDateTime = CronExpression.Parse(recurringUpdateBillingCubePartialJob.Cron).GetOccurrences(DateTime.UtcNow.AddYears(-1), DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(recurringUpdateBillingCubePartialJob.TimeZoneId)).Last();
			monthlyAggregationJobDateTime = DateTime.SpecifyKind(monthlyAggregationJobDateTime, DateTimeKind.Utc);
			updateBillingCubeJobDateTime = DateTime.SpecifyKind(updateBillingCubeJobDateTime, DateTimeKind.Utc);

			var succeededMonthlyJobId = recurringMonthlyAggregationJob.LastJobId;
			var succeededBillingCubeJobId = recurringUpdateBillingCubePartialJob.LastJobId;
			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.UpdateJobStateDateTime(con, succeededMonthlyJobId, "Succeeded", monthlyAggregationJobDateTime.AddSeconds(30));
				BillingDataTestHelper.UpdateJobStateDateTime(con, succeededBillingCubeJobId, "Succeeded", updateBillingCubeJobDateTime.AddMinutes(10));
			}

			RecurringJobManager.Trigger("ProcessBillingDatabaseMonthlyAggregation");
			RecurringJobManager.Trigger("ProcessBillingDatabaseUpdateBillingCubePartial");
			TestHelper.DoWithRetry(() => CheckRecurringJobRunsAndCompleted("ProcessBillingDatabaseMonthlyAggregation"), "AwaitProcessBillingDatabaseMonthlyAggregationFinished", TimeSpan.FromSeconds(5));
			TestHelper.DoWithRetry(() => CheckRecurringJobRunsAndCompleted("ProcessBillingDatabaseUpdateBillingCubePartial"), "AwaitProcessBillingDatabaseUpdateBillingCubePartialFinished", TimeSpan.FromSeconds(5));

			recurringMonthlyAggregationJob = JobStorage.GetConnection().GetRecurringJobs().First(x => x.Id == "ProcessBillingDatabaseMonthlyAggregation");
			recurringUpdateBillingCubePartialJob = JobStorage.GetConnection().GetRecurringJobs().First(x => x.Id == "ProcessBillingDatabaseUpdateBillingCubePartial");
			using (var writeOnlyTransaction = JobStorage.GetConnection().CreateWriteTransaction())
			{
				writeOnlyTransaction.SetJobState(recurringMonthlyAggregationJob.LastJobId, new FailedState(new Exception("Test monthly job error")));
				writeOnlyTransaction.SetJobState(recurringUpdateBillingCubePartialJob.LastJobId, new FailedState(new Exception("Test billing cube job error")));
				writeOnlyTransaction.Commit();
			}

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.DeleteJobState(con, recurringMonthlyAggregationJob.LastJobId, "Succeeded");
				BillingDataTestHelper.DeleteJobState(con, recurringUpdateBillingCubePartialJob.LastJobId, "Succeeded");
				BillingDataTestHelper.UpdateJobStateDateTime(con, recurringMonthlyAggregationJob.LastJobId, "Failed", monthlyAggregationJobDateTime.AddSeconds(55));
				BillingDataTestHelper.UpdateJobStateDateTime(con, recurringUpdateBillingCubePartialJob.LastJobId, "Failed", updateBillingCubeJobDateTime.AddSeconds(1555));
			}

			using (var statusResponse = (HttpWebResponse)GetRequest("wtg/status").GetResponse())
			{
				Assert.That(statusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
				Assert.IsFalse(statusResponse.IsFromCache);
				Assert.That(statusResponse.ContentType, Is.EqualTo("text/plain;charset=utf-8"));

				using (var responseStream = statusResponse.GetResponseStream())
				{
					using (var reader = new StreamReader(responseStream))
					{
						var result = reader.ReadToEnd();
						var items = result.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
						Assert.AreEqual("INFO(BillingWebService): Service is alive.", items[0]);
						Assert.AreEqual("INFO(BillingBackgroundJob): Healthy", items[1]);
						Assert.That(items[2], Does.StartWith("INFO(Staging): Healthy. The time difference in hours "));
						Assert.That(items[3], Does.StartWith($"INFO(ProcessBillingDatabaseMonthlyAggregation): Healthy. Succeeded at {monthlyAggregationJobDateTime.ToLocalTime():yyyy-MM}-01 06:00:30"));
						Assert.That(items[4], Does.StartWith($"INFO(ProcessBillingDatabaseUpdateBillingCubePartial): Healthy. Succeeded at {updateBillingCubeJobDateTime.ToLocalTime():yyyy-MM-dd} 09:40:00"));
					}
				}
			}

			using (var con = BillingDataTestHelper.GetNewOpenConnection())
			{
				BillingDataTestHelper.DeleteJobState(con, succeededMonthlyJobId);
				BillingDataTestHelper.DeleteJobState(con, succeededBillingCubeJobId);
			}

			using (var statusResponse = (HttpWebResponse)GetRequest("wtg/status").GetResponse())
			{
				Assert.That(statusResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
				Assert.IsFalse(statusResponse.IsFromCache);
				Assert.That(statusResponse.ContentType, Is.EqualTo("text/plain;charset=utf-8"));

				using (var responseStream = statusResponse.GetResponseStream())
				{
					using (var reader = new StreamReader(responseStream))
					{
						var result = reader.ReadToEnd();
						var items = result.Split(new [] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
						Assert.AreEqual("INFO(BillingWebService): Service is alive.", items[0]);
						Assert.AreEqual("INFO(BillingBackgroundJob): Healthy", items[1]);
						Assert.That(items[2], Does.StartWith("INFO(Staging): Healthy. The time difference in hours "));
						Assert.That(items[3], Does.StartWith($"ERROR(ProcessBillingDatabaseMonthlyAggregation): The job failed at {monthlyAggregationJobDateTime.ToLocalTime():yyyy-MM}-01 06:00:55"));
						Assert.That(items[4], Does.StartWith($"ERROR(ProcessBillingDatabaseUpdateBillingCubePartial): The job failed at {updateBillingCubeJobDateTime.ToLocalTime():yyyy-MM-dd} 09:55:55"));
					}
				}
			}
		}

		static WebRequest GetRequest(string path)
		{
			var request = HttpWebRequest.Create(WithBillingWcfServiceAttribute.Current.GetUrl(path));
			request.Credentials = CredentialCache.DefaultCredentials;
			return request;
		}
	}
}
