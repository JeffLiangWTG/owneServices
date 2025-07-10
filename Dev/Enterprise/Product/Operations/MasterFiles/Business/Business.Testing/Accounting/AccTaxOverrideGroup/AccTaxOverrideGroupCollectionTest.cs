using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTaxOverrideGroupCollection))]
	class AccTaxOverrideGroupCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionLoadsWithFilterParameter()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.SetCountry("AU");
			var ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			var taxConfiguration = AccountingTestObjectCreator.CreateTaxConfiguration(company, ledger);
			var taxOverrideGroup1 = AccountingTestObjectCreator.CreateTaxOverrideGroup(company, null);
			var taxOverrideGroup2 = AccountingTestObjectCreator.CreateTaxOverrideGroup(company, null);
			var taxOverrideGroup3 = AccountingTestObjectCreator.CreateTaxOverrideGroup(company, taxConfiguration);
			taxOverrideGroup3.RunPreSaveValidation();
			Factory.Save();

			var query1 = new ZDBOnlyQuery(typeof(AccTaxOverrideGroup));
			query1.AddSubQuery(new ZDBOnlySubQuery(typeof(AccTaxOverrideGroupTaxConfigurationPivot), AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_AX_TaxOverrideGroup, true), JoinCondition.And);

			var testCollection = new AccTaxOverrideGroupCollection(Factory, company, query1);
			testCollection.Load();
			AssertEquals("AccTaxOverrideGroupCollection count.", 2, testCollection.Count);
			AssertCollectionContains("Only not null groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroup1.PK), testCollection);
			AssertCollectionContains("Only not null groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroup2.PK), testCollection);

			var query2 = new ZDBOnlyQuery(typeof(AccTaxOverrideGroup));
			query2.AddSubQuery(new ZDBOnlySubQuery(typeof(AccTaxOverrideGroupTaxConfigurationPivot), AccTaxOverrideGroupTaxConfigurationPivotSchema.AXP_AX_TaxOverrideGroup), JoinCondition.And);

			testCollection = new AccTaxOverrideGroupCollection(Factory, company, query2);
			testCollection.Load();
			AssertEquals("AccTaxOverrideGroupCollection count.", 1, testCollection.Count);
			AssertCollectionContains("Only null groups must be in collection.", new ZQuery(AccTaxOverrideGroupSchema.PK, taxOverrideGroup3.PK), testCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new AccTaxOverrideGroupCollection(Factory);
		}

		protected AccountingTestObjectCreator AccountingTestObjectCreator
		{
			get
			{
				return accountingTestObjectCreator ?? (accountingTestObjectCreator = new AccountingTestObjectCreator(Factory));
			}
		}
		AccountingTestObjectCreator accountingTestObjectCreator;
	}
}
