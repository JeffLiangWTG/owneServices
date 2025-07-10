using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class WorkflowTasksForTest
	{
		public ProcessTask Matched { get; set; }
		public ProcessTask NotMatched { get; set; }

		public static WorkflowTasksForTest CreateTask(DummyWithWorkflow dummy, SchemaColumn column, object matchedValue, object notMatchedValue, bool isMilestone)
		{
			var tasks = isMilestone
				? dummy.WorkflowItems.Milestones
				: (MilestoneOrTriggerCollectionView)dummy.WorkflowItems.Triggers;

			var matched = tasks.AddNew();
			matched.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			if (column == ProcessTasksSchema.P9_ActualDate && !isMilestone)
			{
				// Cannot set trigger date directly
				dummy.Logs.AddNew(Events.CustomisableEvent00, new ZDateTimeOffset(matchedValue));
			}
			else
			{
				matched[column] = matchedValue;
			}

			var notMatched = tasks.AddNew();
			notMatched.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;
			if (column == ProcessTasksSchema.P9_ActualDate && !isMilestone)
			{
				// Cannot set trigger date directly
				dummy.Logs.AddNew(Events.CustomisableEvent01, new ZDateTimeOffset(notMatchedValue));
			}
			else
			{
				notMatched[column] = notMatchedValue;
			}

			dummy.Factory.Save();

			NUnit.Framework.Assertion.AssertEquals("GIVEN matched", matchedValue, matched[column]);
			NUnit.Framework.Assertion.AssertNotEquals("GIVEN not-matched", matchedValue, notMatched[column]);

			return new WorkflowTasksForTest { Matched = matched, NotMatched = notMatched };
		}

		public static void AssertMatching(WorkflowTasksForTest tasks, ProcessTaskCollection foundTasks)
		{
			NUnit.Framework.Assertion.AssertCollectionContains("SHOULD find matched", tasks.Matched, foundTasks);
			NUnit.Framework.Assertion.AssertCollectionNotContains("SHOULD not find not-matched", tasks.NotMatched, foundTasks);
		}

		public static void Test(BusinessObjectFactory factory, FilterStripBusinessObject filterStripBizO, SchemaColumn column, object matchedValue, object notMatchedValue, bool isMilestone)
		{
			var dummy = factory.New<DummyWithWorkflow>();

			var tasks = WorkflowTasksForTest.CreateTask(dummy, column, matchedValue, notMatchedValue, isMilestone);

			var foundTasks = new ProcessTaskCollection(factory);
			foundTasks.Load(filterStripBizO.Filter);

			WorkflowTasksForTest.AssertMatching(tasks, foundTasks);
		}
	}
}
