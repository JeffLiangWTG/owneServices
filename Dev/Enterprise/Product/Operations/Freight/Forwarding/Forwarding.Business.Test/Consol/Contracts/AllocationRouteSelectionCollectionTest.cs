using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ContractManagement.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(AllocationRouteSelectionCollection))]
	public sealed class AllocationRouteSelectionCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var route1 = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			var route2 = Factory.NewWithValidTestData<RatingContractAllocationLine>();

			Factory.Save();

			var collection = new AllocationRouteSelectionCollection(Factory,
				new RatingContractAllocationLine[] {
					route1,
					route2
				});

			return collection;
		}
	}
}
