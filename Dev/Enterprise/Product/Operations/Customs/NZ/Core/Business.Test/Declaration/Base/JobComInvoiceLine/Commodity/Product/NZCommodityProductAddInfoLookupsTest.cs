//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCommodityProductAddInfoLookups
//
//    This class should be used for overriding collections in AutoNZCommodityProductAddInfoLookups
//    (for example to add filtering), or for adding your own lookup collections.
//
//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)
//
// </important>
//--------------------------------------------------------------------------------------------------

using Enterprise.Customs.NZ.TradeSingleWindow;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	using CargoWise.EntityFramework.Testing;

	internal class NZCommodityProductAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNameTypeList()
		{
			AssertEquals(typeof(ProductNameTypeList), Lookups.NameTypeList.GetType());
			Assert("Brand Name code", Lookups.NameTypeList.ContainsCode(ProductNameTypeList.Codes.P223));
		}

		public void TestIDTypeList()
		{
			AssertEquals(typeof(IdentityTypeList), Lookups.IDTypeList.GetType());
			Assert("Serial Number code", Lookups.IDTypeList.ContainsCode(IdentityTypeList.Codes.BN));
		}

		NZCommodityProductAddInfoLookups Lookups
		{
			get { return lookups ?? (lookups = new NZCommodityProductAddInfoLookups(CommodityAddInfo)); }
		}
		NZCommodityProductAddInfoLookups lookups;

		NZCommodityProductAddInfo CommodityAddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					var commodityProduct = Factory.New<CommodityProduct>();
					fAddInfo = new NZCommodityProductAddInfo(commodityProduct.B7_AddInfoDataInfo);
				}
				return fAddInfo;
			}
		}
		NZCommodityProductAddInfo fAddInfo;
	}
}
