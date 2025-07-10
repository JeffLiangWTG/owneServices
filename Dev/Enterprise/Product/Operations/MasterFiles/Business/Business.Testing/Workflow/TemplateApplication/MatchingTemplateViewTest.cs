using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(MatchingTemplateView))]
	class MatchingTemplateViewTest : NonPersistentBusinessObjectTestCase
	{
		#region Setup

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MatchingTemplateView(Factory.NewWithValidTestData<DummyWithWorkflow>().WorkflowItems.Tasks);
		}

		void AssertLine(MatchingTemplateViewLine line, string expectedName, string expectedMatch)
		{
			AssertEquals(expectedName, line.TemplateName);
			AssertEquals(expectedMatch, line.MatchDescription);
		}

		ProcessTaskTemplate CreateTemplate(string name = "DumDum template", string fallbackType = null, string subtype1 = "", string subtype2 = "")
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_Name = name;
			template.P0_Description = "The template for numpty-brained ning-nongs";
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template.P0_SubType1 = subtype1;
			template.P0_SubType2 = subtype2;
			template.P0_TaskFallbackMethod = template.P0_TriggerFallbackMethod = template.P0_MilestoneFallbackMethod = fallbackType ?? FallbackTypeList.Codes.EmptyFallback;
			return template;
		}

		DummyWithWorkflow GetDummy(string subtype1 = "", string subtype2 = "")
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.SubType1 = subtype1;
			dummy.Z0_Code = subtype2;
			return dummy;
		}

		ProcessTask CreateTask(ProcessTaskTemplate template)
		{
			var task = template.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Task" + task.P9_Sequence;
			return task;
		}

		ProcessTask CreateMilestone(ProcessTaskTemplate template)
		{
			var milestone = template.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Milestone" + milestone.P9_Sequence;
			milestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			CreateTriggerAction(milestone);
			return milestone;
		}

		ProcessTask CreateTrigger(ProcessTaskTemplate template)
		{
			var trigger = template.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Trigger" + trigger.P9_Sequence;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			CreateTriggerAction(trigger);
			return trigger;
		}

		ProcessTaskNotification CreateTriggerAction(ProcessTask triggerable)
		{
			var notification = triggerable.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "NTF";
			notification.PQ_TriggerParty = "EML";
			notification.PQ_EmailAddr = "bad@janet.com";
			return notification;
		}

		#endregion

		#region Expected Workflow Collections

		public void TestMilestoneCollection()
		{
			var template = CreateTemplate();
			CreateMilestone(template);
			Factory.Save();

			var view = new MatchingTemplateView(Factory.NewWithValidTestData<DummyWithWorkflow>().WorkflowItems.Milestones);
			var line = (MatchingTemplateViewLine)view.MatchingTemplates.Single();
			AssertEquals(template.P0_Name, line.TemplateName);
			AssertEquals(template.P0_Description, line.TemplateDescription);
			AssertEquals(1, line.MatchOrder);
			AssertEquals(MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchAndApply, line.MatchDescription);
		}

		public void TestTaskCollection()
		{
			var template = CreateTemplate();
			CreateTask(template);
			Factory.Save();

			var view = new MatchingTemplateView(Factory.NewWithValidTestData<DummyWithWorkflow>().WorkflowItems.Tasks);
			var line = (MatchingTemplateViewLine)view.MatchingTemplates.Single();
			AssertEquals(template.P0_Name, line.TemplateName);
			AssertEquals(template.P0_Description, line.TemplateDescription);
			AssertEquals(1, line.MatchOrder);
			AssertEquals(MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchAndApply, line.MatchDescription);
		}

		public void TestTriggerCollection()
		{
			var template = CreateTemplate();
			CreateTrigger(template);
			Factory.Save();

			var view = new MatchingTemplateView(Factory.NewWithValidTestData<DummyWithWorkflow>().WorkflowItems.Triggers);
			var line = (MatchingTemplateViewLine)view.MatchingTemplates.Single();
			AssertEquals(template.P0_Name, line.TemplateName);
			AssertEquals(template.P0_Description, line.TemplateDescription);
			AssertEquals(1, line.MatchOrder);
			AssertEquals(MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchAndApply, line.MatchDescription);
		}

		public void TestTriggerCollectionWithCompanyRules()
		{
			var template = CreateTemplate();
			template.P0_Name = "DumDum Template";
			template.P0_ProcessType = "DUM";
			CreateTrigger(template);
			var companyRule = Factory.New<IProcessCompanyLinkRule>();
			companyRule.PCR_Type = "DUM";
			companyRule.PCR_Macro = "\"<P0_Name>\"!=\"DumDum Template\"";
			companyRule.PCR_GC_Company = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var view = new MatchingTemplateView(Factory.NewWithValidTestData<DummyWithWorkflow>().WorkflowItems.Triggers);
			AssertEquals("The template should be filtered via the rule", 0, view.MatchingTemplates.Count);
		}

		#endregion

		#region Fallback visualization

		public void TestAlwaysFallBack()
		{
			var template1 = CreateTemplate("First", FallbackTypeList.Codes.AlwaysFallback, "MIM", "MAM");
			CreateTrigger(template1);

			var template2 = CreateTemplate("Second", FallbackTypeList.Codes.AlwaysFallback, "MIM");
			CreateTrigger(template2);

			var template3 = CreateTemplate("Third", FallbackTypeList.Codes.AlwaysFallback);
			CreateTrigger(template3);
			Factory.Save();

			var view = new MatchingTemplateView(GetDummy("MIM", "MAM").WorkflowItems.Triggers);
			var lines = view.MatchingTemplates.Cast<MatchingTemplateViewLine>().OrderBy(s => s.MatchOrder).ToArray();

			AssertLine(lines[0], "First", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchAndApply);
			AssertLine(lines[1], "Second", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchAndApply);
			AssertLine(lines[2], "Third", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchAndApply);
		}

		public void TestNeverFallBack()
		{
			var template1 = CreateTemplate("First", FallbackTypeList.Codes.NeverFallback, "MIM", "MAM");
			CreateTrigger(template1);

			var template2 = CreateTemplate("Second", FallbackTypeList.Codes.NeverFallback, "MIM");
			CreateTrigger(template2);

			var template3 = CreateTemplate("Third", FallbackTypeList.Codes.NeverFallback);
			CreateTrigger(template3);
			Factory.Save();

			var view = new MatchingTemplateView(GetDummy("MIM", "MAM").WorkflowItems.Triggers);
			var lines = view.MatchingTemplates.Cast<MatchingTemplateViewLine>().OrderBy(s => s.MatchOrder).ToArray();

			AssertLine(lines[0], "First", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchAndApply);
			AssertLine(lines[1], "Second", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchButNeverApplyBecauseOfNFB);
			AssertLine(lines[2], "Third", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchButNeverApplyBecauseOfNFB);
		}

		public void TestFallBackIfEmpty()
		{
			var template1 = CreateTemplate("First", FallbackTypeList.Codes.EmptyFallback, "MIM", "MAM");
			CreateTrigger(template1);

			var template2 = CreateTemplate("Second", FallbackTypeList.Codes.EmptyFallback, "MIM");
			CreateTrigger(template2);

			var template3 = CreateTemplate("Third", FallbackTypeList.Codes.EmptyFallback);
			CreateTrigger(template3);
			Factory.Save();

			var view = new MatchingTemplateView(GetDummy("MIM", "MAM").WorkflowItems.Triggers);
			var lines = view.MatchingTemplates.Cast<MatchingTemplateViewLine>().OrderBy(s => s.MatchOrder).ToArray();

			AssertLine(lines[0], "First", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchAndApply);
			AssertLine(lines[1], "Second", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchButNeverApplyBecauseOfEFB);
			AssertLine(lines[2], "Third", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchButNeverApplyBecauseOfEFB);
		}

		public void TestFallBackIfEmpty_UDF()
		{
			var template1 = CreateTemplate("First", FallbackTypeList.Codes.EmptyFallback, "MIM", "MAM");
			CreateTrigger(template1).TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;

			var template2 = CreateTemplate("Second", FallbackTypeList.Codes.EmptyFallback, "MIM");
			CreateTrigger(template2).TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;

			var template3 = CreateTemplate("Third", FallbackTypeList.Codes.EmptyFallback);
			CreateTrigger(template3);
			Factory.Save();

			var view = new MatchingTemplateView(GetDummy("MIM", "MAM").WorkflowItems.Triggers);
			var lines = view.MatchingTemplates.Cast<MatchingTemplateViewLine>().OrderBy(s => s.MatchOrder).ToArray();

			AssertLine(lines[0], "First", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchAndApply);
			AssertLine(lines[1], "Second", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchBasedOnUdf);
			AssertLine(lines[2], "Third", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchBasedOnUdf);
		}

		public void TestFallBackIfEmpty_ThenNeverFallBack()
		{
			var template1 = CreateTemplate("First", FallbackTypeList.Codes.EmptyFallback, "MIM", "MAM");
			CreateTrigger(template1).TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;

			var template2 = CreateTemplate("Second", FallbackTypeList.Codes.NeverFallback, "MIM");
			CreateTrigger(template2);

			var template3 = CreateTemplate("Third", FallbackTypeList.Codes.NeverFallback);
			CreateTrigger(template3);
			Factory.Save();

			var view = new MatchingTemplateView(GetDummy("MIM", "MAM").WorkflowItems.Triggers);
			var lines = view.MatchingTemplates.Cast<MatchingTemplateViewLine>().OrderBy(s => s.MatchOrder).ToArray();

			AssertLine(lines[0], "First", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchAndApply);
			AssertLine(lines[1], "Second", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchBasedOnUdf);
			AssertLine(lines[2], "Third", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchButNeverApplyBecauseOfNFB);
		}

		public void TestNFB_ThenOther()
		{
			var template1 = CreateTemplate("First", FallbackTypeList.Codes.NeverFallback, "MIM", "MAM");
			CreateTrigger(template1);

			var template2 = CreateTemplate("Second", FallbackTypeList.Codes.AlwaysFallback, "MIM");
			CreateTrigger(template2);

			var template3 = CreateTemplate("Third", FallbackTypeList.Codes.EmptyFallback);
			CreateTrigger(template3);
			Factory.Save();

			var view = new MatchingTemplateView(GetDummy("MIM", "MAM").WorkflowItems.Triggers);
			var lines = view.MatchingTemplates.Cast<MatchingTemplateViewLine>().OrderBy(s => s.MatchOrder).ToArray();

			AssertLine(lines[0], "First", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchAndApply);
			AssertLine(lines[1], "Second", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchButNeverApplyBecauseOfNFB);
			AssertLine(lines[2], "Third", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchButNeverApplyBecauseOfNFB);
		}

		public void TestGlobalAndNonGlobalTemplateFallback()
		{
			var template1 = CreateTemplate("First", FallbackTypeList.Codes.EmptyFallback);
			template1.GlobalTemplate = false;
			CreateTrigger(template1);

			var template2 = CreateTemplate("Second", FallbackTypeList.Codes.EmptyFallback);
			template2.GlobalTemplate = true;
			CreateTrigger(template2);

			Factory.Save();

			var view = new MatchingTemplateView(GetDummy("MIM", "MAM").WorkflowItems.Triggers);
			var lines = view.MatchingTemplates.Cast<MatchingTemplateViewLine>().OrderBy(s => s.MatchOrder).ToArray();

			AssertLine(lines[0], "First", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchAndApply);
			AssertLine(lines[1], "Second", MatchingWorkflowTemplateStatusCodeList.Descriptions.MatchButNeverApplyBecauseOfEFB);
		}

		#endregion

		#region Effective Date Range

		[TestDate(2021, 3, 19)]
		public void TestEffectiveDateRange_UseTheJobCreateTime()
		{
			var template1 = CreateTemplate("First", FallbackTypeList.Codes.AlwaysFallback, "MIM", "MAM");
			CreateTrigger(template1);

			var template2 = CreateTemplate("Second", FallbackTypeList.Codes.AlwaysFallback, "MIM");
			template2.P0_EffectiveEndDateUtc = ZDateTime.UtcNow.AddDays(5);
			CreateTrigger(template2);

			var template3 = CreateTemplate("Third", FallbackTypeList.Codes.AlwaysFallback);
			CreateTrigger(template3);
			Factory.Save();

			var dummy = GetDummy("MIM", "MAM");
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			dummy.Logs.AddNew(Events.AddedARecordToTheSystem, ZDateTimeOffset.Now); // Set the create date into the past
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();

			TestDateAttribute.AddDays(10);

			var dummy2 = GetDummy("MIM", "MAM");
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			dummy2.Logs.AddNew(Events.AddedARecordToTheSystem, ZDateTimeOffset.Now); // Set the create date into the future!
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();

			var view = new MatchingTemplateView(dummy.WorkflowItems.Triggers);
			AssertEquals("We get everything because we're within the matching date range", 3, view.MatchingTemplates.Count);
			var view2 = new MatchingTemplateView(dummy2.WorkflowItems.Triggers);
			AssertEquals("Skipping the second matching line because it is outside of the effective start date.", 2, view2.MatchingTemplates.Count);
		}

		#endregion

		#region Edge Cases

		public void TestNoTemplate()
		{
			var view = new MatchingTemplateView(GetDummy("MIM", "MAM").WorkflowItems.Triggers);
			var lines = view.MatchingTemplates.Cast<MatchingTemplateViewLine>().OrderBy(s => s.MatchOrder).ToArray();
			AssertEquals(0, lines.Length);
		}

		public void TestTemplateTemplate()
		{
			var template1 = CreateTemplate("First", FallbackTypeList.Codes.AlwaysFallback);
			CreateTrigger(template1);

			var view = new MatchingTemplateView(template1.WorkflowItems.Triggers);
			var lines = view.MatchingTemplates.Cast<MatchingTemplateViewLine>().OrderBy(s => s.MatchOrder).ToArray();
			AssertEquals(0, lines.Length);
		}

		#endregion
	}

	[TestedType(typeof(MatchingTemplateViewLine))]
	class MatchingTemplateViewLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new MatchingTemplateViewLine(MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", name: "WUBBALUBBA", description: "DUB DUB! REEEEE"), "NFB", MatchingWorkflowTemplateStatusCodeList.Codes.MatchButNeverApplyBecauseOfNFB, 0);
		}
	}

	[TestedType(typeof(MatchingTemplateViewLineCollection))]
	class MatchingTemplateViewLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MatchingTemplateViewLineCollection>
	{
		protected override MatchingTemplateViewLineCollection GetCollectionToTest() => new MatchingTemplateViewLineCollection();
		protected override BusinessObject GetNewElementToAddToTheCollection() => new MatchingTemplateViewLine(MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", name: "REEEE", description: "IM PICKLE RICK"), "NFB", MatchingWorkflowTemplateStatusCodeList.Codes.MatchBasedOnUdf, 0);
	}
}
