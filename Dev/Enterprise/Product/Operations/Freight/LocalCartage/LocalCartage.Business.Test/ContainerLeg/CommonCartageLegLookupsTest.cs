using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	internal class CommonCartageLegLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCartageAddressList()
		{
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			cartage.RefreshAddresses();
			AssertEquals(5, leg.Lookups.CartageAddressList.Count);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			cartage.DocAddresses.AddNew(org.MainAddress, DocAddressType.LocalCartageImporter);
			AssertEquals(6, leg.Lookups.CartageAddressList.Count);
			var cartage2 = Factory.New<CommonCartage>();
			var move2 = cartage2.LooseBookedMoves.AddNew();
			var leg2 = move2.CartageLegs.AddNew();
			cartage2.RefreshAddresses();
			AssertEquals(5, leg2.Lookups.CartageAddressList.Count);
		}

		public void TestCartageAddressListWithDuplicateAddressUsed()
		{
			var cartage1 = Factory.New<CommonCartage>();
			var move = cartage1.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			var org1 = Helper.CreateOrgHeader("org1", "MainAddress1");
			var org1Address1 = Helper.AddOrgAddress(org1, "org1Address1");
			cartage1.DocAddresses.AddNew(org1Address1, DocAddressType.LocalCartageImporter);
			cartage1.RefreshAddresses();
			AssertEquals(6, leg.Lookups.CartageAddressList.Count);
			var cartage2 = Factory.New<CommonCartage>();
			var move2 = cartage2.LooseBookedMoves.AddNew();
			var leg2 = move2.CartageLegs.AddNew();
			var org2 = Helper.CreateOrgHeader("org2", "MainAddress2");
			cartage2.DocAddresses.AddNew(org1Address1, DocAddressType.LocalCartageImporter);
			var org2Address2 = Helper.AddOrgAddress(org2, "org2Address2");
			cartage2.DocAddresses.AddNew(org2Address2, DocAddressType.LocalCartageImporter);
			cartage2.RefreshAddresses();
			AssertEquals(8, leg2.Lookups.CartageAddressList.Count);
		}

		public void TestCartageAddressElements()
		{
			var cartage = Factory.New<CommonCartage>();
			var move = cartage.LooseBookedMoves.AddNew();
			var leg = move.CartageLegs.AddNew();
			cartage.RefreshAddresses();
			AssertEquals(5, leg.Lookups.GetCartageAddressElements().Length);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			cartage.DocAddresses.AddNew(org.MainAddress, DocAddressType.LocalCartageImporter);
			AssertEquals(6, leg.Lookups.GetCartageAddressElements().Length);
			var cartage2 = Factory.New<CommonCartage>();
			var move2 = cartage2.LooseBookedMoves.AddNew();
			var leg2 = move2.CartageLegs.AddNew();
			cartage2.RefreshAddresses();
			AssertEquals(5, leg2.Lookups.GetCartageAddressElements().Length);
		}

		LocalCartageTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new LocalCartageTestHelper(Factory));
			}
		}

		LocalCartageTestHelper helper;
	}
}
