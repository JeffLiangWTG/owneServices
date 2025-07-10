using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Business.Test.UniversalTriggers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.Business.Test
{
	[TestedType(typeof(ProcessTemplateTrigger))]
	class ProcessTemplateTriggerTest : EnterpriseBusinessObjectTestCase
	{
		#region Related BusinessObjects

		public void TestTriggerActions()
		{
			var trigger = (ProcessTemplateTrigger)GetNewBusinessObject();
			var action1 = trigger.TriggerActions.AddNew();
			var action2 = trigger.TriggerActions.AddNew();

			Factory.Save();

			trigger.Delete();

			AssertEquals(true, action1.IsDeleted);
			AssertEquals(true, action2.IsDeleted);

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		#region IRootTypeProvider Members

		public void TestIRootTypeProvider_ProcessTemplateTriggerAndLink()
		{
			WorkflowTestCase.EnableBufferManagement();

			var template = WorkflowTestCase.CreateTemplate(Factory, isUniversal: true);
			var templateTrigger = WorkflowTestCase.CreateTrigger(template);

			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var jobTriggerLink = WorkflowTestCase.CreateJobTriggerLink(templateTrigger, orgHeader);

			var templateTriggerAsRootTypeProvider = templateTrigger as IRootTypeProvider;
			var jobTriggerLinkAsRootTypeProvider = jobTriggerLink as IRootTypeProvider;

			CombineAssertions("GIVEN No Valid BM System for process type, WHEN GetRootTypes, THEN should not have typeof(IProcessJobHeader)", () =>
			{
				AssertArrayEqualsByElements("template trigger",
					new[] { typeof(OrgHeader), typeof(ProcessJobTriggerLink) },
					templateTriggerAsRootTypeProvider.RootTypes);

				AssertArrayEqualsByElements("job trigger link",
					new[] { typeof(OrgHeader), typeof(ProcessJobTriggerLink) },
					jobTriggerLinkAsRootTypeProvider.RootTypes);
			});

			CombineAssertions("GIVEN No Valid BM System for process type, WHEN GetRootsCore, THEN should not have ProcessJobHeaderProvider.GetForParent", () =>
			{
				AssertEquals("trigger", 0, templateTriggerAsRootTypeProvider.Roots.Length);

				AssertArrayEqualsByElements("trigger link",
					new[] { (BusinessObject)orgHeader, jobTriggerLink },
					jobTriggerLinkAsRootTypeProvider.Roots);
			});

			var orgSystem = WorkflowTestCase.BMTestHelper.CreateSystem(Factory, "ORG");

			Factory.Save();

			CombineAssertions("GIVEN Valid BM System for process type but a template, WHEN GetRootTypes, THEN should have typeof(IProcessJobHeader)", () =>
			{
				AssertArrayEqualsByElements("trigger",
					new[] { typeof(OrgHeader), typeof(IProcessJobHeader), typeof(ProcessJobTriggerLink) },
					templateTriggerAsRootTypeProvider.RootTypes);

				AssertArrayEqualsByElements("trigger link",
					new[] { typeof(OrgHeader), typeof(IProcessJobHeader), typeof(ProcessJobTriggerLink) },
					jobTriggerLinkAsRootTypeProvider.RootTypes);
			});

			CombineAssertions("GIVEN Valid BM System for process type but a template, WHEN GetRootsCore, THEN should not have ProcessJobHeaderProvider.GetForParent", () =>
			{
				AssertEquals("trigger", 0, templateTriggerAsRootTypeProvider.Roots.Length);

				AssertArrayEqualsByElements("trigger link",
					new[] { orgHeader, (BusinessObject)ProcessJobHeaderProvider.GetForParent(orgHeader, Factory), jobTriggerLink },
					jobTriggerLinkAsRootTypeProvider.Roots);
			});
		}

		public void TestIRootTypeProviderMembers()
		{
			var template = WorkflowTestCase.CreateTemplate(Factory);
			var trigger = WorkflowTestCase.CreateTrigger(template);

			WorkflowTestCase.AssertIRootTypeProviderImplementation(trigger);
		}

		#endregion

		#region IBaseTrigger Members

		public void TestIBaseTriggerMembers()
		{
			var trigger = (IBaseTrigger)WorkflowTestCase.CreateTrigger(Factory);
			AssertNotNull(trigger.TriggerActions);
			AssertNotNull(trigger.TriggerConditions_ForBinding);
			AssertEquals(false, trigger.Cascading);
			AssertEquals(ZString.Empty, trigger.CascadingContext);
			AssertExceptionThrown<InvalidOperationException>(() => trigger.AreTriggerConditionsMet(null, null));
			AssertExceptionThrown<InvalidOperationException>(() => trigger.SetEstimateTime(null, ZDateTimeOffset.UtcNow));
			AssertExceptionThrown<InvalidOperationException>(() => trigger.SetEventTime(null, null, ZDateTimeOffset.UtcNow));
			AssertExceptionThrown<InvalidOperationException>(() => trigger.SetEventTimeWithoutFiringWorkflow(ZDateTimeOffset.UtcNow));
		}

		#endregion

		#region Properties

		public void TestPropertyConversions_ShouldSupportUnicode()
		{
			var trigger = WorkflowTestCase.CreateTrigger(Factory);

			trigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			trigger.TemplateConditions.TemplateCondition2Value = "Wubba lubba dub dub, Morty! ♣◘";

			trigger.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			trigger.TriggerConditions.TriggerConditionValue = "Rubber baby buggy bumpers. M○○♣rty.";

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedTrigger = newFactory.Load<ProcessTemplateTrigger>(trigger.PK);

			AssertEquals(ProcessTasksLookups.UserDefinedCondition, loadedTrigger.TemplateConditions.TemplateCondition2);
			AssertEquals("Wubba lubba dub dub, Morty! ♣◘", loadedTrigger.TemplateConditions.TemplateCondition2Value);

			AssertEquals(EventReferenceConditionList.Codes.ConditionWithMacros, loadedTrigger.TriggerConditions.TriggerCondition);
			AssertEquals("Rubber baby buggy bumpers. M○○♣rty.", loadedTrigger.TriggerConditions.TriggerConditionValue);
		}

		public void TestTriggerField_ShouldAddValidationError()
		{
			var template1 = WorkflowTestCase.CreateTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, isUniversal: false);
			var template2 = WorkflowTestCase.CreateTemplate(Factory, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, isUniversal: true);
			var normalTrigger = template1.WorkflowItems.Triggers.AddNew();
			var universalTrigger = WorkflowTestCase.CreateTrigger(template2, eventCode: "");

			universalTrigger.TriggerConditions.TriggerFieldName = "JW_Vessel";
			normalTrigger.TriggerConditions.TriggerFieldName = "JW_Vessel";

			AssertNoErrors(normalTrigger.TriggerConditions.TriggerFieldNameInfo);
			AssertHasError(universalTrigger.TriggerConditions.TriggerFieldNameInfo, "Field Change triggers are not currently supported on Universal Templates.");
		}

		public void TestApplyTemplateConditionalTask_UserDefinedCondition_ProcesssJobHeaderCondition_ForUniversalTemplate()
		{
			WorkflowTestCase.EnableBufferManagement();

			var system = WorkflowTestCase.BMTestHelper.CreateSystem(Factory, "ORG");
			var template = WorkflowTestCase.CreateTemplate(Factory, "ORG", isUniversal: true);
			var templateTrigger = WorkflowTestCase.CreateTrigger(template, AutoEvents.AuthorisedCode);
			templateTrigger.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			templateTrigger.TemplateConditions.TemplateCondition2Value = "\"<FH_CompletionStatement>\" == \"test1\"";
			Factory.Save();
			AssertEquals("GIVEN template TriggerCondition2 is User Defined (UDF)", ProcessTasksLookups.UserDefinedCondition, templateTrigger.TemplateConditions.TemplateCondition2);

			var jobHeader1 = WorkflowTestCase.BMTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var orgHeader1 = jobHeader1.Parent as OrgHeader;
			Factory.Save();
			orgHeader1.ApplyWorkflowTemplates();
			AssertNotEquals("GIVEN condition is not satisfied", "test1", jobHeader1.FH_CompletionStatement);
			AssertEquals("WHEN apply template, THEN none should be applied", 0, orgHeader1.WorkflowItems.TriggersIncludingRelated.Count);

			var jobHeader2 = WorkflowTestCase.BMTestHelper.CreateJobHeader<OrgHeader>(Factory);
			var orgHeader2 = jobHeader2.Parent as OrgHeader;
			jobHeader2.FH_CompletionStatement = "test1";
			Factory.Save();
			orgHeader2.ApplyWorkflowTemplates();
			AssertEquals("GIVEN condition is satisfied", "test1", jobHeader2.FH_CompletionStatement);
			AssertEquals("WHEN apply template, THEN trigger should be copied", 1, orgHeader2.WorkflowItems.TriggersIncludingRelated.Count);
			UniversalTriggerApplicabilityToJobTest.AssertJobContainsGhostedTriggers(orgHeader2, templateTrigger);
		}

		public void TestP9T_TriggerFiredCountdown_ResourceStringDataAttribute()
		{
			var template = WorkflowTestCase.CreateTemplate(Factory);
			var trigger = WorkflowTestCase.CreateTrigger(template);

			AssertHasCustomAttribute<ResourceStringDataAttribute>(trigger.GetType(), "P9T_TriggerFiredCountdown", false, x => x.Caption == "Trigger Remaining Countdown");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(trigger.GetType(), "P9T_TriggerFiredCountdown", false, x => x.ShortCaption == "Countdown");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(trigger.GetType(), "P9T_TriggerFiredCountdown", false, x => x.FullDescription == "The number of times this trigger will be allowed to fire. When this value reaches zero, the trigger will no longer fire.");
		}

		public void TestP9T_IsEstimate_WorksWithInvalidTime()
		{
			var template = WorkflowTestCase.CreateTemplate(Factory);
			var trigger = WorkflowTestCase.CreateTrigger(template);

			var action = WorkflowTestCase.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent);

			AssertNoExceptionThrown(() => action.PQ_Offset = ZDateTime.Invalid);
		}

		#endregion

		#region Delete

		public void TestDelete_WhenTriggerHasFired_ShouldPromptToDeactivateInstead()
		{
			var template = WorkflowTestCase.CreateTemplate(Factory);
			var trigger1 = WorkflowTestCase.CreateTrigger(template, eventCode: AutoEvents.TagWasAddedOrRemovedCode);
			var trigger2 = WorkflowTestCase.CreateTrigger(template, eventCode: AutoEvents.WorkflowTransferredBetweenSystemComponentsCode);

			var job = Factory.NewWithValidTestData<OrgHeader>();
			var triggerLink = WorkflowTestCase.CreateJobTriggerLink(trigger2, job);

			Factory.Save();

			AssertNoExceptionThrown(trigger1.Delete);
			var ex = AssertExceptionThrown<CannotDeleteException>(trigger2.Delete);

			AssertEquals("This trigger has previously fired and cannot be deleted. It should be marked as inactive instead.", ex.Message);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return WorkflowTestCase.CreateTrigger(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		#endregion

		#region MCR
		public void TestGetSampleContext()
		{
			var template = WorkflowTestCase.CreateTemplate(Factory, "SHP", isUniversal: true);
			var templateTrigger = WorkflowTestCase.CreateTrigger(template, AutoEvents.AuthorisedCode);

			var result = ((IAntlrMacroContextProvider)templateTrigger).GetSampleContext();

			AssertEquals(template.WorkflowDescriptor.WorkflowProviderType, result.ParentType);
		}
		#endregion
	}
}
