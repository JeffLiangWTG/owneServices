using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TaxFrameworkConfigurationHelperTest : TestCaseWithFactory
	{
		public void TestGetTaxAuthorities()
		{
			CreateTaxAuthorities();

			var type1_BRNAT = new CodeDescriptionPair("BRNAT", "BR NATIONAL - NAT1");
			var type2_BRMUN = new CodeDescriptionPair("BRM1", "BR MUNICIPAL - MUN1");
			var type3_BRMUN = new CodeDescriptionPair("BRM2", "BR MUNICIPAL - MUN2");
			var type4_AUNAT = new CodeDescriptionPair("AUNAT", "AU NATIONAL - NAT1");

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { type1_BRNAT, type2_BRMUN, type3_BRMUN }, helper.GetTaxAuthorities(CountryCodes.Brazil));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { }, helper.GetTaxAuthorities(CountryCodes.India));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { }, helper.GetTaxAuthorities(ZString.Empty));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { type4_AUNAT }, helper.GetTaxAuthorities(CountryCodes.Australia));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { type4_AUNAT }, helper.GetTaxAuthorities(CountryCodes.Australia, null));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { type1_BRNAT, type2_BRMUN, type3_BRMUN }, helper.GetTaxAuthorities(CountryCodes.Brazil, null));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { type1_BRNAT, type2_BRMUN, type3_BRMUN }, helper.GetTaxAuthorities(CountryCodes.Brazil, ZString.Empty));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { type2_BRMUN, type3_BRMUN }, helper.GetTaxAuthorities(CountryCodes.Brazil, TaxAuthorityTypeList.Municipal.Code));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { }, helper.GetTaxAuthorities(CountryCodes.Brazil, TaxAuthorityTypeList.State.Code));
		}

		public void TestGetTaxSystems()
		{
			CreateTaxSystems();

			var taxSystem1_AUCompany = new CodeDescriptionPair("TS1", "Tax System1 - AU Company");
			var taxSystem2_AUCompany = new CodeDescriptionPair("TS2", "Tax System2 - AU Company");
			var taxSystem3_AUBranch = new CodeDescriptionPair("TS3", "Tax System3 - AU Branch");
			var taxSystem4_BRCompany = new CodeDescriptionPair("TS4", "Tax System4 - BR Company");
			var taxSystem5_BRBranch = new CodeDescriptionPair("TS5", "Tax System5 - BR Branch");

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { }, helper.GetTaxSystems(ZString.Empty));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { }, helper.GetTaxSystems(CountryCodes.India));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { taxSystem1_AUCompany, taxSystem2_AUCompany, taxSystem3_AUBranch }, helper.GetTaxSystems(CountryCodes.Australia));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { taxSystem1_AUCompany, taxSystem2_AUCompany }, helper.GetTaxSystems(CountryCodes.Australia, TaxSystemRegistrationLevels.Company.Code));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { taxSystem4_BRCompany, taxSystem5_BRBranch }, helper.GetTaxSystems(CountryCodes.Brazil));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { taxSystem4_BRCompany, taxSystem5_BRBranch }, helper.GetTaxSystems(CountryCodes.Brazil, null));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { taxSystem4_BRCompany, taxSystem5_BRBranch }, helper.GetTaxSystems(CountryCodes.Brazil, ZString.Empty));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { taxSystem4_BRCompany }, helper.GetTaxSystems(CountryCodes.Brazil, TaxSystemRegistrationLevels.Company.Code));

			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { taxSystem5_BRBranch }, helper.GetTaxSystems(CountryCodes.Brazil, TaxSystemRegistrationLevels.Branch.Code));
		}

		public void TestHasAnyAccTaxConfiguration_CompanyLevel()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var company3 = Factory.NewWithValidTestData<GlbCompany>();

			var taxTestHelper = new AccountingTestObjectCreator(Factory);
			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(company1, taxConfigIsActive: true);
			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(company2, taxConfigIsActive: false);

			Assert("Exist active AccTaxConfiguration for company1", helper.HasAnyAccTaxConfiguration(Factory, company1));
			Assert("Exist inactive AccTaxConfiguration for company2", helper.HasAnyAccTaxConfiguration(Factory, company2));
			Assert("No AccTaxConfiguration", !helper.HasAnyAccTaxConfiguration(Factory, company3));
		}

		public void TestHasAnyAccTaxConfiguration_BranchLevel()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;
			branch2.GB_GC = company2.PK;
			branch3.GB_GC = company3.PK;

			var taxTestHelper = new AccountingTestObjectCreator(Factory);
			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(company1, taxConfigIsActive: true);
			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(company2, taxConfigIsActive: false);

			Assert("Exist active AccTaxConfiguration for branch1", helper.HasAnyAccTaxConfiguration(Factory, company1));
			Assert("Exist inactive AccTaxConfiguration for branch2", helper.HasAnyAccTaxConfiguration(Factory, company2));
			Assert("No AccTaxConfiguration", !helper.HasAnyAccTaxConfiguration(Factory, company3));
		}

		public void TestGetTaxConfigurationThatSupportsOrganisationRates_ThrowsExceptionNullParams()
		{
			AssertExceptionThrown<ArgumentNullException>(() => helper.GetTaxConfigurationThatSupportsOrganisationRates(null, GlbCompany.CurrentCompany));
			AssertExceptionThrown<ArgumentNullException>(() => helper.GetTaxConfigurationThatSupportsOrganisationRates(Factory, null));
		}

		public void TestGetTaxConfigurationThatSupportsOrganisationRates_CompanyLevel()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			var expectedCompany = company1;

			var taxTestHelper = new AccountingTestObjectCreator(Factory);

			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(company2, taxRateSource: TaxRateSources.OrganisationOnly.Code);
			var expectedResult = helper.GetTaxConfigurationThatSupportsOrganisationRates(Factory, expectedCompany);
			AssertEquals("Collection must not contain elements.", 0, expectedResult.Count);

			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(expectedCompany, taxRateSource: TaxRateSources.OrganisationOnly.Code);
			expectedResult = helper.GetTaxConfigurationThatSupportsOrganisationRates(Factory, expectedCompany);
			AssertEquals("Collection must contain elements.", 1, expectedResult.Count);
		}

		public void TestGetTaxConfigurationThatSupportsOrganisationRates_BranchLevel()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();

			branch1.GB_GC = company1.PK;
			branch2.GB_GC = company2.PK;

			var expectedCompany = branch1.Company;

			var taxTestHelper = new AccountingTestObjectCreator(Factory);

			taxTestHelper.ConfigureTaxFrameworkAtBranchLevel(branch2, taxRateSource: TaxRateSources.OrganisationOnly.Code, taxSystemCountry: branch2.BaseCountry.Code);
			var expectedResult = helper.GetTaxConfigurationThatSupportsOrganisationRates(Factory, expectedCompany);
			AssertEquals("Collection must not contain elements.", 0, expectedResult.Count);

			branch1.GB_IsActive = false;
			taxTestHelper.ConfigureTaxFrameworkAtBranchLevel(branch1, taxRateSource: TaxRateSources.OrganisationOnly.Code, taxSystemCountry: branch1.BaseCountry.Code);
			expectedResult = helper.GetTaxConfigurationThatSupportsOrganisationRates(Factory, expectedCompany);
			AssertEquals("Collection must not contain elements.", 0, expectedResult.Count);

			branch1.GB_IsActive = true;
			expectedResult = helper.GetTaxConfigurationThatSupportsOrganisationRates(Factory, expectedCompany);
			AssertEquals("Collection must contain elements.", 1, expectedResult.Count);
		}

		public void TestGetTaxConfigurationThatSupportsOrganisationRates_WhenTaxConfigIsNotActive()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;

			var expectedCompany = branch1.Company;

			var taxTestHelper = new AccountingTestObjectCreator(Factory);
			taxTestHelper.ConfigureTaxFrameworkAtBranchLevel(branch1, taxRateSource: TaxRateSources.OrganisationOnly.Code, taxConfigIsActive: false, taxSystemCode: "TS1");
			var expectedResult = helper.GetTaxConfigurationThatSupportsOrganisationRates(Factory, expectedCompany);
			AssertEquals("Collection must not contain elements.", 0, expectedResult.Count);

			taxTestHelper.ConfigureTaxFrameworkAtBranchLevel(branch1, taxRateSource: TaxRateSources.OrganisationOnly.Code, taxConfigIsActive: true, taxSystemCode: "TS2");
			expectedResult = helper.GetTaxConfigurationThatSupportsOrganisationRates(Factory, expectedCompany);
			AssertEquals("Collection must contain elements.", 1, expectedResult.Count);
		}

		public void TestGetTaxConfigurationThatSupportsOrganisationRates_WithAllTaxRateCodes()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var expectedCompany = company1;

			var testCases = new[]
			{
				new { TaxRateCode = TaxRateSources.OrganisationFallbackToTaxGroup.Code, ExpectedResult = 1, TaxSystemCode = "TS1" },
				new { TaxRateCode = TaxRateSources.OrganisationFallbackToTaxID.Code, ExpectedResult = 1, TaxSystemCode = "TS2" },
				new { TaxRateCode = TaxRateSources.OrganisationOnly.Code, ExpectedResult = 1, TaxSystemCode = "TS3" },
				new { TaxRateCode = TaxRateSources.TaxGroupOnly.Code, ExpectedResult = 0, TaxSystemCode = "TS4" },
				new { TaxRateCode = TaxRateSources.TaxIDOnly.Code, ExpectedResult = 0, TaxSystemCode = "TS5" }
			};

			var taxRateSource = new TaxRateSources().GetAllCodes();
			AssertEquals("It will fail if TaxRateSources contains code that is not in test cases.", testCases.Length, taxRateSource.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "ORG", "OFT", "OFI", "TGR", "TID" }, taxRateSource);

			foreach (var testCase in testCases)
			{
				var taxTestHelper = new AccountingTestObjectCreator(Factory);
				taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(expectedCompany, taxRateSource: testCase.TaxRateCode, taxSystemCode: testCase.TaxSystemCode);
				var expectedResult = helper.GetTaxConfigurationThatSupportsOrganisationRates(Factory, expectedCompany);
				AssertEquals($"Collection {(testCase.ExpectedResult > 0 ? "" : "not")} must contain elements.", testCase.ExpectedResult, expectedResult.Count);
			}
		}

		public void TestGetTaxConfigurationThatSupportsOrganisationRates_WithoutAnyTaxSystemCodeConfigured()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var expectedResult = helper.GetTaxConfigurationThatSupportsOrganisationRates(Factory, company);
			AssertEquals("Collection must not contain elements.", 0, expectedResult.Count);
		}

		public void TestHasAnyTaxConfigurationThatSupportsOrganisationRates_ThrowsExceptionNullParams()
		{
			AssertExceptionThrown<ArgumentNullException>(() => helper.HasAnyTaxConfigurationThatSupportsOrganisationRates(null, GlbCompany.CurrentCompany));
			AssertExceptionThrown<ArgumentNullException>(() => helper.HasAnyTaxConfigurationThatSupportsOrganisationRates(Factory, null));
		}

		public void TestHasAnyTaxConfigurationThatSupportsOrganisationsRates_WhenTaxFrameworkIsDisabled()
		{
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;

			var expectedCompany = branch1.Company;

			var taxTestHelper = new AccountingTestObjectCreator(Factory);
			taxTestHelper.ConfigureTaxFrameworkAtBranchLevel(branch1, taxRateSource: TaxRateSources.OrganisationOnly.Code, taxConfigIsActive: false, taxSystemCode: "TS1");
			var expectedResult = helper.HasAnyTaxConfigurationThatSupportsOrganisationRates(Factory, expectedCompany);
			Assert("Tax configuration that supports organisation rates should be false.", !expectedResult);

			taxTestHelper.ConfigureTaxFrameworkAtBranchLevel(branch1, taxRateSource: TaxRateSources.OrganisationOnly.Code, taxConfigIsActive: true, taxSystemCode: "TS2");
			expectedResult = helper.HasAnyTaxConfigurationThatSupportsOrganisationRates(Factory, expectedCompany);
			Assert("Tax configuration that supports organisation rates should be true.", expectedResult);
		}

		public void TestHasAnyTaxConfigurationThatSupportsOrganisationsRates_EmptyCollection()
		{
			var expectedCompany = Factory.NewWithValidTestData<GlbCompany>();

			var expectedResult = helper.HasAnyTaxConfigurationThatSupportsOrganisationRates(Factory, expectedCompany);
			Assert("Tax configuration that supports organisation rates should be false.", !expectedResult);

			var taxTestHelper = new AccountingTestObjectCreator(Factory);
			taxTestHelper.ConfigureTaxFrameworkAtCompanyLevel(expectedCompany, taxRateSource: TaxRateSources.OrganisationOnly.Code);
			expectedResult = helper.HasAnyTaxConfigurationThatSupportsOrganisationRates(Factory, expectedCompany);
			Assert("Tax configuration that supports organisation rates should be true.", expectedResult);
		}

		public void TestHasAnyActiveAccTaxConfigurationRecieivables_WhenActiveTaxConfigForReceivablesExists()
		{
			AssertHasAnyActiveAccTaxConfiguration(LedgerTypes.AccountsReceivable, true);
		}

		public void TestHasAnyActiveAccTaxConfigurationPayables_WhenActiveTaxConfigForPayablesExists()
		{
			AssertHasAnyActiveAccTaxConfiguration(LedgerTypes.AccountsPayable, true);
		}

		void AssertHasAnyActiveAccTaxConfiguration(ZString ledgerType, ZBool expectedHasAnyActiveAccTaxConfigurationValue)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			CombineAssertions(() =>
			{
				AssertEquals(false, helper.HasAnyActiveAccTaxConfiguration(Factory, org.CompanyData.Company, ledgerType));

				var collection = new AccTaxConfigurationCollectionForCompanyOrBranch(org.CompanyData.Company);
				var config1 = collection.AddNew();
				config1.ETC_Ledger = ledgerType;
				config1.ETC_IsActive = false;

				AssertEquals(false, helper.HasAnyActiveAccTaxConfiguration(Factory, org.CompanyData.Company, ledgerType));

				var config2 = collection.AddNew();
				config2.ETC_Ledger = ledgerType;
				config2.ETC_IsActive = true;

				AssertEquals(expectedHasAnyActiveAccTaxConfigurationValue, helper.HasAnyActiveAccTaxConfiguration(Factory, org.CompanyData.Company, ledgerType));
			});
		}

		public void TestIsCompanyLevelTaxSystemConfigured()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;

			var testObjectCreator = new AccountingTestObjectCreator(Factory);
			var taxSystem1 = testObjectCreator.CreateTaxSystem("SPR1", registrationLevel: TaxSystemRegistrationLevels.Company.Code, country: CountryCodes.India);
			var taxSystem2 = testObjectCreator.CreateTaxSystem("SPR2", registrationLevel: TaxSystemRegistrationLevels.Branch.Code, country: CountryCodes.India);
			var taxSystem3 = testObjectCreator.CreateTaxSystem("SPR3", registrationLevel: TaxSystemRegistrationLevels.Company.Code, country: CountryCodes.NewZealand);
			var taxSystem4 = testObjectCreator.CreateTaxSystem("SPR4", registrationLevel: TaxSystemRegistrationLevels.Branch.Code, country: CountryCodes.NewZealand);

			AssertIsCorrectRegistrationLevelTaxSystemConfigured(() => { return helper.IsCompanyLevelTaxSystemConfigured(company); },
				new TaxSystemsConfiguration[] { taxSystem1, taxSystem2, taxSystem3, taxSystem4 }, taxSystem3);
		}

		public void TestIsBranchLevelTaxSystemConfigured()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			GlbBranch branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.Company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.India;

			var testObjectCreator = new AccountingTestObjectCreator(Factory);
			var taxSystem1 = testObjectCreator.CreateTaxSystem("SPR1", registrationLevel: TaxSystemRegistrationLevels.Company.Code, country: CountryCodes.India);
			var taxSystem2 = testObjectCreator.CreateTaxSystem("SPR2", registrationLevel: TaxSystemRegistrationLevels.Branch.Code, country: CountryCodes.India);
			var taxSystem3 = testObjectCreator.CreateTaxSystem("SPR3", registrationLevel: TaxSystemRegistrationLevels.Branch.Code, country: CountryCodes.NewZealand);
			var taxSystem4 = testObjectCreator.CreateTaxSystem("SPR4", registrationLevel: TaxSystemRegistrationLevels.Company.Code, country: CountryCodes.NewZealand);

			AssertIsCorrectRegistrationLevelTaxSystemConfigured(() => { return helper.IsBranchLevelTaxSystemConfigured(branch); },
				new TaxSystemsConfiguration[] { taxSystem1, taxSystem2, taxSystem3, taxSystem4 }, taxSystem3);
		}

		void AssertIsCorrectRegistrationLevelTaxSystemConfigured(Func<ZBool> testMethod, TaxSystemsConfiguration[] taxSystems, TaxSystemsConfiguration matchingTaxSystem)
		{
			AssertEquals("When tax systems collection is empty", false, testMethod());

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.AddRange(taxSystems);

			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			AssertEquals("When matching tax system exists", true, testMethod());

			taxSystemsConfigCollection.Remove(matchingTaxSystem);
			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
			AssertEquals("When a matching tax system does not exist", false, testMethod());
		}

		void CreateTaxAuthorities()
		{
			var taxAuthority_BRNAT = TestObjectCreator.CreateTaxAuthority("BRNAT", "BR NATIONAL - NAT1", CountryCodes.Brazil, TaxAuthorityTypeList.National.Code);
			var taxAuthority_BRMUN1 = TestObjectCreator.CreateTaxAuthority("BRM1", "BR MUNICIPAL - MUN1", CountryCodes.Brazil, TaxAuthorityTypeList.Municipal.Code);
			var taxAuthority_BRMUN2 = TestObjectCreator.CreateTaxAuthority("BRM2", "BR MUNICIPAL - MUN2", CountryCodes.Brazil, TaxAuthorityTypeList.Municipal.Code);
			var taxAuthority_AUNAT = TestObjectCreator.CreateTaxAuthority("AUNAT", "AU NATIONAL - NAT1", CountryCodes.Australia, TaxAuthorityTypeList.State.Code);

			var taxAuthoritiesConfigCollection = new TaxAuthoritiesConfigurationCollection { taxAuthority_BRNAT, taxAuthority_BRMUN1, taxAuthority_BRMUN2, taxAuthority_AUNAT };
			AccountingMasterFilesRegistry.Instance.TaxAuthorities.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxAuthoritiesConfigCollection);
		}

		void CreateTaxSystems()
		{
			var taxSystem1 = TestObjectCreator.CreateTaxSystem("TS1", name: "Tax System1 - AU Company", country: CountryCodes.Australia, registrationLevel: TaxSystemRegistrationLevels.Company.Code);
			var taxSystem2 = TestObjectCreator.CreateTaxSystem("TS2", name: "Tax System2 - AU Company", country: CountryCodes.Australia, registrationLevel: TaxSystemRegistrationLevels.Company.Code);
			var taxSystem3 = TestObjectCreator.CreateTaxSystem("TS3", name: "Tax System3 - AU Branch", country: CountryCodes.Australia, registrationLevel: TaxSystemRegistrationLevels.Branch.Code);
			var taxSystem4 = TestObjectCreator.CreateTaxSystem("TS4", name: "Tax System4 - BR Company", country: CountryCodes.Brazil, registrationLevel: TaxSystemRegistrationLevels.Company.Code);
			var taxSystem5 = TestObjectCreator.CreateTaxSystem("TS5", name: "Tax System5 - BR Branch", country: CountryCodes.Brazil, registrationLevel: TaxSystemRegistrationLevels.Branch.Code);

			var taxSystemsConfigCollection = new TaxSystemsConfigurationCollection();
			taxSystemsConfigCollection.Add(taxSystem1);
			taxSystemsConfigCollection.Add(taxSystem2);
			taxSystemsConfigCollection.Add(taxSystem3);
			taxSystemsConfigCollection.Add(taxSystem4);
			taxSystemsConfigCollection.Add(taxSystem5);

			AccountingMasterFilesRegistry.Instance.TaxSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, taxSystemsConfigCollection);
			taxSystemsConfigCollection.ClearCachedTaxSystemsByCode_ForTestOnly(Factory);
		}

		AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;

		ITaxFrameworkConfigurationHelper helper => ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper();
	}
}
