using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDepartureCargoDescCollection))]
	class NctsDepartureCargoDescCollectionTest : ActiveBusinessObjectCollectionTestCase<NctsDepartureCargoDescCollection>
	{
		public void TestSetDefaultForNew()
		{
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			AssertEquals("Default Value", Core.Constants.CurrencyCodes.UnitedStates, goodsItem.BY_RX_NKCurrency);
		}

		protected override NctsDepartureCargoDescCollection GetCollectionToTest() => (NctsDepartureCargoDescCollection)nctsHeader.MovementHeader.GoodsItems;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader nctsHeader;
	}
}
