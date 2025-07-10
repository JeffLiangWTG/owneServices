using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(TasksModuleFilter))]
	sealed class TasksModuleFilterTest : ModuleFilterTestCase<TasksModuleFilter>
	{
		public void TestFilter()
		{
			var orgHeaderWithAllSpire = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderWithNoSpire = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderWithSomeSpire = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderWithNoTasks = Factory.NewWithValidTestData<OrgHeader>();

			orgHeaderWithAllSpire.OH_FullName = "TasksModuleFilterTest";
			orgHeaderWithNoSpire.OH_FullName = "TasksModuleFilterTest";
			orgHeaderWithSomeSpire.OH_FullName = "TasksModuleFilterTest";
			orgHeaderWithNoTasks.OH_FullName = "TasksModuleFilterTest";

			var spireTask1 = orgHeaderWithAllSpire.WorkflowItems.Tasks.AddNew();
			spireTask1.P9_Description = "SPIRE!";
			var spireTask2 = orgHeaderWithAllSpire.WorkflowItems.Tasks.AddNew();
			spireTask2.P9_Description = "SPIRE!";

			var blankTask1 = orgHeaderWithNoSpire.WorkflowItems.Tasks.AddNew();
			var blankTask2 = orgHeaderWithNoSpire.WorkflowItems.Tasks.AddNew();

			var spireTask3 = orgHeaderWithSomeSpire.WorkflowItems.Tasks.AddNew();
			spireTask3.P9_Description = "SPIRE!";
			var blankTask3 = orgHeaderWithSomeSpire.WorkflowItems.Tasks.AddNew();
			blankTask3.P9_ParentID = orgHeaderWithSomeSpire.PK;

			Factory.Save();

			Filter.SelectedFilters.AddTextFilterStrip("Description", "SPIRE!");
			var subFilterResult = Factory.Load<OrgHeaderProcessTask>(Filter.SelectedFilters.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { spireTask1.PK, spireTask2.PK, spireTask3.PK }, subFilterResult.Select(x => x.PK));

			var orgFilterBizo = new OrganisationFilterBusinessObject(OrgModuleType.Standard);
			var nameFilter = (ModuleTextFilter)orgFilterBizo["Name"];
			nameFilter.Property = "TasksModuleFilterTest";
			nameFilter.IsActive = true;

			orgFilterBizo.ModuleFilters.AddFilter(Filter);
			Filter.IsActive = true;

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var result = Factory.Load<OrgHeader>(orgFilterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Any match: " + Filter.Query.LiteralTextSqlFormatted, new[] { orgHeaderWithAllSpire.PK, orgHeaderWithSomeSpire.PK }, result.Select(x => x.PK));

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			result = Factory.Load<OrgHeader>(orgFilterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("None match: " + Filter.Query.LiteralTextSqlFormatted, new[] { orgHeaderWithNoSpire.PK, orgHeaderWithNoTasks.PK }, result.Select(x => x.PK));

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			result = Factory.Load<OrgHeader>(orgFilterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("All match: " + Filter.Query.LiteralTextSqlFormatted, new[] { orgHeaderWithAllSpire.PK, orgHeaderWithNoTasks.PK }, result.Select(x => x.PK));
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override TasksModuleFilter GetNewModuleFilter()
		{
			return new TasksModuleFilter("moo", OrgHeaderSchema.PK, ProcessTasksSchema.P9_ParentID, new ProcessTaskCollection(Factory), typeof(OrgHeader));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
	}
}
