using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(RelatedTransportLegsOfConsolFilter))]
	class RelatedTransportLegsOfConsolFilterTest : ModuleFilterTestCase<RelatedTransportLegsOfConsolFilter>
	{
		public void TestAllMatchesWithMultipleLocationFilters()
		{
			var filter = Filter.SelectedFilters.AddFilterStrip<ModuleLocationFilter>("Load / Discharge");
			filter.Property1 = "";
			filter.Property2 = "NZTRG";
			filter.OrCategory = FilterOrCategory.Red;

			filter = Filter.SelectedFilters.AddFilterStrip<ModuleLocationFilter>("Load / Discharge");
			filter.Property1 = "";
			filter.Property2 = "NZWLG";
			filter.OrCategory = FilterOrCategory.Red;

			filter = Filter.SelectedFilters.AddFilterStrip<ModuleLocationFilter>("Load / Discharge");
			filter.Property1 = "";
			filter.Property2 = "NZAKL";
			filter.OrCategory = FilterOrCategory.Red;

			Filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
			Filter.Query.MaximumRows = 1;

			AssertNoExceptionThrown(() => Factory.Load<ForwardingConsol>(Filter.Query));
		}

		#region Implementation

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override RelatedTransportLegsOfConsolFilter GetNewModuleFilter()
		{
			return new RelatedTransportLegsOfConsolFilter("moo", Factory);
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		#endregion
	}
}
