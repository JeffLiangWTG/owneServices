using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(RelatedContainersOfConsolFilter))]
	class RelatedContainersOfConsolFilterTest : ModuleFilterTestCase<RelatedContainersOfConsolFilter>
	{
		public void TestFilter()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CON001";

			var container2 = consol1.Containers.AddNew();
			container2.JC_ContainerNum = "CON002";

			var consol2 = Factory.New<ForwardingConsol>();
			var container3 = consol2.Containers.AddNew();
			container3.JC_ContainerNum = "CON001";

			var container4 = consol2.Containers.AddNew();
			container4.JC_ContainerNum = "XXX002";

			var consol3 = Factory.New<ForwardingConsol>();
			var container5 = consol3.Containers.AddNew();
			container5.JC_ContainerNum = "XXX001";

			var container6 = consol3.Containers.AddNew();
			container6.JC_ContainerNum = "XXX002";

			var consol4 = Factory.New<ForwardingConsol>();
			consol4.Containers.RemoveAndDeleteAll();

			Factory.Save();

			var filterStripBizO = new JobConsolFilterBusinessObject();
			var relatedContainersFilter = (RelatedContainersOfConsolFilter)filterStripBizO["Related Containers"];
			relatedContainersFilter.IsActive = true;
			relatedContainersFilter.SelectedFilters.AddTextFilterStrip("Container #", "CON");
			relatedContainersFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var consols = new ForwardingConsolCollection(Factory);

			consols.Load(filterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2 }, consols);

			relatedContainersFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;

			consols.Load(filterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol4 }, consols);

			relatedContainersFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;

			consols.Load(filterStripBizO.Filter);

			var pks = consols.Select(c => c.PK);
			AssertContainsExactElementsInAnyOrder(new[] { consol3.PK, consol4.PK }, pks);
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override RelatedContainersOfConsolFilter GetNewModuleFilter()
		{
			return new RelatedContainersOfConsolFilter("moo", Factory);
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
	}
}
