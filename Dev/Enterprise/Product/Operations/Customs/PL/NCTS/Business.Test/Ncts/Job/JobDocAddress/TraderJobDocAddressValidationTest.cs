using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class TraderJobDocAddressValidationTest : BusinessObjectValidationTestCase
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Name of trader type is empty (the 3-rd parameter is NctsCommonCargoDesc).", "Value cannot be null.\r\nParameter name: traderType", () => new TraderJobDocAddressValidation(representative, null, (NctsCommonCargoDesc)null));
			AssertExceptionThrown<ArgumentNullException>("Name of trader type is empty (the 3-rd parameter is NctsHeader).", "Value cannot be null.\r\nParameter name: traderType", () => new TraderJobDocAddressValidation(representative, null, (NctsHeader)null));
		});
	}

	public void TestCheckOrganisationPK_RepresentativeAndPrincipal()
	{
		CombineAssertions(() =>
		{
			var testCaseDescription = "Representative and Principal are not selected";
			AssertNoNR0016AndHasNR0017(testCaseDescription, "Representative", representative);
			AssertNoNR0016AndHasNR0017(testCaseDescription, "Principal", principal);

			representative.OrganisationPK = orgHeader.PK;
			testCaseDescription = "Representative doesn't have EORI number and Principal is not selected";
			AssertHasNR0016AndNoNR0017(testCaseDescription, "Representative", representative);
			AssertHasNR0016AndNoNR0017(testCaseDescription, "Principal", principal);

			representative.OrganisationPK = ZGuid.Empty;
			principal.OrganisationPK = orgHeader.PK;
			testCaseDescription = "Representative is not selected and Principal doesn't have EORI number";
			AssertHasNR0016AndNoNR0017(testCaseDescription, "Representative", representative);
			AssertHasNR0016AndNoNR0017(testCaseDescription, "Principal", principal);

			representative.OrganisationPK = orgHeader.PK;
			testCaseDescription = "Representative and Principal don't have EORI numbers";
			AssertHasNR0016AndNoNR0017(testCaseDescription, "Representative", representative);
			AssertHasNR0016AndNoNR0017(testCaseDescription, "Principal", principal);

			representative.OrganisationPK = orgHeaderWithEori.PK;
			testCaseDescription = "Representative has EORI number and Principal doesn't have EORI number";
			AssertNoNR0016AndNoNR0017(testCaseDescription, "Representative", representative);
			AssertNoNR0016AndNoNR0017(testCaseDescription, "Principal", principal);

			representative.OrganisationPK = ZGuid.Empty;
			principal.OrganisationPK = orgHeaderWithEori.PK;
			testCaseDescription = "Representative doesn't have EORI number and Principal has EORI number";
			AssertNoNR0016AndNoNR0017(testCaseDescription, "Representative", representative);
			AssertNoNR0016AndNoNR0017(testCaseDescription, "Principal", principal);
		});
	}

	public void TestCheckOrganisationPK_MovementType()
	{
		CombineAssertions(() =>
		{
			var principal = CreateHeaderAndGetPrincipal(NctsMovementType.Codes.Departure);
			AssertNoNR0016AndHasNR0017("DepartureMovement: Representative and Principal are not selected", "Principal", principal);
			principal.OrganisationPK = orgHeader.PK;
			AssertHasNR0016AndNoNR0017("DepartureMovement: Representative is not selected and Principal doesn't have EORI number", "Principal", principal);

			principal = CreateHeaderAndGetPrincipal(NctsMovementType.Codes.Arrival);
			AssertNoNR0016AndNoNR0017("ArrivalMovement: Representative and Principal are not selected", "Principal", principal);
			principal.OrganisationPK = orgHeader.PK;
			AssertNoNR0016AndNoNR0017("ArrivalMovement: Representative is not selected and Principal doesn't have EORI number", "Principal", principal);
		});

		JobDocAddress CreateHeaderAndGetPrincipal(ZString movementType)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.BH_HeaderType = movementType;
			return nctsHeader.Principal;
		}
	}

	void AssertNoNR0016AndNoNR0017(string description, string caption, JobDocAddress address)
	{
		address.Validation.ValidateAll();
		AssertNoMessageError($"{description}: {caption} should not have NR0016 error message", address.OrganisationPKInfo, errorMessageNR0016);
		AssertNoMessageError($"{description}: {caption} should not have NR0017 error message", address.OrganisationPKInfo, errorMessageNR0017(caption));
	}

	void AssertHasNR0016AndNoNR0017(string description, string caption, JobDocAddress address)
	{
		address.Validation.ValidateAll();
		AssertHasMessageError($"{description}: {caption} should have NR0016 error message", address.OrganisationPKInfo, errorMessageNR0016);
		AssertNoMessageError($"{description}: {caption} should not have NR0017 error message", address.OrganisationPKInfo, errorMessageNR0017(caption));
	}

	void AssertNoNR0016AndHasNR0017(string description, string caption, JobDocAddress address)
	{
		address.Validation.ValidateAll();
		AssertNoMessageError($"{description}: {caption} should not have NR0016 error message", address.OrganisationPKInfo, errorMessageNR0016);
		AssertHasMessageError($"{description}: {caption} should have NR0017 error message", address.OrganisationPKInfo, errorMessageNR0017(caption));
	}

	string errorMessageNR0017(string caption) => $"[NR0017] You have not entered a {caption}.";

	const string errorMessageNR0016 = "[NR0016] EORI number of the Representative or Principal is required for generation of valid LRN.";

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		var departureMovement = nctsHeader.MovementHeader;

		representative = departureMovement.Representative;
		principal = nctsHeader.Principal;

		orgHeader = Factory.New<OrgHeader>();
		orgHeaderWithEori = Factory.New<OrgHeader>();
		orgHeaderWithEori.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", "PL");
	}

	JobDocAddress representative;
	JobDocAddress principal;
	OrgHeader orgHeader;
	OrgHeader orgHeaderWithEori;
}

