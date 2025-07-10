using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing.Workflow.TemplateApplication
{
	class TemplateApplicationScopeEnforcerTest : TestCaseWithFactory
	{
		#region Setup

		ProcessTask CreateTask(IWorkflowProvider provider)
		{
			var task = provider.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "task" + task.P9_Sequence;
			task.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task.TemplateConditions.TemplateCondition2Value = @"""1""==""1""";
			return task;
		}

		ProcessTaskTemplate CreateTemplateWithTask()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			CreateTask(template);
			return template;
		}

		DummyWithWorkflow CreateJob() => Factory.NewWithValidTestData<DummyWithWorkflow>();

		void AssertTemplateApplied(string message, bool applied, IWorkflowProvider provider)
		{
			var fn = applied ? new Func<bool, bool>(f => f) : f => !f;
			Assert(message, fn(provider.WorkflowItems.Tasks.Count > 0));
		}

		public void TestAssumptionThatWorkflowWontApplyToDummyImplticitly()
		{
			CreateTemplateWithTask();
			Factory.Save();
			var job = CreateJob();
			Factory.Save();
			AssertTemplateApplied("Template shouldn't apply until I ask it to", false, job);
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("I asked it to apply.", true, job);
		}

		#endregion

		#region Scope Selector

		public void TestScopeSelection()
		{
			Assert(Globals.IsTest);
			AssertEquals(TemplateApplicationScopeEnforcer.StrategyType.AlwaysTrue, CreateJob().GetWorkflowTemplateScopeEnforcer().Type);
			Globals.IsTest_ForTest.Value = false;
			Assert(Globals.IsUserInteractive);
			AssertEquals(TemplateApplicationScopeEnforcer.StrategyType.Binding, CreateJob().GetWorkflowTemplateScopeEnforcer().Type);
			Globals.IsUserInteractive = false;
			AssertEquals(TemplateApplicationScopeEnforcer.StrategyType.AlwaysTrue, CreateJob().GetWorkflowTemplateScopeEnforcer().Type);
			Globals.IsTest_ForTest.ResetValue();
			CreateJob().SetWorkflowTemplateScopeToMessage();
			AssertEquals(TemplateApplicationScopeEnforcer.StrategyType.Message, CreateJob().GetWorkflowTemplateScopeEnforcer().Type);
		}

		#endregion

		#region Factory Usage

		public void TestScopeOverride()
		{
			CreateTemplateWithTask();
			Factory.Save();
			Factory.SetWorkflowTemplateScopeToDefault();

			var job = CreateJob();
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("By default template should apply", true, job);

			Factory.SetWorkflowTemplateScopeToDisallow();
			job = CreateJob();
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("And thus the most basic use case works.", false, job);
		}

		public void TestScopeRegistryItem()
		{
			WorkflowDataRegistry.Instance.EnableWorkflowTemplateScopeRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CreateTemplateWithTask();
			Factory.Save();
			Factory.SetWorkflowTemplateScopeToDisallow();
			var job = CreateJob();
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("Even though scope was restricted, the registry item turns off the feature.", true, job);
		}

		#endregion

		#region Default Scope

		public void TestDefaultScope_AppliesToItemWithLog()
		{
			CreateTemplateWithTask();
			var job = CreateJob();
			Factory.Save();
			job.Logs.AddNew(Events.CustomisableEvent00);
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("Should apply", true, job);
		}

		public void TestDefaultScope_AppliesToItemWithEdit()
		{
			CreateTemplateWithTask();
			var job = CreateJob();
			Factory.Save();
			job.HasChanges = true;
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("Should apply", true, job);
		}

		public void TestDefaultScope_AppliesToNewItem()
		{
			CreateTemplateWithTask();
			Factory.Save();
			var job = CreateJob();
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("Should apply", true, job);
		}

		public void TestDefaultScope_AppliesToItemWithNoEdit()
		{
			CreateTemplateWithTask();
			var job = CreateJob();
			Factory.Save();
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("Should apply", true, job);
		}

		#endregion

		#region Message Scope

		public void TestMessageScope_AppliesToItemWithLog()
		{
			var template = CreateTemplateWithTask();
			var job = CreateJob();
			Factory.Save();
			CreateJob().SetWorkflowTemplateScopeToMessage(); // Setting scope to unrelated job.
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("Unworthy of template application", false, job);
			job.Logs.AddNew(Events.CustomisableEvent00);
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("As soon as the log is added, it should recall its need.", true, job);
		}

		public void TestMessageScope_AppliesToItemWithEdit()
		{
			var template = CreateTemplateWithTask();
			var job = CreateJob();
			Factory.Save();
			CreateJob().SetWorkflowTemplateScopeToMessage(); // Setting scope to unrelated job.
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("Unworthy of template application", false, job);
			job.HasChanges = true;
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("Works after edit", true, job);
		}

		public void TestMessageScope_AppliesToNewItem()
		{
			var template = CreateTemplateWithTask();
			Factory.Save();
			var job = CreateJob();
			CreateJob().SetWorkflowTemplateScopeToMessage(); // Setting scope to unrelated job.
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("New things should always work.", true, job);
		}

		public void TestMessageScope_NotAppliesToItemWithNoEdit()
		{
			var template = CreateTemplateWithTask();
			var job = CreateJob();
			Factory.Save();
			CreateJob().SetWorkflowTemplateScopeToMessage(); // Setting scope to unrelated job.
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("Do not apply because the job is not related to the workflow scope", false, job);
		}

		#endregion

		#region Form Scope

		public void TestFormScope_AppliesToItemWithLog()
		{
			var template = CreateTemplateWithTask();
			var job = CreateJob();
			Factory.Save();
			CreateJob().SetWorkflowTemplateScopeToForm(); // Setting scope to unrelated job.
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("Unworthy of template application", false, job);
			job.Logs.AddNew(Events.CustomisableEvent00);
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("As soon as the log is added, it should recall its need.", true, job);
		}

		public void TestFormScope_AppliesToItemWithEdit()
		{
			var template = CreateTemplateWithTask();
			var job = CreateJob();
			Factory.Save();
			CreateJob().SetWorkflowTemplateScopeToForm(); // Setting scope to unrelated job.
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("Unworthy of template application", false, job);
			job.HasChanges = true;

			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("Unworthy of template application", true, job);
		}

		public void TestFormScope_AppliesToNewItem()
		{
			var template = CreateTemplateWithTask();
			Factory.Save();
			var job = CreateJob();
			CreateJob().SetWorkflowTemplateScopeToForm(); // Setting scope to unrelated job.
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("New things should always work.", true, job);
		}

		public void TestFormScope_NotAppliesToItemWithNoEdit()
		{
			var template = CreateTemplateWithTask();
			var job = CreateJob();
			Factory.Save();
			CreateJob().SetWorkflowTemplateScopeToForm(); // Setting scope to unrelated job.
			job.ApplyWorkflowTemplates();
			AssertTemplateApplied("Should do nothing since there is no reason for the template to apply.", false, job);
		}

		#endregion
	}
}
