using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ContainerLoadListDataTest : TestCaseWithFactory
	{
		public void TestGetBusinessObjectCollection()
		{
			var containerLoadList1 = Factory.NewWithValidTestData<CYContainerLoadList>();
			containerLoadList1.CLH_Status = Constants.ContainerLoadListHeaderStatus.Incomplete;

			var containerLoadList2 = Factory.NewWithValidTestData<CYContainerLoadList>();
			containerLoadList2.CLH_Status = Constants.ContainerLoadListHeaderStatus.Placed;

			Factory.Save();

			var collection = new ContainerLoadListData().GetBusinessObjectCollection(new BusinessObjectFactory());

			AssertEquals(2, collection.Count);
			AssertNotNull(collection.FindByPK(containerLoadList1.PK));
			AssertNotNull(collection.FindByPK(containerLoadList2.PK));
		}
	}
}
