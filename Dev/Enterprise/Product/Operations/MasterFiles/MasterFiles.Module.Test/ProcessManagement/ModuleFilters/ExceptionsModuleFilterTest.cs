using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ExceptionsModuleFilter))]
	sealed class ExceptionsModuleFilterTest : ModuleFilterTestCase<ExceptionsModuleFilter>
	{
		public void TestFilter()
		{
			var orgHeaderWithAllSpire = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderWithNoSpire = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderWithSomeSpire = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeaderWithNoTasks = Factory.NewWithValidTestData<OrgHeader>();

			orgHeaderWithAllSpire.OH_FullName = "ExceptionsModuleFilterTest";
			orgHeaderWithNoSpire.OH_FullName = "ExceptionsModuleFilterTest";
			orgHeaderWithSomeSpire.OH_FullName = "ExceptionsModuleFilterTest";
			orgHeaderWithNoTasks.OH_FullName = "ExceptionsModuleFilterTest";

			var spireException1 = orgHeaderWithAllSpire.WorkflowItems.Exceptions.AddNew();
			spireException1.P9_Description = "SPIRE!";
			var spireException2 = orgHeaderWithAllSpire.WorkflowItems.Exceptions.AddNew();
			spireException2.P9_Description = "SPIRE!";
			orgHeaderWithAllSpire.WorkflowItems.Tasks.AddNew();

			var blankException1 = orgHeaderWithNoSpire.WorkflowItems.Exceptions.AddNew();
			var blankException2 = orgHeaderWithNoSpire.WorkflowItems.Exceptions.AddNew();
			orgHeaderWithNoSpire.WorkflowItems.Tasks.AddNew();

			var spireException3 = orgHeaderWithSomeSpire.WorkflowItems.Exceptions.AddNew();
			spireException3.P9_Description = "SPIRE!";
			var blankException3 = orgHeaderWithSomeSpire.WorkflowItems.Exceptions.AddNew();
			orgHeaderWithSomeSpire.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			Filter.SelectedFilters.AddTextFilterStrip("Description", "SPIRE!");
			var subFilterResult = Factory.Load<ProcessTask>(Filter.SelectedFilters.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { spireException1, spireException2, spireException3 }, subFilterResult);

			var orgFilterBizo = new OrganisationFilterBusinessObject(OrgModuleType.Standard);
			var nameFilter = (ModuleTextFilter)orgFilterBizo["Name"];
			nameFilter.Property = "ExceptionsModuleFilterTest";
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

		protected override ExceptionsModuleFilter GetNewModuleFilter()
		{
			return new ExceptionsModuleFilter("moo", OrgHeaderSchema.PK, Factory, typeof(OrgHeader));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestQueryIsEmptyByDefault()
		{
			Assert("It's definitely empty", true);
		}
	}
}
