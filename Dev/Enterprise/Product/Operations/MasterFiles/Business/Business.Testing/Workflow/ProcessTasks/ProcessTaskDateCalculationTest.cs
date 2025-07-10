using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTaskDateCalculationTest : TestCaseWithFactory
	{
		#region Test Setup

		/*
		 * The point here is to test all of the combinations of date properties with
		 * all of the types stored in the ProcessTask table.
		 * 
		 * This guarantees consistency between tasks/triggers/milestones/exceptions
		 */

		public IEnumerable<(string, string, string)> GetPropertyTriples()
		{
			yield return (ProcessTasksSchema.Constants.P9_ActualDate, ProcessTasksSchema.Constants.P9_ActualDateUtc, "P9_ActualDateForBinding");
			yield return (ProcessTasksSchema.Constants.P9_ScheduledDate, ProcessTasksSchema.Constants.P9_ScheduledDateUtc, "P9_ScheduledDateForBinding");
			yield return (ProcessTasksSchema.Constants.P9_SuspendedAt, ProcessTasksSchema.Constants.P9_SuspendedAtUtc, "P9_SuspendedAtForBinding");
			yield return (ProcessTasksSchema.Constants.P9_MilestoneExceptionAdded, ProcessTasksSchema.Constants.P9_ExceptionAddedUtc, "P9_ExceptionAddedForBinding");
		}

		public IEnumerable<ProcessTask> GetTasks()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			var list = new List<ProcessTask>();
			list.Add(dummy.WorkflowItems.Tasks.AddNew());
			list.Add(dummy.WorkflowItems.Triggers.AddNew());
			list.Add(dummy.WorkflowItems.Milestones.AddNew());
			list.Add(dummy.WorkflowItems.Exceptions.AddNew());

			return list;
		}

		bool CanSetDirectly(ProcessTask task, PropWrapper<ZDateTime> date)
		{
			// Cant set actual date on a Trigger
			return !task.IsTrigger() || !date.Name.Contains(ProcessTasksSchema.P9_ActualDate.Name);
		}

		class PropWrapper<T> where T : IZType
		{
			public PropWrapper(ZPropertyInfo info)
			{
				this.info = info;
			}
			readonly ZPropertyInfo info;

			public T Value
			{
				get => (T)info.Value;
				set => info.Value = value;
			}

			public string Name => info.Name;

			public override string ToString()
			{
				if (info.BizObj is ProcessTask task)
				{
					return $"{info.Name} on {task.P9_Type}";
				}
				return base.ToString();
			}
		}

		void WithProperties(Action<ProcessTask, PropWrapper<ZDateTime>, PropWrapper<ZDateTime>, PropWrapper<ZDateTimeOffset>> action)
		{
			foreach (var task in GetTasks())
			{
				foreach (var (dateName, utcDateName, offsetDateName) in GetPropertyTriples())
				{
					PropWrapper<T> GetWrapper<T>(string name) where T : IZType => new PropWrapper<T>(task.FindPropertyInfo(name));
					var date = GetWrapper<ZDateTime>(dateName);
					var utc = GetWrapper<ZDateTime>(utcDateName);
					var offset = GetWrapper<ZDateTimeOffset>(offsetDateName);

					AssertEquals($"Precondition: {date} is empty", ZDateTime.Empty, date.Value);
					AssertEquals($"Precondition: {utc} is empty", ZDateTime.Empty, utc.Value);
					AssertEquals($"Precondition: {offset} is empty", ZDateTimeOffset.Empty, offset.Value);

					action(task, date, utc, offset);
				}
			}
		}

		#endregion

		public void TestCalculateDateIfOnlyUTCIsStored()
		{
			WithProperties((task, date, utc, offset) =>
			{
				((INeedRow)task).Row[utc.Name] = ZDateTime.UtcNow;
				AssertEquals($"{offset} is local branch time", offset.Value, utc.Value.ToLocalBranchTimeOffset());
			});
		}

		public void TestCalculateDateIfOnlyLocalIsStored()
		{
			WithProperties((task, date, utc, offset) =>
			{
				((INeedRow)task).Row[date.Name] = ZDateTime.Now;
				AssertEquals($"{offset} is local branch time", offset.Value, date.Value.ToOffset());
			});
		}

		[TestDate(2022, 3, 13)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestCalculateDateIfBothLocalAndUtcStored()
		{
			var now = ZDateTime.Now;
			var utcNow = now.ToUniversalBranchTime();
			WithProperties((task, date, utc, offset) =>
			{
				((INeedRow)task).Row[date.Name] = now.ToDateTime();
				((INeedRow)task).Row[utc.Name] = utcNow.ToDateTime();
				AssertEquals($"{date} is local branch time", date.Value, utc.Value.ToLocalBranchTime());
				AssertEquals($"{utc} has a value", true, utc.Value.IsValid);
				AssertEquals($"{offset} is local branch time", offset.Value, utc.Value.ToLocalBranchTimeOffset());
			});
		}

		public void TestSettingUTCSetsLocal()
		{
			WithProperties((task, date, utc, offset) =>
			{
				utc.Value = ZDateTime.UtcNow;
				AssertEquals($"{date} is local branch time", date.Value, utc.Value.ToLocalBranchTime());
				AssertEquals($"{utc} has a value", true, utc.Value.IsValid);
				AssertEquals($"{offset} is local branch time", offset.Value, utc.Value.ToLocalBranchTimeOffset());
			});
		}

		public void TestSettingLocalSetsUTC()
		{
			WithProperties((task, date, utc, offset) =>
			{
				if (CanSetDirectly(task, date))
				{
					date.Value = ZDateTime.Now;
					AssertEquals($"{date} is local branch time", date.Value, utc.Value.ToLocalBranchTime());
					AssertEquals($"{utc} has a value", true, utc.Value.IsValid);
					AssertEquals($"{offset} is local branch time", offset.Value, utc.Value.ToLocalBranchTimeOffset());
				}
			});
		}

		public void TestSettingOffsetSetsLocalAndUtc()
		{
			WithProperties((task, date, utc, offset) =>
			{
				if (CanSetDirectly(task, date))
				{
					offset.Value = ZDateTimeOffset.Now;
					AssertEquals($"{date} is local branch time for {date}", date.Value, utc.Value.ToLocalBranchTime());
					AssertEquals($"{utc} has a value", true, utc.Value.IsValid);
					AssertEquals($"{offset} is local branch time", offset.Value, utc.Value.ToLocalBranchTimeOffset());
				}
			});
		}

		public void TestOriginalSuspendedAt()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Tasks.AddNew();
			var time = new ZDateTimeOffset(new DateTime(2019, 07, 03), TimeSpan.FromHours(3));
			task.P9_SuspendedAtForBinding = time;
			Factory.Save();

			task.P9_SuspendedAtForBinding = time.AddDays(4);
			AssertEquals(time, task.OriginalSuspendedAtOffset);
			AssertEquals(time.ToDateTimeOffset(), new ProcessTaskWrapper(task).OriginalSuspendedAt);
		}

		public void TestOriginalActualDate()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Tasks.AddNew();
			var time = new ZDateTimeOffset(new DateTime(2019, 07, 03), TimeSpan.FromHours(3));
			task.P9_ActualDateOffset = time;
			Factory.Save();

			task.P9_ActualDateOffset = time.AddDays(4);
			AssertEquals(time, task.OriginalActualDateOffset);
			AssertEquals(time.ToDateTimeOffset(), new ProcessTaskWrapper(task).OriginalActualDate);
		}

		[TestTimeZoneUNLOCO("USCHI")]
		[TestDate(2019, 07, 3, 1, 1, 1, 56)]
		public void TestSuspendingTaskSetsSuspendedAt()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var dummy = Factory.New<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = "WRK";
			Factory.Save();

			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Status = "SUS";
			AssertEquals(ZDateTimeOffset.Now, task.P9_SuspendedAtForBinding);
		}
	}
}
