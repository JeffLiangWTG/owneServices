using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ContainerLoadPlanDataTest : TestCaseWithFactory
	{
		public void TestGetBusinessObjectCollection()
		{
			var containerLoadPlan1 = Factory.NewWithValidTestData<CFSContainerLoadList>();
			containerLoadPlan1.CLH_Status = Constants.ContainerLoadListHeaderStatus.Incomplete;

			var containerLoadPlan2 = Factory.NewWithValidTestData<CFSContainerLoadList>();
			containerLoadPlan2.CLH_Status = Constants.ContainerLoadListHeaderStatus.Placed;

			Factory.Save();

			var collection = new ContainerLoadPlanData().GetBusinessObjectCollection(new BusinessObjectFactory());

			AssertEquals(2, collection.Count);
			AssertNotNull(collection.FindByPK(containerLoadPlan1.PK));
			AssertNotNull(collection.FindByPK(containerLoadPlan2.PK));
		}
	}
}
