using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccTransactionHeaderCompatibilityMatrixTestHelper
	{
		public string GetCompatibleLedger(string transactionType)
		{
			return LedgerTransactionTypesCompatibilityMatrix.FirstOrDefault(x => x.Value.Any(y => y == transactionType)).Key;
		}

		public string GetCompatibleTransactionType(string ledger)
		{
			var result = ZString.Empty;
			List<string> compatibleTypes;
			if (LedgerTransactionTypesCompatibilityMatrix.TryGetValue(ledger, out compatibleTypes))
			{
				result = compatibleTypes.First();
			}
			return result;
		}

		public Dictionary<string, List<string>> LedgerTransactionTypesCompatibilityMatrix
		{
			get
			{
				if (ledgerTransactionTypesCompatibilityMatrix == null)
				{
					ledgerTransactionTypesCompatibilityMatrix = InitializeLedgerTransactionTypesMatrix();
				}
				return ledgerTransactionTypesCompatibilityMatrix;
			}
		}

		Dictionary<string, List<string>> ledgerTransactionTypesCompatibilityMatrix;

		Dictionary<string, List<string>> InitializeLedgerTransactionTypesMatrix()
		{
			var validLedgerTransactionTypePairs = new Dictionary<string, List<string>>();

			validLedgerTransactionTypePairs.Add(LedgerTypes.AccountsPayable, new List<string>
		{
			TransactionTypes.Invoice,
			TransactionTypes.AdjustmentNote,
			TransactionTypes.CreditNote,
			TransactionTypes.Contra,
			TransactionTypes.Discount,
			TransactionTypes.ExchangeDifference,
			TransactionTypes.Journal,
			TransactionTypes.Overpayment,
			TransactionTypes.Payment,
			TransactionTypes.Receipt,
			TransactionTypes.Transfer
		});

			validLedgerTransactionTypePairs.Add(LedgerTypes.IncompleteTransactions, new List<string>
		{
			TransactionTypes.IncompleteInvoice,
			TransactionTypes.IncompleteCreditNote,
			TransactionTypes.IncompleteAdjustmentNote
		});

			validLedgerTransactionTypePairs.Add(LedgerTypes.AccountsReceivable, new List<string>
		{
			TransactionTypes.Invoice,
			TransactionTypes.CreditNote,
			TransactionTypes.AdjustmentNote,
			TransactionTypes.Contra,
			TransactionTypes.Journal,
			TransactionTypes.Payment,
			TransactionTypes.Receipt,
			TransactionTypes.Transfer,
			TransactionTypes.Discount,
			TransactionTypes.Overpayment,
			TransactionTypes.ExchangeDifference,
			TransactionTypes.InvoiceBatch
		});

			validLedgerTransactionTypePairs.Add(LedgerTypes.CashBook, new List<string>
		{
			TransactionTypes.DirectReceipt,
			TransactionTypes.ReceiptBatch,
			TransactionTypes.DirectPayment,
			TransactionTypes.OpeningReceipt,
			TransactionTypes.OpeningPayment,
			TransactionTypes.Transfer,
			TransactionTypes.ExchangeDifference,
			TransactionTypes.DDRBatch
		});

			validLedgerTransactionTypePairs.Add(LedgerTypes.JobCosting, new List<string>
		{
			TransactionTypes.Journal,
			TransactionTypes.JobRevenueJournal
		});

			validLedgerTransactionTypePairs.Add(LedgerTypes.General, new List<string>
		{
			TransactionTypes.GLStandardJournal,
			TransactionTypes.GLAutoJournal,
			TransactionTypes.GLReversingJournal,
			TransactionTypes.GLNoteJournal
		});

			validLedgerTransactionTypePairs.Add(LedgerTypes.UnapprovedPayableTransactions, new List<string>
		{
			TransactionTypes.UAInvoice,
			TransactionTypes.UACreditNote
		});

			validLedgerTransactionTypePairs.Add(LedgerTypes.TransactionsPendingAllocation, new List<string>
		{
			TransactionTypes.InvoicePendingAllocation,
			TransactionTypes.CreditNotePendingAllocation
		});

			return validLedgerTransactionTypePairs;
		}
	}
}
