using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccountDetailsDependentCollection))]
	sealed class AccountDetailsDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgCompanyData companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			return new AccountDetailsDependentCollection(companyData, Factory);
		}

		public void TestAccountDetailsCollection()
		{
			var collection = (AccountDetailsDependentCollection)GetCollectionToTest();
			var accDetails = Factory.New<AccAPAccountDetails>();
			accDetails.A1_OB = collection.Master.PK;
			var accDetails1 = Factory.New<AccARAccountDetails>();
			accDetails1.A1_OB = collection.Master.PK;
			accDetails1.A1_PaymentMethod = "TAX";
			var accDetails2 = Factory.New<AccARAccountDetails>();
			accDetails2.A1_OB = collection.Master.PK;
			accDetails2.A1_PaymentMethod = "CRQ";
			collection.Load();
			AssertEquals("Should be one Account in the collection", 1, collection.Count);
			AssertEquals(AccAPAccountDetailsLookups.DefaultPayment, collection[0].A1_PaymentMethod);
		}

		public void TestGetAccountDetails()
		{
			AccountDetailsDependentCollection collection = (AccountDetailsDependentCollection)GetCollectionToTest();

			AccAPAccountDetails accDetails1 = collection.AddNew();
			accDetails1.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			accDetails1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accDetails1.A1_IsDefaultAccount = true;

			AccAPAccountDetails accDetails2 = collection.AddNew();
			accDetails2.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			accDetails2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accDetails2.A1_IsDefaultAccount = false;

			AccAPAccountDetails accDetails3 = collection.AddNew();
			accDetails3.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accDetails3.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.NewCaledonia;
			accDetails3.A1_IsDefaultAccount = false;

			AssertNull("Should return null - No account for Bahamas", collection.GetAccountDetails(ZArchitecture.Core.ReceiptTypes.DirectDebit, Core.Constants.CurrencyCodes.Bahamas));
			AccAPAccountDetails result = collection.GetAccountDetails(ZArchitecture.Core.ReceiptTypes.Cheque, Core.Constants.CurrencyCodes.Australia);
			AssertEquals("Should return AccDetails1, because it is marked as Default", accDetails1.PK, result.PK);

			result = collection.GetAccountDetails(ZArchitecture.Core.ReceiptTypes.DirectDebit, Core.Constants.CurrencyCodes.NewCaledonia);
			AssertEquals("Should return AccDetails3, its not default, but its the first that matches", accDetails3.PK, result.PK);
		}

		public void TestGetAutoDirectDebitAccount()
		{
			AccountDetailsDependentCollection collection = (AccountDetailsDependentCollection)GetCollectionToTest();

			AccAPAccountDetails accDetails1 = collection.AddNew();
			accDetails1.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			accDetails1.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accDetails1.A1_IsDefaultAccount = true;
			AssertNull("Should be null because payment is not DDR", collection.GetAutoDirectDebitAccount(Core.Constants.CurrencyCodes.Australia));

			accDetails1.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accDetails1.A1_IsDefaultAccount = false;
			AssertNull("Should be null because IsDefault is false", collection.GetAutoDirectDebitAccount(Core.Constants.CurrencyCodes.Australia));

			accDetails1.A1_IsDefaultAccount = true;

			AccAPAccountDetails accDetails2 = collection.AddNew();
			accDetails2.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accDetails2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Aruba;
			accDetails2.A1_IsDefaultAccount = true;

			AssertEquals("Should return AccDetails1, it should match on AUD currency", accDetails1.PK, collection.GetAutoDirectDebitAccount(Core.Constants.CurrencyCodes.Australia).PK);
		}
	}
}
