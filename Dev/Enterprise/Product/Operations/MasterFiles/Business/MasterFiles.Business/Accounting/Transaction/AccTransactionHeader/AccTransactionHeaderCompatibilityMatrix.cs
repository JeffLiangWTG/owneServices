using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class AccTransactionHeaderCompatibilityMatrix
	{
		public static bool IsLedgerCompatibleWithTransactionType(string ledger, string transactionType)
		{
			switch (ledger)
			{
				case LedgerTypes.AccountsPayable:
					{
						return IsAPCompatibleWithTransactionType(transactionType);
					}
				case LedgerTypes.AccountsReceivable:
					{
						return IsARCompatibleWithTransactionType(transactionType);
					}
				case LedgerTypes.IncompleteTransactions:
					{
						return IsINCompatibleWithTransactionType(transactionType);
					}
				case LedgerTypes.CashBook:
					{
						return IsCBCompatibleWithTransactionType(transactionType);
					}
				case LedgerTypes.JobCosting:
					{
						return IsJCCompatibleWithTransactionType(transactionType);
					}
				case LedgerTypes.General:
					{
						return IsGLCompatibleWithTransactionType(transactionType);
					}
				case LedgerTypes.UnapprovedPayableTransactions:
					{
						return IsUACompatibleWithTransactionType(transactionType);
					}
				case LedgerTypes.TransactionsPendingAllocation:
					{
						return IsPACompatibleWithTransactionType(transactionType);
					}
			}
			return false;
		}

		static bool IsAPCompatibleWithTransactionType(string transactionType)
		{
			return transactionType == TransactionTypes.Invoice
				|| transactionType == TransactionTypes.CreditNote
				|| transactionType == TransactionTypes.AdjustmentNote
				|| transactionType == TransactionTypes.Contra
				|| transactionType == TransactionTypes.Journal
				|| transactionType == TransactionTypes.Payment
				|| transactionType == TransactionTypes.Receipt
				|| transactionType == TransactionTypes.Transfer
				|| transactionType == TransactionTypes.Discount
				|| transactionType == TransactionTypes.Overpayment
				|| transactionType == TransactionTypes.ExchangeDifference;
		}

		static bool IsARCompatibleWithTransactionType(string transactionType)
		{
			return transactionType == TransactionTypes.Invoice
				|| transactionType == TransactionTypes.CreditNote
				|| transactionType == TransactionTypes.AdjustmentNote
				|| transactionType == TransactionTypes.Contra
				|| transactionType == TransactionTypes.Journal
				|| transactionType == TransactionTypes.Payment
				|| transactionType == TransactionTypes.Receipt
				|| transactionType == TransactionTypes.Transfer
				|| transactionType == TransactionTypes.Discount
				|| transactionType == TransactionTypes.Overpayment
				|| transactionType == TransactionTypes.ExchangeDifference
				|| transactionType == TransactionTypes.InvoiceBatch;
		}

		static bool IsINCompatibleWithTransactionType(string transactionType)
		{
			return transactionType == TransactionTypes.IncompleteInvoice
				|| transactionType == TransactionTypes.IncompleteCreditNote
				|| transactionType == TransactionTypes.IncompleteAdjustmentNote;
		}
		static bool IsCBCompatibleWithTransactionType(string transactionType)
		{
			return transactionType == TransactionTypes.ReceiptBatch
				|| transactionType == TransactionTypes.DirectPayment
				|| transactionType == TransactionTypes.DirectReceipt
				|| transactionType == TransactionTypes.OpeningReceipt
				|| transactionType == TransactionTypes.OpeningPayment
				|| transactionType == TransactionTypes.Transfer
				|| transactionType == TransactionTypes.ExchangeDifference
				|| transactionType == TransactionTypes.DDRBatch;
		}
		static bool IsJCCompatibleWithTransactionType(string transactionType)
		{
			return transactionType == TransactionTypes.Journal
				|| transactionType == TransactionTypes.JobRevenueJournal;
		}
		static bool IsGLCompatibleWithTransactionType(string transactionType)
		{
			return transactionType == TransactionTypes.GLAutoJournal
				|| transactionType == TransactionTypes.GLReversingJournal
				|| transactionType == TransactionTypes.GLStandardJournal
				|| transactionType == TransactionTypes.GLNoteJournal;
		}
		static bool IsUACompatibleWithTransactionType(string transactionType)
		{
			return transactionType == TransactionTypes.UAInvoice
				|| transactionType == TransactionTypes.UACreditNote;
		}

		static bool IsPACompatibleWithTransactionType(string transactionType)
		{
			return transactionType == TransactionTypes.CreditNotePendingAllocation
				|| transactionType == TransactionTypes.InvoicePendingAllocation;
		}
	}
}
