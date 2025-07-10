using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Universal.Testing
{
	internal class CusRefRateCodeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRateTypeList()
		{
			var testItem = Factory.NewWithValidTestData<CusRefRateCode>();
			var testLookup = testItem.Lookups;
			var testList = testLookup.RateTypeList;
			AssertContainsExactElementsInAnyOrder("Codes", new[] { "DTY", "EXC", "ADD", "LEV", "CVD", "OTH" }, ((ReadOnlyCodeDescriptionPairList)testList).GetAllCodesZString());
			var newList = Factory.NewWithValidTestData<CusRefRateCode>().Lookups.RateTypeList;
			AssertSame("Should have cached", testList, newList);
		}
	}
}
