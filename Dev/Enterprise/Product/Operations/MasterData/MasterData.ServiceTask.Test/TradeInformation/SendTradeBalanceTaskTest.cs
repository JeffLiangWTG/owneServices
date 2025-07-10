using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.MasterData.ServiceTask.Test
{
	[TestedType(typeof(SendTradeBalanceTask))]
	public class SendTradeBalanceTaskTest : ServiceTaskTestCase<SendTradeBalanceTask>
	{
		[TestDate(2021, 5, 1)]
		public void TestInitialiseSchedule()
		{
			var testTask = new SendTradeBalanceTask();
			InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);
			AssertEquals("IsActive", ZBool.True, taskSchedule.SST_Active);
		}

		public void TestHostedServiceAttributes()
		{
			var result = GetHostedServiceAttributes()
				.SingleOrDefault();

			AssertNotNull(result);
			AssertEquals(true, result.IsMandatory);
			AssertEquals(true, result.CanRunInAnyBranch);
			AssertEquals(false, result.AllowsMultipleInstances);
			AssertEquals(true, result.IsScheduleReadOnly);
			AssertEquals("1month", result.MinimumPeriod);
			AssertEquals("1month", result.DefaultSchedule.RunEvery);
			AssertEquals(1, result.DefaultSchedule.DayOfMonth);
			AssertEquals("15minutes", result.DefaultSchedule.StartAtUtc);
			AssertEquals("225minutes", result.DefaultSchedule.RandomStartOffset);
		}

		[TestDate(2021, 5, 17)]
		public void TestRunTask_Successful()
		{
			var companies = Factory.Load<GlbCompany>(new ZQuery());
			foreach (var company in companies)
			{
				company.GC_IsActive = false;
			}

			var companyForTest = Factory.NewWithValidTestData<GlbCompany>();
			companyForTest.CompanyName = "TestCompany";
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_Code = "YYY";
			newBranch.GB_GC = companyForTest.PK;
			newBranch.GB_IsActive = true;

			Factory.Save();

			AssertNull("Precondition", companyForTest.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.TradeInformationSend).FirstOrDefault());

			var logger = new DummyLogger();
			var logs = new List<DummyLogEventArgs>();
			logger.OnLog += LoggerOnLog;

			var serviceUrl = $"http://localhost:{HttpServiceForTest.GetFreeTcpPort()}/";
			var service = TradeInformationHelperTest.GetServicesForTest(WTG.ROPE.Model.ResultCode.Successful, null, serviceUrl);

			using (OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetServiceUrlCollection(serviceUrl)))
			using (OrganisationsDataRegistry.Instance.EnableTradeBalanceServiceTaskForTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				try
				{
					service.Start();

					var sendTradeBalanceTask = new SendTradeBalanceTask();
					sendTradeBalanceTask.ServiceLogger = logger;
					AssertNoExceptionThrown(() => sendTradeBalanceTask.RunTask(default));

					var dateList = TradeInformationHelper.GetReportDateList(null, ZDateTime.Now).ToArray();
					AssertEquals(24, dateList.Length);

					var companyForTestLogs = companyForTest.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(AutoEvents.TradeInformationSend).ToArray();
					AssertNotNull(companyForTestLogs);
					AssertEquals(24, companyForTestLogs.Length);
					for (int i = 0; i < dateList.Length; i++)
					{
						var expectedRef = $"Send 0 trade balance data in {dateList[i]:yyyy-MM} to server";
						var companyForTestLog = companyForTestLogs.Single(x => x.SL_Reference == expectedRef);
						AssertNotNull(companyForTestLog);
						var eventDate = dateList[i].AddMonths(1);
						TradeInformationHelperTest.AssertStmALog(companyForTestLog, expectedRef, eventDate.Year, eventDate.Month, 1);
					}
				}
				finally
				{
					if (service.IsStarted)
					{
						service.Stop();
					}
				}
			}

			void LoggerOnLog(object sender, DummyLogEventArgs e)
			{
				logs.Add(e);
			}
		}

		public void TestRunTask_WithWebException_LogWebExceptionAndReportErrorButNoExceptionThrown()
		{
			var logger = new DummyLogger();
			var logs = new List<DummyLogEventArgs>();
			logger.OnLog += LoggerOnLog;

			using (OrganisationsDataRegistry.Instance.EnableTradeBalanceServiceTaskForTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var sendTradeBalanceTask = new SendTradeBalanceTaskForWebExceptionTest();
				sendTradeBalanceTask.ServiceLogger = logger;
				AssertNoExceptionThrown(() => sendTradeBalanceTask.RunTask(default));
				AssertEquals(1, logs.Count(u => u.Type == LogType.Warning && u.Message.StartsWith("Web exception occur, retry 1 time after 10 minutes.")));
				AssertEquals(1, logs.Count(u => u.Message.StartsWith("Unhandled exception when sending Trade Balance Information")));
				Assert(ErrorReporter.LastMessageReported.Contains("Unhandled exception when sending Trade Balance Information"));
			}

			ErrorReporter.Clear();

			void LoggerOnLog(object sender, DummyLogEventArgs e)
			{
				logs.Add(e);
			}
		}

		public void TestRunTask_WithUnhandledException_LogExceptionAndReportErrorButNoExceptionThrown()
		{
			var logger = new DummyLogger();
			var logs = new List<DummyLogEventArgs>();
			logger.OnLog += LoggerOnLog;

			using (OrganisationsDataRegistry.Instance.EnableRealCreditCheckServiceInTestingSystem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var tokenSource = new CancellationTokenSource())
			{
				var sendTradeBalanceTask = new SendTradeBalanceTask();
				sendTradeBalanceTask.ServiceLogger = logger;
				tokenSource.Cancel();
				AssertNoExceptionThrown(() => sendTradeBalanceTask.RunTask(tokenSource.Token));
				AssertEquals(0, logs.Count(u => u.Message.StartsWith("Web exception occur, retry 1 time after 10 minutes.")));
				AssertEquals(1, logs.Count(u => u.Message.StartsWith("Unhandled exception when sending Trade Balance Information")));
				Assert(ErrorReporter.LastMessageReported.Contains("Unhandled exception when sending Trade Balance Information"));
			}

			ErrorReporter.Clear();

			void LoggerOnLog(object sender, DummyLogEventArgs e)
			{
				logs.Add(e);
			}
		}

		public void TestAccessCurrentBranchSafely()
		{
			using var tokenSource = new CancellationTokenSource();
			var sendTradeBalanceTask = new SendTradeBalanceTaskForWebExceptionTest { TestDisposableEnvironment = true };
			sendTradeBalanceTask.ServiceLogger = new DummyLogger();

			using (EnvProxy.Instance.TemporaryServiceTaskContext("Fake_TBS", true))
			{
				AssertNoExceptionThrown(() => sendTradeBalanceTask.RunTask(tokenSource.Token));
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		IDisposable serviceUrlDisposable;

		protected override void SetUpCore()
		{
			base.SetUpCore();
			serviceUrlDisposable = OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, GetServiceUrlCollection("http://localhost.dummy"));
		}

		protected override void TearDownCore()
		{
			serviceUrlDisposable?.Dispose();
		}

		CodeDescriptionBoolWithSingleTrueCollection GetServiceUrlCollection(string serviceUrl)
		{
			var collection = new CodeDescriptionBoolWithSingleTrueCollection();
			collection.Add(new CodeDescriptionBoolWithSingleTrue() { CodeMaxLength = 4, Code = "SYD", Description = (NoResString)serviceUrl, Bool = true });
			return collection;
		}

		class SendTradeBalanceTaskForWebExceptionTest : SendTradeBalanceTask
		{
			public bool TestDisposableEnvironment { get; set; }

			protected override int RetryWaitMilliseconds => 1;

			protected override void RunWithRetry(bool shouldRetry, CancellationToken youMustReactToThisToken)
			{
				if (TestDisposableEnvironment)
				{
					_ = Environment.Env.CurrentBranch;
				}
				else
				{
					base.RunWithRetry(shouldRetry, youMustReactToThisToken);
				}
			}
		}
	}
}
