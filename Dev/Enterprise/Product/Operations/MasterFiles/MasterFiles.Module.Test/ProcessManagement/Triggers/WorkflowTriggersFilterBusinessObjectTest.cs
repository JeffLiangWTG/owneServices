using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(WorkflowTriggersFilterBusinessObject))]
	sealed class WorkflowTriggersFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Status and Flags

		public void TestEventCodeFilter()
		{
			var matchedValue = "111";
			var notMatchedValue = "000";

			var filter = new WorkflowTriggersFilterBusinessObject();
			((ModuleTextFilter)filter["Event Code"]).Property = matchedValue;
			((ModuleTextFilter)filter["Event Code"]).IsActive = true;

			var dummy = Factory.New<DummyWithWorkflow>();

			var matchedTrigger = dummy.WorkflowItems.Triggers.AddNew();
			matchedTrigger.TriggerConditions.TriggerEventCode = matchedValue;

			var notMatchedTrigger = dummy.WorkflowItems.Triggers.AddNew();
			notMatchedTrigger.TriggerConditions.TriggerEventCode = notMatchedValue;

			dummy.Factory.Save();

			AssertEquals("GIVEN matched", matchedValue, matchedTrigger.TriggerConditions.TriggerEventCode);
			AssertNotEquals("GIVEN not-matched", matchedValue, notMatchedTrigger.TriggerConditions.TriggerEventCode);

			var tasks = new WorkflowTasksForTest { Matched = matchedTrigger, NotMatched = notMatchedTrigger };

			var foundTasks = new ProcessTaskCollection(Factory);
			foundTasks.Load(filter.Filter);

			WorkflowTasksForTest.AssertMatching(tasks, foundTasks);
		}

		#endregion

		#region Text

		public void TestDescriptionFilter()
		{
			var filter = new WorkflowTriggersFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "matched";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_Description,
				matchedValue: "matched",
				notMatchedValue: "not matched",
				isMilestone: false);
		}

		public void TestTriggerConditionFilter()
		{
			var matchedValue = "111";
			var notMatchedValue = "000";

			var filter = new WorkflowTriggersFilterBusinessObject();
			((ModuleTextFilter)filter["Trigger Condition"]).Property = matchedValue;
			((ModuleTextFilter)filter["Trigger Condition"]).IsActive = true;

			var dummy = Factory.New<DummyWithWorkflow>();

			var matchedTrigger = dummy.WorkflowItems.Triggers.AddNew();
			matchedTrigger.TriggerConditions.TriggerCondition = matchedValue;

			var notMatchedTrigger = dummy.WorkflowItems.Triggers.AddNew();
			notMatchedTrigger.TriggerConditions.TriggerCondition = notMatchedValue;

			dummy.Factory.Save();

			AssertEquals("GIVEN matched", matchedValue, matchedTrigger.TriggerConditions.TriggerCondition);
			AssertNotEquals("GIVEN not-matched", matchedValue, notMatchedTrigger.TriggerConditions.TriggerCondition);

			var tasks = new WorkflowTasksForTest { Matched = matchedTrigger, NotMatched = notMatchedTrigger };

			var foundTasks = new ProcessTaskCollection(Factory);
			foundTasks.Load(filter.Filter);

			WorkflowTasksForTest.AssertMatching(tasks, foundTasks);
		}

		public void TestTriggerConditionValueFilter()
		{
			var matchedValue = "111";
			var notMatchedValue = "000";

			var filter = new WorkflowTriggersFilterBusinessObject();
			((ModuleTextFilter)filter["Trigger Condition Value"]).Property = matchedValue;
			((ModuleTextFilter)filter["Trigger Condition Value"]).IsActive = true;

			var dummy = Factory.New<DummyWithWorkflow>();

			var matchedTrigger = dummy.WorkflowItems.Triggers.AddNew();
			matchedTrigger.TriggerConditions.TriggerConditionValue = matchedValue;

			var notMatchedTrigger = dummy.WorkflowItems.Triggers.AddNew();
			notMatchedTrigger.TriggerConditions.TriggerConditionValue = notMatchedValue;

			dummy.Factory.Save();

			AssertEquals("GIVEN matched", matchedValue, matchedTrigger.TriggerConditions.TriggerConditionValue);
			AssertNotEquals("GIVEN not-matched", matchedValue, notMatchedTrigger.TriggerConditions.TriggerConditionValue);

			var tasks = new WorkflowTasksForTest { Matched = matchedTrigger, NotMatched = notMatchedTrigger };

			var foundTasks = new ProcessTaskCollection(Factory);
			foundTasks.Load(filter.Filter);

			WorkflowTasksForTest.AssertMatching(tasks, foundTasks);
		}

		public void TestTriggerConditionValueFilter_IsAndIsNotBlank()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			var blankTrigger = dummy.WorkflowItems.Triggers.AddNew();
			blankTrigger.P9_Description = "Blank";
			blankTrigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent69Code;

			var nonBlankTrigger = dummy.WorkflowItems.Triggers.AddNew();
			nonBlankTrigger.P9_Description = "NonBlank";
			nonBlankTrigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent69Code;
			nonBlankTrigger.TriggerConditions.TriggerConditionValue = "ABC";

			Factory.Save();

			var filterBizo = new WorkflowTriggersFilterBusinessObject();
			filterBizo.AddTextFilterStrip("Event Code", Events.CustomisableEvent69Code);
			var filter = filterBizo.AddTextFilterStrip("Trigger Condition Value");
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			ProcessTask[] results = null;

			AssertNoExceptionThrown("Using the 'is blank' comparison operator for this filter should work just fine. SAD!", () => results = Factory.Load<ProcessTask>(filterBizo.Filter));
			AssertContainsExactElementsInAnyOrder(new[] { "Blank" }, results.Select(x => x.P9_Description));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;

			AssertNoExceptionThrown("Using the 'is not blank' comparison operator for this filter should work just fine. SAD!", () => results = Factory.Load<ProcessTask>(filterBizo.Filter));
			AssertContainsExactElementsInAnyOrder(new[] { "NonBlank" }, results.Select(x => x.P9_Description));
		}

		public void TestTriggerFieldFilter()
		{
			var matchedValue = "111";
			var notMatchedValue = "000";

			var filter = new WorkflowTriggersFilterBusinessObject();
			((ModuleTextFilter)filter["Trigger Field"]).Property = matchedValue;
			((ModuleTextFilter)filter["Trigger Field"]).IsActive = true;

			var dummy = Factory.New<DummyWithWorkflow>();

			var matchedTrigger = dummy.WorkflowItems.Triggers.AddNew();
			matchedTrigger.TriggerConditions.TriggerFieldName = matchedValue;

			var notMatchedTrigger = dummy.WorkflowItems.Triggers.AddNew();
			notMatchedTrigger.TriggerConditions.TriggerFieldName = notMatchedValue;

			dummy.Factory.Save();

			AssertEquals("GIVEN matched", matchedValue, matchedTrigger.TriggerConditions.TriggerFieldName);
			AssertNotEquals("GIVEN not-matched", matchedValue, notMatchedTrigger.TriggerConditions.TriggerFieldName);

			var tasks = new WorkflowTasksForTest { Matched = matchedTrigger, NotMatched = notMatchedTrigger };

			var foundTasks = new ProcessTaskCollection(Factory);
			foundTasks.Load(filter.Filter);

			WorkflowTasksForTest.AssertMatching(tasks, foundTasks);
		}

		public void TestLineTriggerTypeFilter()
		{
			var filter = new WorkflowTriggersFilterBusinessObject();
			((ModuleTextFilter)filter["Line Trigger Type"]).Property = "111";
			((ModuleTextFilter)filter["Line Trigger Type"]).IsActive = true;

			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_LineTriggerType,
				matchedValue: "111",
				notMatchedValue: "000",
				isMilestone: false);
		}

		#endregion

		#region Date

		public void TestEventDateFilter()
		{
			var filter = new WorkflowTriggersFilterBusinessObject();
			((ModuleDateFilter)filter["Event Date"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["Event Date"]).Property1 = new ZDateTime(2005, 1, 1);
			((ModuleDateFilter)filter["Event Date"]).Property2 = new ZDateTime(2005, 1, 3);
			((ModuleDateFilter)filter["Event Date"]).IsActive = true;

			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_ActualDate,
				matchedValue: new ZDateTime(2005, 1, 2),
				notMatchedValue: new ZDateTime(2015, 1, 1),
				isMilestone: false);
		}

		#endregion

		public void TestSourceTemplateFilter()
		{
			var matchedTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var matchedTemplateTrigger = matchedTemplate.WorkflowItems.Triggers.AddNew();
			matchedTemplateTrigger.P9_Description = "Matched Template Trigger";

			var notMatchedTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA");
			var notMatchedTemplateTrigger = notMatchedTemplate.WorkflowItems.Triggers.AddNew();
			notMatchedTemplateTrigger.P9_Description = "Not Matched Template Trigger";

			Factory.Save();

			var filter = new WorkflowTriggersFilterBusinessObject();
			((ModuleGuidFilter)filter["Source Template"]).Property = matchedTemplate.PK;
			((ModuleGuidFilter)filter["Source Template"]).IsActive = true;

			((ModuleGuidFilter)filter["Source Template"]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_ParentTemplateID,
				matchedValue: matchedTemplateTrigger.PK,
				notMatchedValue: notMatchedTemplateTrigger.PK,
				isMilestone: false);

			((ModuleGuidFilter)filter["Source Template"]).SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_ParentTemplateID,
				matchedValue: notMatchedTemplateTrigger.PK,
				notMatchedValue: matchedTemplateTrigger.PK,
				isMilestone: false);

			var dummy = Factory.New<DummyWithWorkflow>();
			var triggerWithBlankSourceTemplate = dummy.WorkflowItems.Triggers.AddNew();
			triggerWithBlankSourceTemplate.P9_Description = "Trigger With Blank Source Template";
			Factory.Save();

			((ModuleGuidFilter)filter["Source Template"]).SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			var foundTasks = new ProcessTaskCollection(Factory);
			foundTasks.Load(filter.Filter);
			AssertCollectionContains("SHOULD find blank-template-Trigger", triggerWithBlankSourceTemplate, foundTasks);
		}

		public void TestSourceTemplateFilter_NotEqualOperator()
		{
			var matchedTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var matchedTemplateTrigger = matchedTemplate.WorkflowItems.Triggers.AddNew();
			matchedTemplateTrigger.P9_Description = "Matched Template Trigger";

			var notMatchedTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA");
			var notMatchedTemplateTrigger = notMatchedTemplate.WorkflowItems.Triggers.AddNew();
			notMatchedTemplateTrigger.P9_Description = "Not Matched Template Trigger";

			var dummyForTriggerWithMatchedTemplate = Factory.New<DummyWithWorkflow>();
			var triggerWithMatchedTemplate = dummyForTriggerWithMatchedTemplate.WorkflowItems.Triggers.AddNew();
			triggerWithMatchedTemplate.P9_ParentTemplateID = matchedTemplate.PK;
			triggerWithMatchedTemplate.P9_Description = "TestSourceTemplateFilter_NotEqualOperator";

			var dummyForTriggerWithNotMatchedTemplate = Factory.New<DummyWithWorkflow>();
			var triggerWithNotMatchedTemplate = dummyForTriggerWithNotMatchedTemplate.WorkflowItems.Triggers.AddNew();
			triggerWithNotMatchedTemplate.P9_ParentTemplateID = notMatchedTemplate.PK;
			triggerWithNotMatchedTemplate.P9_Description = "TestSourceTemplateFilter_NotEqualOperator";

			var dummyForTriggerWithNoSourceTemplate = Factory.New<DummyWithWorkflow>();
			var triggerWithNoSourceTemplate = dummyForTriggerWithNoSourceTemplate.WorkflowItems.Triggers.AddNew();
			triggerWithNoSourceTemplate.P9_Description = "TestSourceTemplateFilter_NotEqualOperator";
			Factory.Save();

			var filter = new WorkflowTriggersFilterBusinessObject();
			((ModuleGuidFilter)filter["Source Template"]).IsActive = true;
			((ModuleGuidFilter)filter["Source Template"]).SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			((ModuleTextFilter)filter["Description"]).IsActive = true;
			((ModuleTextFilter)filter["Description"]).Property = "TestSourceTemplateFilter_NotEqualOperator";
			var foundTasks = new ProcessTaskCollection(Factory);
			foundTasks.Load(filter.Filter);

			AssertCollectionContains("SHOULD find not matched-Trigger", triggerWithNotMatchedTemplate, foundTasks);
			AssertCollectionContains("SHOULD find blank-template-Trigger", triggerWithNoSourceTemplate, foundTasks);
		}

		public void TestSourceTemplateFilter_IsNotBlankFilter()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM");

			var templateTrigger = template.WorkflowItems.Triggers.AddNew();
			templateTrigger.P9_Description = "Template Trigger";

			var dummyForTriggerWithSourceTemplate = Factory.New<DummyWithWorkflow>();
			var triggerWithSourceTemplate = dummyForTriggerWithSourceTemplate.WorkflowItems.Triggers.AddNew();
			triggerWithSourceTemplate.P9_ParentTemplateID = templateTrigger.PK;

			var dummyForTriggerWithNoSourceTemplate = Factory.New<DummyWithWorkflow>();
			var triggerWithNoSourceTemplate = dummyForTriggerWithNoSourceTemplate.WorkflowItems.Triggers.AddNew();
			Factory.Save();

			var filter = new WorkflowTriggersFilterBusinessObject();
			((ModuleGuidFilter)filter["Source Template"]).IsActive = true;
			((ModuleGuidFilter)filter["Source Template"]).SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			var foundTasks = new ProcessTaskCollection(Factory);
			foundTasks.Load(filter.Filter);
			AssertCollectionContains("SHOULD find not blank-template-Trigger", triggerWithSourceTemplate, foundTasks);
			AssertCollectionNotContains("SHOULD not find blank-template-Trigger", triggerWithNoSourceTemplate, foundTasks);
		}

		public void TestCompletionTriggerActionsFilter()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger1 = job.WorkflowItems.Triggers.AddNew();
			var trigger2 = job.WorkflowItems.Triggers.AddNew();
			var trigger3 = job.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "CTA Trigger 1: NTF & NTF";
			trigger2.P9_Description = "CTA Trigger 2: NTF & FLD";
			trigger3.P9_Description = "CTA Trigger 3: DOC & XUE";

			var action1 = trigger1.ProcessTaskNotifications.AddNew();
			var action2 = trigger1.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = "NTF";
			action2.PQ_TriggerType = "NTF";

			var action3 = trigger2.ProcessTaskNotifications.AddNew();
			var action4 = trigger2.ProcessTaskNotifications.AddNew();
			action3.PQ_TriggerType = "NTF";
			action4.PQ_TriggerType = "FLD";

			var action5 = trigger3.ProcessTaskNotifications.AddNew();
			var action6 = trigger3.ProcessTaskNotifications.AddNew();
			action5.PQ_TriggerType = "DOC";
			action6.PQ_TriggerType = "XUE";

			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "CTA Milestone 4";
			var milestoneAction = milestone.ProcessTaskNotifications.AddNew();
			milestoneAction.PQ_TriggerType = "NTF";

			Factory.Save();

			var filterBizo = new WorkflowTriggersFilterBusinessObject();
			filterBizo.AddTextFilterStrip("Description", "CTA");
			var filter = filterBizo.AddFilterStrip<ModuleGuidForeignCollectionFilter>("Completion Trigger Actions");
			filter.SelectedFilters.AddTextFilterStrip("Action", "NTF");

			AssertEquals(FilterCategories.Other, filter.Category);

			AssertEquals(ModuleTextFilter.ComparisonConstants.AnyMatch, filter.ComparisonOperator);
			var results = Factory.Load<ProcessTask>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "CTA Trigger 1: NTF & NTF", "CTA Trigger 2: NTF & FLD" }, results.Select(x => x.P9_Description));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			results = Factory.Load<ProcessTask>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "CTA Trigger 1: NTF & NTF" }, results.Select(x => x.P9_Description));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			results = Factory.Load<ProcessTask>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "CTA Trigger 3: DOC & XUE" }, results.Select(x => x.P9_Description));
		}

		#region Overall

		public void TestIncludeTriggerOnly()
		{
			var task = Factory.New<ProcessTask>();
			var milestone = Factory.New<ProcessTask>();
			var exception = Factory.New<ProcessTask>();
			var trigger = Factory.New<ProcessTask>();

			milestone.IsMilestone = true;
			exception.IsException = true;
			trigger.IsWorkflowTrigger = true;

			Factory.Save();

			var tasks = new ProcessTaskCollection(Factory);
			tasks.Load((new WorkflowTriggersFilterBusinessObject()).Filter);
			AssertCollectionNotContains("Tasks should NOT be loaded", task, tasks);
			AssertCollectionNotContains("Milestones should NOT be loaded", milestone, tasks);
			AssertCollectionNotContains("Workflow exceptions should NOT be loaded", exception, tasks);
			AssertCollectionContains("Triggers should be loaded", trigger, tasks);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WorkflowTriggersFilterBusinessObject();
		}

		#endregion
	}
}
