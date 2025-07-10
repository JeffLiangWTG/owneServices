using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccARAccountDetailsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckA1_IsDefaultAccount()
		{
			AssertNoErrors("Should have no errors", AccountDetails.A1_IsDefaultAccountInfo);

			AccountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			AccountDetails.A1_IsDefaultAccount = false;
			AssertHasErrors("Should have errors, must have 1 account details marked as default with the same currency and payment method", AccountDetails.A1_IsDefaultAccountInfo);
			AssertEquals("Expecting One error", 1, AccountDetails.A1_IsDefaultAccountInfo.GetErrors().Count());
			ZString message = "This account must be the default because there is no other account listed with currency \"AUD\" and payment type \"Tax Invoice's Bank Details\" and marked as \"Is Default Account\".";
			AssertEquals("Incorrect Message", message, AccountDetails.A1_IsDefaultAccountInfo.GetErrors().GetFirstMessage());

			var accountDetails2 = Org.CompanyData.ARAccountDetailsCollection.AddNew();
			accountDetails2.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails2.A1_IsDefaultAccount = true;
			AssertNoErrors("Should have no errors", accountDetails2.A1_IsDefaultAccountInfo);

			var accountDetails3 = Org.CompanyData.ARAccountDetailsCollection.AddNew();
			accountDetails3.A1_IsDefaultAccount = true;
			accountDetails3.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Australia;
			accountDetails3.Validation.ValidateA1_IsDefaultAccount();
			AssertHasErrors("Should have errors because more than 1 account details are marked as default with the same currency and payment method", accountDetails3.A1_IsDefaultAccountInfo);
			AssertEquals("Expecting One Error", 1, accountDetails3.A1_IsDefaultAccountInfo.GetErrors().Count());
			message = "You have more than one account listed as the default account for the currency \"AUD\" and the payment type \"Tax Invoice's Bank Details\".\r\n\r\nPlease check which account really should be the default, and untick the \"Is Default Account\" flag of any others.";
			AssertEquals("Expected Message is incorrect", message, accountDetails3.A1_IsDefaultAccountInfo.GetErrors().GetFirstMessage());

			Org.CompanyData.OB_IsDebtor = false;
			foreach (AccARAccountDetails accDetails in Org.CompanyData.ARAccountDetailsCollection)
			{
				accDetails.Validation.ValidateA1_IsDefaultAccount();
				AssertNoErrors("should be no errors because Company is NOT creditor, should not validate", accDetails.A1_IsDefaultAccountInfo);
			}
		}

		public void TestCheckA1_PaymentMethod()
		{
			AccountDetails.A1_PaymentMethod = "";
			AssertHasErrors("Should errors", AccountDetails.A1_PaymentMethodInfo);

			AccountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertHasErrors("Should errors", AccountDetails.A1_PaymentMethodInfo);

			AccountDetails.A1_PaymentMethod = AccARAccountDetails.ARBankAccPayment;
			AssertNoErrors("Should have no errors", AccountDetails.A1_PaymentMethodInfo);
		}

		public void TestCheckA1_BankAccount()
		{
			AccountDetails.A1_BankAccount = "";
			Assert("A1_BankAccount = '', has errors", AccountDetails.A1_BankAccountInfo.HasErrors());
			AccountDetails.A1_BankAccount = "123456789";
			Assert(" A1_BankAccount = '123456789', no errors", !AccountDetails.A1_BankAccountInfo.HasErrors());
		}

		public void TestCheckA1_BankName()
		{
			AccountDetails.A1_BankName = "";
			AccountDetails.Validation.ValidateA1_BankName();
			Assert("Company is creditor, A1_BankName = '', has errors", AccountDetails.A1_BankNameInfo.HasErrors());

			AccountDetails.A1_BankName = "Test Bank";
			Assert("Company is creditor, A1_BankName = 'Test Bank', no notifications", !AccountDetails.A1_BankNameInfo.HasNotifications());
		}

		OrgHeader Org;
		AccARAccountDetails AccountDetails;

		protected override void SetUp()
		{
			base.SetUp();
			Org = Factory.New<OrgHeader>();
			Org.CompanyData.OB_OH = Factory.New(typeof(OrgHeader)).PK;
			Org.CompanyData.OB_IsDebtor = true;

			AccountDetails = Org.CompanyData.ARAccountDetailsCollection.AddNew();
		}
	}
}
