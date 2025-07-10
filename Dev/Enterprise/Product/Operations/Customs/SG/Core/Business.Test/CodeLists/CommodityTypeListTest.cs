using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CommodityTypeListTest : TestCase
	{
		public void TestContainerSizeList()
		{
			CommodityTypeList commodityTypeList = new CommodityTypeList();
			AssertEquals(4, commodityTypeList.Count);
			Assert(commodityTypeList.ContainsCode("ALC"));
			Assert(commodityTypeList.ContainsCode("TOB"));
			Assert(commodityTypeList.ContainsCode("PET"));
			Assert(commodityTypeList.ContainsCode("VEH"));
		}
	}
}
