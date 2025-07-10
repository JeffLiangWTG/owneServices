using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class HolderOfTheTransitProcedureProviderTest : Customs.Business.Testing.DataProviderTestCase<HolderOfTheTransitProcedureProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>("Null JobDocAddress", "Value cannot be null.\r\nParameter name: docAddress", () => new HolderOfTheTransitProcedureProvider(null, null));
			AssertExceptionThrown<ArgumentNullException>("Null NctsDepartureMovementHeader", "Value cannot be null.\r\nParameter name: movementHeader", () => new HolderOfTheTransitProcedureProvider(Factory.New<JobDocAddress>(), null));
		});
	}

	public void TestIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			AssertEquals("No Organisation", string.Empty, GetProvider().IdentificationNumber);

			principal.OrganisationPK = orgHeader.PK;
			AssertEquals("No Eori", string.Empty, GetProvider().IdentificationNumber);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", CountryCodes.Poland);
			AssertEquals("Eori", "PL111", GetProvider().IdentificationNumber);
		});
	}

	public void TestTIRHolderIdentificationNumber()
	{
		CombineAssertions(() =>
		{
			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.TIR;
			AssertNull("No Organisation", Provider.TIRHolderIdentificationNumber);

			principal.OrganisationPK = orgHeader.PK;
			AssertEquals("No Eori and no TIR", string.Empty, GetProvider().TIRHolderIdentificationNumber);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", CountryCodes.Poland);
			AssertEquals("Eori", "PL111", GetProvider().TIRHolderIdentificationNumber);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "222");
			AssertEquals("TIR", "222", GetProvider().TIRHolderIdentificationNumber);

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
			AssertNull("BM_InBondEntryType != TIR", Provider.TIRHolderIdentificationNumber);
		});
	}

	public void TestName()
	{
		CombineAssertions(() =>
		{
			AssertNull("No header", Provider.Name);

			principal.OrganisationPK = orgHeader.PK;
			AssertEquals("Abba", GetProvider().Name);

			principal.Address.OA_CompanyNameOverride = "Baab";
			AssertEquals("Baab", GetProvider().Name);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", CountryCodes.Poland);
			AssertNull("Name should be null if IdentificationNumber is present", GetProvider().Name);
		});
	}

	public void TestAddress()
	{
		CombineAssertions(() =>
		{
			AssertNull("No organisation", Provider.Address);

			principal.OrganisationPK = orgHeader.PK;
			AssertNotNull("Organisation", GetProvider().Address);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", CountryCodes.Poland);
			AssertNull("Address should be null if IdentificationNumber is present", GetProvider().Address);
		});
	}

	public void TestContactPerson()
	{
		principal.OrganisationPK = orgHeader.PK;
		CombineAssertions(() =>
		{
			ContactPersonTestHelper.TestNewOrNull_OnePerson(orgHeader, () => GetProvider().ContactPerson);
			ContactPersonTestHelper.TestNewOrNull_MultiplePersonsWithAllocations(orgHeader, () => GetProvider().ContactPerson);
		});
	}

	public void TestNameMaxLength()
	{
		const int inNCTSTPPeriod = 35;
		const int outNCTSTPPeriod = 70;

		CombineAssertions(() =>
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertEquals("Name max length in transition period", inNCTSTPPeriod, GetProvider().NameMaxLength);

				var providerNoDependency = new HolderOfTheTransitProcedureProvider(principal, movementHeader, useMaxLengthWithDependency: false);
				AssertEquals("When max length dependencies are disabled Name max length should be outNCTSTPPeriod", outNCTSTPPeriod, providerNoDependency.NameMaxLength);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertEquals("Name max length outside transition period", outNCTSTPPeriod, GetProvider().NameMaxLength);
			}
		});
	}

	protected override HolderOfTheTransitProcedureProvider GetProvider() => new HolderOfTheTransitProcedureProvider(principal, movementHeader);

	protected override void SetUp()
	{
		base.SetUp();

		orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Abba";
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		movementHeader = nctsHeader.MovementHeader;
		principal = nctsHeader.Principal;
	}
	OrgHeader orgHeader;
	NctsDepartureMovementHeader movementHeader;
	JobDocAddress principal;
}
