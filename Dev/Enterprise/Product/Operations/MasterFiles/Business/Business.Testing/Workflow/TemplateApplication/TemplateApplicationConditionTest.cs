using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	class TemplateApplicationConditionTest : TemplateApplicationTestCase
	{
		public void TestMaxApplicationError()
		{
			var template = MakeTemplate();
			var udfCondition = FormattableString.Invariant($@"""<HasEvent({Events.CustomisableEvent00Code})>""==""Y""");
			var trigger1 = MakeTrigger(template, Events.CustomisableEvent01Code);
			var trigger2 = MakeTrigger(template, Events.CustomisableEvent01Code, udfCondition: udfCondition);
			WorkflowDataRegistry.Instance.MaximumNumberOfWorkflowItemsInTemplateApplication.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates(new[] { template }, true));
			AssertCollectionContains(trigger1.PK, dummy.WorkflowItems.Select(c => c.P9_ParentTemplateID));
			ErrorReporter.Clear();
		}

		public void TestHasEvent()
		{
			var template = MakeTemplate();
			var udfCondition = FormattableString.Invariant($@"""<HasEvent({Events.CustomisableEvent00Code})>""==""Y""");
			var trigger1 = MakeTrigger(template, Events.CustomisableEvent01Code);
			var trigger2 = MakeTrigger(template, Events.CustomisableEvent01Code, udfCondition: udfCondition);
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates(new[] { template }, true));
			AssertCollectionContains(trigger1.PK, dummy.WorkflowItems.Select(c => c.P9_ParentTemplateID));
			AssertCollectionNotContains(trigger2.PK, dummy.WorkflowItems.Select(c => c.P9_ParentTemplateID));

			dummy.Logs.AddNew(Events.CustomisableEvent00);

			dummy.ApplyWorkflowTemplates(TemplateApplicationParameters.ApplySpecificTemplates(new[] { template }, true));
			AssertCollectionContains(trigger1.PK, dummy.WorkflowItems.Select(c => c.P9_ParentTemplateID));
			AssertCollectionContains(trigger2.PK, dummy.WorkflowItems.Select(c => c.P9_ParentTemplateID));
		}

		public void TestUdfTaskTemplateIsAppliedCorrectlyWhenRowCondition2ValueIsNotEmpty()
		{
			var template = MakeTemplate();
			var templateTask = MakeTask(template, udfCondition: UdfConditionLongerThanCondition2ValueMaxLength);
			InjectBadCondition2ValueIntoDataRow(templateTask);
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			dummy.ApplyWorkflowTemplates(); // Ensure we do not create duplicates
			AssertEquals(1, dummy.WorkflowItems.Tasks.Count);
		}

		public void TestUdfMilestoneTemplateIsAppliedCorrectlyWhenRowCondition2ValueIsNotEmpty()
		{
			var template = MakeTemplate();
			var templateMilestone = MakeMilestone(template, udfCondition: UdfConditionLongerThanCondition2ValueMaxLength);
			InjectBadCondition2ValueIntoDataRow(templateMilestone);
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			dummy.ApplyWorkflowTemplates(); // Ensure we do not create duplicates
			AssertEquals(1, dummy.WorkflowItems.Milestones.Count);
		}

		public void TestUdfTriggerTemplateIsAppliedCorrectlyWhenRowCondition2ValueIsNotEmpty()
		{
			var template = MakeTemplate();
			var templateTrigger = MakeTrigger(template, udfCondition: UdfConditionLongerThanCondition2ValueMaxLength);
			InjectBadCondition2ValueIntoDataRow(templateTrigger);
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			dummy.ApplyWorkflowTemplates(); // Ensure we do not create duplicates
			AssertEquals(1, dummy.WorkflowItems.Triggers.Count);
		}

		public void TestUdfContainingGuid()
		{
			var guid = ZGuid.NewZGuid();
			var template = MakeTemplate();
			MakeTrigger(template, udfCondition: $"\"<Z0_Guid>\"==\"{guid.ToString()}\"");
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_Guid = guid;
			dummy.ApplyWorkflowTemplates();
			AssertEquals(1, dummy.WorkflowItems.Triggers.Count);
		}

		public void TestUdfContainingGuid2()
		{
			ZGuid.TryParse("2f492652-315b-4155-8c34-e27816a7ba1a", out var guid);
			var template = MakeTemplate();
			MakeTrigger(template, udfCondition: $"\"<Z0_Guid>\"==\"2f492652-315b-4155-8c34-e27816a7ba1a\"");
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_Guid = guid;
			dummy.ApplyWorkflowTemplates();
			AssertEquals(1, dummy.WorkflowItems.Triggers.Count);
		}

		public void TestUdfContainingGuid3()
		{
			ZGuid.TryParse("2f492652-315b-4155-8c34-e27816a7ba1a", out var guid);
			var template = MakeTemplate();
			MakeTrigger(template, udfCondition: $"\"<Z0_Guid>\"!=\"2f492652-315b-4155-8c34-e27816a7ba1a\"");
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();

			dummy.Z0_Guid = ZGuid.Empty;
			dummy.ApplyWorkflowTemplates();
			AssertEquals(1, dummy.WorkflowItems.Triggers.Count);
		}

		static string UdfConditionLongerThanCondition2ValueMaxLength
		{
			get
			{
				const string udfCondition = @"""1""==""1""";
				AssertGreaterThan("Precondition: UDF condition length must be greater than P9_Condition2Value MaxLength", udfCondition.Length, ProcessTasksSchema.P9_Condition2Value.MaxLength);
				return udfCondition;
			}
		}

		void InjectBadCondition2ValueIntoDataRow(INeedRow processTask)
		{
			processTask.Row[ProcessTasksSchema.Constants.P9_Condition2Value] = "OBR";
		}
	}
}
