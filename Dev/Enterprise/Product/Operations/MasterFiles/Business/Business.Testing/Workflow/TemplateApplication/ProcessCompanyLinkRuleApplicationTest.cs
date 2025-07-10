using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.Business.Testing
{
	class ProcessCompanyLinkRuleApplicationTest : TemplateApplicationTestCase
	{
		#region Testing whether or not templates will apply

		public void TestDefaultBehaviour()
		{
			var template = MakeTemplate();
			MakeTask(template);
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals("By default, apply templates", 1, dummy.WorkflowItems.Tasks.Count);
		}

		public void TestAllow()
		{
			MakeProcessCompanyLinkRule();
			var template = MakeTemplate();
			MakeTask(template);
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals("By default, apply templates", 1, dummy.WorkflowItems.Tasks.Count);
		}

		public void TestRestriction()
		{
			MakeProcessCompanyLinkRule(macro: "\"1\"==\"2\"");
			var template = MakeTemplate();
			MakeTask(template);
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals("Template application should do nothing due to the macro always returning false", 0, dummy.WorkflowItems.Tasks.Count);
		}

		public void TestRestrictionIsPerCompany()
		{
			MakeProcessCompanyLinkRule(macro: "\"1\"==\"2\"");
			var template = MakeTemplate();
			MakeTask(template);
			var companies = new CompanyTestProvider(Factory, 1);
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			companies.ForAllCompanies(c => dummy.ApplyWorkflowTemplates());
			AssertEquals("Template application in an alternate company to the restrictive rule succeeds", 1, dummy.WorkflowItems.Tasks.Count);
		}

		public void TestRestrictionInUnrelatedCompany()
		{
			MakeProcessCompanyLinkRule(company: new CompanyTestProvider(Factory, 1)[0].Company.PK, macro: "\"1\"==\"2\"");
			var template = MakeTemplate();
			MakeTask(template);
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals("Template application in an alternate company to the restrictive rule succeeds", 1, dummy.WorkflowItems.Tasks.Count);
		}

		public void TestRestrictionIsPerType()
		{
			MakeProcessCompanyLinkRule(macro: "\"1\"==\"2\"", type: "JAM");
			var template = MakeTemplate();
			MakeTask(template);
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals("Template application for an alternate type, the restrictive rule succeeds", 1, dummy.WorkflowItems.Tasks.Count);
		}

		#endregion

		#region Multiple Rule

		public void TestMacroScopeContainsJob()
		{
			MakeProcessCompanyLinkRule(macro: "\"<Z0_Description>\"==\"I had an adventure\"");
			var template = MakeTemplate();
			MakeTask(template);
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals("At first it didn't apply", 0, dummy.WorkflowItems.Tasks.Count);
			dummy.Z0_Description = "I had an adventure";
			Factory.Save(); // Saving to clear cache
			dummy.ApplyWorkflowTemplates();
			AssertEquals("Now the macro matches, the template applied", 1, dummy.WorkflowItems.Tasks.Count);
		}

		public void TestMacroScopeContainsTemplate()
		{
			MakeProcessCompanyLinkRule(macro: "\"<P0_Name>\"==\"I had an adventure\"");
			var template = MakeTemplate();
			MakeTask(template);
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals("At first it didn't apply", 0, dummy.WorkflowItems.Tasks.Count);
			template.P0_Name = "I had an adventure";
			Factory.Save(); // Saving to clear cache
			dummy.ApplyWorkflowTemplates();
			AssertEquals("Now the macro matches, the template applied", 1, dummy.WorkflowItems.Tasks.Count);
		}

		public void TestMacroDoesNotCacheValue()
		{
			MakeProcessCompanyLinkRule(macro: "\"<P0_Name>\"==\"Ok... maybe one last time\"");
			var template1 = MakeTemplate();
			MakeTask(template1);
			template1.P0_Name = "No adventures this time";

			var template2 = MakeTemplate();
			MakeTrigger(template2);
			template2.P0_Name = "Unmatched template";
			Factory.Save();

			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals("At first it didn't apply", 0, dummy.WorkflowItems.Tasks.Count);
			template1.P0_Name = "Ok... maybe one last time";
			dummy.ApplyWorkflowTemplates();
			AssertEquals("Now the macro matches, the template1 should be applied", 1, dummy.WorkflowItems.Tasks.Count);
			AssertEquals("Template2 should not be applied", 0, dummy.WorkflowItems.Triggers.Count);
		}

		#endregion

		#region Testing macro scope

		public void TestScopeContainsJob()
		{
			MakeProcessCompanyLinkRule(macro: "\"<Z0_Description>\"==\"I had an adventure\"");
			var template = MakeTemplate();
			MakeTask(template);
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals("At first it didn't apply", 0, dummy.WorkflowItems.Tasks.Count);
			dummy.Z0_Description = "I had an adventure";
			Factory.Save(); // Saving to clear cache
			dummy.ApplyWorkflowTemplates();
			AssertEquals("Now the macro matches, the template applied", 1, dummy.WorkflowItems.Tasks.Count);
		}

		public void TestScopeAppliesToBranch()
		{
			MakeProcessCompanyLinkRule(macro: "\"<Logs.AddedLog.SL_GB_NKBranch>\"==\"A01\"");
			var template = MakeTemplate(isGlobal: false);
			MakeTask(template);
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			using (Environment.Env.SetTemporaryUserContext(Environment.Env.CurrentUserPK, branch.PK.ToGuid(), Environment.Env.CurrentDepartmentPK))
			using (WorkflowDataRegistry.Instance.CalculateTemplateUsingCurrentCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				DummyWithWorkflow.AutoLogState.Value = EnterpriseBusinessObject.AutologState.AutoLogged;
				var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
				dummy.ApplyWorkflowTemplates();
				AssertEquals("At first it didn't apply", 0, dummy.WorkflowItems.Tasks.Count);
				branch.GB_Code = "A01";
				Factory.Save();

				var dummy2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
				dummy2.ApplyWorkflowTemplates();
				AssertEquals("Now the macro matches, the template applied", 1, dummy2.WorkflowItems.Tasks.Count);
			}
		}

		#endregion
	}
}
