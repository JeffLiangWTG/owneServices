using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Workflow.Business.Test.UniversalTriggers
{
	class UniversalTriggerPerformanceTest : WorkflowTestCase
	{
		public void TestDbHits_CalculatingTriggersToShowOnJob()
		{
			var template1 = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true);
			var template2 = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true, subType1: "INQ");
			var template3 = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true, subType1: "INQ", subType2: "WEB");

			template1.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			template2.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			template3.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			CreateTrigger(template1, AutoEvents.AddedARecordToTheSystemCode);
			CreateTrigger(template2, AutoEvents.TagWasAddedOrRemovedCode);
			CreateTrigger(template3, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			var job = newFactory.New<SalesEnquiry>();
			job.O1_EnquiryType = "INQ";
			job.O1_LeadSource = "WEB";

			AssertEquals(3, job.WorkflowItems.TriggersIncludingRelated.Count);

			AssertDbHits(new Dictionary<string, int>
			{
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ ProcessTemplateTriggerSchema.Constants.TableName, 1 },
				{ ProcessJobTriggerLinkSchema.Constants.TableName, 1 },
				{ StmEventSchema.Constants.TableName, 1 },
			}, newFactory);
		}

		public void TestDbHits_FiringTriggers()
		{
			var template1 = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true);
			var template2 = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true, subType1: "INQ");
			var template3 = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true, subType1: "INQ", subType2: "WEB");

			template1.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			template2.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			template3.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;

			CreateTrigger(template1, AutoEvents.StatusChangeCode);
			CreateTrigger(template2, AutoEvents.TagWasAddedOrRemovedCode);
			CreateTrigger(template3, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			var job = newFactory.New<SalesEnquiry>();
			job.O1_EnquiryType = "INQ";
			job.O1_LeadSource = "WEB";

			newFactory.Save();

			newFactory = newFactory.CreateNewFactory();
			job = newFactory.Load<SalesEnquiry>(job.PK);

			job.GetLogs().AddNew(AutoEvents.StatusChange);
			job.GetLogs().AddNew(AutoEvents.TagWasAddedOrRemoved);
			job.GetLogs().AddNew(AutoEvents.WorkflowTransferredBetweenSystemComponents);

			const int eventsRaisedInThisTest = 3;

			AssertDbHits(new Dictionary<string, int>
			{
				{ OrgColdCallRegisterSchema.Constants.TableName, 1 },
				{ ProcessTaskTemplateSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 2 }, // One extra for firing milestones that come after this trigger in sequence (this should probably be changed to only happen for milestones firing not triggers).
				{ ProcessTemplateTriggerSchema.Constants.TableName, eventsRaisedInThisTest },
				{ ProcessJobTriggerLinkSchema.Constants.TableName, eventsRaisedInThisTest },
				{ ProcessTaskNotificationSchema.Constants.TableName, eventsRaisedInThisTest } // Each event added will also load the trigger's actions from ProcessTaskNotifications table
			}, newFactory, ignoreHitsFromTablesCachedInUberFactory: true);

			AssertUniversalTriggerFired(job, AutoEvents.StatusChangeCode);
			AssertUniversalTriggerFired(job, AutoEvents.TagWasAddedOrRemovedCode);
			AssertUniversalTriggerFired(job, AutoEvents.WorkflowTransferredBetweenSystemComponentsCode);
		}

		public void TestApplyWorkflowTemplates_WhenTriggersCollectionHasNotBeenBound_ShouldNotMaintainUniversalTriggers()
		{
			var template = CreateTemplate(Factory, WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode, isUniversal: true);
			var templateTrigger1 = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			var templateTrigger2 = CreateTrigger(template, AutoEvents.TagWasAddedOrRemovedCode);
			templateTrigger2.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTrigger2.TemplateConditions.TemplateCondition2Value = @"""<O1_EnquiryType>""==""INQ""";

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var job = newFactory.New<SalesEnquiry>();
			job.O1_LeadSource = "WEB";

			newFactory.Save();

			AssertGhostedTriggerCount("Universal trigger should not be added to the job because we haven't accessed the TriggersIncludingRelated collection", 0, job);

			job.ApplyWorkflowTemplates();

			AssertGhostedTriggerCount("Universal trigger should still not be added to the job because we haven't caused the TriggersIncludingRelated collection to be populated, even though we instantiated it", 0, job);

			job.O1_EnquiryType = "INQ";
			newFactory.Save();

			AssertGhostedTriggerCount("Universal trigger should still not be added to the job because we haven't caused the TriggersIncludingRelated collection to be populated, even though we instantiated it", 0, job);

			AssertEquals("Accessing this collection's Count property causes it to populated", 2, job.WorkflowItems.TriggersIncludingRelated.Count);
			AssertGhostedTriggerCount("Now triggers should be present in the job's factory", 2, job);

			job.O1_EnquiryType = "";
			newFactory.Save();

			AssertGhostedTriggerCount("Universal triggers should be maintained now that TriggersIncludingRelated has been populated", 1, job);
		}

		static void AssertGhostedTriggerCount(string message, int expectedCount, SalesEnquiry enquiry)
		{
			var triggers = enquiry.Factory.Load<ProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, enquiry.PK).AddToFilter(ProcessTasksSchema.P9_Type, Core.Constants.Workflow.WorkflowTriggerType)).Where(t => t.IsNonPersistedRepresentationOfTemplateTrigger).ToArray();

			AssertEquals(expectedCount, triggers.Length);
		}
	}
}
