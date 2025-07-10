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
	[TestedType(typeof(WorkflowMilestonesFilterBusinessObject))]
	sealed class WorkflowMilestonesFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestSequenceFilter()
		{
			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleNumberRangeFilter)filter["Sequence"]).Property1 = 80;
			((ModuleNumberRangeFilter)filter["Sequence"]).Property2 = 110;
			((ModuleNumberRangeFilter)filter["Sequence"]).IsActive = true;

			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_Sequence,
				matchedValue: 100,
				notMatchedValue: 1000,
				isMilestone: true);
		}

		#region Status and Flags

		public void TestEventCodeFilter()
		{
			var matchedValue = "111";
			var notMatchedValue = "000";

			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleTextFilter)filter["Event Code"]).Property = matchedValue;
			((ModuleTextFilter)filter["Event Code"]).IsActive = true;

			var dummy = Factory.New<DummyWithWorkflow>();

			var matchedMilestone = dummy.WorkflowItems.Milestones.AddNew();
			matchedMilestone.TriggerConditions.TriggerEventCode = matchedValue;

			var notMatchedMilestone = dummy.WorkflowItems.Milestones.AddNew();
			notMatchedMilestone.TriggerConditions.TriggerEventCode = notMatchedValue;

			dummy.Factory.Save();

			AssertEquals("GIVEN matched", matchedValue, matchedMilestone.TriggerConditions.TriggerEventCode);
			AssertNotEquals("GIVEN not-matched", matchedValue, notMatchedMilestone.TriggerConditions.TriggerEventCode);

			var tasks = new WorkflowTasksForTest { Matched = matchedMilestone, NotMatched = notMatchedMilestone };

			var foundTasks = new ProcessTaskCollection(Factory);
			foundTasks.Load(filter.Filter);

			WorkflowTasksForTest.AssertMatching(tasks, foundTasks);
		}

		public void TestExceptionEventCodeFilter()
		{
			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleTextFilter)filter["Exception Event Code"]).Property = "111";
			((ModuleTextFilter)filter["Exception Event Code"]).IsActive = true;

			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_SE_NKExceptionEvent,
				matchedValue: "111",
				notMatchedValue: "000",
				isMilestone: true);
		}

		#endregion

		#region Text

		public void TestDescriptionFilter()
		{
			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleTextFilter)filter["Description"]).Property = "matched";
			((ModuleTextFilter)filter["Description"]).IsActive = true;

			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_Description,
				matchedValue: "matched",
				notMatchedValue: "not matched",
				isMilestone: true);
		}

		public void TestTriggerConditionFilter()
		{
			var matchedValue = "111";
			var notMatchedValue = "000";

			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleTextFilter)filter["Trigger Condition"]).Property = matchedValue;
			((ModuleTextFilter)filter["Trigger Condition"]).IsActive = true;

			var dummy = Factory.New<DummyWithWorkflow>();

			var matchedMilestone = dummy.WorkflowItems.Milestones.AddNew();
			matchedMilestone.TriggerConditions.TriggerCondition = matchedValue;

			var notMatchedMilestone = dummy.WorkflowItems.Milestones.AddNew();
			notMatchedMilestone.TriggerConditions.TriggerCondition = notMatchedValue;

			dummy.Factory.Save();

			AssertEquals("GIVEN matched", matchedValue, matchedMilestone.TriggerConditions.TriggerCondition);
			AssertNotEquals("GIVEN not-matched", matchedValue, notMatchedMilestone.TriggerConditions.TriggerCondition);

			var tasks = new WorkflowTasksForTest { Matched = matchedMilestone, NotMatched = notMatchedMilestone };

			var foundTasks = new ProcessTaskCollection(Factory);
			foundTasks.Load(filter.Filter);

			WorkflowTasksForTest.AssertMatching(tasks, foundTasks);
		}

		public void TestTriggerConditionValueFilter()
		{
			var matchedValue = "111";
			var notMatchedValue = "000";

			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleTextFilter)filter["Trigger Condition Value"]).Property = matchedValue;
			((ModuleTextFilter)filter["Trigger Condition Value"]).IsActive = true;

			var dummy = Factory.New<DummyWithWorkflow>();

			var matchedMilestone = dummy.WorkflowItems.Milestones.AddNew();
			matchedMilestone.TriggerConditions.TriggerConditionValue = matchedValue;

			var notMatchedMilestone = dummy.WorkflowItems.Milestones.AddNew();
			notMatchedMilestone.TriggerConditions.TriggerConditionValue = notMatchedValue;

			dummy.Factory.Save();

			AssertEquals("GIVEN matched", matchedValue, matchedMilestone.TriggerConditions.TriggerConditionValue);
			AssertNotEquals("GIVEN not-matched", matchedValue, notMatchedMilestone.TriggerConditions.TriggerConditionValue);

			var tasks = new WorkflowTasksForTest { Matched = matchedMilestone, NotMatched = notMatchedMilestone };

			var foundTasks = new ProcessTaskCollection(Factory);
			foundTasks.Load(filter.Filter);

			WorkflowTasksForTest.AssertMatching(tasks, foundTasks);
		}

		public void TestTriggerConditionValueFilter_IsAndIsNotBlank()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var dummy = Factory.New<DummyWithWorkflow>();

			var blankMilestone = dummy.WorkflowItems.Milestones.AddNew();
			blankMilestone.P9_Description = "Blank";
			blankMilestone.P9_GS_NKAssignedStaffMember = staff.GS_Code;

			var nonBlankMilestone = dummy.WorkflowItems.Milestones.AddNew();
			nonBlankMilestone.P9_Description = "NonBlank";
			nonBlankMilestone.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			nonBlankMilestone.TriggerConditions.TriggerConditionValue = "ABC";

			Factory.Save();

			var filterBizo = new WorkflowMilestonesFilterBusinessObject();
			filterBizo.AddNkFilterStrip("Staff", staff.GS_Code);
			var filter = filterBizo.AddTextFilterStrip("Trigger Condition Value");
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsBlank;

			ProcessTask[] results = null;

			AssertNoExceptionThrown("Using the 'is blank' comparison operator for this filter should work just fine. SAD!", () => results = Factory.Load<ProcessTask>(filterBizo.Filter));
			AssertContainsExactElementsInAnyOrder(new[] { "Blank" }, results.Select(x => x.P9_Description));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.IsNotBlank;

			AssertNoExceptionThrown("Using the 'is not blank' comparison operator for this filter should work just fine. SAD!", () => results = Factory.Load<ProcessTask>(filterBizo.Filter));
			AssertContainsExactElementsInAnyOrder(new[] { "NonBlank" }, results.Select(x => x.P9_Description));
		}

		#endregion

		#region Date

		public void TestEstimatedFilter()
		{
			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleDateFilter)filter["Estimated"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["Estimated"]).Property1 = new ZDateTime(2005, 1, 1);
			((ModuleDateFilter)filter["Estimated"]).Property2 = new ZDateTime(2005, 1, 3);
			((ModuleDateFilter)filter["Estimated"]).IsActive = true;

			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_ScheduledDate,
				matchedValue: new ZDateTime(2005, 1, 2),
				notMatchedValue: new ZDateTime(2015, 1, 10),
				isMilestone: true);
		}

		[TestDate(2017, 07, 03, 7, 30, 0)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestEstimatedFilter_UsesUtc()
		{
			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleDateFilter)filter["Estimated"]).PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			((ModuleDateFilter)filter["Estimated"]).Property1 = ZDateTime.Now.AddMinutes(-5);
			((ModuleDateFilter)filter["Estimated"]).Property2 = ZDateTime.Now.AddMinutes(5);
			((ModuleDateFilter)filter["Estimated"]).IsActive = true;

			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_ScheduledDate,
				matchedValue: ZDateTime.Now,
				notMatchedValue: ZDateTime.Now.AddMinutes(20),
				isMilestone: true);
		}

		public void TestOriginalEstimatedFilter()
		{
			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleDateFilter)filter["Original Estimated"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["Original Estimated"]).Property1 = new ZDateTime(2005, 1, 1);
			((ModuleDateFilter)filter["Original Estimated"]).Property2 = new ZDateTime(2005, 1, 3);
			((ModuleDateFilter)filter["Original Estimated"]).IsActive = true;

			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_OriginalScheduledDateUtc,
				matchedValue: new ZDateTime(2005, 1, 2),
				notMatchedValue: new ZDateTime(2015, 1, 1),
				isMilestone: true);
		}

		public void TestActualStartFilter()
		{
			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleDateFilter)filter["Actual Start"]).PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			((ModuleDateFilter)filter["Actual Start"]).Property1 = new ZDateTime(2005, 1, 1);
			((ModuleDateFilter)filter["Actual Start"]).Property2 = new ZDateTime(2005, 1, 3);
			((ModuleDateFilter)filter["Actual Start"]).IsActive = true;

			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_ActualDate,
				matchedValue: new ZDateTime(2005, 1, 2),
				notMatchedValue: new ZDateTime(2015, 1, 1),
				isMilestone: true);
		}

		#endregion

		#region Findbox Filters

		public void TestStaffFilter()
		{
			var matchedValue = Factory.NewWithValidTestData<GlbStaff>().GS_Code;
			var notMatchedValue = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleNkFilter)filter["Staff"]).Property = matchedValue;
			((ModuleNkFilter)filter["Staff"]).IsActive = true;

			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_GS_NKAssignedStaffMember,
				matchedValue: matchedValue,
				notMatchedValue: notMatchedValue,
				isMilestone: true);
		}

		public void TestGroupFilter()
		{
			var matchedValue = Factory.NewWithValidTestData<GlbGroup>().PK;
			var notMatchedValue = Factory.NewWithValidTestData<GlbGroup>().PK;

			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleGuidFilter)filter["Group"]).Property = matchedValue;
			((ModuleGuidFilter)filter["Group"]).IsActive = true;

			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_GG_AssignedGroup,
				matchedValue: matchedValue,
				notMatchedValue: notMatchedValue,
				isMilestone: true);
		}

		public void TestCompanyFilter()
		{
			var matchedValue = GlbCompany.CurrentCompany.PK;
			var notMatchedValue = Factory.NewWithValidTestData<GlbCompany>().PK;

			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleGuidFilter)filter["Company"]).Property = matchedValue;
			((ModuleGuidFilter)filter["Company"]).IsActive = true;

			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_GC,
				matchedValue: matchedValue,
				notMatchedValue: notMatchedValue,
				isMilestone: true);
		}

		public void TestSourceTemplateFilter()
		{
			var matchedTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var matchedTemplateMilestone = matchedTemplate.WorkflowItems.Milestones.AddNew();
			matchedTemplateMilestone.P9_Description = "Matched Template Milestone";

			var notMatchedTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA");
			var notMatchedTemplateMilestone = notMatchedTemplate.WorkflowItems.Milestones.AddNew();
			notMatchedTemplateMilestone.P9_Description = "Not Matched Template Milestone";

			Factory.Save();

			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleGuidFilter)filter["Source Template"]).Property = matchedTemplate.PK;
			((ModuleGuidFilter)filter["Source Template"]).IsActive = true;

			((ModuleGuidFilter)filter["Source Template"]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_ParentTemplateID,
				matchedValue: matchedTemplateMilestone.PK,
				notMatchedValue: notMatchedTemplateMilestone.PK,
				isMilestone: true);

			((ModuleGuidFilter)filter["Source Template"]).SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			WorkflowTasksForTest.Test(
				factory: Factory,
				filterStripBizO: filter,
				column: ProcessTasksSchema.P9_ParentTemplateID,
				matchedValue: notMatchedTemplateMilestone.PK,
				notMatchedValue: matchedTemplateMilestone.PK,
				isMilestone: true);

			var dummy = Factory.New<DummyWithWorkflow>();
			var milestoneWithBlankSourceTemplate = dummy.WorkflowItems.Milestones.AddNew();
			milestoneWithBlankSourceTemplate.P9_Description = "Milestone With Blank Source Template";
			Factory.Save();

			((ModuleGuidFilter)filter["Source Template"]).SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			var foundTasks = new ProcessTaskCollection(Factory);
			foundTasks.Load(filter.Filter);
			AssertCollectionContains("SHOULD find blank-template-milestone", milestoneWithBlankSourceTemplate, foundTasks);
		}

		public void TestSourceTemplateFilter_NotEqualOperator()
		{
			var matchedTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var matchedTemplateMilestone = matchedTemplate.WorkflowItems.Milestones.AddNew();
			matchedTemplateMilestone.P9_Description = "Matched Template Milestone";

			var notMatchedTemplate = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA");
			var notMatchedTemplateMilestone = notMatchedTemplate.WorkflowItems.Milestones.AddNew();
			notMatchedTemplateMilestone.P9_Description = "Not Matched Template Milestone";

			var dummyForMilestoneWithMatchedTemplate = Factory.New<DummyWithWorkflow>();
			var milestoneWithMatchedTemplate = dummyForMilestoneWithMatchedTemplate.WorkflowItems.Milestones.AddNew();
			milestoneWithMatchedTemplate.P9_ParentTemplateID = matchedTemplate.PK;
			milestoneWithMatchedTemplate.P9_Description = "TestSourceTemplateFilter_NotEqualOperator";

			var dummyForMilestoneWithNotMatchedTemplate = Factory.New<DummyWithWorkflow>();
			var milestoneWithNotMatchedTemplate = dummyForMilestoneWithNotMatchedTemplate.WorkflowItems.Milestones.AddNew();
			milestoneWithNotMatchedTemplate.P9_ParentTemplateID = notMatchedTemplate.PK;
			milestoneWithNotMatchedTemplate.P9_Description = "TestSourceTemplateFilter_NotEqualOperator";

			var dummyForMilestoneWithNoSourceTemplate = Factory.New<DummyWithWorkflow>();
			var milestoneWithNoSourceTemplate = dummyForMilestoneWithNoSourceTemplate.WorkflowItems.Milestones.AddNew();
			milestoneWithNoSourceTemplate.P9_Description = "TestSourceTemplateFilter_NotEqualOperator";
			Factory.Save();

			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleGuidFilter)filter["Source Template"]).IsActive = true;
			((ModuleGuidFilter)filter["Source Template"]).SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			((ModuleTextFilter)filter["Description"]).IsActive = true;
			((ModuleTextFilter)filter["Description"]).Property = "TestSourceTemplateFilter_NotEqualOperator";
			var foundTasks = new ProcessTaskCollection(Factory);
			foundTasks.Load(filter.Filter);

			AssertCollectionContains("SHOULD find not matched-milestone", milestoneWithNotMatchedTemplate, foundTasks);
			AssertCollectionContains("SHOULD find blank-template-milestone", milestoneWithNoSourceTemplate, foundTasks);
		}

		public void TestSourceTemplateFilter_IsNotBlankOperator()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM");

			var templateMilestone = template.WorkflowItems.Milestones.AddNew();
			templateMilestone.P9_Description = "Template Milestone";

			var dummyForMilestoneWithSourceTemplate = Factory.New<DummyWithWorkflow>();
			var milestoneWithSourceTemplate = dummyForMilestoneWithSourceTemplate.WorkflowItems.Milestones.AddNew();
			milestoneWithSourceTemplate.P9_ParentTemplateID = templateMilestone.PK;

			var dummyMilestoneWithNoSourceTemplate = Factory.New<DummyWithWorkflow>();
			var milestoneWithNoSourceTemplate = dummyMilestoneWithNoSourceTemplate.WorkflowItems.Milestones.AddNew();
			Factory.Save();

			var filter = new WorkflowMilestonesFilterBusinessObject();
			((ModuleGuidFilter)filter["Source Template"]).IsActive = true;
			((ModuleGuidFilter)filter["Source Template"]).SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			var foundTasks = new ProcessTaskCollection(Factory);
			foundTasks.Load(filter.Filter);

			AssertCollectionContains("SHOULD find not blank-template-milestone", milestoneWithSourceTemplate, foundTasks);
			AssertCollectionNotContains("SHOULD not find blank-template-milestone", milestoneWithNoSourceTemplate, foundTasks);
		}

		public void TestCompletionTriggerActionsFilter()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone1 = job.WorkflowItems.Milestones.AddNew();
			var milestone2 = job.WorkflowItems.Milestones.AddNew();
			var milestone3 = job.WorkflowItems.Milestones.AddNew();
			milestone1.P9_Description = "CTA Milestone 1: NTF & NTF";
			milestone2.P9_Description = "CTA Milestone 2: NTF & FLD";
			milestone3.P9_Description = "CTA Milestone 3: DOC & XUE";

			var action1 = milestone1.ProcessTaskNotifications.AddNew();
			var action2 = milestone1.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = "NTF";
			action2.PQ_TriggerType = "NTF";

			var action3 = milestone2.ProcessTaskNotifications.AddNew();
			var action4 = milestone2.ProcessTaskNotifications.AddNew();
			action3.PQ_TriggerType = "NTF";
			action4.PQ_TriggerType = "FLD";

			var action5 = milestone3.ProcessTaskNotifications.AddNew();
			var action6 = milestone3.ProcessTaskNotifications.AddNew();
			action5.PQ_TriggerType = "DOC";
			action6.PQ_TriggerType = "XUE";

			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "CTA Trigger 4";
			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = "NTF";

			Factory.Save();

			var filterBizo = new WorkflowMilestonesFilterBusinessObject();
			filterBizo.AddTextFilterStrip("Description", "CTA");
			var filter = filterBizo.AddFilterStrip<ModuleGuidForeignCollectionFilter>("Completion Trigger Actions");
			filter.SelectedFilters.AddTextFilterStrip("Action", "NTF");

			AssertEquals(FilterCategories.Other, filter.Category);

			AssertEquals(ModuleTextFilter.ComparisonConstants.AnyMatch, filter.ComparisonOperator);
			var results = Factory.Load<ProcessTask>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "CTA Milestone 1: NTF & NTF", "CTA Milestone 2: NTF & FLD" }, results.Select(x => x.P9_Description));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			results = Factory.Load<ProcessTask>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "CTA Milestone 1: NTF & NTF" }, results.Select(x => x.P9_Description));

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			results = Factory.Load<ProcessTask>(filterBizo.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { "CTA Milestone 3: DOC & XUE" }, results.Select(x => x.P9_Description));
		}

		#endregion

		#region Overall

		public void TestIncludeMilestoneOnly()
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
			tasks.Load((new WorkflowMilestonesFilterBusinessObject()).Filter);
			AssertCollectionNotContains("Tasks should NOT be loaded", task, tasks);
			AssertCollectionContains("Milestones should be loaded", milestone, tasks);
			AssertCollectionNotContains("Workflow exceptions should NOT be loaded", exception, tasks);
			AssertCollectionNotContains("Triggers should NOT be loaded", trigger, tasks);
		}

		#endregion

		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new WorkflowMilestonesFilterBusinessObject();
		}

		#endregion
	}
}
