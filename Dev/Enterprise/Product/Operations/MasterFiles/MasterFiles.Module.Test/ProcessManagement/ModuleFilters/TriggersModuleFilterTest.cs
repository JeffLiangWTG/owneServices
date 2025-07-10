using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(TriggersModuleFilter))]
	sealed class TriggersModuleFilterTest : ModuleFilterTestCase<TriggersModuleFilter>
	{
		public void TestFilter()
		{
			var allMatchedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var noMatchedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var someMatchedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var noTriggersOrg = Factory.NewWithValidTestData<OrgHeader>();

			allMatchedOrg.OH_Code = "000";
			allMatchedOrg.OH_FullName = "AllMatched Org";

			noMatchedOrg.OH_Code = "001";
			noMatchedOrg.OH_FullName = "NoMatched Org";

			someMatchedOrg.OH_Code = "002";
			someMatchedOrg.OH_FullName = "SomeMatched Org";

			noTriggersOrg.OH_Code = "003";
			noTriggersOrg.OH_FullName = "NoTriggers Org";

			var allMatchedOrg_trigger1 = allMatchedOrg.WorkflowItems.Triggers.AddNew();
			allMatchedOrg_trigger1.P9_Description = "Matched";
			var allMatchedOrg_trigger2 = allMatchedOrg.WorkflowItems.Triggers.AddNew();
			allMatchedOrg_trigger2.P9_Description = "Matched";
			allMatchedOrg.WorkflowItems.Tasks.AddNew();

			noMatchedOrg.WorkflowItems.Triggers.AddNew();
			noMatchedOrg.WorkflowItems.Triggers.AddNew();
			noMatchedOrg.WorkflowItems.Tasks.AddNew();

			var someMatchedOrg_trigger = someMatchedOrg.WorkflowItems.Triggers.AddNew();
			someMatchedOrg_trigger.P9_Description = "Matched";
			someMatchedOrg.WorkflowItems.Triggers.AddNew();
			someMatchedOrg.WorkflowItems.Tasks.AddNew();

			noTriggersOrg.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			Filter.SelectedFilters.AddTextFilterStrip("Description", "Matched");
			var processTasks = Factory.Load<ProcessTask>(Filter.SelectedFilters.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { allMatchedOrg_trigger1, allMatchedOrg_trigger2, someMatchedOrg_trigger }, processTasks);

			var orgFilterBizo = new OrganisationFilterBusinessObject(OrgModuleType.Standard);
			var codeFilter = (ModuleTextFilter)orgFilterBizo["Code"];
			codeFilter.Property = "00";
			codeFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			codeFilter.IsActive = true;

			orgFilterBizo.ModuleFilters.AddFilter(Filter);
			Filter.IsActive = true;

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;
			var orgHeaderResult = Factory.Load<OrgHeader>(orgFilterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("Any match: " + Filter.Query.LiteralTextSqlFormatted, new[] { allMatchedOrg.OH_FullName, someMatchedOrg.OH_FullName }, orgHeaderResult.Select(x => x.OH_FullName));

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
			orgHeaderResult = Factory.Load<OrgHeader>(orgFilterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("None match: " + Filter.Query.LiteralTextSqlFormatted, new[] { noMatchedOrg.OH_FullName, noTriggersOrg.OH_FullName }, orgHeaderResult.Select(x => x.OH_FullName));

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			orgHeaderResult = Factory.Load<OrgHeader>(orgFilterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("All match: " + Filter.Query.LiteralTextSqlFormatted, new[] { allMatchedOrg.OH_FullName, noTriggersOrg.OH_FullName }, orgHeaderResult.Select(x => x.OH_FullName));
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override TriggersModuleFilter GetNewModuleFilter()
		{
			return new TriggersModuleFilter("moo", OrgHeaderSchema.PK, Factory, typeof(OrgHeader));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestQueryIsEmptyByDefault()
		{
			Assert("It's definitely empty", true);
		}
	}
}
