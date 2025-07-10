using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessCompanyLinkRule))]
	public class ProcessCompanyLinkRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIRootTypeProvider_Roots()
		{
			var job = Factory.New<ProcessCompanyLinkRule>();
			job.PCR_Type = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			AssertEquals(Array.Empty<BusinessObject>(), ((IRootTypeProvider)job).Roots);
		}

		public void TestIRootTypeProvider_RootTypes()
		{
			var job = Factory.New<ProcessCompanyLinkRule>();
			job.PCR_Type = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			AssertArrayEqualsByElements(new[] {
				typeof(OrgHeader),
				typeof(ProcessTaskTemplate)
				}, ((IRootTypeProvider)job).RootTypes);
		}

		public void TestCalculateTemplateUsingCurrentCompanyWithCompanyRules()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_Name = "Test Partial Template";
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template.P0_IsPartialTemplate = true;

			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;

			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			action.PQ_P0_WorkflowTemplate = template.PK;

			var templateCompanyRule = Factory.New<ProcessCompanyLinkRule>();
			templateCompanyRule.PCR_GC_Company = GlbCompany.CurrentCompany.PK;
			templateCompanyRule.PCR_Type = DummyWorkflowDescriptor.Instance.Code;
			templateCompanyRule.PCR_IsActive = true;
			templateCompanyRule.PCR_Macro = "false";

			Factory.Save();

			using (WorkflowDataRegistry.Instance.CalculateTemplateUsingCurrentCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				dummy.Logs.AddNew(AutoEvents.CustomisableEvent00);
				Factory.Save();

				var logwalker = MasterFilesTestHelper.RunLogWalker();
				var expectedLog = ApplyTemplateProcessorLogger.GetTemplateApplicationResultLog(dummy.JobNumber, template.P0_Name, TemplateApplicationResult.TemplateScopeEnforcerRestriction).Log;
				AssertContains(expectedLog, logwalker);
			}
		}

		public void TestCloneForOtherCompanies()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			var job = Factory.New<ProcessCompanyLinkRule>();
			job.PCR_GC_Company = company1.PK;
			job.PCR_Macro = "blah";
			job.PCR_IsActive = true;
			job.PCR_Type = DummyWorkflowDescriptor.Instance.Code;

			var clonedJob = job.RulesForBinding.AddNew();
			clonedJob.PCR_GC_Company = company2.PK;

			job.CloneForOtherCompanies();
			Factory.Save();

			AssertEquals("Cloned rule for another company", company2, clonedJob.Company);
			AssertEquals("Cloned macro", job.PCR_Macro, clonedJob.PCR_Macro);
			AssertEquals("Cloned active status", job.PCR_IsActive, clonedJob.PCR_IsActive);
			AssertEquals("Cloned type", job.PCR_Type, clonedJob.PCR_Type);
			AssertEquals("Only 2 rules created", 2, Factory.Load<ProcessCompanyLinkRule>(new ZQuery()).Length);
		}
	}
}
