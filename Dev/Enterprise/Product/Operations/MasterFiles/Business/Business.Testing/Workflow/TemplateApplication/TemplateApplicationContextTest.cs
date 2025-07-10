using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing.Workflow.TemplateApplication
{
	class TemplateApplicationContextTest : TemplateApplicationTestCase
	{
		public void TestDoNotAssumeEachCompanyHasABranch()
		{
			var template = MakeTemplate();
			var task = MakeTask(template);
			var dummyCompany = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			using (WorkflowDataRegistry.Instance.CalculateTemplateUsingCurrentCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				IUserContext[] userContexts = null;
				var dummy = Factory.New<DummyWithWorkflow>();
				dummy.WorkflowInformationProviderOverride = new WorkflowInformationProvider(new[] { dummyCompany.PK, GlbCompany.CurrentCompany.PK });

				AssertNoExceptionThrown("No exception is thrown when company has no active branch", () => userContexts = TemplateApplicationUserContextProvider.GetTemplateUserContexts(dummy, Factory).ToArray());
				AssertEquals("1 context created from CurrentCompany, no context created from dummyCompany.", 1, userContexts.Length);

				dummyCompany.Branches.AddNew();
				Factory.Save();
				AssertNoExceptionThrown("No exception is thrown when company has active branch", () => userContexts = TemplateApplicationUserContextProvider.GetTemplateUserContexts(dummy, Factory).ToArray());
				AssertEquals("1 context created from CurrentCompany, 1 context created from dummyCompany.", 2, userContexts.Length);
			}
		}

		public void TestDoNotApplyTemplateWhenNoUserIsLoggedIn()
		{
			var template = MakeTemplate();
			var task = MakeTask(template);
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				dummy.ApplyWorkflowTemplates();
				AssertEquals("How can you apply workflow template when nobody is logged in?", 0, dummy.WorkflowItems.Count);
			}

			dummy.ApplyWorkflowTemplates();
			AssertEquals("But now it is ok.", 1, dummy.WorkflowItems.Count);
		}

		[TestDate(2018, 7, 3)]
		public void TestUDFConditionsContainingAuditFieldsCanCheat()
		{
			var template = MakeTemplate();
			var templateTask1 = MakeTask(template, udfCondition: @"(""<DateTimeAsString('<Z0_SystemCreateTimeUtc>', 'yyyy-MM-dd HH:mm:ss')>"" > ""2018-06-27 00:00:00"")");
			var templateTask2 = MakeTask(template, udfCondition: @"""1""==""1""");
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals(2, dummy.WorkflowItems.Tasks.Count);
			AssertContainsExactElementsInAnyOrder(new[] { templateTask1.PK, templateTask2.PK }, dummy.WorkflowItems.Tasks.Cast<ProcessTask>().Select(t => t.P9_ParentTemplateID));
		}

		[TestDate(2018, 7, 3)]
		public void TestUDFConditionsContainingAuditFieldsCanCheat_Chain()
		{
			var template = MakeTemplate();
			var templateTask1 = MakeTask(template, udfCondition: @"(""<DateTimeAsString('<OtherDummy.Z0_SystemCreateTimeUtc>', 'yyyy-MM-dd HH:mm:ss')>"" > ""2018-06-27 00:00:00"")");
			var templateTask2 = MakeTask(template, udfCondition: @"""1""==""1""");
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.OtherDummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals(2, dummy.WorkflowItems.Tasks.Count);
			AssertContainsExactElementsInAnyOrder(new[] { templateTask1.PK, templateTask2.PK }, dummy.WorkflowItems.Tasks.Cast<ProcessTask>().Select(t => t.P9_ParentTemplateID));
		}

		[TestDate(2018, 7, 3)]
		public void TestUDFConditionsContainingAuditFieldsCanCheat_Sub()
		{
			var template = MakeTemplate();
			var templateTask1 = MakeTask(template, udfCondition: @"(""<DateTimeAsString('<OtherDummy.Z0_SystemCreateTimeUtc.Date>', 'yyyy-MM-dd HH:mm:ss')>"" > ""2018-06-27 00:00:00"")");
			var templateTask2 = MakeTask(template, udfCondition: @"""1""==""1""");
			Factory.Save();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.OtherDummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals(2, dummy.WorkflowItems.Tasks.Count);
			AssertContainsExactElementsInAnyOrder(new[] { templateTask1.PK, templateTask2.PK }, dummy.WorkflowItems.Tasks.Cast<ProcessTask>().Select(t => t.P9_ParentTemplateID));
		}
	}
}
