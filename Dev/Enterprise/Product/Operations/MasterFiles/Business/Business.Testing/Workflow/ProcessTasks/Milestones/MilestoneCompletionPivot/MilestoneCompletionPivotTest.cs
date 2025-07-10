using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	class MilestoneCompletionPivotTest : TestCaseWithFactory
	{
		public void TestTask_CompletionMilestone()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = job.WorkflowItems.Tasks.AddNew();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = "Z01";
			milestone.P9_Description = "Foo b";

			AssertEquals(ZString.Empty, task.P9_MilestoneCompletionPivotKey);

			task.P9_MilestoneCompletionPivotKey = milestone.P9_MilestoneCompletionPivotKey;
			AssertEquals(task.P9_MilestoneCompletionPivotKey, milestone.P9_MilestoneCompletionPivotKey);
		}

		public void TestMilestoneCompletionMilestone()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Foo";
			milestone.TriggerConditions.TriggerEventCode = "Z00";

			AssertEquals("Z00 - Foo", milestone.P9_MilestoneCompletionPivotKey);
		}
	}
}
