using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccOrgTaxConfigurationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCorrespondingLedger()
		{
			OrgCompanyData orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();

			AccTaxConfiguration taxConfigurationAR = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigurationAR.ETC_Ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			taxConfigurationAR.ETC_ParentId = orgCompanyData.Company.PK;
			AccTaxConfiguration taxConfigurationAP = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigurationAP.ETC_Ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsPayable.Code;
			taxConfigurationAP.ETC_ParentId = orgCompanyData.Company.PK;
			Factory.Save();

			AccOrgTaxConfiguration orgTaxConfiguration = Factory.New<AccOrgTaxConfiguration>();
			orgTaxConfiguration.Ledger = taxConfigurationAR.ETC_Ledger;
			orgTaxConfiguration.OTC_OB = orgCompanyData.PK;

			var taxConfigurationLookups = orgTaxConfiguration.Lookups.TaxConfigurations;

			AssertEquals(1, taxConfigurationLookups.Count);
			AssertCollectionContains(taxConfigurationAR, taxConfigurationLookups);

			AccOrgTaxConfiguration orgTaxConfigurationAP = Factory.New<AccOrgTaxConfiguration>();
			orgTaxConfigurationAP.Ledger = taxConfigurationAP.ETC_Ledger;
			orgTaxConfigurationAP.OTC_OB = orgCompanyData.PK;

			taxConfigurationLookups = orgTaxConfigurationAP.Lookups.TaxConfigurations;
			AssertEquals(1, taxConfigurationLookups.Count);
			AssertCollectionContains(taxConfigurationAP, taxConfigurationLookups);
		}

		public void TestCorrespondingCompany()
		{
			OrgCompanyData orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();

			GlbCompany companyNotCurrent = Factory.NewWithValidTestData<GlbCompany>();
			AccTaxConfiguration taxConfigurationNotCurrentCompany = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigurationNotCurrentCompany.ETC_ParentId = companyNotCurrent.PK;
			AccTaxConfiguration taxConfigurationCurrentCompany = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigurationCurrentCompany.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			orgCompanyData.OB_GC = companyNotCurrent.PK;
			AccOrgTaxConfiguration orgTaxConfigurationNotCurrentCompany = Factory.New<AccOrgTaxConfiguration>();
			orgTaxConfigurationNotCurrentCompany.Ledger = taxConfigurationNotCurrentCompany.ETC_Ledger;
			orgTaxConfigurationNotCurrentCompany.OTC_OB = orgCompanyData.PK;
			var taxConfigurationLookups = orgTaxConfigurationNotCurrentCompany.Lookups.TaxConfigurations;
			AssertEquals(1, taxConfigurationLookups.Count);
			AssertCollectionContains(taxConfigurationNotCurrentCompany, taxConfigurationLookups);

			orgCompanyData.OB_GC = GlbCompany.CurrentCompany.PK;
			AccOrgTaxConfiguration orgTaxConfigurationCurrentCompany = Factory.New<AccOrgTaxConfiguration>();
			orgTaxConfigurationCurrentCompany.Ledger = taxConfigurationCurrentCompany.ETC_Ledger;
			orgTaxConfigurationCurrentCompany.OTC_OB = orgCompanyData.PK;
			var taxConfigurationLookupsCurrent = orgTaxConfigurationNotCurrentCompany.Lookups.TaxConfigurations;
			AssertEquals(1, taxConfigurationLookupsCurrent.Count);
			AssertCollectionContains(taxConfigurationCurrentCompany, taxConfigurationLookupsCurrent);
		}

		public void TestEmptyCompany()
		{
			OrgCompanyData orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();

			AccTaxConfiguration taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			orgCompanyData.OB_GC = taxConfiguration.ETC_ParentId;
			AccOrgTaxConfiguration orgTaxConfiguration = Factory.New<AccOrgTaxConfiguration>();
			orgTaxConfiguration.Ledger = taxConfiguration.ETC_Ledger;
			orgTaxConfiguration.OTC_OB = orgCompanyData.PK;

			AssertCollectionContains(taxConfiguration, orgTaxConfiguration.Lookups.TaxConfigurations);

			orgCompanyData.OB_GC = ZGuid.Empty;
			AssertEquals(0, orgTaxConfiguration.Lookups.TaxConfigurations.Count);
		}

		public void TestCorrespondingBranches()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "BR1";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "BR2";
			var branch3 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "BR3";

			var tax1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			tax1.ETC_ParentTableCode = "GB";
			tax1.ETC_ParentId = branch1.PK;
			var tax2 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			tax2.ETC_ParentTableCode = "GB";
			tax2.ETC_ParentId = branch2.PK;
			var tax3 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			tax3.ETC_ParentTableCode = "GB";
			tax3.ETC_ParentId = branch3.PK;
			Factory.Save();

			OrgCompanyData orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_GC = company.PK;
			AccOrgTaxConfiguration orgTaxConfiguration = Factory.New<AccOrgTaxConfiguration>();
			orgTaxConfiguration.Ledger = tax1.ETC_Ledger;
			orgTaxConfiguration.OTC_OB = orgCompanyData.PK;
			var taxConfigurationLookups = orgTaxConfiguration.Lookups.TaxConfigurations;
			AssertEquals(2, taxConfigurationLookups.Count);
			AssertCollectionContains(tax1, taxConfigurationLookups);
			AssertCollectionContains(tax2, taxConfigurationLookups);
		}

		public void TestOnlyActiveBranches()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			var activeBranch = company.Branches.AddNew();
			activeBranch.GB_Code = "BRA";
			activeBranch.GB_IsActive = true;
			var inactiveBranch = company.Branches.AddNew();
			inactiveBranch.GB_Code = "BRI";
			inactiveBranch.GB_IsActive = false;

			var tax1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			tax1.ETC_ParentTableCode = "GB";
			tax1.ETC_ParentId = activeBranch.PK;
			var tax2 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			tax2.ETC_ParentTableCode = "GB";
			tax2.ETC_ParentId = inactiveBranch.PK;
			Factory.Save();

			OrgCompanyData orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_GC = company.PK;
			AccOrgTaxConfiguration orgTaxConfiguration = Factory.New<AccOrgTaxConfiguration>();
			orgTaxConfiguration.Ledger = tax1.ETC_Ledger;
			orgTaxConfiguration.OTC_OB = orgCompanyData.PK;
			var taxConfigurationLookups = orgTaxConfiguration.Lookups.TaxConfigurations;
			AssertEquals(1, taxConfigurationLookups.Count);
			AssertCollectionContains(tax1, taxConfigurationLookups);

			inactiveBranch.GB_IsActive = true;
			taxConfigurationLookups = orgTaxConfiguration.Lookups.TaxConfigurations;
			AssertEquals(2, taxConfigurationLookups.Count);
			AssertCollectionContains(tax2, taxConfigurationLookups);
		}

		public void TestInactiveTaxConfigurationExistsInLookups()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			var taxConfiguration1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			var taxConfiguration2 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration1.ETC_ParentId = company.PK;
			taxConfiguration2.ETC_ParentId = company.PK;

			taxConfiguration1.ETC_IsActive = false;
			Factory.Save();

			OrgCompanyData orgCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			orgCompanyData.OB_GC = company.PK;
			AccOrgTaxConfiguration orgTaxConfig = Factory.New<AccOrgTaxConfiguration>();
			orgTaxConfig.Ledger = taxConfiguration1.ETC_Ledger;
			orgTaxConfig.OTC_OB = orgCompanyData.PK;
			var taxConfigurationLookups = orgTaxConfig.Lookups.TaxConfigurations;
			AssertEquals(2, taxConfigurationLookups.Count);
			AssertContainsExactElementsInAnyOrder(new[] { taxConfiguration1, taxConfiguration2 }, taxConfigurationLookups);
		}
	}
}
