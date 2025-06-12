using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.eServices.Billing.WcfService.Hangfire.BillingDatabase;
using Castle.Windsor;
using Common.Logging;
using Common.Logging.Simple;
using Hangfire;
using Moq;
using NUnit.Framework;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	[TestFixture]
	public class HangfireDatabaseProcessorTests
	{
		[Test]
		public void TestCommandShouldCancelIfCancellationTokenRequested_WithHangfireInMemory()
		{
			LogManager.Adapter = new ConsoleOutLoggerFactoryAdapter
			{
				Level = LogLevel.All,
				ShowLevel = true,
				ShowDateTime = true,
				DateTimeFormat = "yyyy-MM-dd HH:mm:ss:fff"
			};

			GlobalConfiguration.Configuration.UseInMemoryStorage();

			RecurringJob.AddOrUpdate("test-job-id", "default", () => new BillingDatabaseProcessor(false, false, LogManager.GetLogger<HangfireDatabaseProcessorTests>()).StartProcess(CancellationToken.None, null), "*/45 * * * *", new RecurringJobOptions()
			{
				TimeZone = TimeZoneInfo.Utc
			});

			using (var _ = new BackgroundJobServer(JobStorage.Current))
			using (var sw = new StringWriter())
			{
				Console.SetOut(sw);
				// Lock the process calling UpdateChargeable
				using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["BillingContext"].ConnectionString))
				{
					con.Open();
					using (var cmd = con.CreateCommand())
					{
						cmd.CommandText = "EXEC [edi].[LockStagingForSession] 300000";
						cmd.ExecuteNonQuery();
					}


					// Trigger recurring job 
					var jobId = RecurringJob.TriggerJob("test-job-id");

					var maxRetries = 10;
					for (int i = 0; i < maxRetries; i++)
					{
						var jobDetails = JobStorage.Current.GetMonitoringApi().JobDetails(jobId);

						if (jobDetails.History.First().StateName == "Processing")
						{
							BackgroundJob.Delete(jobId);
							// Wait for the job to be cancelled gracefully
							Task.Delay(5000).Wait();
							break;
						}

						Task.Delay(2000).Wait();
						if (i + 1 >= maxRetries)
						{
							throw new InvalidOperationException("No processing job");
						}

					}


					using (var cmd = con.CreateCommand())
					{
						cmd.CommandText = "EXEC [edi].[UnlockStagingForSession]";
						cmd.ExecuteNonQuery();
					}
				}

				Assert.That(sw.ToString().Contains("Cancellation requested by user"));
			}
		}

		[SetUp]
		public void Setup()
		{
			var configurationMock = new Mock<IConfigurationProvider>();
			configurationMock.Setup(x => x.BilledELKKafkaTopic).Returns("billed-topic");
			configurationMock.Setup(x => x.UsageELKKafkaTopic).Returns("usage-topic");
			configurationMock.Setup(x => x.MaxRetries).Returns(3);
			configurationMock.Setup(x => x.RetryTimeoutInMilliseconds).Returns(10);
			configurationMock.Setup(x => x.ELKResubmissionBatchSize).Returns(3);
			var containerMock = new Mock<IWindsorContainer>();
			containerMock.Setup(x => x.Resolve<IConfigurationProvider>()).Returns(configurationMock.Object);
			Global.WindsorContainer = containerMock.Object;
		}
	}
}
