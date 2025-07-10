using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	sealed class CusInBondContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestContainerTypes()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container = moveDetail.Containers.AddNew();
			AssertNotNull(container.Lookups.ContainerTypes);
		}

		public void TestServiceTypes()
		{
			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			var container = moveDetail.Containers.AddNew();
			AssertEquals(Factory.GetCachedValue<ServiceTypeList>(), container.Lookups.ServiceTypes);
		}
	}
}
