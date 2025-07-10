using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TempleApplicationDeletedItemsTest : TemplateApplicationTestCase
	{
		public void TestTaskDeletedByDataRefresh()
		{
			var template = MakeTemplate();
			MakeTask(template, udfCondition: "\"1\"==\"1\"");
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals(1, dummy.WorkflowItems.Count);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var dummyReloaded = newFactory.Load<DummyWithWorkflow>(dummy.PK);
			dummyReloaded.WorkflowItems.Single().Delete();
			newFactory.Save();

			// Make sure to touch the collections before template application
			AssertEquals(0, dummy.WorkflowItems.Count);
			AssertEquals(0, dummy.WorkflowItems.Tasks.Count);
			dummy.ApplyWorkflowTemplates();
			AssertEquals(1, dummy.WorkflowItems.Count);
			AssertEquals(1, dummy.WorkflowItems.Tasks.Count);
		}

		public void TestTaskDeletedByDataRefresh_DontTouchTheCollections()
		{
			var template = MakeTemplate();
			MakeTask(template, udfCondition: "\"1\"==\"1\"");
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals(1, dummy.WorkflowItems.Count);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var dummyReloaded = newFactory.Load<DummyWithWorkflow>(dummy.PK);

			dummyReloaded.WorkflowItems.Single();
			dummy.WorkflowItems.Single().Delete();
			Factory.Save();
			newFactory.Save();

			dummyReloaded.ApplyWorkflowTemplates();
			AssertEquals(1, dummyReloaded.WorkflowItems.Count);
			AssertEquals(1, dummyReloaded.WorkflowItems.Tasks.Count);
		}

		public void TestTaskDeletedByMergeConflict()
		{
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: true));
			var template = MakeTemplate();
			MakeTask(template, udfCondition: "\"1\"==\"1\"");
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(newFactory, tryHandleConflictsAutomatically: true));
			var dummyReloaded = newFactory.Load<DummyWithWorkflow>(dummy.PK);

			dummy.ApplyWorkflowTemplates();
			dummyReloaded.ApplyWorkflowTemplates();

			AssertEquals(1, dummy.WorkflowItems.Count);
			AssertEquals(1, dummyReloaded.WorkflowItems.Count);

			Factory.Save();

			AssertEquals(1, dummy.WorkflowItems.Count);
			AssertEquals(2, dummyReloaded.WorkflowItems.Count);

			newFactory.Save();

			AssertEquals(1, dummy.WorkflowItems.Count);
			AssertEquals(1, dummyReloaded.WorkflowItems.Count);

			dummy.ApplyWorkflowTemplates();
			dummyReloaded.ApplyWorkflowTemplates();

			AssertEquals(1, dummy.WorkflowItems.Count);
			AssertEquals(1, dummyReloaded.WorkflowItems.Count);
		}

		public void TestTaskDeletedByMergeConflict_NoRefresh()
		{
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, tryHandleConflictsAutomatically: true));
			Factory.RefreshEnabled = false;
			var template = MakeTemplate();
			MakeTask(template, udfCondition: "\"1\"==\"1\"");
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			Assert(TemplateApplicationRaceConditionHandler.TryHookupFactory(newFactory, tryHandleConflictsAutomatically: true));
			var dummyReloaded = newFactory.Load<DummyWithWorkflow>(dummy.PK);

			dummy.ApplyWorkflowTemplates();
			dummyReloaded.ApplyWorkflowTemplates();

			AssertEquals(1, dummy.WorkflowItems.Count);
			AssertEquals(1, dummyReloaded.WorkflowItems.Count);

			Factory.Save();

			AssertEquals(1, dummy.WorkflowItems.Count);
			AssertEquals(1, dummyReloaded.WorkflowItems.Count);

			newFactory.Save();

			AssertEquals(1, dummy.WorkflowItems.Count);
			AssertEquals(0, dummyReloaded.WorkflowItems.Count);

			dummy.ApplyWorkflowTemplates();
			dummyReloaded.ApplyWorkflowTemplates();

			AssertEquals(1, dummy.WorkflowItems.Count);
			AssertEquals(1, dummyReloaded.WorkflowItems.Count);
		}

		public void TestSomethingIJustThoughtOf()
		{
			Factory.RefreshEnabled = false;
			var job = Factory.New<DummyWithWorkflow>();
			var task = MakeTask(job);
			AssertEquals(1, job.WorkflowItems.Count);
			Factory.Save();

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var jobReloaded = otherFactory.Load<DummyWithWorkflow>(job.PK);
			jobReloaded.WorkflowItems[0].Delete();
			otherFactory.Save();

			AssertEquals(1, job.WorkflowItems.Count);
			job.WorkflowItems.Reload(reLoadExistingRows: false, assumeRowsMissingFromQueryResultsAreDeleted: true);
			AssertEquals(0, job.WorkflowItems.Count);
		}
	}
}
