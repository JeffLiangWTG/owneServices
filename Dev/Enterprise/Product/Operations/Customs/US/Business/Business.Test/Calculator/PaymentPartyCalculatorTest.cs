using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PaymentPartyCalculatorTest : TestCaseWithFactory
	{
		public void TestCalculatePaymentParty()
		{
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "222222";
			account.BankAccount = bankAccount2.PK;
			account.ClientBranchDesignation = "12";
			USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);

			var calculator = new PaymentPartyCalculator();
			AssertEquals(PaymentPartyList.Codes.Broker, calculator.CalculatePaymentParty(null, "", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, ""));
			AssertEquals(PaymentPartyList.Codes.Broker, calculator.CalculatePaymentParty(GlbCompany.CurrentCompany, "", PaymentTypeList.Codes.BatchedByDailyPrintDateAndFilerCode, ""));
			AssertEquals(PaymentPartyList.Codes.Importer, calculator.CalculatePaymentParty(GlbCompany.CurrentCompany, "111111", "", ""));
			AssertEquals(PaymentPartyList.Codes.Broker, calculator.CalculatePaymentParty(GlbCompany.CurrentCompany, "222222", "", "12"));
			AssertEquals(PaymentPartyList.Codes.Importer, calculator.CalculatePaymentParty(GlbCompany.CurrentCompany, "123456", "", "12"));
		}

		public void TestCalculateBankAccount()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();

			var brokerAccounts = new BrokersAccountCollection();
			var account = brokerAccounts.AddNew();
			account.PayerUnitNumber = "222222";
			account.BankAccount = bankAccount2.PK;
			account.ClientBranchDesignation = "14";
			USCustomsDataRegistry.Instance.BrokersAccounts.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, brokerAccounts);

			var calculator = new BankAccountCalculator();
			AssertEquals(ZGuid.Empty, calculator.CalculateBankAccount(GlbCompany.CurrentCompany, "111111", "14"));
			AssertEquals(bankAccount2.PK, calculator.CalculateBankAccount(GlbCompany.CurrentCompany, "222222", "14"));
		}
	}
}
