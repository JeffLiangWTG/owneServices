using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgTaxConfigurationModuleFilter))]
	sealed class OrgTaxConfigurationModuleFilterTest : ModuleTextFilterTest
	{
		public void TestOrgTaxConfigurationModuleFilter()
		{
			/*
			 Org1 -> taxConfig1 + taxConfig2 - Inactive
			 Org2 -> taxConfig1
			 Org3 -> [No tax cofig]
			 Org4 -> taxConfig3
			 */

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var taxConfig1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
				taxConfig1.ETC_ParentId = company.PK;

				var taxConfig2 = Factory.NewWithValidTestData<AccTaxConfiguration>();
				taxConfig2.ETC_ParentId = company.PK;

				var taxConfig3 = Factory.NewWithValidTestData<AccTaxConfiguration>();
				taxConfig3.ETC_ParentId = company.PK;

				var org1 = Factory.NewWithValidTestData<OrgHeader>();
				org1.CompanyData.OB_GC = company.PK;

				var org2 = Factory.NewWithValidTestData<OrgHeader>();
				org2.CompanyData.OB_GC = company.PK;

				var org3 = Factory.NewWithValidTestData<OrgHeader>(); //Org3 does not have any AccTaxOrgConfiguration setup at all
				org3.CompanyData.OB_GC = company.PK;

				var org4 = Factory.NewWithValidTestData<OrgHeader>();
				org4.CompanyData.OB_GC = company.PK;

				Factory.Save();

				CreateOrgTaxConfiguration(taxConfig1, org1.CompanyData, active: true);
				CreateOrgTaxConfiguration(taxConfig1, org2.CompanyData, active: false);
				CreateOrgTaxConfiguration(taxConfig2, org1.CompanyData, active: true);
				CreateOrgTaxConfiguration(taxConfig3, org4.CompanyData, active: true);

				Factory.Save();

				var filter = new OrgTaxConfigurationModuleFilter("Test");
				filter.TaxConfiguration = taxConfig2.PK;
				filter.ConfigurationStatus = OrgTaxConfigurationModuleFilter.StatusAll;

				var filteredOrgs = new OrgHeaderCollection(Factory, filter.Query);
				filteredOrgs.Load(filter.Query);
				AssertCollectionContains("should return org1", org1, filteredOrgs);
				AssertCollectionNotContains("should not return org2", org2, filteredOrgs);

				filter.TaxConfiguration = taxConfig2.PK;
				filter.ConfigurationStatus = OrgTaxConfigurationModuleFilter.StatusNotConfigured;
				filteredOrgs.Load(filter.Query);
				AssertCollectionNotContains("should not return org1 since org1 is configured", org1, filteredOrgs);
				AssertCollectionContains("should return org2", org2, filteredOrgs);
				AssertCollectionContains("should return org3, even if it does not have AccTaxOrgConfiguration at all", org3, filteredOrgs);
				AssertCollectionContains("should return org4", org4, filteredOrgs);

				filter.TaxConfiguration = taxConfig1.PK;
				filter.ConfigurationStatus = OrgTaxConfigurationModuleFilter.StatusAll;
				filteredOrgs.Load(filter.Query);
				AssertCollectionContains("should return org1", org1, filteredOrgs);
				AssertCollectionContains("should return org2", org2, filteredOrgs);

				filter.TaxConfiguration = taxConfig1.PK;
				filter.ConfigurationStatus = OrgTaxConfigurationModuleFilter.StatusActive;
				filteredOrgs.Load(filter.Query);
				AssertCollectionContains("should return org1", org1, filteredOrgs);
				AssertCollectionNotContains("should not return org2", org2, filteredOrgs);

				filter.TaxConfiguration = taxConfig1.PK;
				filter.ConfigurationStatus = OrgTaxConfigurationModuleFilter.StatusInactive;
				filteredOrgs.Load(filter.Query);
				AssertCollectionNotContains("should not return org1", org1, filteredOrgs);
				AssertCollectionContains("should return org2", org2, filteredOrgs);

				filter.TaxConfiguration = taxConfig1.PK;
				filter.ConfigurationStatus = OrgTaxConfigurationModuleFilter.StatusNotConfigured;
				filteredOrgs.Load(filter.Query);
				AssertCollectionNotContains("should not return org1", org1, filteredOrgs);
				AssertCollectionNotContains("should not return org2", org2, filteredOrgs);
				AssertCollectionContains("should return org3, even if it does not have AccTaxOrgConfiguration at all", org3, filteredOrgs);
				AssertCollectionContains("should return org4", org4, filteredOrgs);

				filter.TaxConfiguration = taxConfig3.PK;
				filter.ConfigurationStatus = OrgTaxConfigurationModuleFilter.StatusNotConfigured;
				filteredOrgs.Load(filter.Query);
				AssertCollectionContains("should return org1", org1, filteredOrgs);
				AssertCollectionContains("should return org2", org2, filteredOrgs);
				AssertCollectionContains("should return org3, even if it does not have AccTaxOrgConfiguration at all", org3, filteredOrgs);
				AssertCollectionNotContains("should not return org4 since org4 is configured", org4, filteredOrgs);
			}
		}

		public void TestTaxConfigurations()
		{
			var taxConfig1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig1.ETC_ParentId = GlbCompany.CurrentCompany.PK;

			var taxConfig2 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig2.ETC_ParentId = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var filter = new OrgTaxConfigurationModuleFilter("Test");
			AssertEquals(2, filter.TaxConfigurations.Count);

			var pks = filter.TaxConfigurations.Select(x => x.PK).ToList();
			AssertCollectionContains(taxConfig1.PK, pks);
			AssertCollectionContains(taxConfig2.PK, pks);
		}

		public void TestConfigurationStatusOptionsList()
		{
			var filter = new OrgTaxConfigurationModuleFilter("Test");
			AssertEquals(4, filter.ConfigurationStatusOptions.Count);
			var optionCodes = filter.ConfigurationStatusOptions.GetAllCodes();
			AssertCollectionContains(OrgTaxConfigurationModuleFilter.StatusActive, optionCodes);
			AssertCollectionContains(OrgTaxConfigurationModuleFilter.StatusInactive, optionCodes);
			AssertCollectionContains(OrgTaxConfigurationModuleFilter.StatusAll, optionCodes);
			AssertCollectionContains(OrgTaxConfigurationModuleFilter.StatusNotConfigured, optionCodes);
		}

		public void TestClearAndIsEmpty()
		{
			var taxConfig1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig1.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var filter = new OrgTaxConfigurationModuleFilter("Test");

			filter.TaxConfiguration = taxConfig1.PK;
			filter.ConfigurationStatus = OrgTaxConfigurationModuleFilter.StatusActive;
			AssertEquals(false, filter.IsEmpty);
			AssertEquals(false, filter.Query.IsEmpty);

			filter.Clear();
			AssertEquals(true, filter.IsEmpty);
			AssertEquals(true, filter.Query.IsEmpty);

			AssertEquals(ZGuid.Empty, filter.TaxConfiguration);
			AssertEquals("", filter.ConfigurationStatus);
		}

		public void TestSerialize_Deserialize_PropertiesFromToXml()
		{
			var taxConfig1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfig1.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var filter = new OrgTaxConfigurationModuleFilter("Tax Configuration");
			var filterStripBizO = new DummyFilterStripBusinessObject();

			filterStripBizO.AddModuleFilterForTest(filter);

			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = filter.Description;
			filter.TaxConfiguration = taxConfig1.PK;
			filter.ConfigurationStatus = OrgTaxConfigurationModuleFilter.StatusActive;

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "layoutName", false, false, SaveColumnLayout.Ignore);

			strip.FilterDescription = "";
			strip.Delete();

			var loadedFilter = (OrgTaxConfigurationModuleFilter)filterStripBizO[filter.Description];

			filterStripBizO.LoadLayout(savedLayout);

			AssertEquals("Tax Configuration", taxConfig1.PK, loadedFilter.TaxConfiguration);
			AssertEquals("ConfigurationStatus", OrgTaxConfigurationModuleFilter.StatusActive, loadedFilter.ConfigurationStatus);
		}

		public AccOrgTaxConfiguration CreateOrgTaxConfiguration(AccTaxConfiguration taxConfiguration, OrgCompanyData companyData, bool active = true)
		{
			var orgTaxConfiguration = Factory.New<AccOrgTaxConfiguration>();
			orgTaxConfiguration.Ledger = taxConfiguration.ETC_Ledger;
			orgTaxConfiguration.OTC_ETC = taxConfiguration.PK;
			orgTaxConfiguration.OTC_OB = companyData.PK;
			orgTaxConfiguration.OTC_IsActive = active;

			return orgTaxConfiguration;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new OrgTaxConfigurationModuleFilter("Test");
		}
	}
}
