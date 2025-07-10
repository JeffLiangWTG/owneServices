using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Workflow.Business.Test.LineTriggerTest;

namespace Enterprise.Workflow.Business.Test
{
	public class WorkflowMacroContextDeciderTest : TestCaseWithFactory
	{
		void AssertIAntlrMacroContextParent(IAntlrMacroContext context, BusinessObject parent, Type parentType)
		{
			AssertEquals("context.Scope.Data", parent, context.Scope.Data);
			AssertEquals("context.Parent", parent, context.Parent);
			AssertEquals("context.ParentType", parentType, context.ParentType);
		}

		void AssertIAntlrMacroContextContainsVariables(IAntlrMacroContext context, string key, Type variableType, object variableObject, bool assertVariableObject)
		{
			string PrettyPrintVariables(Dictionary<string, (object, Type)> variables)
			{
				return string.Join("\n", variables.Select(v => $"{v.Key}: {v.Value.Item1} ({v.Value.Item2.Name})"));
			}

			var actualVariable = context.Variables[key];
			AssertNotNull($"Expecting variables to contain '{key}'.\nActual variables: {PrettyPrintVariables(context.Variables)}", actualVariable);
			AssertEquals($"Wrong type for variable '{key}'", variableType, actualVariable.Item2);
			if (assertVariableObject)
			{
				AssertEquals($"Wrong instance for variable '{key}'", variableObject, actualVariable.Item1);
			}
		}

		void AssertEnvironmentVariable(IAntlrMacroContext context) => AssertIAntlrMacroContextContainsVariables(context, WorkflowMacroVariables.EnvironmentVariableName, typeof(MasterFiles.Business.Macros.Environment), null, false);
		void AssertWorkflowItemVariable(IAntlrMacroContext context, Type type, object value) => AssertIAntlrMacroContextContainsVariables(context, WorkflowMacroVariables.WorkflowItemVariableName, type, value, true);
		void AssertEventVariable(IAntlrMacroContext context, Type type, object value) => AssertIAntlrMacroContextContainsVariables(context, WorkflowMacroVariables.EventVariableName, type, value, true);
		void AssertEventSourceVariable(IAntlrMacroContext context, Type type, object value) => AssertIAntlrMacroContextContainsVariables(context, WorkflowMacroVariables.EventSourceVariableName, type, value, true);

		public void TestTrigger_GetSampleContext()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var trigger = job.WorkflowItems.Triggers.AddNew();

			using (var context = (trigger as IAntlrMacroContextProvider).GetSampleContext())
			{
				AssertIAntlrMacroContextParent(context, job, typeof(DummyWithWorkflow));
				AssertEnvironmentVariable(context);
				AssertWorkflowItemVariable(context, typeof(DummyProcessTask), trigger);
				AssertEventSourceVariable(context, typeof(DummyWithWorkflow), job);
				AssertEventVariable(context, typeof(IStmALog), null);
			}
		}

		public void TestTemplateTrigger_GetSampleContext()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Dummy Workflow";
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			var trigger = template.WorkflowItems.Triggers.AddNew();

			using (var context = (trigger as IAntlrMacroContextProvider).GetSampleContext())
			{
				AssertIAntlrMacroContextParent(context, null, typeof(DummyWithWorkflow));
				AssertEnvironmentVariable(context);
				AssertWorkflowItemVariable(context, typeof(TemplateProcessTask), trigger);
				AssertEventSourceVariable(context, typeof(DummyWithWorkflow), null);
				AssertEventVariable(context, typeof(IStmALog), null);
			}
		}

		public void TestUniversalTrigger_GetSampleContext()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "Dummy Workflow";
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template.P0_IsUniversal = true;
			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			var universalTrigger = template.TemplateTriggers.AddNew() as ITemplateTrigger;
			universalTrigger.Description = "Universal Trigger";
			universalTrigger.TriggerEventCode = AutoEvents.CustomisableEvent00Code;

			Factory.Save();

			using (var context = (universalTrigger as IAntlrMacroContextProvider).GetSampleContext())
			{
				AssertIAntlrMacroContextParent(context, null, typeof(DummyWithWorkflow));
				AssertEnvironmentVariable(context);
				AssertWorkflowItemVariable(context, typeof(ProcessTemplateTrigger), universalTrigger);
				AssertEventSourceVariable(context, typeof(DummyWithWorkflow), null);
				AssertEventVariable(context, typeof(IStmALog), null);
			}
		}

		public void TestLineTrigger_GetSampleContext()
		{
			var job = Factory.New<DummyWithLines>();
			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.P9_LineTriggerType = DummyLineType;

			using (new WorkflowDescriptorsForTest())
			using (var context = (trigger as IAntlrMacroContextProvider).GetSampleContext())
			{
				AssertIAntlrMacroContextParent(context, null, typeof(DummyLine));
				AssertEnvironmentVariable(context);
				AssertWorkflowItemVariable(context, typeof(DummyProcessTask), trigger);
				AssertEventSourceVariable(context, typeof(DummyLine), null);
				AssertEventVariable(context, typeof(IStmALog), null);
			}
		}

		public void TestTemplateLineTrigger_GetSampleContext()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_LineTriggerType = DummyLineType;

			using (new WorkflowDescriptorsForTest())
			using (var context = (trigger as IAntlrMacroContextProvider).GetSampleContext())
			{
				AssertIAntlrMacroContextParent(context, null, typeof(DummyLine));
				AssertEnvironmentVariable(context);
				AssertWorkflowItemVariable(context, typeof(TemplateProcessTask), trigger);
				AssertEventSourceVariable(context, typeof(DummyLine), null);
				AssertEventVariable(context, typeof(IStmALog), null);
			}
		}
	}
}
