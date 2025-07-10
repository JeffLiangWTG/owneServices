using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class CusInBondContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestModeList()
		{
			var list = container.Lookups.ModeList;
			AssertSame(Factory.GetCachedValue<CusInBondContainerModeList>(), list);
			AssertEquals(5, list.Count);
		}

		public void TestContainerTypes()
		{
			var collection = container.Lookups.ContainerTypes;
			AssertNotNull(collection);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
			moveHeader = header.MovementHeaders.AddNew();
			moveDetail = moveHeader.MovementDetails.AddNew();
			container = moveDetail.Containers.AddNew();
		}

		CusInBondHeader header;
		CusInBondMoveHeader moveHeader;
		CusInBondMoveDetail moveDetail;
		CusInBondContainer container;
	}
}
