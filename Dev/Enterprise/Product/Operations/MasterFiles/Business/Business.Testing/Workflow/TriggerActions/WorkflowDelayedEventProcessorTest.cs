using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowDelayedEventProcessorTest : NonTransactionedTestCase
	{
		[TestDate(2022, 6, 8)]
		public void TestShouldGenerateDLYEvent_WhenThereAreNoMoreRecentScheduledActionsWithTheSameParameters()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			// no more recent, but with the same parameters
			ScheduleDLYAction(ZDateTime.UtcNow, dummy.PK, "Z0");

			Factory.Save();

			TestDateAttribute.AddMinutes(2);
			var futureTime = ZDateTime.UtcNow.AddDays(1);

			// more recent, but with different parameters
			ScheduleDLYAction(futureTime, dummy.PK, "Z0", evt: "Z01");
			ScheduleDLYAction(futureTime, dummy.PK, "Z0", off: "001:00");
			ScheduleDLYAction(futureTime, dummy.PK, "Z0", act: "Another reference");
			ScheduleDLYAction(futureTime, dummy.PK, "Z0", est: "Y");

			Factory.Save();

			TestDateAttribute.AddMinutes(-1); // to ensure the actions above are more recent

			var action = new WorkflowDelayedEventProcessor();
			var logger = new DummyLogger();

			using (DisposableEnvironment.ForBranch(Env.CurrentBranch.PK))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("TAS", canRunInAnyBranch: true))
			{
				var result = action.Execute(new CancellationToken(), Factory, logger, dummy.PK, "Z0", "|EVT=Z00|OFF=000:00|ACT=Reference|EST=N|USR=CWSupport|BRN=TST|DEP=DP1", ZDateTime.UtcNow);
				AssertEquals("Successfully created a DLY event.", result);
			}

			MasterFilesTestHelper.AssertEventRaised("DLY event should be generated even if there are more recent scheduled action, but with different parameters",
				dummy, "DLY", "|EVT=Z00|OFF=000:00|ACT=Reference|EST=N|USR=CWSupport|BRN=TST|DEP=DP1");
		}

		[TestDate(2022, 6, 8)]
		public void TestShouldNotGenerateDLYEvent_WhenThereAreMoreRecentScheduledActionsWithTheSameParameters()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			var mostRecentActionExecutionDateTimeUtc = ZDateTime.UtcNow.AddMinutes(-10);
			ScheduleDLYAction(mostRecentActionExecutionDateTimeUtc, dummy.PK, "Z0", usr: "Some other user - users do not matter", brn: "NEW", dep: "DEP"); // branches, departments and users are not considered

			Factory.Save();

			TestDateAttribute.AddMinutes(1); // the most recent action was created 1 minute ago
			var currentActionMockCreateTime = ZDateTime.UtcNow.AddMinutes(-2); // emulate the tested action was created 2 minutes ago
																			   // all of them were created before UtcNow

			var action = new WorkflowDelayedEventProcessor();
			var logger = new DummyLogger();

			using (DisposableEnvironment.ForBranch(Env.CurrentBranch.PK))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("TAS", canRunInAnyBranch: true))
			{
				var result = action.Execute(new CancellationToken(), Factory, logger, dummy.PK, "Z0", "|EVT=Z00|OFF=000:00|ACT=Reference|EST=N|USR=CWSupport|BRN=TST|DEP=DP1", currentActionMockCreateTime);
				AssertEquals("No action needed: there is a more recent DLY action scheduled for 07-Jun-22 23:50:00 on 08-Jun-22 00:00:00.", result);
			}

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedDummy = newFactory.Load<DummyWithWorkflow>(dummy.PK);
			MasterFilesTestHelper.AssertNoEventRaised("DLY event should not be generated as there are more recent scheduled action with the same parameters",
				loadedDummy, "DLY");
		}

		[TestDate(2022, 6, 8)]
		public void TestShouldGenerateDLYEvent_WhenThereAreMoreRecentScheduledActionsWithTheSameParameters_ButClosedOrFailed()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			var futureTime = ZDateTime.UtcNow.AddDays(1);
			ScheduleDLYAction(futureTime, dummy.PK, "Z0", executionStatus: Constants.TimeActionScheduleStatus.Closed);
			ScheduleDLYAction(futureTime, dummy.PK, "Z0", executionStatus: Constants.TimeActionScheduleStatus.Failed);

			Factory.Save();

			TestDateAttribute.AddMinutes(-1); // to ensure the actions above are more recent

			var action = new WorkflowDelayedEventProcessor();
			var logger = new DummyLogger();

			using (DisposableEnvironment.ForBranch(Env.CurrentBranch.PK))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("TAS", canRunInAnyBranch: true))
			{
				var result = action.Execute(new CancellationToken(), Factory, logger, dummy.PK, "Z0", "|EVT=Z00|OFF=000:00|ACT=Reference|EST=N|USR=CWSupport|BRN=TST|DEP=DP1", ZDateTime.UtcNow);
				AssertEquals("Successfully created a DLY event.", result);
			}

			MasterFilesTestHelper.AssertEventRaised("DLY event should be generated even if there are more recent scheduled action, but closed or failed",
				dummy, "DLY", "|EVT=Z00|OFF=000:00|ACT=Reference|EST=N|USR=CWSupport|BRN=TST|DEP=DP1");
		}

		[TestDate(2022, 6, 8)]
		public void TestShouldNotGenerateDLYEvent_WhenDLYEventWithTheSameParametersIsAlreadyGenerated()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.GetLogs().AddNew(Events.DelayedEvent, "|EVT=Z00|OFF=000:00|ACT=Reference|EST=N|USR=SomeOtherUser|BRN=NEW|DEP=NEW");
			TestDateAttribute.AddDays(1);
			dummy.GetLogs().AddNew(Events.DelayedEvent, "|EVT=Z00|OFF=000:00|ACT=Reference|EST=N|USR=SomeOtherUser|BRN=NEW|DEP=NEW");

			Factory.Save();

			AssertEquals("Precondition", 2, dummy.GetLogs().Find(l => l.SL_SE_NKEvent == Events.DelayedEventCode).Count());

			TestDateAttribute.AddDays(1);

			var action = new WorkflowDelayedEventProcessor();
			var logger = new DummyLogger();

			using (DisposableEnvironment.ForBranch(Env.CurrentBranch.PK))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("TAS", canRunInAnyBranch: true))
			{
				var result = action.Execute(new CancellationToken(), Factory, logger, dummy.PK, "Z0", "|EVT=Z00|OFF=000:00|ACT=Reference|EST=N|USR=CWSupport|BRN=TST|DEP=DP1", ZDateTime.UtcNow);
				AssertEquals("No action needed: a DLY event with the same reference parameters was already generated at 09-Jun-22 00:00:00 (|EVT=Z00|OFF=000:00|ACT=Reference|EST=N|USR=SomeOtherUser|BRN=NEW|DEP=NEW).", result);
			}

			AssertEquals("DLY event should not be generated as a DLY event with the same reference parameters already exists.",
				2, dummy.GetLogs().Find(l => l.SL_SE_NKEvent == Events.DelayedEventCode).Count());
		}

		[TestDate(2022, 6, 8)]
		public void TestShouldGenerateDLYEvent_WhenDLYEventIsAlreadyGenerated_ButWithDifferentParameters()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.GetLogs().AddNew(Events.DelayedEvent, "|EVT=Z01|OFF=000:00|ACT=Reference|EST=N|USR=CWSupport|BRN=TST|DEP=DP1");
			dummy.GetLogs().AddNew(Events.DelayedEvent, "|EVT=Z00|OFF=-001:00|ACT=Reference|EST=N|USR=CWSupport|BRN=TST|DEP=DP1");
			dummy.GetLogs().AddNew(Events.DelayedEvent, "|EVT=Z00|OFF=000:00|ACT=AnotherReference|EST=N|USR=CWSupport|BRN=TST|DEP=DP1");
			dummy.GetLogs().AddNew(Events.DelayedEvent, "|EVT=Z00|OFF=000:00|ACT=Reference|EST=Y|USR=CWSupport|BRN=TST|DEP=DP1");

			Factory.Save();

			AssertEquals("Precondition", 4, dummy.GetLogs().Find(l => l.SL_SE_NKEvent == Events.DelayedEventCode).Count());

			TestDateAttribute.AddDays(1);

			var action = new WorkflowDelayedEventProcessor();
			var logger = new DummyLogger();

			using (DisposableEnvironment.ForBranch(Env.CurrentBranch.PK))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("TAS", canRunInAnyBranch: true))
			{
				var result = action.Execute(new CancellationToken(), Factory, logger, dummy.PK, "Z0", "|EVT=Z00|OFF=000:00|ACT=Reference|EST=N|USR=CWSupport|BRN=TST|DEP=DP1", ZDateTime.UtcNow);
				AssertEquals("Successfully created a DLY event.", result);
			}

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var newLogs = dummy.GetLogs().Find(l => l.SL_SE_NKEvent == Events.DelayedEventCode);
			AssertEquals(5, newLogs.Count());
			var mostRecentLog = dummy.GetLogs().MostRecentLog;

			AssertEquals("DLY event should be generated even if there are other DLY events generated, but with different reference parameters.",
				"|EVT=Z00|OFF=000:00|ACT=Reference|EST=N|USR=CWSupport|BRN=TST|DEP=DP1", mostRecentLog.SL_Reference);
		}

		[ExpectNoExceptions]
		public void TestShouldNotGenerateDLYEventAndReport_WhenInvalidParameters()
		{
			var dummyPk = ZGuid.NewZGuid();

			var action = new WorkflowDelayedEventProcessor();
			var logger = new DummyLogger();

			ErrorReporter.Clear();

			using (DisposableEnvironment.ForBranch(Env.CurrentBranch.PK))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("TAS", canRunInAnyBranch: true))
			{
				var result = action.Execute(new CancellationToken(), Factory, logger, dummyPk, "ZZ", "ACT=|EVT=Z00|OFF=000:00", ZDateTime.UtcNow);
				AssertEquals("Canceled: the DLY action has invalid parameters. ACT: EVT:Z00 OFF:000:00 EST:", result);
				AssertEquals($@"Invalid Workflow Scheduled Action
targetPk:{dummyPk}
targetCode:ZZ
parameter:ACT=|EVT=Z00|OFF=000:00
ACT:
EVT:Z00
OFF:000:00
EST:", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
				result = action.Execute(new CancellationToken(), Factory, logger, dummyPk, "ZZ", "ACT=|EVT=Z00", ZDateTime.UtcNow);

				AssertEquals("Canceled: the DLY action has invalid parameters. ACT: EVT:Z00 OFF: EST:", result);
				AssertEquals($@"Invalid Workflow Scheduled Action
targetPk:{dummyPk}
targetCode:ZZ
parameter:ACT=|EVT=Z00
ACT:
EVT:Z00
OFF:
EST:", ErrorReporter.LastMessageReported);

				ErrorReporter.Clear();
				result = action.Execute(new CancellationToken(), Factory, logger, dummyPk, "ZZ", "ACT=", ZDateTime.UtcNow);

				AssertEquals("Canceled: the DLY action has invalid parameters. ACT: EVT: OFF: EST:", result);
				AssertEquals($@"Invalid Workflow Scheduled Action
targetPk:{dummyPk}
targetCode:ZZ
parameter:ACT=
ACT:
EVT:
OFF:
EST:", ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Clear();
		}

		[TestDate(2022, 6, 8)]
		public void TestShouldNotGenerateDLYEvent_WhenJobIsDeleted()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			Factory.Save();

			dummy.Delete();
			Factory.Save();

			var action = new WorkflowDelayedEventProcessor();
			var logger = new DummyLogger();

			using (DisposableEnvironment.ForBranch(Env.CurrentBranch.PK))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("TAS", canRunInAnyBranch: true))
			{
				var result = action.Execute(new CancellationToken(), new BusinessObjectFactory(), logger, dummy.PK, "Z0", "|EVT=Z00|OFF=000:00|ACT=Reference|EST=N|USR=CWSupport|BRN=TST|DEP=DP1", ZDateTime.UtcNow);
				AssertEquals("Canceled: the job has been deleted.", result);
			}
		}

		[TestDate(2022, 6, 8)]
		public void TestDLYEventTime()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			Factory.Save();

			var action = new WorkflowDelayedEventProcessor();
			var logger = new DummyLogger();

			using (DisposableEnvironment.ForBranch(Env.CurrentBranch.PK))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("TAS", canRunInAnyBranch: true))
			{
				var result = action.Execute(new CancellationToken(), Factory, logger, dummy.PK, "Z0", "|EVT=Z00|OFF=000:00|ACT=Reference|EST=N|USR=CWSupport|BRN=TST|DEP=DP1", ZDateTime.UtcNow);
				AssertEquals("Successfully created a DLY event.", result);
			}

			MasterFilesTestHelper.AssertEventRaised("DLY event should be generated",
				dummy, "DLY", "|EVT=Z00|OFF=000:00|ACT=Reference|EST=N|USR=CWSupport|BRN=TST|DEP=DP1");

			var latestLog = dummy.GetLogs().MostRecentLog;
			AssertEquals("DLY", latestLog.SL_SE_NKEvent);
			AssertEquals(ZDateTime.UtcNow, latestLog.SL_EventTime);
		}

		IActionSchedule ScheduleDLYAction(ZDateTime executionDateTimeUtc,
			ZGuid targetPK, ZString targetTableCode,
			string evt = "Z00", string off = "000:00", string act = "Reference", string est = "N", string usr = "CWSupport", string brn = "TST", string dep = "DP1",
			string executionStatus = Constants.TimeActionScheduleStatus.Scheduled)
		{
			var jsonParameter = $"|EVT={evt}|OFF={off}|ACT={act}|EST={est}|USR={usr}|BRN={brn}|DEP={dep}";

			var helper = ObjectFactory.Get<ITimeEngineSchedulerTestHelper>();
			var scheduledAction = helper.ScheduleAction(Factory, "DLY", targetPK.ToGuid(), targetTableCode,
				executionDateTimeUtc.ToDateTime(), jsonParameter, executionStatus);
			return scheduledAction;
		}
	}
}
