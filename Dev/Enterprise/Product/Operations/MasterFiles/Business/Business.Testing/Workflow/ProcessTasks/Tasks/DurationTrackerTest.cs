using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	class DurationTrackerTest : TestCaseWithFactory
	{
		#region Actual Start

		void AssertActualDurationChanges(ZDateTime baseDuration, ZDateTimeOffset startDate, ProcessTask task)
		{
			AssertEquals("Start at the start.", startDate, task.TaskProperties.ActualDate);
			AssertEquals("Duration of an hour", baseDuration, task.P9_ActualDuration);

			task.TaskProperties.ActualDate = startDate.AddHours(-1);
			AssertEquals("Duration should grow", baseDuration.AddHours(1), task.P9_ActualDuration);

			task.TaskProperties.ActualDate = startDate.AddMinutes(-30);
			AssertEquals("Duration should shrink", baseDuration.AddMinutes(30), task.P9_ActualDuration);
		}

		[TestDate(2017, 10, 30, 16, 0, 0)]
		public void TestSettingP9_ActualStart_ItemStartWithDuration_ManualDuration()
		{
			var baseDuration = (ZDateTime)TimeSpan.FromHours(1);
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_ActualDuration = baseDuration;
			AssertEquals("Don't expect the date to have changed, since in my opinion it is better to be unopinionated about the order in which this field is set.", ZDateTimeOffset.Empty, task.TaskProperties.ActualDate);

			task.TaskProperties.ActualDate = ZDateTimeOffset.Now.AddHours(-1);
			AssertEquals("Duration should not have changed.", baseDuration, task.P9_ActualDuration);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Duration should not have changed.", baseDuration, task.P9_ActualDuration);
		}

		[TestDate(2017, 10, 30, 16, 0, 0)]
		public void TestSettingP9_ActualStart_ItemStartWithDuration_ItemInDb()
		{
			var baseDuration = (ZDateTime)TimeSpan.FromHours(1);
			var startDate = ZDateTimeOffset.Now;
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.AddHours(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertActualDurationChanges(baseDuration, startDate, task);
		}

		[TestDate(2017, 10, 30, 16, 0, 0)]
		public void TestSettingP9_ActualStart_ItemStartWithDuration_LoadFromDb()
		{
			var baseDuration = (ZDateTime)TimeSpan.FromHours(1);
			var startDate = ZDateTimeOffset.Now;
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.AddHours(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();
			AssertActualDurationChanges(baseDuration, startDate, Factory.CreateNewFactory().Load<ProcessTask>(task.PK));
		}

		[TestDate(2017, 10, 30, 16, 0, 0)]
		public void TestSettingP9_ActualStart_ItemStartWithDuration_ItemNotSaved()
		{
			var baseDuration = (ZDateTime)TimeSpan.FromHours(1);
			var startDate = ZDateTimeOffset.Now;
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.AddHours(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertActualDurationChanges(baseDuration, startDate, task);
		}

		[TestDate(2017, 10, 30, 16, 0, 0)]
		public void TestSettingP9_ActualStart_ItemDurationUnset_ManualDuration()
		{
			var baseDuration = (ZDateTime)TimeSpan.FromHours(1);
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.TaskProperties.ActualDate = ZDateTimeOffset.Now.AddHours(-1);
			AssertEquals("Duration should not have changed, since there is nothing to base this off of.", ZDateTime.Empty, task.P9_ActualDuration);

			task.P9_ActualDuration = baseDuration;
			AssertEquals("Actual date should be what we set it to.", ZDateTimeOffset.Now.AddHours(-1), task.TaskProperties.ActualDate);

			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Duration should not have changed.", baseDuration, task.P9_ActualDuration);
			AssertEquals("Actual date should be what we set it to.", ZDateTimeOffset.Now.AddHours(-1), task.TaskProperties.ActualDate);
		}

		[TestDate(2017, 10, 30, 16, 0, 0)]
		public void TestSettingP9_ActualStart_ItemDurationUnset_ItemInDb()
		{
			var baseDuration = (ZDateTime)TimeSpan.FromHours(1);
			var startDate = ZDateTimeOffset.Now;
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.TaskProperties.ActualDate = startDate.AddHours(-1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task.Factory.Save();
			TestDateAttribute.AddHours(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("The actual start date should have trumped the initial WRK log.", baseDuration.AddHours(1), task.P9_ActualDuration);
		}

		[TestDate(2017, 10, 30, 16, 0, 0)]
		public void TestSettingP9_ActualStart_ItemDurationUnset_LoadFromDb()
		{
			var baseDuration = (ZDateTime)TimeSpan.FromHours(1);
			var startDate = ZDateTimeOffset.Now;
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.TaskProperties.ActualDate = startDate.AddHours(-1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			task.Factory.Save();
			task = task.Factory.CreateNewFactory().Load<ProcessTask>(task.PK);
			TestDateAttribute.AddHours(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("The actual start date should have trumped the initial WRK log.", baseDuration.AddHours(1), task.P9_ActualDuration);
		}

		[TestDate(2017, 10, 30, 16, 0, 0)]
		public void TestSettingP9_ActualStart_ItemDurationUnset_ItemNotSaved()
		{
			var baseDuration = (ZDateTime)TimeSpan.FromHours(1);
			var startDate = ZDateTimeOffset.Now;
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.TaskProperties.ActualDate = startDate.AddHours(-1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.AddHours(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("The actual start date should have trumped the initial WRK log.", baseDuration.AddHours(1), task.P9_ActualDuration);
		}

		[TestDate(2017, 10, 24, 13, 0, 0)]
		public void TestSettingP9_ActualStart_UseWorkingDaysRule()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);
			Factory.Save();

			var baseDuration = (ZDateTime)TimeSpan.FromHours(1);
			var startDate = ZDateTimeOffset.Now;
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.TaskProperties.ActualDate = startDate.AddHours(-24);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.AddHours(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Since we have worked more than 10 hours ourside of working hours, only use working days.", baseDuration.AddHours(8), task.P9_ActualDuration);
			task.TaskProperties.ActualDate = startDate;
			AssertEquals("After setting, go back to normal time.", baseDuration, task.P9_ActualDuration);
			task.TaskProperties.ActualDate = startDate.AddHours(-24);
			AssertEquals("And now back to a day ago!", baseDuration.AddHours(8), task.P9_ActualDuration);
		}

		[TestDate(2017, 10, 30, 16, 0, 0)]
		public void TestSettingP9_ActualStart_DoNotUnderflow()
		{
			var baseDuration = (ZDateTime)TimeSpan.FromHours(1);
			var startDate = ZDateTimeOffset.Now;
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.AddHours(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			task.TaskProperties.ActualDate = startDate.AddMinutes(3000);
			AssertEquals("On underflow, set date as empty.", ZDateTime.DefaultDurationEpoch, task.P9_ActualDuration);
		}

		#endregion

		#region Working Days duration tests

		[TestTimeZoneUNLOCO("AUBNE")]
		[TestDate(2022, 7, 3)]
		public void TestChangingTimezoneBetweenStatusChanges()
		{
			TestDateAttribute.UseUNLOCO = true;
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(4);
			TestTimeZoneUNLOCOAttribute.UNLOCO = "USLAX";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(4m, task.ActualDurationHours);
		}

		[TestDate(2017, 10, 30, 1, 0, 0)]
		public void TestTooManyPeopleComplainedAboutTheLunchHoursToKeepIt()
		{
			WorkingDaysTestHelper.UpdateAllStaffDays(Factory, GlbStaff.CurrentUser.PK, "****  **********"); // Use a weird setup for people that work midnight till whenever with a 1 hour gap so that testing is EZ-er
			Factory.Save();

			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.AddHours(4);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Ignore the hour long gap because it isn't big enough.", (ZDateTime)TimeSpan.FromHours(4), task.P9_ActualDuration);
		}

		[TestDate(2017, 10, 30, 16, 0, 0)]
		public void TestWorkingDays_AssumeContinuousWorkIfSpanIsNotBetweenTwoDays()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);
			Factory.Save();

			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1).Date.AddHours(11); // 11 am the next day.
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("It's 3 hours, since it's assumed that you went home and came back.", 3m, task.ActualDurationHours);
		}

		[TestDate(2017, 10, 30, 16, 0, 0)]
		public void TestWorkingDays_ClosingTheTaskAnHourBeforeYouStartWorking_AssumeThatYouAreNotAHardWorkerThatWorkedAllNight()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);
			Factory.Save();

			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1).Date.AddHours(8); // 8 am the next day.
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("Just one hour, since so long has passed since it was work time.", 1m, task.ActualDurationHours);
		}

		[TestDate(2017, 10, 30, 16, 0, 0)]
		public void TestWorkingDays_AssumeContinuousWorkIfSpanIsNotBetweenTwoWorkingPeriods()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);
			Factory.Save();

			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(2); // Two hours from now, but only 1 hour of work time.
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals("It's two hours, since it is outside of working time.", 2m, task.ActualDurationHours);
		}

		[TestDate(2017, 10, 30, 16, 0, 0)]
		public void TestWorkingDays_None()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, GlbDepartment.CurrentDepartment.PK);
			Factory.Save();

			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(2); // Two hours from now, but only 1 hour of work time.
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(2m, task.ActualDurationHours);
		}

		[TestDate(2017, 10, 30, 16, 0, 0)]
		public void TestBlankStatusWeirdness()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			task.P9_Status = "";
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(9);
			task.P9_Status = "";
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(9);
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			AssertEquals("Fold 'blank' time into the next status change", 70d, task.P9_ActualDuration.GetMinutesFromDateTimeSpan());
			AssertEquals("Fold 'blank' time into the next status change", 10d, task.SuspendedDuration.GetMinutesFromDateTimeSpan());
		}

		[TestDate(2017, 10, 30, 16, 0, 0)]
		public void TestBlankStatusWeirdness_OnSaveAndReload()
		{
			var task = Factory.NewWithValidTestData<ProcessTask>();
			task.P9_Status = "";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			task.Factory.Save();
			task = Factory.CreateNewFactory().Load<ProcessTask>(task.PK);
			task.P9_Status = "";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
			AssertEquals(60d, task.P9_ActualDuration.GetMinutesFromDateTimeSpan());

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			task.Factory.Save();
			task = Factory.CreateNewFactory().Load<ProcessTask>(task.PK);
			task.P9_Status = "";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			AssertEquals(60d, task.P9_ActualDuration.GetMinutesFromDateTimeSpan());

			TestDateAttribute.Date = TestDateAttribute.Date.AddHours(1);
			task.Factory.Save();
			task = Factory.CreateNewFactory().Load<ProcessTask>(task.PK);
			task.P9_Status = "";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			AssertEquals(120d, task.P9_ActualDuration.GetMinutesFromDateTimeSpan());
		}

		#endregion

		#region Variant basher

		[TestDate(1991, 7, 3)]
		public void TestAllTheVariants()
		{
			var wrkActions = new NamedTaskAction[]
			{
				new NamedTaskAction("Assign Wrk", t => t.P9_Status = "WRK"),
				new NamedTaskAction("Assign Wrk", t =>
				{
					t.P9_Status = "";
					t.P9_Status = "WRK";
				}),
				new NamedTaskAction("Assign Wrk And Save", t =>
				{
					t.P9_Status = "WRK";
					t.Factory.Save();
				}),
			};

			var susActions = new NamedTaskAction[]
			{
				new NamedTaskAction("assing sus", t => t.P9_Status = "SUS"),
				new NamedTaskAction("assing blank->sus", t =>
				{
					t.P9_Status = "";
					t.P9_Status = "SUS";
				}),
				new NamedTaskAction("assign cls", t => t.P9_Status = "CLS"),
				new NamedTaskAction("assign blank->cls",
				t =>
				{
					t.P9_Status = "";
					t.P9_Status = "CLS";
				}),
				new NamedTaskAction("assign can", t => t.P9_Status = "CAN"),
			};

			var actionPairs = CreateActionPairs(wrkActions, susActions).ToArray();

			CombineAssertions(() =>
			{
				foreach (var pair1 in actionPairs)
				{
					foreach (var pair2 in actionPairs)
					{
						AssertTaskDurationVariant(pair1, pair2);
					}
				}
			});
		}

		class NamedTaskAction
		{
			public NamedTaskAction(string v, Action<ProcessTask> p)
			{
				Name = v;
				Action = p;
			}

			public string Name { get; set; }
			public Action<ProcessTask> Action { get; set; }
		}

		static IEnumerable<Tuple<NamedTaskAction, NamedTaskAction>> CreateActionPairs(NamedTaskAction[] wrkActions, NamedTaskAction[] susActions)
		{
			for (int i = 0; i < wrkActions.Length; i++)
			{
				for (int j = 0; j < susActions.Length; j++)
				{
					var wrkAction = wrkActions[i];
					var susAction = susActions[j];
					yield return Tuple.Create(wrkAction, susAction);
				}
			}
		}

		void AssertTaskDurationVariant(params Tuple<NamedTaskAction, NamedTaskAction>[] actionPairs)
		{
			var minutesPerAction = 10;
			var factory = new BusinessObjectFactory();
			var task = factory.NewWithValidTestData<ProcessTask>();
			task.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;

			foreach (var action in actionPairs)
			{
				action.Item1.Action(task);
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(minutesPerAction);
				action.Item2.Action(task);
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(minutesPerAction);
			}

			factory.Save();

			var actionMessage = "These are the things that happen:\r\n" + string.Join(System.Environment.NewLine, actionPairs.SelectMany(s => new[] { s.Item1.Name, s.Item2.Name }).ToArray());
			AssertEquals(actionMessage, actionPairs.Length * minutesPerAction, (int)factory.CreateNewFactory().Load<ProcessTask>(task.PK).P9_ActualDuration.ToTimeSpan().TotalMinutes);
		}

		#endregion
	}
}
