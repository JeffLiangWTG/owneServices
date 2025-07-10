using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.GUI.Workflow;
using Enterprise.TimeEngineScheduler.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(ScheduledDelayedEventViewModel))]
	class ScheduledDelayedEventViewModelTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2022, 06, 19)]
		public void TestShouldRepresentTimeActionSheduleProperties()
		{
			AssertScheduledActionViewModel(new ZDateTime(2022, 06, 20), new ZDateTime(2022, 06, 19),
				"|ACT=1 minute|EVT=Z00|OFF=000:00|EST=Y|USR=E|BRN=BR0|DEP=DP0",
				"1 minute", "Z00", "000:00", true, "E", "BR0", "DP0",
				"SCH");

			AssertScheduledActionViewModel(new ZDateTime(2022, 06, 21), new ZDateTime(2022, 06, 19),
				"|ACT=|EVT=Z01|OFF=000:01|EST=Y|USR=E1|BRN=BR1|DEP=DP1",
				"", "Z01", "000:01", true, "E1", "BR1", "DP1",
				"SCH");

			TestDateAttribute.AddHours(1);
			AssertScheduledActionViewModel(new ZDateTime(2022, 06, 22), new ZDateTime(2022, 06, 19, 1, 0, 0),
				"|ACT=-1 hour|EVT=Z02|OFF=-001:00|EST=N|USR=E2|BRN=BR2|DEP=DP2",
				"-1 hour", "Z02", "-001:00", false, "E2", "BR2", "DP2",
				"CLS", "Execution result");
		}

		void AssertScheduledActionViewModel(ZDateTime executionDateTimeUtc, ZDateTime expectedSystemCreateTimeUtc,
			string jsonParameter,
			ZString expectedSourceActionReference,
			ZString expectedSourceTriggerEventCode,
			ZString expectedSourceActionOffset,
			ZBool expectedSourceTriggerIsEstimate,
			ZString expectedSourceEventUserCode,
			ZString expectedSourceEventBranchCode,
			ZString expectedSourceEventDepartmentCode,
			string executionStatus = Constants.TimeActionScheduleStatus.Scheduled,
			string executionResult = "")
		{
			var helper = ObjectFactory.Get<ITimeEngineSchedulerTestHelper>();
			var schedule = helper.ScheduleAction(Factory, "DLY", Guid.NewGuid(), "WKI", executionDateTimeUtc.ToDateTime(),
				jsonParameter, executionStatus, executionResult);
			Factory.Save();
			var model = new ScheduledDelayedEventViewModel(schedule);
			AssertEquals(executionDateTimeUtc, model.ExecutionDateTimeUtc);
			AssertEquals(jsonParameter, model.EventReference);
			AssertEquals(expectedSourceActionReference, model.SourceActionReference);
			AssertEquals(expectedSourceTriggerEventCode, model.SourceTriggerEventCode);
			AssertEquals(expectedSourceActionOffset, model.SourceActionOffset);
			AssertEquals(expectedSourceTriggerIsEstimate, model.SourceTriggerIsEstimate);
			AssertEquals(expectedSourceEventUserCode, model.SourceEventUserCode);
			AssertEquals(expectedSourceEventBranchCode, model.SourceEventBranchCode);
			AssertEquals(expectedSourceEventDepartmentCode, model.SourceEventDepartmentCode);
			AssertEquals(executionStatus, model.ExecutionStatus);
			AssertEquals(executionResult, model.ExecutionResult);
			AssertEquals(expectedSystemCreateTimeUtc, model.SystemCreateTimeUtc);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = ObjectFactory.Get<ITimeEngineSchedulerTestHelper>();
			var schedule = helper.ScheduleAction(Factory, "DLY", Guid.NewGuid(), "Z0",
				new DateTime(2022, 6, 21), "Ref1");
			return new ScheduledDelayedEventViewModel(schedule);
		}
	}
}
