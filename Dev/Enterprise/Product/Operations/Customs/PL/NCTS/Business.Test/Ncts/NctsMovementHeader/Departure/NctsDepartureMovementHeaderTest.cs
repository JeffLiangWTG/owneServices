using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using MovementType = Enterprise.Customs.EU.NCTS.Business.NctsMovementType.Codes;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(NctsDepartureMovementHeader))]
sealed class NctsDepartureMovementHeaderTest : EnterpriseBusinessObjectTestCase
{
	public void TestMessages()
	{
		var messages = departureMovement.Messages;
		AssertEquals(departureMovement, messages.Master);
		AssertType<EDIMessageCollection>(messages);
	}

	public void TestHeaderType() => AssertType<NctsHeader>(EuDepartureMovement.Header);

	public void TestGoodsLocationType() => AssertType<CusGoodsLocation>(EuDepartureMovement.GoodsLocation);

	public void TestRepresentativeDocAddressValidationType() => AssertType<TraderJobDocAddressValidation>(DocAddresses.PiggyBackedDocAddressValidation(departureMovement.Representative));

	public void TestCarrierDocAddressValidationType() => AssertType<NctsJobDocAddressValidation>(DocAddresses.PiggyBackedDocAddressValidation(departureMovement.Carrier));

	public void TestValidateRepresentativeContactPhones()
		=> OrgContactsTestsHelper.ValidateContactsPhones(Factory, (_, movementHeader) => movementHeader.Representative, MovementType.Departure);

	public void TestValidateCarrierContactPhones()
		=> OrgContactsTestsHelper.ValidateContactsPhones(Factory, (_, movementHeader) => movementHeader.Carrier, MovementType.Departure);

	public void TestValidation() => AssertType<NctsDepartureMovementHeaderPhase5Validation>(departureMovement.Validation);

	public void TestGetLRNAndSetIfNeeded_TraderIsNotSelectedOrHasNotEORI()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.FillWithValidTestData();

		departureMovement.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		var representative = departureMovement.Representative;
		var principal = nctsHeader.Principal;

		CombineAssertions(() =>
		{
			AssertEquals("Representative and Principal are not selected", ZString.Empty, departureMovement.GetLRNAndSetIfNeeded());

			representative.OrganisationPK = orgHeader.PK;
			AssertEquals("Representative doesn't have EORI number and Principal is not selected", ZString.Empty, departureMovement.GetLRNAndSetIfNeeded());

			principal.OrganisationPK = orgHeader.PK;
			AssertEquals("Representative and Principal don't have EORI numbers", ZString.Empty, departureMovement.GetLRNAndSetIfNeeded());

			representative.OrganisationPK = ZGuid.Empty;
			AssertEquals("Representative is not selected and Principal doesn't have EORI number", ZString.Empty, departureMovement.GetLRNAndSetIfNeeded());
		});
	}

	public void TestGetLRNAndSetIfNeeded_TraderHasEORI()
	{
		const string testCountryCode = "PL";

		CombineAssertions(() =>
		{
			AssertGetLRNAndSetIfNeeded("Representative has EORI number and Principal doesn't have EORI number", "1234567890", "", "1234567890");
			AssertGetLRNAndSetIfNeeded("Representative doesn't have EORI number and Principal has EORI number", "", "9876543210", "9876543210");
			AssertGetLRNAndSetIfNeeded("Representative has EORI number and Principal has EORI number", "1234567890", "9876543210", "1234567890");
		});

		void AssertGetLRNAndSetIfNeeded(string description, string representativeEORI, string principalEORI, string expectedPartOfLRN)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var representative = nctsHeader.MovementHeader.Representative;
			var principal = nctsHeader.Principal;

			representative.OrganisationPK = createOrganisation(representativeEORI).PK;
			principal.OrganisationPK = createOrganisation(principalEORI).PK;

			ZString expected = $"{testCountryCode}{expectedPartOfLRN}";
			AssertStartsWith(description, expected, nctsHeader.MovementHeader.GetLRNAndSetIfNeeded());
		}

		OrgHeader createOrganisation(string eoriNumber = "")
		{
			var result = Factory.New<OrgHeader>();
			result.FillWithValidTestData();
			if (!string.IsNullOrEmpty(eoriNumber))
			{
				result.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriNumber, testCountryCode);
			}
			return result;
		}
	}

	[TestDate(2022, 10, 22)]
	public void TestGetLRNAndSetIfNeeded_Format()
	{
		const string testCountryCode = "PL";
		const string testYearDigits = "22";

		var organisation = Factory.New<OrgHeader>();
		organisation.FillWithValidTestData();
		var code = organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, string.Empty, testCountryCode);

		CombineAssertions(() =>
		{
			var testEORI = "1234567890";
			ZString expectedLRN = $"{testCountryCode}{"1234567890"}{testYearDigits}{"00000001"}";
			generateLRNAndAssertFormat("EORI length = 10", testEORI, expectedLRN);

			testEORI = "987654321012";
			expectedLRN = $"{testCountryCode}{"9876543210"}{testYearDigits}{"00000002"}";
			generateLRNAndAssertFormat("EORI length > 10", testEORI, expectedLRN);

			testEORI = "54321";
			expectedLRN = $"{testCountryCode}{"5432100000"}{testYearDigits}{"00000003"}";
			generateLRNAndAssertFormat("EORI length < 10", testEORI, expectedLRN);
		});

		void generateLRNAndAssertFormat(string description, string testEORI, string expectedLRN)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.MovementHeader.Representative.OrganisationPK = organisation.PK;
			code.OK_CustomsRegNo = testEORI;
			AssertEquals(description, expectedLRN, nctsHeader.MovementHeader.GetLRNAndSetIfNeeded());
		}
	}

	[TestDate(2022, 10, 22)]
	public void TestGetLRNAndSetIfNeeded_OnlyIf_BM_PaperlessInbondNumInfo_IsNotPresented()
	{
		const string countryCode = "PL";
		const string year = "22";
		const string eoriNumber = "1122334455";
		const string sequence = "00000001";

		var eoriOrganization = Factory.New<OrgHeader>();
		eoriOrganization.FillWithValidTestData();
		eoriOrganization.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriNumber, countryCode);

		CombineAssertions(() =>
		{
			var expectedLRN = $"{countryCode}{eoriNumber}{year}{sequence}";
			AssertBM_PaperlessInboundNum("If LRN is empty then it should be changed.", string.Empty, expectedLRN);

			const string testLRN = "PL00667788992200000001";
			AssertBM_PaperlessInboundNum("If LRN is not empty then it shouldn't be changed.", testLRN, testLRN);
		});

		void AssertBM_PaperlessInboundNum(string descriptiopn, string initialLRN, string expectedLRN)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = initialLRN;
			nctsHeader.MovementHeader.Representative.OrganisationPK = eoriOrganization.PK;

			var result = nctsHeader.MovementHeader.GetLRNAndSetIfNeeded();
			AssertEquals($"{descriptiopn}", expectedLRN, nctsHeader.MovementHeader.BM_PaperlessInbondNum);
			AssertEquals($"{descriptiopn}. LRN is equal to returned value", nctsHeader.MovementHeader.BM_PaperlessInbondNum, result);
		}
	}

	public void TestGenerateLocalReferenceNumberOnSaving()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		departureMovement.HeaderBranch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "PL1234567890", "PL");
		Factory.Save();

		AssertEquals("LRN should not be generated on Save. Now LRN is created only when message is sent.", string.Empty, departureMovement.BM_PaperlessInbondNum);
	}

	public void TestGetExportDate()
	{
		CombineAssertions(() =>
		{
			AssertNull("Default data", departureMovement.GetExportDate());

			departureMovement.BM_ExportDate = ZDateTime.Today;
			departureMovement.IsSimplifiedNctsProcedure = false;
			AssertNull("is not empty but also is not simplified procedure", departureMovement.GetExportDate());

			departureMovement.IsSimplifiedNctsProcedure = true;
			AssertNotNull("is not empty and is simplified procedure", departureMovement.GetExportDate());

			departureMovement.BM_ExportDate = ZDateTime.Empty;
			AssertNull("is empty and is simplified procedure", departureMovement.GetExportDate());
		});
	}

	public void TestCustomsOffices()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		AssertType<NctsPLOfficeCodeCollection>(header.MovementHeader.CustomsOffices);
	}

	public void TestBM_GrossWeightUQ_ReadOnly() => Assert(!departureMovement.BM_GrossWeightUQInfo.ReadOnly);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => departureMovement;

	protected override BusinessObject GetNewBusinessObject() => departureMovement;

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => departureMovement;

	protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest) => new LightValidationTesterExcludingGoodsLocation(bizObjToTest);

	protected override void SetUp()
	{
		base.SetUp();
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		departureMovement = nctsHeader.MovementHeader;
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader departureMovement;

	EU.NCTS.Business.NctsDepartureMovementHeader EuDepartureMovement => departureMovement;
	IDocAddresses DocAddresses => departureMovement;

	sealed class LightValidationTesterExcludingGoodsLocation : LightValidationTester
	{
		public LightValidationTesterExcludingGoodsLocation(BusinessObject bo) : base(bo) { }

		protected override bool ShouldTestProperty(ZPropertyInfo info)
			=> !(info.BizObj is CusGoodsLocation || info.BizObj is CusGoodsLocationAddress) && base.ShouldTestProperty(info);
	}
}
