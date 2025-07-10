using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusInBondCargoDescLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTransportChargesModeOfPaymentList()
		{
			var list = lookups.TransportChargesModeOfPaymentList;
			CombineAssertions(() =>
			{
				AssertEquals("CodesAsString", "A, B, C, D, H, Y, Z", list.CodesAsString);
				AssertEquals("Cached", list, lookups.TransportChargesModeOfPaymentList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new CusInBondCargoDescLookups(Factory.New<CusInBondCargoDescForTest>());
		}
		CusInBondCargoDescLookups lookups;
	}
}
