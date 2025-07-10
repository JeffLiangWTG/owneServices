using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(ManagedAccount))]
	sealed class BrokersAccountTest : RegistryBusinessObjectTemplateTestCase<ManagedAccount>
	{
		public void TestValidateBankAccount()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var coll = new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var account = coll.AddNew();
			account.BankAccount = ZGuid.Empty;
			account.PayerUnitNumber = "456123";
			AssertHasError(account.BankAccountInfo, MandatoryValidation.MustBeEntered + " a bank account.");
			account.BankAccount = bankAccount.PK;
			AssertNoError(account.BankAccountInfo, MandatoryValidation.MustBeEntered + " a bank account.");
		}

		public void TestValidatePayerUnitNo()
		{
			var coll = new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var account = coll.AddNew();
			account.PayerUnitNumber = "";
			AssertHasError(account.PayerUnitNumberInfo, MandatoryValidation.MustBeEntered + " a payer's unit number.");
			account.PayerUnitNumber = "1";
			AssertNoError(account.PayerUnitNumberInfo, MandatoryValidation.MustBeEntered + " a payer's unit number.");
			AssertHasError(account.PayerUnitNumberInfo, ValidationConstants.Statement.PayerUnitNoLength);
			account.PayerUnitNumber = "12345";
			AssertHasError(account.PayerUnitNumberInfo, ValidationConstants.Statement.PayerUnitNoLength);
			account.PayerUnitNumber = "123456";
			AssertNoError(account.PayerUnitNumberInfo, ValidationConstants.Statement.PayerUnitNoLength);
			var customsOrg = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeaderWrapper.New(customsOrg).ZO_PayMethod = ACHPaymentTypeList.Codes.ACHCredit;
			Factory.Save();
			RatingDataRegistry.Instance.CustomsDisbursementCreditor.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsOrg.PK.ToGuid());
			account.PayerUnitNumber = "";
			AssertNoError(account.PayerUnitNumberInfo, MandatoryValidation.MustBeEntered + " a payer's unit number.");
			coll = new BrokersAccountCollection();
			account = coll.AddNew();
			AssertNoExceptionThrown(() => account.RunPreSaveValidation());
			account.PayerUnitNumber = "";
			AssertNoError(account.PayerUnitNumberInfo, MandatoryValidation.MustBeEntered + " a payer's unit number.");
		}

		public void TestValidateDuplicateBankAccountsPerPUN()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var coll = new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var account = coll.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			Assert(!account.PayerUnitNumberInfo.HasError(ManagedAccount.DuplicateBankAccountsForSamePUN));
			var account2 = coll.AddNew();
			account2.PayerUnitNumber = "123456";
			account2.BankAccount = bankAccount2.PK;
			Assert(account2.PayerUnitNumberInfo.HasError(ManagedAccount.DuplicateBankAccountsForSamePUN));
			account2.BankAccount = bankAccount.PK;
			Assert(!account2.PayerUnitNumberInfo.HasError(ManagedAccount.DuplicateBankAccountsForSamePUN));
		}

		public void TestDeleteWillRefreshValidation()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var coll = new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var account = coll.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			Assert(!account.PayerUnitNumberInfo.HasError(ManagedAccount.DuplicateBankAccountsForSamePUN));
			var account2 = coll.AddNew();
			account2.PayerUnitNumber = "123456";
			account2.BankAccount = bankAccount2.PK;
			Assert(account2.PayerUnitNumberInfo.HasError(ManagedAccount.DuplicateBankAccountsForSamePUN));
			coll.RemoveAndDelete(account);
			Assert(!account2.PayerUnitNumberInfo.HasError(ManagedAccount.DuplicateBankAccountsForSamePUN));
		}

		public void TestValidateMultiplePUNsPerBankAccount()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var coll = new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var account = coll.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			account.ClientBranchDesignation = "";
			var account2 = coll.AddNew();
			account2.PayerUnitNumber = "";
			account2.BankAccount = bankAccount.PK;
			account2.ClientBranchDesignation = "20";
			Assert(account2.HasErrors);
		}

		public void TestValidateMultiplePUNsPerClientBranchDesignation()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var coll = new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var account = coll.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			account.ClientBranchDesignation = "01";
			Assert(!account.ClientBranchDesignationInfo.HasError(ManagedAccount.DuplicatePUNsForSameCBD));
			var account2 = coll.AddNew();
			account2.PayerUnitNumber = "";
			account2.BankAccount = bankAccount2.PK;
			account2.ClientBranchDesignation = "01";
			Assert("PUN per CBD is not duplicate", !account2.ClientBranchDesignationInfo.HasError(ManagedAccount.DuplicatePUNsForSameCBD));
			account2.PayerUnitNumber = "123457";
			Assert("PUN per CBD is duplicate", account2.ClientBranchDesignationInfo.HasError(ManagedAccount.DuplicatePUNsForSameCBD));
		}

		public void TestValidateMultiplePUNsForEmptyClientBranchDesignation()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var coll = new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var account = coll.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			account.ClientBranchDesignation = "";
			var account2 = coll.AddNew();
			account2.PayerUnitNumber = "123457";
			account2.BankAccount = bankAccount2.PK;
			account2.ClientBranchDesignation = "";
			Assert(!account2.ClientBranchDesignationInfo.HasError(ManagedAccount.DuplicatePUNsForSameCBD));
		}

		public void TestValidateMultiplePUNsForCreditAccount()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var coll = new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var account = coll.AddNew();
			account.PayerUnitNumber = "";
			account.BankAccount = bankAccount.PK;
			account.ClientBranchDesignation = "20";
			var account2 = coll.AddNew();
			account2.PayerUnitNumber = "123457";
			account2.BankAccount = bankAccount2.PK;
			account2.ClientBranchDesignation = "20";
			Assert(!account2.HasErrors);
		}

		public void TestValidateMultipleRows()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var coll = new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var account = coll.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			account.ClientBranchDesignation = "01";
			Assert(!account.PayerUnitNumberInfo.HasError(ManagedAccount.DuplicateRowsForSamePUNAndCBD));
			Assert(!account.ClientBranchDesignationInfo.HasError(ManagedAccount.DuplicateRowsForSamePUNAndCBD));
			var account2 = coll.AddNew();
			account2.PayerUnitNumber = "123456";
			account2.BankAccount = bankAccount.PK;
			account2.ClientBranchDesignation = "01";
			Assert(account2.PayerUnitNumberInfo.HasError(ManagedAccount.DuplicateRowsForSamePUNAndCBD));
			Assert(account2.ClientBranchDesignationInfo.HasError(ManagedAccount.DuplicateRowsForSamePUNAndCBD));
			account2.ClientBranchDesignation = "";
			Assert(!account2.PayerUnitNumberInfo.HasError(ManagedAccount.DuplicateRowsForSamePUNAndCBD));
			Assert(!account2.ClientBranchDesignationInfo.HasError(ManagedAccount.DuplicateRowsForSamePUNAndCBD));
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BusinessObject GetNewBusinessObject()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var accounts = new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var result = accounts.AddNew();
			result.BankAccount = bankAccount.PK;
			result.PayerUnitNumber = "111111";
			return result;
		}

		protected override ManagedAccount GetBusinessObjectToClone() => (ManagedAccount)GetNewBusinessObject();

		protected override ManagedAccount GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
