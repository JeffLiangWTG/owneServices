using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTaxOverrideGroupTaxConfigurationPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTaxIDs()
		{
			var taxID1 = Factory.NewWithValidTestData<AccTaxRate>();
			taxID1.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			taxID1.AT_Code = "AABBCC";
			taxID1.AT_Type = AccTaxRate.Types.NotReportable;
			taxID1.AT_TaxSystemCode = "TESTSYS";

			var taxID2 = Factory.NewWithValidTestData<AccTaxRate>();
			taxID2.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			taxID2.AT_Code = "BBCXYZ";

			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_ParentId = GlbCompany.CurrentCompany.PK;
			taxConfiguration.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;
			taxConfiguration.ETC_TaxSystemCode = "TEST";

			//TaxID3 TaxSystem SAME as TaxConfiguration TaxSystem.
			var taxID3 = Factory.NewWithValidTestData<AccTaxRate>();
			taxID3.AT_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			taxID3.AT_Code = "AABBZZ";
			taxID3.AT_Type = AccTaxRate.Types.NotReportable;
			taxID3.AT_TaxSystemCode = "TEST";

			Factory.Save();

			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup.AX_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			var taxOverrideGroupPivot = taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			var collection = taxOverrideGroupPivot.Lookups.TaxIDs;
			collection.Load();
			Assert("taxID1", collection.Contains(taxID1));
			Assert("taxID2", !collection.Contains(taxID2));

			taxOverrideGroupPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			collection = taxOverrideGroupPivot.Lookups.TaxIDs;
			collection.Load();
			Assert("taxID1", !collection.Contains(taxID1));
			Assert("taxID2", !collection.Contains(taxID2));
			Assert("taxID3", collection.Contains(taxID3));
		}

		public void TestDefaultVATClasses()
		{
			var taxMsg = Factory.NewWithValidTestData<AccInvMsg>();
			taxMsg.A9_Code = "TAXMSG";
			taxMsg.A9_EnglishMsg = "ENG msg";
			taxMsg.A9_LocalMsg = "Local msg";
			taxMsg.A9_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var taxRate = Factory.NewWithValidTestData<AccTaxRate>();
			taxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;
			taxRate.AT_Code = "COD";
			Factory.Save();

			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			taxOverrideGroup.AX_RN_NKCountry = Core.Constants.CountryCodes.UnitedStates;

			var taxOverrideGroupPivot = taxOverrideGroup.TaxOverrideGroupTaxConfigurationPivots.AddNew();
			taxOverrideGroupPivot.AXP_AT_TaxID = taxRate.PK;

			var taxOverrideGroupPivotLookup = new AccTaxOverrideGroupTaxConfigurationPivotLookups(taxOverrideGroupPivot);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				Assert("The TaxMessage should be found if the TaxRate country and message country are the same", taxOverrideGroupPivotLookup.DefaultVATClasses.Contains(taxMsg));
				taxOverrideGroupPivot.AXP_AX_TaxOverrideGroup = ZGuid.Empty;
				Assert("The TaxMessage should not be found if the Tax Override Group is null and the Current Company is different from the message country", !taxOverrideGroupPivotLookup.DefaultVATClasses.Contains(taxMsg));
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Assert("The TaxMessage should be found if the Tax Override Group company is null and the Current Company and message country are the same", taxOverrideGroupPivotLookup.DefaultVATClasses.Contains(taxMsg));
			}
		}

		public void TestCurrentCompanyAndItsBranches()
		{
			var currentCompany = GlbCompany.CurrentCompany;
			var branch1 = currentCompany.Branches.AddNew();
			var branch2 = currentCompany.Branches.AddNew();

			var notCurrentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var notCurrentCompanyBranch1 = notCurrentCompany.Branches.AddNew();

			var taxConfigurationCurrentCompany = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigurationCurrentCompany.ETC_ParentId = currentCompany.PK;
			var taxConfigurationCurrentCompanyBranch1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigurationCurrentCompanyBranch1.ETC_ParentId = branch1.PK;
			var taxConfigurationCurrentCompanyBranch2 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigurationCurrentCompanyBranch2.ETC_ParentId = branch2.PK;
			taxConfigurationCurrentCompanyBranch2.ETC_IsActive = false;
			var taxConfigurationNotCurrentCompany = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigurationNotCurrentCompany.ETC_ParentId = notCurrentCompany.PK;
			var taxConfigurationNotCurrentCompanyBranch1 = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfigurationNotCurrentCompanyBranch1.ETC_ParentId = notCurrentCompanyBranch1.PK;
			Factory.Save();

			var taxOverrideGroupTaxConfigurationPivot = Factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			var taxConfigurationLookups = taxOverrideGroupTaxConfigurationPivot.Lookups.TaxConfigurations;
			AssertEquals(3, taxConfigurationLookups.Count);
			AssertContainsExactElementsInAnyOrder(new[] { taxConfigurationCurrentCompany, taxConfigurationCurrentCompanyBranch1, taxConfigurationCurrentCompanyBranch2 }, taxConfigurationLookups);
		}
	}
}
