using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTaxOverrideGroupCollectionCompany))]
	sealed class AccTaxOverrideGroupCollectionCompanyTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionLoadsCompaniesCountryGroups()
		{
			var companyAr1 = Factory.NewWithValidTestData<GlbCompany>();
			companyAr1.SetCountry(Core.Constants.CountryCodes.Argentina);
			var branch1Ar1 = companyAr1.Branches.AddNew();
			branch1Ar1.GB_Code = "CB1";
			var branch2Ar1 = companyAr1.Branches.AddNew();
			branch2Ar1.GB_Code = "CB2";

			var companyAr2 = Factory.NewWithValidTestData<GlbCompany>();
			companyAr2.SetCountry(Core.Constants.CountryCodes.Argentina);

			var companyUr = Factory.NewWithValidTestData<GlbCompany>();
			companyUr.SetCountry(Core.Constants.CountryCodes.Uruguay);

			Factory.Save();
			var ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			var taxConfigurationCompany1 = AccountingTestObjectCreator.CreateTaxConfiguration(companyAr1, ledger);
			var taxConfigurationBranch1Ar1 = AccountingTestObjectCreator.CreateTaxConfiguration(branch1Ar1, ledger);
			var taxConfigurationBranch2A1 = AccountingTestObjectCreator.CreateTaxConfiguration(branch2Ar1, ledger);
			var taxOverrideGroupCompany1 = AccountingTestObjectCreator.CreateTaxOverrideGroup(companyAr1, taxConfigurationCompany1);
			var taxOverrideGroup2Company1 = AccountingTestObjectCreator.CreateTaxOverrideGroup(companyAr1, null);
			var taxOverrideGroupBranch1Ar1 = AccountingTestObjectCreator.CreateTaxOverrideGroup(companyAr1, taxConfigurationBranch1Ar1);
			var taxOverrideGroupBranch2Ar1 = AccountingTestObjectCreator.CreateTaxOverrideGroup(companyAr1, taxConfigurationBranch2A1);
			var taxConfigurationCompany2 = AccountingTestObjectCreator.CreateTaxConfiguration(companyAr2, ledger);
			var taxOverrideGroupCompany2 = AccountingTestObjectCreator.CreateTaxOverrideGroup(companyAr2, taxConfigurationCompany2);

			var taxConfigurationCompanyUr = AccountingTestObjectCreator.CreateTaxConfiguration(companyUr, ledger);
			var taxOverrideGroupCompanyUr = AccountingTestObjectCreator.CreateTaxOverrideGroup(companyUr, taxConfigurationCompanyUr);
			Factory.Save();

			var testCollection = GetCollectionToTest(companyAr1);
			testCollection.Load();
			AssertEquals("AccTaxOverrideGroupCollectionCompany Ar1 count.", 4, testCollection.Count);
			AssertCollectionContains("Only Ar1 groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroupCompany1.PK), testCollection);
			AssertCollectionContains("Only Ar1 groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroup2Company1.PK), testCollection);
			AssertCollectionContains("Only Ar1 groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroupBranch1Ar1.PK), testCollection);
			AssertCollectionContains("Only Ar1 groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroupBranch2Ar1.PK), testCollection);

			testCollection = GetCollectionToTest(companyAr2);
			testCollection.Load();
			AssertEquals("AccTaxOverrideGroupCollectionCompany Ar2 count.", 2, testCollection.Count);
			AssertCollectionContains("Only Ar2 groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroup2Company1.PK), testCollection);
			AssertCollectionContains("Only Ar2 groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroupCompany2.PK), testCollection);

			companyAr1.Branches.Delete(branch1Ar1);
			testCollection = GetCollectionToTest(companyAr1);
			testCollection.Load();
			AssertEquals("AccTaxOverrideGroupCollectionCompany Ar1 count.", 3, testCollection.Count);
			AssertCollectionContains("Only Ar1 groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroupCompany1.PK), testCollection);
			AssertCollectionContains("Only Ar1 groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroup2Company1.PK), testCollection);
			AssertCollectionContains("Only Ar1 groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroupBranch2Ar1.PK), testCollection);

			taxOverrideGroupCompany1.Delete();
			taxOverrideGroupBranch1Ar1.Delete();
			taxOverrideGroupBranch2Ar1.Delete();
			companyAr1.Branches.Delete(branch2Ar1);
			testCollection = GetCollectionToTest(companyAr1);
			testCollection.Load();
			AssertEquals("AccTaxOverrideGroupCollectionCompany Ar1 count.", 1, testCollection.Count);
			AssertCollectionContains("Only Ar1 groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroup2Company1.PK), testCollection);

			taxOverrideGroup2Company1.Delete();
			testCollection = GetCollectionToTest(companyAr1);
			testCollection.Load();
			AssertEquals("AccTaxOverrideGroupCollectionCompany Ar1 count.", 0, testCollection.Count);

			testCollection = GetCollectionToTest(companyAr2);
			testCollection.Load();
			AssertEquals("AccTaxOverrideGroupCollectionCompany Ar2 count.", 1, testCollection.Count);
			AssertCollectionContains("Only Ar2 groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroupCompany2.PK), testCollection);

			taxOverrideGroupCompany2.Delete();
			testCollection = GetCollectionToTest(companyAr2);
			testCollection.Load();
			AssertEquals("AccTaxOverrideGroupCollectionCompany Ar2 count.", 0, testCollection.Count);

			testCollection = GetCollectionToTest(companyUr);
			testCollection.Load();
			AssertEquals("AccTaxOverrideGroupCollectionCompany Uru count.", 1, testCollection.Count);
			AssertCollectionContains("Only Uru groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroupCompanyUr.PK), testCollection);

			GlbCompany uSCompany = Factory.NewWithValidTestData<GlbCompany>();
			uSCompany.SetCountry("US");
			testCollection = GetCollectionToTest(uSCompany);
			testCollection.Load();
			AssertEquals("AccTaxOverrideGroupWithTaxConfigurationCollection US count.", 0, testCollection.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new AccTaxOverrideGroupCollectionCompany(Factory);

		AccTaxOverrideGroupCollectionCompany GetCollectionToTest(GlbCompany company) => new AccTaxOverrideGroupCollectionCompany(Factory, company);

		AccountingTestObjectCreator accountingTestObjectCreator;
		AccountingTestObjectCreator AccountingTestObjectCreator => accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
	}
}
