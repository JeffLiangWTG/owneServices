using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccTaxOverrideGroupTaxConfigurationPivotCollection))]
	class AccTaxOverrideGroupTaxConfigurationPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<AccTaxOverrideGroupTaxConfigurationPivotCollection>
	{
		protected override AccTaxOverrideGroupTaxConfigurationPivotCollection GetCollectionToTest()
		{
			return new AccTaxOverrideGroupTaxConfigurationPivotCollection(Factory.New<AccTaxOverrideGroup>());
		}

		public void TestInMemoryFilter()
		{
			var ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			var taxConfiguration1 = AccountingTestObjectCreator.CreateTaxConfiguration(ledger);
			var taxConfiguration2 = AccountingTestObjectCreator.CreateTaxConfiguration(ledger);

			var taxOverrideGroup1 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var taxOverrideGroup2 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var collection = new AccTaxOverrideGroupTaxConfigurationPivotCollection(taxOverrideGroup1);

			AssertEquals(0, collection.Count);

			var taxOverrideGroupTaxConfigurationPivot1 = Factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot1.AXP_ETC_TaxConfiguration = taxConfiguration1.PK;
			taxOverrideGroupTaxConfigurationPivot1.AXP_AX_TaxOverrideGroup = taxOverrideGroup1.PK;

			var taxOverrideGroupTaxConfigurationPivot2 = Factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot2.AXP_ETC_TaxConfiguration = taxConfiguration2.PK;
			taxOverrideGroupTaxConfigurationPivot2.AXP_AX_TaxOverrideGroup = taxOverrideGroup2.PK;

			AssertEquals(1, collection.Count);
			AssertCollectionContains(taxOverrideGroupTaxConfigurationPivot1, collection);

			taxOverrideGroupTaxConfigurationPivot2.AXP_AX_TaxOverrideGroup = taxOverrideGroup1.PK;

			AssertEquals(2, collection.Count);
			AssertCollectionContains(taxOverrideGroupTaxConfigurationPivot1, collection);
			AssertCollectionContains(taxOverrideGroupTaxConfigurationPivot2, collection);
		}

		public void TestLoadingFromDb()
		{
			var ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			var taxConfiguration1 = AccountingTestObjectCreator.CreateTaxConfiguration(ledger);
			var taxConfiguration2 = AccountingTestObjectCreator.CreateTaxConfiguration(ledger);

			var taxOverrideGroup1 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var taxOverrideGroup2 = Factory.NewWithValidTestData<AccTaxOverrideGroup>();

			var taxId = Factory.NewWithValidTestData<AccTaxRate>();

			var taxOverrideGroupTaxConfigurationPivot1 = Factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot1.AXP_ETC_TaxConfiguration = taxConfiguration1.PK;
			taxOverrideGroupTaxConfigurationPivot1.AXP_AX_TaxOverrideGroup = taxOverrideGroup1.PK;
			taxOverrideGroupTaxConfigurationPivot1.AXP_AT_TaxID = taxId.PK;

			var taxOverrideGroupTaxConfigurationPivot2 = Factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot2.AXP_ETC_TaxConfiguration = taxConfiguration2.PK;
			taxOverrideGroupTaxConfigurationPivot2.AXP_AX_TaxOverrideGroup = taxOverrideGroup1.PK;
			taxOverrideGroupTaxConfigurationPivot2.AXP_AT_TaxID = taxId.PK;

			var taxOverrideGroupTaxConfigurationPivot3 = Factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot3.AXP_ETC_TaxConfiguration = taxConfiguration1.PK;
			taxOverrideGroupTaxConfigurationPivot3.AXP_AX_TaxOverrideGroup = taxOverrideGroup2.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var taxOverrideGroup1InNewFactory = newFactory.Load<AccTaxOverrideGroup>(taxOverrideGroup1.PK);
			var taxOverrideGroup2InNewFactory = newFactory.Load<AccTaxOverrideGroup>(taxOverrideGroup2.PK);
			var collection1 = new AccTaxOverrideGroupTaxConfigurationPivotCollection(taxOverrideGroup1InNewFactory);
			var collection2 = new AccTaxOverrideGroupTaxConfigurationPivotCollection(taxOverrideGroup2InNewFactory);

			AssertEquals(2, collection1.Count);
			var taxOverrideGroupTaxConfigurationPivot1InNewFactory = newFactory.Load<AccTaxOverrideGroupTaxConfigurationPivot>(taxOverrideGroupTaxConfigurationPivot1.PK);
			var taxOverrideGroupTaxConfigurationPivot2InNewFactory = newFactory.Load<AccTaxOverrideGroupTaxConfigurationPivot>(taxOverrideGroupTaxConfigurationPivot2.PK);
			AssertCollectionContains(taxOverrideGroupTaxConfigurationPivot1InNewFactory, collection1);
			AssertCollectionContains(taxOverrideGroupTaxConfigurationPivot2InNewFactory, collection1);

			AssertEquals(1, collection2.Count);
			var taxOverrideGroupTaxConfigurationPivot3InNewFactory = newFactory.Load<AccTaxOverrideGroupTaxConfigurationPivot>(taxOverrideGroupTaxConfigurationPivot3.PK);
			AssertCollectionContains(taxOverrideGroupTaxConfigurationPivot3InNewFactory, collection2);
		}

		public void TestNoAuditLogsWithAttibute()
		{
			var ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			var taxConfiguration = AccountingTestObjectCreator.CreateTaxConfiguration(ledger);
			var taxOverrideGroup = Factory.NewWithValidTestData<AccTaxOverrideGroup>();
			var taxId = Factory.NewWithValidTestData<AccTaxRate>();
			var taxOverrideGroupTaxConfigurationPivot = Factory.New<AccTaxOverrideGroupTaxConfigurationPivot>();
			taxOverrideGroupTaxConfigurationPivot.AXP_ETC_TaxConfiguration = taxConfiguration.PK;
			taxOverrideGroupTaxConfigurationPivot.AXP_AX_TaxOverrideGroup = taxOverrideGroup.PK;
			taxOverrideGroupTaxConfigurationPivot.AXP_AT_TaxID = taxId.PK;

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, taxOverrideGroupTaxConfigurationPivot.PK);
			AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
			taxOverrideGroupTaxConfigurationPivot.AXP_RateNumerator = 66;
			Factory.Save();
			AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
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
