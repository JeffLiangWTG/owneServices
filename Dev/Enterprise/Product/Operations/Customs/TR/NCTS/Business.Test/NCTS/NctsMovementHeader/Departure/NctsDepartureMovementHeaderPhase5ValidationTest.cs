using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	sealed class NctsDepartureMovementHeaderPhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestBM_LocationOfGoodsCode_List()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(departureMovement.BM_LocationOfGoodsCodeInfo, "ASDFGH", "CUSTOMS");
			departureMovement.BM_LocationOfGoodsCode = "";
			AssertNoMessageErrorContaining(departureMovement.BM_LocationOfGoodsCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckTankerStatus()
		{
			CombineAssertions(() =>
			{
				var validation = departureMovement.Validation;
				var tankerStatusInfo = departureMovement.TankerStatusInfo;
				departureMovement.TankerStatus = ZString.Empty;
				validation.ValidateAll();
				AssertListValidationInvalidCodeMessageError(tankerStatusInfo, false);

				departureMovement.TankerStatus = "1";
				validation.ValidateAll();
				AssertListValidationInvalidCodeMessageError(tankerStatusInfo, false);

				departureMovement.TankerStatus = "3";
				validation.ValidateAll();
				Assert(tankerStatusInfo.Notifications.Any(n => n.Message.Contains("Enter a valid Tanker Status.")));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			departureMovement = header.MovementHeader;
		}

		NctsDepartureMovementHeader departureMovement;
	}
}
