using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class IssuerAndBillOfLadingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCY_CodeList()
		{
			var issuerAndBillOfLading = Factory.New<IssuerAndBillOfLading>();
			AssertEquals(Factory.GetCachedValue<IssuerAndBillOfLadingStatusList>(), issuerAndBillOfLading.Lookups.CY_CodeList);
		}
	}
}
