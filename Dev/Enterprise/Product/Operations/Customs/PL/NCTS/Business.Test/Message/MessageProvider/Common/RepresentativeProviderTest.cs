using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class RepresentativeProviderTest : Customs.Business.Testing.DataProviderTestCase<RepresentativeProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null JobDocAddress", "Value cannot be null.\r\nParameter name: docAddress", () => new RepresentativeProvider(null, null));
			AssertExceptionThrown<ArgumentNullException>("Null Parent Provider", "Value cannot be null.\r\nParameter name: holderOfTheTransitProcedure", () => new RepresentativeProvider(Factory.New<JobDocAddress>(), null));
		});
	}

	public void TestIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No Eori", string.Empty, Provider.IdentificationNumber);

			representativeOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", CountryCodes.Poland);
			AssertEquals("No Eori", "PL111", GetProvider().IdentificationNumber);
		});
	}

	public void TestStatus()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Empty IdentificationNumber For Principal and Representative", "2", GetProvider().Status);

			representativeOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", CountryCodes.Poland);
			principalOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", CountryCodes.Poland);
			AssertEquals("Same IdentificationNumber For Principal and Representative", "2", GetProvider().Status);

			representativeOrgHeader.CustomsCodes.RemoveAll();
			representativeOrgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", CountryCodes.Germany);
			AssertEquals("Different IdentificationNumber between Principal and Representative", "3", GetProvider().Status);
		});
	}

	public void TestStatusEmptyRepresentative()
	{
		CombineAssertions(() =>
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var representative2 = nctsHeader.MovementHeader.Representative;
			var provider = new RepresentativeProvider(representative2, GetCC015CProvider().HolderOfTheTransitProcedure);
			AssertEquals("Status is Empty When Representative is Empty", string.Empty, provider.Status);
		});
	}

	public void TestContactPerson()
	{
		CombineAssertions(() =>
		{
			ContactPersonTestHelper.TestNewOrNull_OnePerson(representativeOrgHeader, () => GetProvider().ContactPerson);
			ContactPersonTestHelper.TestNewOrNull_MultiplePersonsWithAllocations(representativeOrgHeader, () => GetProvider().ContactPerson);
		});
	}
	ICC015C GetCC015CProvider() => new CC015CProvider(movementHeader, "Type");

	protected override RepresentativeProvider GetProvider() => new RepresentativeProvider(representative, GetCC015CProvider().HolderOfTheTransitProcedure);

	protected override void SetUp()
	{
		base.SetUp();

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
		representative = movementHeader.Representative;

		representativeOrgHeader = Factory.New<OrgHeader>();
		representative.OrganisationPK = representativeOrgHeader.PK;
		principalOrgHeader = Factory.New<OrgHeader>();
		nctsHeader.Principal.OrganisationPK = principalOrgHeader.PK;
	}
	NctsDepartureMovementHeader movementHeader;
	JobDocAddress representative;
	OrgHeader representativeOrgHeader;
	OrgHeader principalOrgHeader;
}
