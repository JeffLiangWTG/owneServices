using System.Linq;
using Enterprise.HRM.Common.ServiceTasks;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.HRM.ServiceTasks.Testing
{
	[TestedType(typeof(CancelChangeRequestLogSubscriber))]
	sealed class CancelChangeRequestLogSubscriberTests : LogSubscriberTest<CancelChangeRequestLogSubscriber>
	{
		public void TestNoAttachedTasks()
		{
			var gcr = Factory.NewWithValidTestData<GlbStaffChangeRequest>();
			Factory.Save();

			AssertEquals(0, gcr.WorkflowItems.Count);

			gcr.Logs.AddNew(AutoEvents.Cancelled);
			Factory.Save();

			RunLogWalkerCycleForTest();

			AssertEquals(0, gcr.WorkflowItems.Count);
		}

		public void TestCancelWithAttachedTask()
		{
			var gcr = Factory.NewWithValidTestData<GlbStaffChangeRequest>();
			var task = gcr.WorkflowItems.Tasks.AddNew();
			task.P9_GS_NKAssignedStaffMember = "E";
			task.P9_Status = "ASN";

			Factory.Save();

			Assert(task.IsTask);
			AssertEquals(1, gcr.WorkflowItems.Count);

			gcr.Logs.AddNew(AutoEvents.Cancelled);
			Factory.Save();

			RunLogWalkerCycleForTest();

			gcr.WorkflowItems.Reload(true);
			AssertEquals("CAN", gcr.WorkflowItems.Tasks[0].P9_Status);
		}

		public void TestCancelWithAttachedNonTask()
		{
			var gcr = Factory.NewWithValidTestData<GlbStaffChangeRequest>();
			var trigger = gcr.WorkflowItems.Triggers.AddNew();

			Factory.Save();

			Assert(!trigger.IsTask);
			AssertEquals(1, gcr.WorkflowItems.Count);

			gcr.Logs.AddNew(AutoEvents.Cancelled);
			Factory.Save();

			RunLogWalkerCycleForTest();

			trigger.Reload();
			AssertNotEquals("CAN", trigger.P9_Status);
		}

		public void TestCancelForDifferentStatus()
		{
			var gcr = Factory.NewWithValidTestData<GlbStaffChangeRequest>();

			var closedTask = gcr.WorkflowItems.Tasks.AddNew();
			closedTask.P9_GS_NKAssignedStaffMember = "E";
			closedTask.P9_Status = "CLS";

			var assignedTask = gcr.WorkflowItems.Tasks.AddNew();
			assignedTask.P9_GS_NKAssignedStaffMember = "E";
			assignedTask.P9_Status = "ASN";

			var suspendedTask = gcr.WorkflowItems.Tasks.AddNew();
			suspendedTask.P9_GS_NKAssignedStaffMember = "E";
			suspendedTask.P9_Status = "SUS";

			var workingTask = gcr.WorkflowItems.Tasks.AddNew();
			workingTask.P9_GS_NKAssignedStaffMember = "E";
			workingTask.P9_Status = "WRK";

			var cancelledTask = gcr.WorkflowItems.Tasks.AddNew();
			cancelledTask.P9_GS_NKAssignedStaffMember = "E";
			cancelledTask.P9_Status = "CAN";

			var openTask = gcr.WorkflowItems.Tasks.AddNew();
			openTask.P9_Status = "OPN";

			Factory.Save();

			gcr.Logs.AddNew(AutoEvents.Cancelled);
			Factory.Save();

			RunLogWalkerCycleForTest();

			gcr.WorkflowItems.Reload(true);

			var expectCancelled = (new[] { assignedTask, openTask, cancelledTask }).Select(t => t.PK).ToHashSet();
			var cancelled = gcr.WorkflowItems.Cast<ProcessTask>().Where(p => expectCancelled.Contains(p.PK));

			AssertEquals(expectCancelled.Count, cancelled.Count());
			Assert(cancelled.All(t => t.P9_Status == "CAN"));

			var expectClosed = (new[] { closedTask, suspendedTask, workingTask }).Select(t => t.PK).ToHashSet();
			var notCancelled = gcr.WorkflowItems.Cast<ProcessTask>().Where(p => expectClosed.Contains(p.PK));

			AssertEquals(expectClosed.Count, notCancelled.Count());
			Assert(notCancelled.All(t => t.P9_Status == "CLS"));
		}
	}
}
