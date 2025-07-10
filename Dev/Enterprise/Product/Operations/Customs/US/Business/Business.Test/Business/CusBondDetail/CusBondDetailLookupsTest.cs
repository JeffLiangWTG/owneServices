using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class CusBondDetailLookupsTest : TestCaseWithFactory
	{
		public void TestBondDetailDataLookups()
		{
			var bondData = Factory.New<CusBondDetail>();
			bondData.Parent = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals("BondTypeList", Factory.GetCachedValue<ImporterBondTypeList>(), bondData.Lookups.BondTypeList);
			AssertEquals("RegionPorts", typeof(ZZRefCusCodeListCombinedCollection), bondData.Lookups.RegionPorts.GetType());
			AssertEquals("ActivityCodeList", Factory.GetCachedValue<ActivityCodeList>(), bondData.Lookups.ActivityCodeList);
			AssertEquals("FundIndicatorList", Factory.GetCachedValue<FundIndicatorList>(), bondData.Lookups.FundIndicatorList);
		}
	}
}
