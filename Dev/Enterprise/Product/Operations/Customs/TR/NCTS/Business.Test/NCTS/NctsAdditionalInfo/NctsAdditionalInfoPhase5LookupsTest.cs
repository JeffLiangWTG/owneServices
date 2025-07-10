using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	public class NctsAdditionalInfoPhase5LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupsTypePhase5()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var additionalInfo = goodsItem.AdditionalInfos.AddNew();
			AssertType<NctsAdditionalInfoPhase5Lookups>(additionalInfo.Lookups);
		}
	}
}
