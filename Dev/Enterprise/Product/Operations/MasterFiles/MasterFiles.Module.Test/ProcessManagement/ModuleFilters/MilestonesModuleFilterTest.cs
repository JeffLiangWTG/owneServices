using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(MilestonesModuleFilter))]
	sealed class MilestonesModuleFilterTest : ModuleFilterTestCase<MilestonesModuleFilter>
	{
		public void TestFilter()
		{
			var allMatchedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var noMatchedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var someMatchedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var noMilestonesOrg = Factory.NewWithValidTestData<OrgHeader>();

			allMatchedOrg.OH_Code = "000";
			allMatchedOrg.OH_FullName = "AllMatched Org";

			noMatchedOrg.OH_Code = "001";
			noMatchedOrg.OH_FullName = "NoMatched Org";

			someMatchedOrg.OH_Code = "002";
			someMatchedOrg.OH_FullName = "SomeMatched Org";

			noMilestonesOrg.OH_Code = "003";
			noMilestonesOrg.OH_FullName = "NoMilestones Org";

			var allMatchedOrg_milestone1 = allMatchedOrg.WorkflowItems.Milestones.AddNew();
			allMatchedOrg_milestone1.P9_Description = "Matched";
			var allMatchedOrg_milestone2 = allMatchedOrg.WorkflowItems.Milestones.AddNew();
			allMatchedOrg_milestone2.P9_Description = "Matched";
			allMatchedOrg.WorkflowItems.Tasks.AddNew();

			noMatchedOrg.WorkflowItems.Milestones.AddNew();
			noMatchedOrg.WorkflowItems.Milestones.AddNew();
			noMatchedOrg.WorkflowItems.Tasks.AddNew();

			var someMatchedOrg_milestone = someMatchedOrg.WorkflowItems.Milestones.AddNew();
			someMatchedOrg_milestone.P9_Description = "Matched";
			someMatchedOrg.WorkflowItems.Milestones.AddNew();
			someMatchedOrg.WorkflowItems.Tasks.AddNew();

			noMilestonesOrg.WorkflowItems.Tasks.AddNew();

			Factory.Save();

			Filter.SelectedFilters.AddTextFilterStrip("Description", "Matched");
			var processTasks = Factory.Load<ProcessTask>(Filter.SelectedFilters.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { allMatchedOrg_milestone1, allMatchedOrg_milestone2, someMatchedOrg_milestone }, processTasks);

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
			AssertContainsExactElementsInAnyOrder("None match: " + Filter.Query.LiteralTextSqlFormatted, new[] { noMatchedOrg.OH_FullName, noMilestonesOrg.OH_FullName }, orgHeaderResult.Select(x => x.OH_FullName));

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			orgHeaderResult = Factory.Load<OrgHeader>(orgFilterBizo.Filter);
			AssertContainsExactElementsInAnyOrder("All match: " + Filter.Query.LiteralTextSqlFormatted, new[] { allMatchedOrg.OH_FullName, noMilestonesOrg.OH_FullName }, orgHeaderResult.Select(x => x.OH_FullName));
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override MilestonesModuleFilter GetNewModuleFilter()
		{
			return new MilestonesModuleFilter("moo", OrgHeaderSchema.PK, Factory, typeof(OrgHeader));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestQueryIsEmptyByDefault()
		{
			Assert("It's definitely empty", true);
		}
	}
}
