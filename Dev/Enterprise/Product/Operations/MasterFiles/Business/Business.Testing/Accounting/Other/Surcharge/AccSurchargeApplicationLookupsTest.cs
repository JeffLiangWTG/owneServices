using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccSurchargeApplicationLookupsTest : BusinessObjectLookupsTestCase
	{
		#region Tax Rates

		public void TestTaxRates_CompanyIsNotSetYet()
		{
			var surchargeApplication = Factory.New<AccSurchargeApplication>();
			surchargeApplication.ASP_GC_Company = ZGuid.Empty;

			var taxRates = new AccSurchargeApplicationLookups(surchargeApplication).TaxRates;
			AssertNoExceptionThrown(() => taxRates.Load());
		}

		public void TestTaxRates_VATTaxForCurrentCountry()
		{
			var taxID = CreateTaxIDForTaxRatesLookup(AccountingTestObjectCreator);

			var taxRates = new AccSurchargeApplicationLookups(CreateAccSurchargeApplication(Factory)).TaxRates;
			taxRates.Load();

			AssertCollectionContains("Does collection contain tax ID?", taxID, taxRates);
		}

		public void TestTaxRates_VATTaxForNonCurrentCountry()
		{
			var taxID = CreateTaxIDForTaxRatesLookup(AccountingTestObjectCreator);
			taxID.AT_RN_NKCountry = CountryCodes.NewZealand;
			AssertNotEquals("Precondition: taxRate country", GetDefaultCountryCode(), taxID.AT_RN_NKCountry);

			var taxRates = new AccSurchargeApplicationLookups(CreateAccSurchargeApplication(Factory)).TaxRates;
			taxRates.Load();

			AssertCollectionNotContains("Does collection contain tax ID?", taxID, taxRates);
		}

		public void TestTaxRates_TaxFrameworkTaxID()
		{
			var taxID = CreateTaxIDForTaxRatesLookup(AccountingTestObjectCreator);
			taxID.AT_TaxSystemCode = "OTHERTAX";
			AssertNotEquals("Precondition: taxRate is Tax Framework tax ID", ZString.Empty, taxID.AT_TaxSystemCode);

			var taxRates = new AccSurchargeApplicationLookups(CreateAccSurchargeApplication(Factory)).TaxRates;
			taxRates.Load();

			AssertCollectionNotContains("Does collection contain tax ID?", taxID, taxRates);
		}

		internal static AccTaxRate CreateTaxIDForTaxRatesLookup(AccountingTestObjectCreator testObjectCreator)
		{
			var taxID = testObjectCreator.CreateTaxRate("TAX1");
			taxID.AT_RN_NKCountry = GetDefaultCountryCode();
			AssertEquals("Precondition: taxRate country is default one", GetDefaultCountryCode(), taxID.AT_RN_NKCountry);
			AssertEquals("Precondition: taxRate is VAT tax ID", ZString.Empty, taxID.AT_TaxSystemCode);

			var taxRates = new AccSurchargeApplicationLookups(CreateAccSurchargeApplication(testObjectCreator.Factory)).TaxRates;
			taxRates.Load();

			AssertCollectionContains("Precondition: does collection contain tax ID?", taxID, taxRates);

			return taxID;
		}

		#endregion

		#region Job Types

		public void TestJobTypes()
		{
			AssertNotNull(new AccSurchargeApplicationLookups(Factory.New<AccSurchargeApplication>()).JobTypes);
		}

		#endregion

		#region Supply Types

		public void TestSupplyTypes()
		{
			AssertNotNull(new AccSurchargeApplicationLookups(Factory.New<AccSurchargeApplication>()).SupplyTypes);
		}

		#endregion

		#region Locations

		public void TestLocations()
		{
			var surchargeApplicationLookpus = new AccSurchargeApplicationLookups(Factory.New<AccSurchargeApplication>());
			AssertNotNull(surchargeApplicationLookpus.Locations);

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(false, surchargeApplicationLookpus.Locations.ContainsCode(AccChargeTaxOverride.SameCountryAndStateAsLineBranch));
			AssertEquals(false, surchargeApplicationLookpus.Locations.ContainsCode(AccChargeTaxOverride.SameCountryDifferentStateAsLineBranch));

			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(false, surchargeApplicationLookpus.Locations.ContainsCode(AccChargeTaxOverride.SameCountryAndStateAsLineBranch));
			AssertEquals(false, surchargeApplicationLookpus.Locations.ContainsCode(AccChargeTaxOverride.SameCountryDifferentStateAsLineBranch));
		}

		#endregion

		#region OrganisationCategoryList

		public void TestOrganisationCategoryList()
		{
			AssertNotNull(new AccSurchargeApplicationLookups(Factory.New<AccSurchargeApplication>()).OrganisationCategoryList);
		}

		#endregion

		#region SurchargeCodes

		public void TestSurchargeCodes()
		{
			var surcharge1 = Factory.NewWithValidTestData<AccSurchargeConfiguration>();
			surcharge1.ASC_Code = "AAA";
			surcharge1.ASC_GC_Company = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var surchargeApplication = Factory.New<AccSurchargeApplication>();
			surchargeApplication.ASP_GC_Company = GlbCompany.CurrentCompany.PK;

			var surchargeCodes = new AccSurchargeApplicationLookups(surchargeApplication).SurchargeCodes;
			AssertNotNull(surchargeCodes);
			AssertEquals(1, surchargeCodes.Count);
		}

		#endregion

		AccountingTestObjectCreator AccountingTestObjectCreator => accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator accountingTestObjectCreator;

		internal static AccSurchargeApplication CreateAccSurchargeApplication(BusinessObjectFactory factory)
		{
			var company = factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = GetDefaultCountryCode();
			var surchargeApplication = factory.New<AccSurchargeApplication>();
			surchargeApplication.ASP_GC_Company = company.PK;

			return surchargeApplication;
		}

		internal static ZString GetDefaultCountryCode()
		{
			var countryCode = CountryCodes.Pitcairn;
			AssertNotEquals("Precondition: default country code is not current company country", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, countryCode);

			return countryCode;
		}
	}
}
