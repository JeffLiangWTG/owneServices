using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class NctsArrivalMovementHeaderValidationTest : TestCaseWithFactory
{
	public void TestCheckGoodsLocationDescriptionCore()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

		const string errorMessage = "Location of Goods details not declared. Please declare details for Location of Goods.";

		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var arrivalMovementHeaderPhase5 = nctsHeader.ArrivalMovementHeader;
			arrivalMovementHeaderPhase5.Validation.ValidateGoodsLocationDescription();
			AssertHasMessageError("The GoodsLocationDescription is empty", arrivalMovementHeaderPhase5.GoodsLocationDescriptionInfo, errorMessage);

			arrivalMovementHeaderPhase5.GoodsLocation.CGL_Qualifier = "A";
			arrivalMovementHeaderPhase5.Validation.ValidateGoodsLocationDescription();
			AssertNoMessageError("The GoodsLocationDescription is not empty", arrivalMovementHeaderPhase5.GoodsLocationDescriptionInfo, errorMessage);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var arrivalMovementHeaderPhase4 = nctsHeader.ArrivalMovementHeader;
			arrivalMovementHeaderPhase4.Validation.ValidateGoodsLocationDescription();
			AssertNoMessageError("The GoodsLocationDescription is empty but with phase 4", arrivalMovementHeaderPhase4.GoodsLocationDescriptionInfo, errorMessage);
		});
	}
}
