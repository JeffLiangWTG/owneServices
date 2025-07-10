using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowItemRelatedViewCoreTest : TestCaseWithFactory
	{
		public void TestNullRelatedObjectCollection()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.InitRelatedDummyWithTasks();
			dummy.RelatedDummyWithTasks = dummy.RelatedDummyWithTasks2;
			dummy.RelatedDummyWithTasks.WorkflowItems.Triggers.AddNew().P9_LineTriggerType = "DUM";
			dummy.RelatedDummyWithTasks.WorkflowItems.Triggers.AddNew().P9_LineTriggerType = "DUM";
			dummy.WorkflowItems.TriggersIncludingRelated.AddNew();
			dummy.WorkflowItems.TriggersIncludingRelated.AddNew();
			dummy.BusinessObjectsWithRelatedEvents_ReturnNull = true;
			var triggas = dummy.WorkflowItems.TriggersIncludingRelated;
			AssertNoExceptionThrown(() => triggas.Sort(triggas.GetDefaultOrderComparer()));
			AssertEquals(4, triggas.Count);
		}

		public void TestNullObjectInRelatedObjectCollection()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.InitRelatedDummyWithTasks();
			dummy.RelatedDummyWithTasks = dummy.RelatedDummyWithTasks2;
			dummy.RelatedDummyWithTasks.WorkflowItems.Triggers.AddNew().P9_LineTriggerType = "DUM";
			dummy.RelatedDummyWithTasks.WorkflowItems.Triggers.AddNew().P9_LineTriggerType = "DUM";
			dummy.WorkflowItems.TriggersIncludingRelated.AddNew();
			dummy.WorkflowItems.TriggersIncludingRelated.AddNew();
			dummy.HaveNullAsRelatedObject = true;
			var triggas = dummy.WorkflowItems.TriggersIncludingRelated;
			AssertNoExceptionThrown(() => triggas.Sort(triggas.GetDefaultOrderComparer()));
			AssertEquals(4, triggas.Count);
		}

		public void TestSort_AllowDuplicatesInDummyCollection()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.InitRelatedDummyWithTasks();
			dummy.RelatedDummyWithTasks = dummy.RelatedDummyWithTasks2;

			dummy.RelatedDummyWithTasks.WorkflowItems.Triggers.AddNew().P9_LineTriggerType = "DUM";
			dummy.RelatedDummyWithTasks.WorkflowItems.Triggers.AddNew().P9_LineTriggerType = "DUM";
			dummy.WorkflowItems.TriggersIncludingRelated.AddNew();
			dummy.WorkflowItems.TriggersIncludingRelated.AddNew();
			var triggas = dummy.WorkflowItems.TriggersIncludingRelated;
			AssertNoExceptionThrown(triggas.Rebuild);
			AssertNoExceptionThrown(() => triggas.Sort(triggas.GetDefaultOrderComparer()));
			AssertEquals(4, triggas.Count);
		}

		public void TestSort_DefaultVsSequence()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.InitRelatedDummyWithTasks();
			dummy.RelatedDummyWithTasks = dummy.RelatedDummyWithTasks2;

			var t1 = dummy.RelatedDummyWithTasks.WorkflowItems.Triggers.AddNew();
			t1.P9_LineTriggerType = "DUM";
			t1.P9_Sequence = 1;
			var t2 = dummy.RelatedDummyWithTasks.WorkflowItems.Triggers.AddNew();
			t2.P9_LineTriggerType = "DUM";
			t2.P9_Sequence = 3;

			var d1 = dummy.WorkflowItems.TriggersIncludingRelated.AddNew();
			d1.P9_Sequence = 2;
			var d2 = dummy.WorkflowItems.TriggersIncludingRelated.AddNew();
			d2.P9_Sequence = 4;

			var triggas = dummy.WorkflowItems.TriggersIncludingRelated;
			triggas.Rebuild();
			triggas.Sort(triggas.GetDefaultOrderComparer());
			AssertSequencesEqual("Default ordering", new[] { t1, t2, d1, d2 }, triggas);

			triggas.Sort("P9_Sequence");
			AssertSequencesEqual("Sequence Ordering", new[] { t1, d1, t2, d2 }, triggas);
		}
	}
}
