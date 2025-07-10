//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoNZCommodityAddInfoLookups
//
//    This class should be used for overriding collections in AutoNZCommodityAddInfoLookups
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

	internal class NZCommodityAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestClassTypeList()
		{
			AssertEquals(typeof(ClassificationTypeList), Lookups.ClassTypeList.GetType());
			AssertEquals("HS Code", false, Lookups.ClassTypeList.ContainsCode(ClassificationTypeList.Codes.HS));
			AssertEquals("CV Code", false, Lookups.ClassTypeList.ContainsCode(ClassificationTypeList.Codes.CV));
			AssertEquals("SSO Code", false, Lookups.ClassTypeList.ContainsCode(ClassificationTypeList.Codes.SSO));
		}

		NZCommodityAddInfoLookups Lookups
		{
			get { return lookups ?? (lookups = new NZCommodityAddInfoLookups(CommodityAddInfo)); }
		}
		NZCommodityAddInfoLookups lookups;

		NZCommodityAddInfo CommodityAddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					var commodityLine = Factory.New<CommodityLine>();
					fAddInfo = new NZCommodityAddInfo(commodityLine.B7_AddInfoDataInfo);
				}
				return fAddInfo;
			}
		}
		NZCommodityAddInfo fAddInfo;
	}
}
