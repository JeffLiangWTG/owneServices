using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;

namespace Enterprise.MasterFiles.Business.Testing
{
	class AccBankAccountLookupsTest : BusinessObjectLookupsTestCase
	{
		protected AccBankAccountLookups Lookups => lookups ?? (lookups = new AccBankAccountLookups(Factory.New<AccBankAccount>()));
		AccBankAccountLookups lookups;

		public void TestProviderCodeList()
		{
			AssertEquals(1, Lookups.PaymentProviderCodeList.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "OFX" }, Lookups.PaymentProviderCodeList.GetAllCodes());
		}

		public void TestAccountTypesListForBankAccount()
		{
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				AssertEquals(4, Lookups.AccountTypes.Count);
				AssertContainsExactElementsInAnyOrder(new string[] { "BNK", "CCD", "EPA", "LNK" }, Lookups.AccountTypes.GetAllCodes());
			}

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, false))
			{
				AssertEquals(3, Lookups.AccountTypes.Count);
				AssertContainsExactElementsInAnyOrder(new string[] { "BNK", "CCD", "LNK" }, Lookups.AccountTypes.GetAllCodes());
			}
		}

		public void TestAccountTypesListForCashAccount()
		{
			var bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			var lookups = new AccBankAccountLookups(bankAccount);
			AssertEquals(1, lookups.AccountTypes.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "CSH" }, lookups.AccountTypes.GetAllCodes());

			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.BNK;
			AssertEquals(4, lookups.AccountTypes.Count);
			AssertContainsExactElementsInAnyOrder(new string[] { "BNK", "CCD", "EPA", "LNK" }, lookups.AccountTypes.GetAllCodes());
		}

		public void TestGetBankAccountTypesList()
		{
			var result = AccBankAccountLookups.GetBankAccountTypesList();
			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				AssertEquals(5, result.Count);
				AssertContainsExactElementsInAnyOrder(new string[] { "BNK", "CCD", "EPA", "LNK", "CSH" }, result.GetAllCodes());

				result = AccBankAccountLookups.GetBankAccountTypesList(accountType: "ANY");
				AssertEquals(4, result.Count);
				AssertContainsExactElementsInAnyOrder(new string[] { "BNK", "CCD", "EPA", "LNK" }, result.GetAllCodes());

				result = AccBankAccountLookups.GetBankAccountTypesList(accountType: AccountTypeCodeDescriptionPairList.Codes.CSH);
				AssertEquals(1, result.Count);
				AssertContainsExactElementsInAnyOrder(new string[] { "CSH" }, result.GetAllCodes());
			}

			using (ObjectFactory.Get<IAccounting>().SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, false))
			{
				result = AccBankAccountLookups.GetBankAccountTypesList();
				AssertEquals(4, result.Count);
				AssertContainsExactElementsInAnyOrder(new string[] { "BNK", "CCD", "LNK", "CSH" }, result.GetAllCodes());

				result = AccBankAccountLookups.GetBankAccountTypesList(accountType: "ANY");
				AssertEquals(3, result.Count);
				AssertContainsExactElementsInAnyOrder(new string[] { "BNK", "CCD", "LNK" }, result.GetAllCodes());
			}
		}
	}
}
