using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class NctsJobDocAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidatePrincipalPhones()
		=> OrgContactsTestsHelper.ValidateContactsPhones(Factory, (header, _) => header.Principal, NctsMovementType.Codes.Departure, NctsMovementType.Codes.Arrival);

	public void TestValidateJobDocAddressEmployerIdentificationNumber()
	{
		const string errorMessage = "EORI not declared for Destination Trader. Please update EORI for Destination Trader in Organization master.";

		nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var destinationTrader = nctsHeader.DestinationTrader;

		CombineAssertions(() =>
		{
			destinationTrader.Validation.ValidateOrganisationPK();
			AssertNoMessageError(destinationTrader.OrganisationPKInfo, errorMessage);

			var orgHeader = Factory.New<OrgHeader>();
			destinationTrader.OrganisationPK = orgHeader.PK;
			AssertHasMessageError("In phase5 with no EORI", destinationTrader.OrganisationPKInfo, errorMessage);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			destinationTrader.Validation.ValidateOrganisationPK();
			AssertNoMessageError("In phase5 with EORI", destinationTrader.OrganisationPKInfo, errorMessage);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var customsCode = destinationTrader.Organisation.CustomsCodes.AddNew();
			customsCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			customsCode.OK_CustomsRegNo = "1";
			destinationTrader.Validation.ValidateOrganisationPK();
			AssertNoMessageError("The validation will not be triggered in phase 4", destinationTrader.OrganisationPKInfo, errorMessage);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
	}

	NctsHeader nctsHeader;
}
