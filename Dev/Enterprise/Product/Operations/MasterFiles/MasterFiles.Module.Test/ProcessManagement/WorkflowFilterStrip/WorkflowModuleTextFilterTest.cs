using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(WorkflowModuleTextFilter))]
	public class WorkflowModuleTextFilterTest : ModuleFilterTestCase<WorkflowModuleTextFilter>
	{
		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestClearResetsToDefaults

		public void TestClearResetsToDefaults()
		{
			ZString searchValue = "Today";
			ZString eventValue = "ADD";
			ZString eventReference = "abcde";
			ZString eventReferenceOption = "contains";

			var allFilters = new DummyFilterBizOForWorkflowModuleText();
			var filterText = (WorkflowModuleTextFilter)allFilters["text"];

			filterText.Property = searchValue;
			filterText.MilestoneEvent = eventValue;
			filterText.EventReference = eventReference;
			filterText.EventReferenceComparisonOption = eventReferenceOption;

			AssertEquals("Precondition", searchValue, filterText.Property);
			AssertEquals("Precondition", eventValue, filterText.MilestoneEvent);
			AssertEquals("Precondition", eventReference, filterText.EventReference);
			AssertEquals("Precondition", eventReferenceOption, filterText.EventReferenceComparisonOption);

			filterText.Clear();
			AssertEquals("", filterText.Property);
			AssertEquals("", filterText.MilestoneEvent);
			AssertEquals("", filterText.EventReference);
			AssertEquals("starts with", filterText.EventReferenceComparisonOption);
		}

		#endregion

		#region TestIsEmpty

		public void TestIsEmpty()
		{
			var allFilters = new DummyFilterBizOForWorkflowModuleText();
			var filterText = (WorkflowModuleTextFilter)allFilters["text"];

			filterText.Property = "";
			filterText.MilestoneEvent = "";
			AssertEquals(true, filterText.IsEmpty);

			filterText.Property = "str";
			AssertEquals(false, filterText.IsEmpty);

			filterText.MilestoneEvent = "ADD";
			AssertEquals(false, filterText.IsEmpty);

			filterText.Property = "";
			AssertEquals(false, filterText.IsEmpty);

			filterText.MilestoneEvent = "";
			AssertEquals(true, filterText.IsEmpty);

			filterText.EventReference = "abcde";
			AssertEquals(false, filterText.IsEmpty);

			filterText.EventReference = "";
			filterText.EventReferenceComparisonOption = "exact";
			AssertEquals(true, filterText.IsEmpty);

			filterText.EventReferenceComparisonOption = "is blank";
			AssertEquals(false, filterText.IsEmpty);

			filterText.EventReferenceComparisonOption = "is not blank";
			AssertEquals(false, filterText.IsEmpty);

			filterText.EventReferenceComparisonOption = "contains";
			AssertEquals(true, filterText.IsEmpty);
		}

		#endregion

		#region Test event reference comparison option

		public void TestEventReferenceComparisonOption()
		{
			var allFilters = new DummyFilterBizOForWorkflowModuleText();
			var filterText = (WorkflowModuleTextFilter)allFilters["text"];

			AssertEquals("starts with", filterText.EventReferenceComparisonOption);
			Assert(!filterText.EventReferenceInfo.ReadOnly);

			filterText.EventReference = "abcde";

			filterText.EventReferenceComparisonOption = "contains";
			AssertEquals("abcde", filterText.EventReference);
			Assert(!filterText.EventReferenceInfo.ReadOnly);

			filterText.EventReferenceComparisonOption = "is blank";
			AssertEquals("", filterText.EventReference);
			Assert(filterText.EventReferenceInfo.ReadOnly);

			filterText.EventReferenceComparisonOption = "is not blank";
			Assert(filterText.EventReferenceInfo.ReadOnly);

			filterText.EventReferenceComparisonOption = "exact";
			Assert(!filterText.EventReferenceInfo.ReadOnly);

			filterText.EventReferenceComparisonOption = "something";
			AssertEquals("?", filterText.EventReferenceComparisonOption);
		}

		#endregion

		#region TestQuery

		#region TestParentTableCodeIsNotAddedToFinalQueryWhenRelatedParentSubQueriesAreUsed

		public void TestParentTableCodeIsNotAddedToFinalQueryWhenRelatedParentSubQueriesAreUsed()
		{
			var allFilters = new DummyFilterBizOForWorkflowModuleText();
			var filterText = (WorkflowModuleTextFilter)allFilters["text"];
			filterText.MilestoneEvent = "EV1";
			filterText.Property = "";
			filterText.IsActive = true;

			string parentTableCodeSQL = string.Format("{0} = '{1}'", ProcessTasksSchema.Constants.P9_ParentTableCode, DummyBizoSchema.Constants.Prefix);
			AssertEquals("ParentTableCode added to query", true, allFilters.Filter.LiteralTextSqlFormatted.Contains(parentTableCodeSQL));

			filterText.RelatedParentSubQueries = System.Array.Empty<ZDBOnlySubQuery>();
			AssertEquals("ParentTableCode added to query", true, allFilters.Filter.LiteralTextSqlFormatted.Contains(parentTableCodeSQL));

			filterText.RelatedParentSubQueries = new[] { new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0) };
			AssertEquals("ParentTableCode not added to query", false, allFilters.Filter.LiteralTextSqlFormatted.Contains(parentTableCodeSQL));
		}

		#endregion

		#region TestDateAndTypeQuery

		public void TestDateAndTypeQuery()
		{
			var bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO3 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO4 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = bizO2.WorkflowItems.Milestones.AddNew();
			var milestone3 = bizO3.WorkflowItems.Milestones.AddNew();
			var milestone4 = bizO4.WorkflowItems.Milestones.AddNew();

			milestone1.SetMilestoneActualDateForTest(ZDateTime.Empty);
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 3, 4));
			milestone3.SetMilestoneActualDateForTest(new ZDateTime(2000, 5, 6));
			milestone4.SetMilestoneActualDateForTest(ZDateTime.Empty);

			milestone1.TriggerConditions.TriggerEventCode = "EV1";
			milestone2.TriggerConditions.TriggerEventCode = "EV1";
			milestone3.TriggerConditions.TriggerEventCode = "EV2";
			milestone4.TriggerConditions.TriggerEventCode = "EV2";

			Factory.Save();

			var allFilters = new DummyFilterBizOForWorkflowModuleText();
			var milestoneFilter = (WorkflowModuleTextFilter)allFilters["text"];
			milestoneFilter.MilestoneEvent = "EV1";
			milestoneFilter.Property = "";
			milestoneFilter.IsActive = true;

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(allFilters.Filter);

			Assert(collection.Contains(bizO1));
			Assert(collection.Contains(bizO2));
			Assert(!collection.Contains(bizO3));
			Assert(!collection.Contains(bizO4));

			milestoneFilter.MilestoneEvent = "EV1";
			milestoneFilter.Property = "rightvalue";
			milestoneFilter.IsActive = true;

			collection.Load(allFilters.Filter);

			Assert(!collection.Contains(bizO1));
			Assert(collection.Contains(bizO2));
			Assert(!collection.Contains(bizO3));
			Assert(!collection.Contains(bizO4));

			milestoneFilter.MilestoneEvent = "";
			milestoneFilter.Property = "wrongvalue";
			milestoneFilter.IsActive = true;

			collection.Load(allFilters.Filter);

			Assert(collection.Contains(bizO1));
			Assert(!collection.Contains(bizO2));
			Assert(!collection.Contains(bizO3));
			Assert(collection.Contains(bizO4));

			milestoneFilter.MilestoneEvent = "EV2";
			milestoneFilter.Property = "";
			milestoneFilter.IsActive = true;

			collection.Load(allFilters.Filter);

			Assert(!collection.Contains(bizO1));
			Assert(!collection.Contains(bizO2));
			Assert(collection.Contains(bizO3));
			Assert(collection.Contains(bizO4));
		}

		#endregion

		#region TestDateAndTypeQueryChecksParentTableCode

		public void TestDateAndTypeQueryChecksParentTableCode()
		{
			var bizO1 = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var bizO2 = Factory.NewWithValidTestData<DummyWithWorkflow>();

			var milestone1 = bizO1.WorkflowItems.Milestones.AddNew();
			var milestone2 = bizO2.WorkflowItems.Milestones.AddNew();

			milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 3, 4));
			milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 3, 4));

			milestone1.TriggerConditions.TriggerEventCode = "EV1";
			milestone2.TriggerConditions.TriggerEventCode = "EV1";

			milestone2.P9_ParentTableCode = "P0";

			Factory.Save();

			var allFilters = new DummyFilterBizOForWorkflowModuleText();
			var milestoneFilter = (WorkflowModuleTextFilter)allFilters["text"];
			milestoneFilter.MilestoneEvent = "EV1";
			milestoneFilter.Property = "rightvalue";
			milestoneFilter.IsActive = true;

			var collection = new DummyBusinessObjectCollection(Factory);
			collection.Load(allFilters.Filter);

			Assert(collection.Contains(bizO1));
			Assert(!collection.Contains(bizO2));
		}

		#endregion

		#region Test event reference query

		public void TestEventReferenceQuery()
		{
			var allFilters = new DummyFilterBizOForWorkflowModuleText();
			var milestoneFilter = (WorkflowModuleTextFilter)allFilters["text"];

			milestoneFilter.EventReferenceComparisonOption = "exact";
			milestoneFilter.EventReference = "abcde";
			Assert(milestoneFilter.Query.LiteralTextSqlFormatted.Replace("\r\n", " ").Replace("\t", "").Replace("  ", " ").Contains("dbo.CLRUncompressAsString ( P9_Notes ) = 'abcde'"));
			Assert(milestoneFilter.Query.LiteralTextADO.Contains("P9_Notes = 'abcde'")); // Stored procedure can't be called in ADO, and anyway this is db-only query

			milestoneFilter.EventReferenceComparisonOption = "is blank";
			Assert(milestoneFilter.Query.LiteralTextSqlFormatted.Replace("\r\n", " ").Replace("\t", "").Replace("  ", " ").Contains("P9_Notes is NULL"));

			milestoneFilter.EventReferenceComparisonOption = "is not blank";
			Assert(milestoneFilter.Query.LiteralTextSqlFormatted.Replace("\r\n", " ").Replace("\t", "").Replace("  ", " ").Contains("P9_Notes is not NULL"));
		}

		#endregion

		#endregion

		#region Validation

		public void TestValidate_WhenEventReferenceHasValue_AndInFilterRuleMode_ShouldHaveError()
		{
			var filterBizo = new DummyFilterBizOForWorkflowModuleText { IsInFilterRuleMode = true };
			var filter = filterBizo.AddFilterStrip<WorkflowModuleTextFilter>("text");
			filter.EventReference = "A";

			AssertHasError(filter.EventReferenceInfo, "This option cannot be used on filter rules for performance reasons.");
		}

		public void TestValidate_WhenEventReferenceHasNoValue_AndInFilterRuleMode_ShouldNotHaveError()
		{
			var filterBizo = new DummyFilterBizOForWorkflowModuleText { IsInFilterRuleMode = true };
			var filter = filterBizo.AddFilterStrip<WorkflowModuleTextFilter>("text");
			filter.Validation.ValidateAll();

			AssertNoErrors("There should only be an error if EventReference is used on a filter rule.", filter.EventReferenceInfo);
		}

		public void TestValidate_WhenEventReferenceHasNoValue_AndNotInFilterRuleMode_ShouldNotHaveError()
		{
			var filterBizo = new DummyFilterBizOForWorkflowModuleText { IsInFilterRuleMode = false };
			var filter = filterBizo.AddFilterStrip<WorkflowModuleTextFilter>("text");
			filter.Validation.ValidateAll();

			AssertNoErrors("There should only be an error if EventReference is used on a filter rule.", filter.EventReferenceInfo);
		}

		public void TestValidate_WhenEventReferenceHasValue_AndNotInFilterRuleMode_ShouldNotHaveError()
		{
			var filterBizo = new DummyFilterBizOForWorkflowModuleText { IsInFilterRuleMode = false };
			var filter = filterBizo.AddFilterStrip<WorkflowModuleTextFilter>("text");
			filter.EventReference = "A";
			filter.Validation.ValidateAll();

			AssertNoErrors("There should only be an error if EventReference is used on a filter rule.", filter.EventReferenceInfo);
		}

		#endregion

		#region TestCopyTransient

		public void TestCopyTransient()
		{
			var allFilters = new DummyFilterBizOForWorkflowModuleText();
			var milestoneFilter = (WorkflowModuleTextFilter)allFilters["text"];
			milestoneFilter.Property = "searchValue";
			milestoneFilter.MilestoneEvent = "ARV";
			milestoneFilter.EventReferenceComparisonOption = "exact";
			milestoneFilter.EventReference = "abcde";
			milestoneFilter.IsActive = true;

			var allFilters2 = new DummyFilterBizOForWorkflowModuleText();
			var milestoneFilter2 = (WorkflowModuleTextFilter)allFilters2["text"];

			milestoneFilter2.CopyTransientProperties(milestoneFilter);

			CombineAssertions(() =>
			{
				AssertEquals(milestoneFilter.Property, milestoneFilter2.Property);
				AssertEquals(milestoneFilter.MilestoneEvent, milestoneFilter2.MilestoneEvent);
				AssertEquals(milestoneFilter.EventReferenceComparisonOption, milestoneFilter2.EventReferenceComparisonOption);
				AssertEquals(milestoneFilter.EventReference, milestoneFilter2.EventReference);
				AssertEquals(milestoneFilter.IsActive, milestoneFilter2.IsActive);
			});
		}

		#endregion

		#region Implementation

		public static ZQuery GetMilestoneTextFilter(ZString value)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);

			if (value == "rightvalue")
			{
				subQuery.AddToFilter(ProcessTasksSchema.P9_ActualDate, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
			}
			else if (value == "wrongvalue")
			{
				subQuery.AddToFilter(ProcessTasksSchema.P9_ActualDate, SQLComparisonOperator.Equal, ZDateTime.Empty);
			}
			return subQuery;
		}

		protected override WorkflowModuleTextFilter GetNewModuleFilter()
		{
			return new WorkflowModuleTextFilter("moo", GetMilestoneTextFilter, new CodeDescriptionPairList(), typeof(DummyBusinessObject));
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.TextSearch; }
		}

		#endregion
	}
}
