using CargoWise.eServices.Billing.WcfService.Hangfire.BillingDatabase;
using Hangfire.Common;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace CargoWise.eServices.Billing.WcfService.Tests
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Threading;
	using CargoWise.eServices.Billing.WcfService.Hangfire;
	using CargoWise.eServices.Billing.WcfService.HealthCheck;
	using CargoWise.eServices.Monitoring.HealthCheck.API;
	using CargoWise.eServices.Monitoring.HealthCheck.API.Test;
	using Castle.Windsor;
	using Moq;
	using NUnit.Framework;

	class ProcessingBillingDbUpdateBillingCubePartialHealthCheckItemProviderTest : HealthCheckItemProviderBaseClass<ProcessingBillingDatabaseUpdateBillingCubePartialHealthCheckItemProvider>
	{
		[Test]
		[TestCaseSource(nameof(SuccessTestCases))]
		public string TestUpdateBillingCubePartialHealthCheckItemProvider_Success(DateTime dtNow, KeyValuePair<string, SucceededJobDto>[] jobs, KeyValuePair<string, ProcessingJobDto>[] processingJobs)
		{
			mockStorageConnection.Setup(x => x.GetAllEntriesFromHash("recurring-job:ProcessBillingDatabaseUpdateBillingCubePartial"))
				.Returns(new Dictionary<string, string>()
				{
					{ "LastJobId", "1" },
					{ "LastExecution", "2024-05-31T20:00:30Z" },
					{ "LastJobState", "Succeeded"},
					{ "NextExecution", "2024-06-30T20:00:30Z" },
					{ "Cron", Cron },
					{ "TimeZoneId", TimeZoneId },
					{ "Job", JobType }
				});
			mockMonitoringAPI.Setup(x => x.SucceededListCount()).Returns(jobs.Length);
			mockMonitoringAPI.Setup(x => x.SucceededJobs(0, jobs.Length)).Returns(new JobList<SucceededJobDto>(jobs));
			mockMonitoringAPI.Setup(x => x.ProcessingCount()).Returns(processingJobs.Length);
			mockMonitoringAPI.Setup(x => x.ProcessingJobs(0, processingJobs.Length)).Returns(new JobList<ProcessingJobDto>(processingJobs));
			mockMonitoringAPI.Setup(x => x.FailedCount()).Returns(0);
			var mockDtProvider = new Mock<IDateTimeProvider>();
			mockDtProvider.Setup(x => x.DateTimeNow).Returns(dtNow);
			mockDtProvider.Setup(x => x.UtcNow).Returns(dtNow.ToUniversalTime());
			mockProvider.Setup(x => x.DateTimeProvider).Returns(mockDtProvider.Object).Verifiable();
			var provider = mockProvider.Object;

			Assert.That(provider.Name, Is.EqualTo(HealthCheckName));

			var checkItem = provider.CheckHealthAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.OK));

			return checkItem.Description;
		}

		[Test]
		[TestCaseSource(nameof(FailTestCases))]
		public string TestUpdateBillingCubePartialHealthCheckItemProvider_Error(DateTime dtNow, KeyValuePair<string, FailedJobDto>[] failedJobs, KeyValuePair<string, SucceededJobDto>[] succeededJobs, KeyValuePair<string, ProcessingJobDto>[] processingJobs)
		{
			mockStorageConnection.Setup(x => x.GetAllEntriesFromHash("recurring-job:ProcessBillingDatabaseUpdateBillingCubePartial"))
				.Returns(new Dictionary<string, string>()
				{
					{ "LastJobId", "1" },
					{ "LastExecution", "2024-05-31T20:00:30Z" },
					{ "LastJobState", "Failed"},
					{ "NextExecution", "2024-06-30T20:00:30Z" },
					{ "Cron", Cron },
					{ "TimeZoneId", TimeZoneId },
					{ "Job", JobType }
				});

			mockMonitoringAPI.Setup(x => x.FailedCount()).Returns(failedJobs.Length);
			mockMonitoringAPI.Setup(x => x.FailedJobs(0, failedJobs.Length)).Returns(new JobList<FailedJobDto>(failedJobs));

			mockMonitoringAPI.Setup(x => x.SucceededListCount()).Returns(succeededJobs.Length);
			mockMonitoringAPI.Setup(x => x.SucceededJobs(0, succeededJobs.Length)).Returns(new JobList<SucceededJobDto>(succeededJobs));

			mockMonitoringAPI.Setup(x => x.ProcessingCount()).Returns(processingJobs.Length);
			mockMonitoringAPI.Setup(x => x.ProcessingJobs(0, processingJobs.Length)).Returns(new JobList<ProcessingJobDto>(processingJobs));

			var mockDtProvider = new Mock<IDateTimeProvider>();
			mockDtProvider.Setup(x => x.DateTimeNow).Returns(dtNow);
			mockDtProvider.Setup(x => x.UtcNow).Returns(dtNow.ToUniversalTime());
			mockProvider.Setup(x => x.DateTimeProvider).Returns(mockDtProvider.Object).Verifiable();
			var provider = mockProvider.Object;

			Assert.That(provider.Name, Is.EqualTo(HealthCheckName));

			var checkItem = provider.CheckHealthAsync().ConfigureAwait(false).GetAwaiter().GetResult();
			Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));

			return checkItem.Description;
		}

		[SetUp]
		public void Setup()
		{
			var configProviderMock = new Mock<IConfigurationProvider>();
			configProviderMock.Setup(x => x.MaxBillingProcessingTimeInMinutes).Returns(30);
			var containerMock = new Mock<IWindsorContainer>();
			containerMock.Setup(x => x.Resolve<IMonitoringApi>()).Returns(mockMonitoringAPI.Object);
			containerMock.Setup(x => x.Resolve<IConfigurationProvider>()).Returns(configProviderMock.Object);
			Global.WindsorContainer = containerMock.Object;
			var ids = new HashSet<string> { HealthCheckName };
			mockStorageConnection.Setup(x => x.GetAllItemsFromSet("recurring-jobs")).Returns(ids);
			mockProvider.Setup(x => x.GetStorageConnection()).Returns(mockStorageConnection.Object);
		}

		[TearDown]
		public void TearDown()
		{
			mockMonitoringAPI.Reset();
			mockStorageConnection.Reset();
			mockProvider.Reset();
		}

		protected override List<string> GetAllKindsOfDescriptions()
		{
			var descriptions = new List<string>();
			mockStorageConnection.Setup(x => x.GetAllEntriesFromHash("recurring-job:ProcessBillingDatabaseUpdateBillingCubePartial"))
				.Returns(new Dictionary<string, string>()
				{
					{ "LastJobId", "1" },
					{ "Cron", Cron },
					{ "TimeZoneId", TimeZoneId },
					{ "Job", JobType }
				});
			var mockDtProvider = new Mock<IDateTimeProvider>();
			mockDtProvider.Setup(x => x.UtcNow).Returns(new DateTime(2024, 5, 23, 5, 0, 0, DateTimeKind.Utc));
			var provider = mockProvider.Object;

			mockProvider.Setup(x => x.DateTimeProvider).Returns(mockDtProvider.Object).Verifiable();
			var checkItem = provider.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			mockDtProvider.Setup(x => x.DateTimeNow).Throws(new InvalidOperationException());
			checkItem = provider.CheckHealthAsync().Result;
			descriptions.Add(checkItem.Description);

			return descriptions;
		}

		readonly Mock<IMonitoringApi> mockMonitoringAPI = new Mock<IMonitoringApi>();
		readonly Mock<ProcessingBillingDatabaseUpdateBillingCubePartialHealthCheckItemProvider> mockProvider = new Mock<ProcessingBillingDatabaseUpdateBillingCubePartialHealthCheckItemProvider>
		{
			CallBase = true
		};
		readonly Mock<IStorageConnection> mockStorageConnection = new Mock<IStorageConnection>();

		public static IEnumerable SuccessTestCases
		{
			get
			{
				yield return new TestCaseData(new DateTime(2024, 2, 1, 6, 0, 0, DateTimeKind.Local), SucceededJobs, Array.Empty<KeyValuePair<string, ProcessingJobDto>>())
				{
					TestName = "TestUpdateBillingCubePartialHealthCheckItemProvider_DaylightSaving_Success",
					ExpectedResult = "Healthy. Succeeded at 2024-01-31 09:31:00+11:00",
				};
				yield return new TestCaseData(new DateTime(2024, 6, 29, 8, 40, 0, DateTimeKind.Local), SucceededJobs, Array.Empty<KeyValuePair<string, ProcessingJobDto>>())
				{
					TestName = "TestUpdateBillingCubePartialHealthCheckItemProvider_SucceededYesterday_HasNotRun_Success",
					ExpectedResult = "Healthy. Succeeded at 2024-06-28 10:30:00+10:00"
				};
				yield return new TestCaseData(new DateTime(2024, 6, 29, 9, 30, 0, DateTimeKind.Local), SucceededJobs, Array.Empty<KeyValuePair<string, ProcessingJobDto>>())
				{
					TestName = "TestUpdateBillingCubePartialHealthCheckItemProvider_ScheduleNow_Success",
					ExpectedResult = "Healthy. Succeeded at 2024-06-28 10:30:00+10:00"
				};
				yield return new TestCaseData(new DateTime(2024, 6, 29, 9, 31, 0, DateTimeKind.Local), SucceededJobs, Array.Empty<KeyValuePair<string, ProcessingJobDto>>())
				{
					TestName = "TestUpdateBillingCubePartialHealthCheckItemProvider_HasSucceeded_Success",
					ExpectedResult = "Healthy. Succeeded at 2024-06-29 09:30:17+10:00"
				};
				yield return new TestCaseData(new DateTime(2024, 6, 30, 9, 35, 0, DateTimeKind.Local), SucceededJobs, ProcessingJobs)
				{
					TestName = "TestUpdateBillingCubePartialHealthCheckItemProvider_IsRunning_Success",
					ExpectedResult = "Processing billing job is running. Job Id: 9999"
				};
			}
		}

		public static IEnumerable FailTestCases
		{
			get
			{
				yield return new TestCaseData(new DateTime(2024, 6, 29, 9, 29, 59, DateTimeKind.Local), FailedJobs, Array.Empty<KeyValuePair<string, SucceededJobDto>>(), Array.Empty<KeyValuePair<string, ProcessingJobDto>>())
				{
					TestName = "TestUpdateBillingCubePartialHealthCheckItemProvider_FailedYesterday_HasNotRun_Error",
					ExpectedResult = "The job failed at 2024-06-28 10:30:00+10:00. See https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/10748/Processing-Billing-database"
				};
				yield return new TestCaseData(new DateTime(2024, 6, 29, 9, 40, 0, DateTimeKind.Local), FailedJobs, Array.Empty<KeyValuePair<string, SucceededJobDto>>(), Array.Empty<KeyValuePair<string, ProcessingJobDto>>())
				{
					TestName = "TestUpdateBillingCubePartialHealthCheckItemProvider_HasJustFailed_Error",
					ExpectedResult = "The job failed at 2024-06-29 09:35:00+10:00. See https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/10748/Processing-Billing-database"
				};
				yield return new TestCaseData(new DateTime(2024, 6, 30, 9, 31, 0, DateTimeKind.Local), Array.Empty<KeyValuePair<string, FailedJobDto>>(), SucceededJobs, Array.Empty<KeyValuePair<string, ProcessingJobDto>>())
				{
					TestName = "TestUpdateBillingCubePartialHealthCheckItemProvider_DidnotSucceed_Error",
					ExpectedResult = "No succeeded job. See https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki/10748/Processing-Billing-database"
				};
				yield return new TestCaseData(new DateTime(2024, 6, 30, 10, 0, 10, DateTimeKind.Local), Array.Empty<KeyValuePair<string, FailedJobDto>>(), SucceededJobs, ProcessingJobs)
				{
					TestName = "TestUpdateBillingCubePartialHealthCheckItemProvider_LongRun_Error",
					ExpectedResult = "Processing billing exceeded 30 minutes. Job Id: 9999"
				};
			}
		}

		static readonly KeyValuePair<string, SucceededJobDto>[] SucceededJobs =
		{
			new KeyValuePair<string, SucceededJobDto>("", new SucceededJobDto{ Job = new Job(typeof(BillingDatabaseProcessor).GetMethod("RecurringProcessUpdateBillingCubePartial"), new object[]{ null, CancellationToken.None }), SucceededAt = new DateTime(2024, 1, 30, 22, 31, 0, DateTimeKind.Unspecified)}),
			new KeyValuePair<string, SucceededJobDto>("", new SucceededJobDto{ Job = new Job(typeof(BillingDatabaseProcessor).GetMethod("RecurringProcessUpdateBillingCubePartial"), new object[]{ null, CancellationToken.None }), SucceededAt = new DateTime(2024, 6, 28, 0, 30, 0, DateTimeKind.Unspecified)}),
			new KeyValuePair<string, SucceededJobDto>("", new SucceededJobDto{ Job = new Job(typeof(BillingDatabaseProcessor).GetMethod("RecurringProcessUpdateBillingCubePartial"), new object[]{ null, CancellationToken.None }), SucceededAt = new DateTime(2024, 6, 28, 23, 30, 17, DateTimeKind.Unspecified)})
		};

		static readonly KeyValuePair<string, FailedJobDto>[] FailedJobs =
		{
			new KeyValuePair<string, FailedJobDto>("", new FailedJobDto{ Job = new Job(typeof(BillingDatabaseProcessor).GetMethod("RecurringProcessUpdateBillingCubePartial"), new object[]{ null, CancellationToken.None }), FailedAt = new DateTime(2024, 6, 28, 0, 30, 0, DateTimeKind.Unspecified)}),
			new KeyValuePair<string, FailedJobDto>("", new FailedJobDto{ Job = new Job(typeof(BillingDatabaseProcessor).GetMethod("RecurringProcessUpdateBillingCubePartial"), new object[]{ null, CancellationToken.None }), FailedAt = new DateTime(2024, 6, 28, 23, 35, 0, DateTimeKind.Unspecified)})
		};

		static readonly KeyValuePair<string, ProcessingJobDto>[] ProcessingJobs =
		{
			new KeyValuePair<string, ProcessingJobDto>("9999", new ProcessingJobDto{ Job = new Job(typeof(BillingDatabaseProcessor).GetMethod("RecurringProcessUpdateBillingCubePartial"), new object[]{ null, CancellationToken.None }), StartedAt = new DateTime(2024, 6, 29, 23, 30, 8, DateTimeKind.Unspecified)}),
		};

		const string Cron = "30 9 * * *";
		const string TimeZoneId = "AUS Eastern Standard Time";
		const string JobType = "{\"Type\":\"CargoWise.eServices.Billing.WcfService.Hangfire.BillingDatabase.BillingDatabaseProcessor, CargoWise.eServices.Billing.WcfService, Version=1.0.0.0, Culture=neutral, PublicKeyToken=4f570df270576350\",\"Method\":\"RecurringProcessUpdateBillingCubePartial\",\"ParameterTypes\":\"[\\\"Hangfire.Server.PerformContext, Hangfire.Core, Version=1.7.11.0, Culture=neutral, PublicKeyToken=e33b67d3bb5581e4\\\",\\\"System.Threading.CancellationToken, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\\\"]\",\"Arguments\":\"[null,null]\"}";
		const string HealthCheckName = "ProcessBillingDatabaseUpdateBillingCubePartial";
	}
}
