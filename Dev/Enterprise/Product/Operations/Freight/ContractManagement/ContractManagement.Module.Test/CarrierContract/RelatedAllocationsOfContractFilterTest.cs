using Enterprise.ContractManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ContractManagement.Module.Testing
{
	[TestedType(typeof(RelatedAllocationsOfContractFilter))]
	class RelatedAllocationsOfContractFilterTest : ModuleFilterTestCase<RelatedAllocationsOfContractFilter>
	{
		public void TestRelatedAllocations()
		{
			var filter = Filter.SelectedFilters.AddFilterStrip<ModuleTextFilter>("Voyage");
			filter.Property = "Voyage1";
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

			var contract1 = Factory.NewWithValidTestData<RatingContract>();
			var contract2 = Factory.NewWithValidTestData<RatingContract>();
			var contract3 = Factory.NewWithValidTestData<RatingContract>();
			var contract4 = Factory.NewWithValidTestData<RatingContract>();

			var allocationLine1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationLine1.RCA_VoyageNumber = "Voyage1";

			var allocationLine2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationLine2.RCA_VoyageNumber = "Voyage1";

			var allocationLine3 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationLine3.RCA_VoyageNumber = "Voyage2";

			var allocationLine4 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationLine4.RCA_VoyageNumber = "Voyage2";

			contract1.Allocations.Add(allocationLine1);
			contract2.Allocations.Add(allocationLine2);
			contract3.Allocations.Add(allocationLine3);
			contract4.Allocations.Add(allocationLine4);

			Factory.Save();

			var loadedContracts = Factory.Load<RatingContract>(Filter.Query);
			AssertContainsExactElementsInAnyOrder(new RatingContract[] { contract1, contract2 }, loadedContracts);
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override RelatedAllocationsOfContractFilter GetNewModuleFilter()
		{
			return new RelatedAllocationsOfContractFilter("moo", Factory);
		}
	}
}
