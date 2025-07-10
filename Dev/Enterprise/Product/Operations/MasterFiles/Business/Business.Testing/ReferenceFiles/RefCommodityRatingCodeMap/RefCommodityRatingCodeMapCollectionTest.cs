using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefCommodityRatingCodeMapCollection))]
	class RefCommodityRatingCodeMapCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCommodityRatingCodeMapCollection>
	{
		public void TestParentSetOnCreation()
		{
			var commodityCode = Factory.New<RefCommodityCode>();
			commodityCode.RH_Code = "XAPL";

			var child = commodityCode.RefCommodityRatingCodeMaps.AddNew();
			AssertEquals("XAPL", child.RI_RH_NKCommodityParent);
		}
	}
}
