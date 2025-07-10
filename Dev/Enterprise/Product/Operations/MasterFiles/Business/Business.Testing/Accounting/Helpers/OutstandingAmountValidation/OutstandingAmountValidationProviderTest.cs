using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing.Accounting.Helpers
{
	sealed class OutstandingAmountValidationProviderTest : TestCaseWithFactory
	{
		public void TestAllTransactions()
		{
			var allLedgerTypes = new LedgerTypesList().GetAllCodes();
			var ledgerTransactionTypeCompatibilityMatrix = new AccTransactionHeaderCompatibilityMatrixTestHelper().LedgerTransactionTypesCompatibilityMatrix;
			foreach (var ledgerType in allLedgerTypes)
			{
				foreach (var transactionType in ledgerTransactionTypeCompatibilityMatrix[ledgerType])
				{
					var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
					transactionHeader.AH_InvoiceAmount = 10m;
					transactionHeader.AH_OutstandingAmount = -9m;
					transactionHeader.AH_Ledger = ledgerType;
					transactionHeader.AH_TransactionType = transactionType;

					var runner = new OutstandingAmountValidationProvider(transactionHeader);
					AssertEquals("Outstanding Amount is incorrect: outstanding amount = -9, local total amount = 10", runner.ValidateLocal());
				}
			}
		}

		public void TestValidation_ForARAP()
		{
			var transactionCreator = ObjectFactory.Get<ITransactionCreator>();

			var arInv = transactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			arInv.MakeOSOutstandingAmountApplicable(10m);
			var runnerForARInv = new OutstandingAmountValidationProvider(arInv);
			AssertEquals("OS Outstanding Amount is incorrect: os outstanding amount = 10, os total amount = 100, sum ap os amount = 0", runnerForARInv.ValidateOverseas());

			var apInv = transactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsPayable, TransactionTypes.Invoice);
			apInv.MakeOSOutstandingAmountApplicable(-10m);
			var runnerForAPInv = new OutstandingAmountValidationProvider(apInv);
			AssertEquals("OS Outstanding Amount is incorrect: os outstanding amount = -10, os total amount = -100, sum ap os amount = 0", runnerForAPInv.ValidateOverseas());
		}

		public void TestValidation_ForOpeningReceiptAndOpeningPayment()
		{
			var creator = ObjectFactory.Get<ITransactionCreator>();

			var openingReceipt = creator.CreateTransaction(Factory, LedgerTypes.CashBook, TransactionTypes.OpeningReceipt);
			openingReceipt.AH_OSOutstandingAmount = 101m;
			openingReceipt.AH_IsOSOutstandingAmountApplicable = true;
			var runnerForOpeningReceipt = new OutstandingAmountValidationProvider(openingReceipt);
			AssertEquals("OS Outstanding Amount is incorrect: os outstanding amount = 101, os total amount = -100", runnerForOpeningReceipt.ValidateOverseas());

			var openingPayment = creator.CreateTransaction(Factory, LedgerTypes.CashBook, TransactionTypes.OpeningPayment);
			openingPayment.AH_OSOutstandingAmount = 101m;
			openingPayment.AH_IsOSOutstandingAmountApplicable = true;
			var runnerForOpeningPayment = new OutstandingAmountValidationProvider(openingPayment);
			AssertEquals("OS Outstanding Amount is incorrect: os outstanding amount = 101, os total amount = 100", runnerForOpeningPayment.ValidateOverseas());
		}

		public void TestValidateOverseas()
		{
			var transactionCreator = ObjectFactory.Get<ITransactionCreator>();
			var transaction = transactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			transaction.MakeOSOutstandingAmountApplicable(10m);
			var runner = new OutstandingAmountValidationProvider(transaction);
			AssertEquals("OS Outstanding Amount is incorrect: os outstanding amount = 10, os total amount = 100, sum ap os amount = 0", runner.ValidateOverseas());
		}

		public void TestValidateLocal()
		{
			var transactionCreator = ObjectFactory.Get<ITransactionCreator>();
			var transaction = transactionCreator.CreateTransaction(Factory, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice);
			transaction.AH_OutstandingAmount = 10m;
			var runner = new OutstandingAmountValidationProvider(transaction);
			AssertEquals("Outstanding Amount is incorrect: outstanding amount = 10, local total amount = 100, sum ap amount = 0", runner.ValidateLocal());
		}
	}
}
