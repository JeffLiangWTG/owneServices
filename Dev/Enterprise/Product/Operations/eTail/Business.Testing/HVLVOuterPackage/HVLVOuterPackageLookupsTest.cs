using CargoWise.EntityFramework.Testing;

namespace Enterprise.eTail.Business.Testing
{
	class HVLVOuterPackageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestHVO_HVL_LoadList_List()
		{
			var loadList1 = Factory.New<HVLVOriginLoadList>();
			var loadList2 = Factory.New<HVLVOriginLoadList>();
			var loadList3 = Factory.New<HVLVOriginLoadList>();

			var outerPackage = Factory.New<HVLVOuterPackage>();
			var lookups = outerPackage.Lookups.HVO_HVL_LoadList_List;

			AssertContainsExactElementsInAnyOrder(new[] { loadList1, loadList2, loadList3 }, lookups);
		}
	}
}
