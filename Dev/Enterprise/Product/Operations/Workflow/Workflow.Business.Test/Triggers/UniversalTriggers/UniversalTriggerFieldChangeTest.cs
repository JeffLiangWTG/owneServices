using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test.UniversalTriggers
{
	class UniversalTriggerFieldChangeTest : WorkflowTestCase
	{
		[TestDate(2015, 7, 14)]
		public void TestFieldChangeTrigger_ShouldFireCompletionTriggerActions()
		{
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_Code);
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_VarCharMax);

			var universalTemplate = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger1 = CreateTrigger(universalTemplate, eventCode: "", triggerFieldName: DummyBizoSchema.Constants.Z0_Code);
			var trigger2 = CreateTrigger(universalTemplate, eventCode: "", triggerFieldName: DummyBizoSchema.Constants.Z0_VarCharMax);
			var triggerAction1 = CreateTriggerAction(trigger1, WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce);
			var triggerAction2 = CreateTriggerAction(trigger2, WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce);

			var partialTemplate1 = CreateTemplate(Factory, "DUM", isPartial: true);
			CreateTask(partialTemplate1, "Partial Template Task 1", 1);

			var partialTemplate2 = CreateTemplate(Factory, "DUM", isPartial: true);
			CreateTask(partialTemplate2, "Partial Template Task 2", 2);

			trigger1.P9T_Sequence = 1;
			trigger2.P9T_Sequence = 2;

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();

			Factory.Save();

			AssertEquals(2, job.WorkflowItems.TriggersIncludingRelated.Count);

			var ghostedTrigger1 = job.WorkflowItems.TriggersIncludingRelated[0];
			var ghostedTrigger2 = job.WorkflowItems.TriggersIncludingRelated[1];

			AssertEquals(DummyBizoSchema.Constants.Z0_Code, ghostedTrigger1.TriggerConditions.TriggerFieldName);
			AssertEquals(DummyBizoSchema.Constants.Z0_VarCharMax, ghostedTrigger2.TriggerConditions.TriggerFieldName);

			AssertEquals(ZDateTime.Empty, ghostedTrigger1.P9_ActualDate.ToZDateTime());
			AssertEquals(ZDateTime.Empty, ghostedTrigger2.P9_ActualDate.ToZDateTime());

			job.Z0_Code = "ZAP";
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			RunModifiedFieldChangeTriggerServiceTask();
			RunLogWalker();

			var newFactory = Factory.CreateNewFactory();
			var loadedJob = newFactory.Load<DummyWithWorkflow>(job.PK);
			loadedJob.WorkflowItems.TriggersIncludingRelated.Rebuild();
			var loadedTrigger1 = loadedJob.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == trigger1.PK);
			var loadedTrigger2 = loadedJob.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == trigger2.PK);

			//AssertEquals(new ZDateTime(2015, 7, 14), loadedTrigger1.P9_ActualDate);
			//AssertEquals(ZDateTime.Empty, loadedTrigger2.P9_ActualDate);

			//AssertEquals(1, loadedJob.WorkflowItems.Tasks.Count);
			//AssertEquals("Partial Template Task 1", loadedJob.WorkflowItems.Tasks[0].P9_Description);

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(1);

			job.Z0_VarCharMax = "Zzzzzap";
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(5);

			RunModifiedFieldChangeTriggerServiceTask();
			RunLogWalker();

			newFactory = Factory.CreateNewFactory();
			loadedJob = newFactory.Load<DummyWithWorkflow>(job.PK);
			loadedTrigger1 = loadedJob.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == trigger1.PK);
			loadedTrigger2 = loadedJob.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == trigger2.PK);

			//AssertEquals(new ZDateTime(2015, 7, 14), loadedTrigger1.P9_ActualDate);
			//AssertEquals(new ZDateTime(2015, 7, 14, 0, 1, 0), loadedTrigger2.P9_ActualDate);

			//AssertEquals(2, loadedJob.WorkflowItems.Tasks.Count);
			//AssertEquals("Partial Template Task 2", loadedJob.WorkflowItems.Tasks[1].P9_Description);

			Assert("TODO in WI00127805: add support for Field Change Universal Triggers", true);
		}
	}
}
