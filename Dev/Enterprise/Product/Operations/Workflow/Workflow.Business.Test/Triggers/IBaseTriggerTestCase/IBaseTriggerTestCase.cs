using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business.Test
{
	/* TODO: Need another schema change first. (Whoops)

	class UniversalTemplateIBaseTriggerTestCase : IBaseTriggerTestCase
	{
		protected override int CountFiredTimes(IBaseTrigger template, IWorkflowProvider provider)
		{
			var task = Factory.LoadTop1<ProcessJobTriggerLink>(new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, provider.PK).AddToFilter(ProcessJobTriggerLinkSchema.P9L_P9T_TemplateTrigger, template.Identifier));
			return Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode)
				.AddToFilter(StmALogSchema.SL_Parent, task.PK)).Length;
		}

		protected override ProcessTaskTemplate GetTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			template.P0_IsUniversal = true;
			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.NeverFallback;
			return template;
		}

		protected override IBaseTrigger GetTrigger(ProcessTaskTemplate template)
		{
			var trigga = (IBaseTrigger)template.TemplateTriggers.AddNew();
			trigga.Description = "TheDescriptoManipto";
			return trigga;
		}
	}
	*/

	class ProcessTaskIBaseTriggerTestCase : IBaseTriggerTestCase
	{
		protected override int CountFiredTimes(IBaseTrigger template, IWorkflowProvider provider)
		{
			var task = Factory.LoadTop1<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, provider.PK).AddToFilter(ProcessTasksSchema.P9_ParentTemplateID, template.Identifier));
			return Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode)
				.AddToFilter(StmALogSchema.SL_Parent, task.PK)).Length;
		}

		protected override ProcessTaskTemplate GetTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			return template;
		}

		protected override IBaseTrigger GetTrigger(ProcessTaskTemplate template)
		{
			return template.WorkflowItems.Triggers.AddNew();
		}
	}

	abstract class IBaseTriggerTestCase : WorkflowTestCase
	{
		#region Setup

		protected abstract ProcessTaskTemplate GetTemplate();

		protected abstract IBaseTrigger GetTrigger(ProcessTaskTemplate template);

		protected abstract int CountFiredTimes(IBaseTrigger template, IWorkflowProvider provider);

		#endregion

		public void TestLimitFireWorkflow()
		{
			AssertLimitFireWorkflow(0);
		}

		public void TestLimitFireWorkflow_Fence()
		{
			AssertLimitFireWorkflow(1);
		}

		public void TestLimitFireWorkflow_Many()
		{
			AssertLimitFireWorkflow(5);
		}

		void AssertLimitFireWorkflow(ZShort count)
		{
			var template = GetTemplate();
			var trigger = GetTrigger(template);
			trigger.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.TriggerFiredCountdown = count;
			var triggerAction = (ProcessTaskNotification)trigger.TriggerActions.AddNew();
			CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, "eeuurrggh@Dylan.Com");

			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();

			AssertEquals(1, dummy.WorkflowItems.TriggersIncludingRelated.Count);

			for (int i = 0; i < count + 1; i++)
			{
				dummy.Logs.AddNew(AutoEvents.CustomisableEvent00);
				Factory.Save();
			}

			AssertEquals(count, CountFiredTimes(trigger, dummy));
		}
	}
}
