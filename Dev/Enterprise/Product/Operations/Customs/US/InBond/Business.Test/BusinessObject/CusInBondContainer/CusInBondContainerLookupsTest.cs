using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class CusInBondContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCarrierCollection()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			RefContainerCollection collection = container.Lookups.ContainerTypes;
			AssertNotNull(collection);
		}

		public void TestMessageStatusList()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			CusInBondMoveHeader moveHeader = header.MovementHeaders.AddNew();
			CusInBondMoveDetail moveDetail = moveHeader.MovementDetails.AddNew();
			CusInBondContainer container = moveDetail.Containers.AddNew();
			AssertEquals(Factory.GetCachedValue<ImportMessageStatusList>(), container.Lookups.MessageStatusList);
		}
	}
}
