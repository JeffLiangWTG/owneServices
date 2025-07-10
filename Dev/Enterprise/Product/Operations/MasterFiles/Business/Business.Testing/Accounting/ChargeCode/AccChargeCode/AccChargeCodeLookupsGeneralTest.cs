using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AccChargeCodeLookupsGeneralTest : TestCaseWithFactory
	{
		public void TestTaxOverrideGroupCollectionWithFilterParameter()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.SetCountry("AU");

			var chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_GC = company.PK;
			var ledger = AccountingMasterFilesTaxFrameworkConstants.TaxConfigurationLedgers.AccountsReceivable.Code;
			var taxConfiguration = AccountingTestObjectCreator.CreateTaxConfiguration(company, ledger);
			var taxOverrideGroupWithTaxConfiguration = AccountingTestObjectCreator.CreateTaxOverrideGroup(company, taxConfiguration);
			var taxOverrideGroupWithOutTaxConfiguration = AccountingTestObjectCreator.CreateTaxOverrideGroup(company, null);
			Factory.Save();
			var lookups = new AccChargeCodeLookups(chargeCode);

			var lookupFilter = chargeCode.Lookups.TaxOverrideGroups.CompleteFilter;
			var taxOverrideGroupCollection = Factory.Load<AccTaxOverrideGroup>(lookupFilter);

			AssertCollectionContains(taxOverrideGroupWithOutTaxConfiguration, taxOverrideGroupCollection);
			AssertCollectionNotContains(taxOverrideGroupWithTaxConfiguration, taxOverrideGroupCollection);
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
