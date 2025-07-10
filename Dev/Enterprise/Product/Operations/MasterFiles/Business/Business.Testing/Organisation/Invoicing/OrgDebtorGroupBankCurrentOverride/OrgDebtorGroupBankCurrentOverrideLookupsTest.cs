using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgDebtorGroupBankCurrentOverrideLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCurrencyList()
		{
			OrgDebtorGroupBankCurrentOverride parent = Factory.NewWithValidTestData<OrgDebtorGroupBankCurrentOverride>();
			OrgDebtorGroupBankCurrentOverrideLookups lookups = new OrgDebtorGroupBankCurrentOverrideLookups(parent);
			AssertNotNull("Currency List is not null", lookups.CurrencyList);
		}

		public void TestBankAccountList()
		{
			OrgDebtorGroupBankCurrentOverride parent = Factory.NewWithValidTestData<OrgDebtorGroupBankCurrentOverride>();
			OrgDebtorGroupBankCurrentOverrideLookups lookups = new OrgDebtorGroupBankCurrentOverrideLookups(parent);
			AssertNotNull("Bank Account List is not null", lookups.BankAccountList);
		}
	}
}
