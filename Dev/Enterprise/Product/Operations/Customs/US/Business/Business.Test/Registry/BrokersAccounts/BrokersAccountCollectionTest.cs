using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DataRegistry.Business.Testing
{
	[TestedType(typeof(BrokersAccountCollection))]
	sealed class BrokersAccountCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<BrokersAccountCollection>
	{
		public void TestContainsPayerUnitNo()
		{
			var coll = GetCollectionToTest();
			var account = coll.AddNew();
			account.PayerUnitNumber = "1";
			var account2 = coll.AddNew();
			account2.PayerUnitNumber = "2";
			var account3 = coll.AddNew();
			account3.PayerUnitNumber = "3";
			account3.ClientBranchDesignation = "12";
			Assert(coll.ContainsPayerUnitNo("2"));
			Assert(!coll.ContainsPayerUnitNo("3"));
			Assert(coll.ContainsPayerUnitNo("3", "12"));
		}

		public void TestGetAssociatedBankAccount()
		{
			var coll = GetCollectionToTest();
			var account = coll.AddNew();
			account.PayerUnitNumber = "1";
			account.BankAccount = ZGuid.NewZGuid();
			var account2 = coll.AddNew();
			account2.PayerUnitNumber = "2";
			account2.BankAccount = ZGuid.NewZGuid();
			var account3 = coll.AddNew();
			account3.PayerUnitNumber = "3";
			account3.ClientBranchDesignation = "14";
			AssertEquals(account2.BankAccount, coll.GetAssociatedBankAccount("2", ""));
			AssertEquals(account2.BankAccount, coll.GetAssociatedBankAccount("2", "33"));
			AssertEquals(ZGuid.Empty, coll.GetAssociatedBankAccount("3", ""));
			AssertEquals(account3.BankAccount, coll.GetAssociatedBankAccount("3", "14"));
		}

		public void TestHasDuplicateBankAccountsPerPUN()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var coll = new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var account = coll.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			Assert(!coll.HasMultipleBankAccountsPerPUN("123456"));
			var account2 = coll.AddNew();
			account2.PayerUnitNumber = "123456";
			account2.BankAccount = bankAccount2.PK;
			Assert(coll.HasMultipleBankAccountsPerPUN("123456"));
			account2.BankAccount = bankAccount.PK;
			Assert(!coll.HasMultipleBankAccountsPerPUN("123456"));
		}

		public void TestHasDuplicatePUNsPerBankAccount()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var coll = new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			bool hasEmptyPUN;
			var account = coll.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			Assert(!coll.HasMultiplePUNsPerBankAccount(bankAccount.PK, out hasEmptyPUN));
			Assert(!hasEmptyPUN);
			var account2 = coll.AddNew();
			account2.PayerUnitNumber = "";
			account2.BankAccount = bankAccount.PK;
			Assert(coll.HasMultiplePUNsPerBankAccount(bankAccount.PK, out hasEmptyPUN));
			Assert(hasEmptyPUN);
			account2.BankAccount = bankAccount2.PK;
			Assert(!coll.HasMultiplePUNsPerBankAccount(bankAccount.PK, out hasEmptyPUN));
			Assert(!hasEmptyPUN);
			Assert(!coll.HasMultiplePUNsPerBankAccount(bankAccount2.PK, out hasEmptyPUN));
			Assert(hasEmptyPUN);
		}

		public void TestHasDuplicatePUNsPerClientBranchDesignation()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var coll = new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var account = coll.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			account.ClientBranchDesignation = "01";
			Assert(!coll.HasMultiplePUNsPerClientBranchDesignation("01"));
			var account2 = coll.AddNew();
			account2.PayerUnitNumber = "123457";
			account2.BankAccount = bankAccount2.PK;
			account2.ClientBranchDesignation = "01";
			Assert(coll.HasMultiplePUNsPerClientBranchDesignation("01"));
			account2.ClientBranchDesignation = "02";
			Assert(!coll.HasMultiplePUNsPerClientBranchDesignation("01"));
			Assert(!coll.HasMultiplePUNsPerClientBranchDesignation("02"));
		}

		public void TestHasDuplicatePUNsPerClientBranchDesignation2()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var coll = new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var account = coll.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			account.ClientBranchDesignation = "01";
			Assert(!coll.HasMultiplePUNsPerClientBranchDesignation("01"));
			var account2 = coll.AddNew();
			account2.PayerUnitNumber = "";
			account2.BankAccount = bankAccount2.PK;
			account2.ClientBranchDesignation = "01";
			Assert(!coll.HasMultiplePUNsPerClientBranchDesignation("01"));
		}

		public void TestMultipleRows()
		{
			var bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			var bankAccount2 = Factory.NewWithValidTestData<AccBankAccount>();
			Factory.Save();
			var coll = new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			var account = coll.AddNew();
			account.PayerUnitNumber = "123456";
			account.BankAccount = bankAccount.PK;
			account.ClientBranchDesignation = "01";
			Assert(!coll.HasMultipleRows("123456", "01"));
			var account2 = coll.AddNew();
			account2.PayerUnitNumber = "123456";
			account2.BankAccount = bankAccount2.PK;
			account2.ClientBranchDesignation = "01";
			Assert(coll.HasMultipleRows("123456", "01"));
			account2.ClientBranchDesignation = "";
			Assert(!coll.HasMultipleRows("123456", "01"));
			Assert(!coll.HasMultipleRows("123456", ""));
		}

		public void TestGetPayerUnitNoByBranchDesignation()
		{
			var coll = GetCollectionToTest();
			var account = coll.AddNew();
			account.PayerUnitNumber = "1";
			var account2 = coll.AddNew();
			account2.PayerUnitNumber = "2";
			account2.ClientBranchDesignation = "10";
			var account3 = coll.AddNew();
			account3.PayerUnitNumber = "3";
			account3.ClientBranchDesignation = "14";
			AssertEquals("1", coll.GetPayerUnitNoByBranchDesignation(""));
			AssertEquals("1", coll.GetPayerUnitNoByBranchDesignation("89"));
			AssertEquals("2", coll.GetPayerUnitNoByBranchDesignation("10"));
			AssertEquals("3", coll.GetPayerUnitNoByBranchDesignation("14"));
		}

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override BrokersAccountCollection GetCollectionToTest() => new BrokersAccountCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ManagedAccount();
	}
}
