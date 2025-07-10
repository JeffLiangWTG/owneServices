using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class RepresentativeJobDocAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckOrganisationPK()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var arrivalMovementHeaderPhase5 = nctsHeader.ArrivalMovementHeader;
		var orgHeader = Factory.New<OrgHeader>();

		const string warningMessage = "EORI for Trader Representative is not present.";

		CombineAssertions(() =>
		{
			arrivalMovementHeaderPhase5.RepresentativeTrader = ZGuid.Empty;
			arrivalMovementHeaderPhase5.Representative.AdditionalValidation.ValidateAll();
			AssertNoWarning("No organisation and EORI", arrivalMovementHeaderPhase5.RepresentativeTraderInfo, warningMessage);

			arrivalMovementHeaderPhase5.RepresentativeTrader = orgHeader.PK;
			arrivalMovementHeaderPhase5.Representative.AdditionalValidation.ValidateAll();
			AssertHasWarning("Organisation is present but no EORI", arrivalMovementHeaderPhase5.RepresentativeTraderInfo, warningMessage);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", CountryCodes.Poland);
			arrivalMovementHeaderPhase5.Representative.AdditionalValidation.ValidateAll();
			AssertNoWarning("Organisation and EORI are both present", arrivalMovementHeaderPhase5.RepresentativeTraderInfo, warningMessage);

			arrivalMovementHeaderPhase5.RepresentativeTrader = ZGuid.Empty;
			arrivalMovementHeaderPhase5.Representative.AdditionalValidation.ValidateAll();
			AssertNoWarning("No organisation but EORI is present", arrivalMovementHeaderPhase5.RepresentativeTraderInfo, warningMessage);
		});
	}
}
