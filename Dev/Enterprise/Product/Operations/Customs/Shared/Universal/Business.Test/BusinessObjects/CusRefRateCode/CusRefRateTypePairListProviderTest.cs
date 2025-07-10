using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	class CusRefRateTypePairListProviderTest : TestCase
	{
		public void TestGetCusRefRateTypeList()
		{
			var provider = new CusRefRateTypePairListProvider();
			var testList = provider.GetCusRefRateTypeList();
			AssertContainsExactElementsInAnyOrder("Codes", new ZString[] { "DTY", "EXC", "ADD", "LEV", "CVD", "OTH" }, ((CodeDescriptionPairList)testList).GetAllCodesZString());
		}
	}
}
