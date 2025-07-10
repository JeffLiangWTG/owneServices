using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class NctsDepartureMovementHeaderPhase4ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBM_PlaceOfLoading()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
				nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
				var departureMovement = nctsHeader.MovementHeader;

				var errorMessage = "Place Of Loading should be alphanumeric.";

				nctsHeader.BH_FTZMove = true;
				departureMovement.BM_PlaceOfLoading = ZString.Empty;
				AssertHasMessageErrorContaining("Error message should appear when BH_FTZMove is true", departureMovement.BM_PlaceOfLoadingInfo, MandatoryValidation.YouHaveNotEntered);

				departureMovement.BM_PlaceOfLoading = "AAABBC-";
				AssertHasMessageErrorContaining("Place Of Loading should be alphanumeric.", departureMovement.BM_PlaceOfLoadingInfo, errorMessage);

				departureMovement.BM_PlaceOfLoading = "AAABB";
				AssertNoMessageErrorContaining("Place Of Loading should be alphanumeric.", departureMovement.BM_PlaceOfLoadingInfo, errorMessage);
			});
		}

		public void TestCheckBM_BTAIndicator()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var movementHeader = header.MovementHeader;

			movementHeader.BM_BTAIndicator = "O";

			AssertHasMessageErrorContaining(movementHeader.BM_BTAIndicatorInfo, "The code you have selected is not in the list.");

			movementHeader.BM_BTAIndicator = SpesificCircumstanceIndicatorList.Codes.C;

			AssertNoMessageErrorContaining(movementHeader.BM_BTAIndicatorInfo, "The code you have selected is not in the list.");
		}
	}
}
