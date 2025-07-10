using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Workflow.Business.Test.UniversalTriggers
{
	class UniversalTriggerApplicabilityToJobTest : WorkflowTestCase
	{
		public void TestRebuildTriggerCollectionMultipleTimes_DoNotDuplicate()
		{
			var template = CreateTemplate(Factory, "DUM", isUniversal: true);
			var trigger = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);

			Factory.Save();

			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			job.WorkflowItems.TriggersIncludingRelated.Rebuild();

			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);
			job.WorkflowItems.TriggersIncludingRelated.Rebuild();
			job.WorkflowItems.TriggersIncludingRelated.Rebuild();
			AssertEquals(1, job.WorkflowItems.TriggersIncludingRelated.Count);
		}

		public void TestEnsureGhostedTriggersPresent_WhenMultipleTemplatesExist_AlwaysFallBack()
		{
			SetTriggerFallbackMethod(FallbackTypeList.Codes.AlwaysFallback);

			AssertJobContainsGhostedTriggers(job, trigger1);

			job.O1_EnquiryType = "INQ";

			AssertJobContainsGhostedTriggers(job, trigger1, trigger2);

			job.O1_LeadSource = "WEB";

			AssertJobContainsGhostedTriggers(job, trigger1, trigger2, trigger3);

			trigger3.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			trigger3.TemplateConditions.TemplateCondition2Value = @"""<O1_City>""==""FakeNews""";

			Factory.Save();

			AssertJobContainsGhostedTriggers(job, trigger1, trigger2);

			job.O1_EnquiryType = "";

			AssertJobContainsGhostedTriggers(job, trigger1);
		}

		public void TestEnsureGhostedTriggersPresent_WhenMultipleTemplatesExist_NeverFallBack()
		{
			SetTriggerFallbackMethod(FallbackTypeList.Codes.NeverFallback);

			AssertJobContainsGhostedTriggers(job, trigger1);

			job.O1_EnquiryType = "INQ";

			AssertJobContainsGhostedTriggers(job, trigger2);

			job.O1_LeadSource = "WEB";

			AssertJobContainsGhostedTriggers(job, trigger3);

			trigger3.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			trigger3.TemplateConditions.TemplateCondition2Value = @"""<O1_City>""==""FakeNews""";

			Factory.Save();

			AssertJobContainsGhostedTriggers(job);

			job.O1_EnquiryType = "";

			AssertJobContainsGhostedTriggers(job, trigger1);
		}

		#region Implementation

		SalesEnquiry job;
		ProcessTaskTemplate template1, template2, template3;
		ProcessTemplateTrigger trigger1, trigger2, trigger3;

		protected override void SetUp()
		{
			base.SetUp();

			template1 = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true);
			template2 = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true, subType1: "INQ");
			template3 = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true, subType1: "INQ", subType2: "WEB");

			trigger1 = CreateTrigger(template1, AutoEvents.AddedARecordToTheSystemCode);
			trigger2 = CreateTrigger(template2, AutoEvents.TagWasAddedOrRemovedCode);
			trigger3 = CreateTrigger(template3, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode);

			Factory.Save();

			job = Factory.New<SalesEnquiry>();
		}

		void SetTriggerFallbackMethod(string fallbackMethod)
		{
			template1.P0_TriggerFallbackMethod = fallbackMethod;
			template2.P0_TriggerFallbackMethod = fallbackMethod;
			template3.P0_TriggerFallbackMethod = fallbackMethod;
		}

		internal static void AssertJobContainsGhostedTriggers(IWorkflowProvider job, params ProcessTemplateTrigger[] templateTriggers)
		{
			job.WorkflowItems.TriggersIncludingRelated.Rebuild();
			CombineAssertions($"Job [{job}] should contain ghosted representations of template triggers", () =>
			{
				AssertEquals("Total trigger count on this job", templateTriggers.Length, job.WorkflowItems.TriggersIncludingRelated.Count);

				foreach (var templateTrigger in templateTriggers)
				{
					var ghostedTrigger = job.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().SingleOrDefault(t => t.P9_ParentTemplateID == templateTrigger.PK);

					if (ghostedTrigger == null)
					{
						Fail("Missing ghost of template trigger: " + templateTrigger.GetDiagnosticLogInfo());
					}
				}
			});
		}

		#endregion
	}
}
