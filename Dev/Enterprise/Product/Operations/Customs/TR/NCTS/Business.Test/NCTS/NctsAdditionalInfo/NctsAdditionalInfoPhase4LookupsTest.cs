using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	public class NctsAdditionalInfoPhase4LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookupsTypePhase4()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var additionalInfo = goodsItem.AdditionalInfos.AddNew();
			AssertType<NctsAdditionalInfoPhase4Lookups>(additionalInfo.Lookups);
		}
	}
}
