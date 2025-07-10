using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	internal class CommonBookedCtgMoveLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCartageAddressList()
		{
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			cartage.RefreshAddresses();
			AssertEquals(5, move.Lookups.CartageAddressList.Count);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			cartage.DocAddresses.AddNew(org.MainAddress, DocAddressType.LocalCartageImporter);
			AssertEquals(6, move.Lookups.CartageAddressList.Count);
			var cartage2 = Factory.New<CommonCartage>();
			var move2 = cartage2.LooseBookedMoves.AddNew();
			cartage2.RefreshAddresses();
			AssertEquals(5, move2.Lookups.CartageAddressList.Count);
		}

		public void TestCartageAddressElements()
		{
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			cartage.RefreshAddresses();
			AssertEquals(5, move.Lookups.GetCartageAddressElements().Length);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			cartage.DocAddresses.AddNew(org.MainAddress, DocAddressType.LocalCartageImporter);
			AssertEquals(6, move.Lookups.GetCartageAddressElements().Length);
			var cartage2 = Factory.New<CommonCartage>();
			var move2 = cartage2.LooseBookedMoves.AddNew();
			cartage2.RefreshAddresses();
			AssertEquals(5, move2.Lookups.GetCartageAddressElements().Length);
		}
	}
}
