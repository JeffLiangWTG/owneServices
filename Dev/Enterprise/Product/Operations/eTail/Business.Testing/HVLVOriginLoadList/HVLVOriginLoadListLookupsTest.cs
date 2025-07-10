using CargoWise.EntityFramework.Testing;

namespace Enterprise.eTail.Business.Testing
{
	class HVLVOriginLoadListLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestHVL_Status_List()
		{
			var originLoadList = Factory.New<HVLVOriginLoadList>();
			var lookups = originLoadList.Lookups.HVL_Status_List;
			var statusList = new[] { "OPN", "PEN", "CLS", "CON", "LDG", "FAL" };

			AssertEquals("HVL Status List Count", statusList.Length, lookups.Count);
			AssertContainsExactElementsInAnyOrder("HVL Status List", statusList, lookups.GetAllCodes());
		}
	}
}
