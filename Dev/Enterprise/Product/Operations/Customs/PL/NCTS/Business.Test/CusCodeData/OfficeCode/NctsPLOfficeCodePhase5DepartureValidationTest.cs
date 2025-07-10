using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using static Enterprise.Customs.EU.Business.OfficeCodes_NCTS.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class NctsPLOfficeCodePhase5DepartureValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCY_Data_RP57()
	{
		var expectedError = "[RP57] Customs office of exit (DEP) must start with 'PL'.";

		CombineAssertions(() =>
		{
			var movementHeader = header.MovementHeader;
			movementHeader.CustomsOffices.RemoveAndDeleteAll();
			var officeCode = movementHeader.CustomsOffices.AddNew();

			officeCode.CY_Code = NCTSOfficeOfTransit;
			officeCode.CY_Data = "PL431020";
			AssertNoMessageError("CY_Code is not DEP, CY_Data starts with PL", officeCode.CY_DataInfo, expectedError);

			officeCode.CY_Code = NCTSOfficeOfDeparture;
			officeCode.Validation.ValidateCY_Data();
			AssertNoMessageError("CY_Code is DEP, CY_Data starts with PL", officeCode.CY_DataInfo, expectedError);

			officeCode.CY_Data = "DE002307";
			AssertHasMessageErrorContaining("CY_Code is DEP, CY_Data doen't start with PL", officeCode.CY_DataInfo, expectedError);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.New<NctsHeader>();
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		header.SetMovementType(NctsMovementType.Codes.Departure);
	}
	NctsHeader header;
}
