using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business.Customs.Asycuda;

namespace Enterprise.Customs.SG.Access.Business.Testing
{
	sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCycleNumbers()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			AssertEquals(@"1 - 23:40
2 - 00:25
3 - 01:10
4 - 01:55
5 - 02:40
6 - 03:25
7 - 04:10
8 - 04:55
9 - 05:40
10 - 06:25
11 - 07:10
12 - 07:55
13 - 08:40
14 - 09:25
15 - 10:10
16 - 10:55
17 - 11:40
18 - 12:25
19 - 13:10
20 - 13:55
21 - 14:40
22 - 15:25
23 - 16:10
24 - 16:55
25 - 17:40
26 - 18:25
27 - 19:10
28 - 19:55
29 - 20:40
30 - 21:25
31 - 22:10
32 - 22:55", bill.Lookups.CycleNumbers.ElementsAsString);
		}

		public void TestSGPayeeIndicatorList()
		{
			var billCountry = Factory.New<AsycudaBill>();
			Assert("SGPayeeIndicatorList should be cached", ReferenceEquals(billCountry.Lookups.SGPayeeIndicatorList, Factory.GetCachedValue<SGPayeeIndicatorList>()));
		}

		public void TestSGPartyStatusList()
		{
			var billCountry = Factory.New<AsycudaBill>();
			Assert("SGPayeeIndicatorList should be cached", ReferenceEquals(billCountry.Lookups.SGPartyStatusList, Factory.GetCachedValue<SGPartyStatusList>()));
		}
	}
}
