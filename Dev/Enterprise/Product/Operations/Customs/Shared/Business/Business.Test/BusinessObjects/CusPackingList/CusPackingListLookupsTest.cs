using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPackingListLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestWeightUQs()
		{
			var packingList = Factory.New<CusPackingList>();
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), packingList.Lookups.WeightUQs);
		}
	}
}
