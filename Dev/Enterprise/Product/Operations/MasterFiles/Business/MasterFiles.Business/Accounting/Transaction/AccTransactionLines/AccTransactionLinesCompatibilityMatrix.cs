using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class AccTransactionLinesCompatibilityMatrix
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static bool IsLineTypeCompatibleWithTransactionHeader(string lineType, string ledger, string transactionType)
		{
			switch (lineType)
			{
				case TransactionLineTypes.Cost:
					{
						return IsCSTCompatibleWithTransactionHeader(ledger, transactionType);
					}
				case TransactionLineTypes.Revenue:
					{
						return IsREVCompatibleWithTransactionHeader(ledger, transactionType);
					}
				case TransactionLineTypes.UnapprovedCost:
					{
						return IsUCTCompatibleWithTransactionHeader(ledger, transactionType);
					}
				case TransactionLineTypes.WIP:
					{
						return true;
					}
				case TransactionLineTypes.Accrual:
					{
						return true;
					}
				case TransactionTypes.DirectPayment:
					{
						return IsDPYCompatibleWithTransactionHeader(ledger, transactionType);
					}
				case TransactionTypes.DirectReceipt:
					{
						return IsDRCCompatibleWithTransactionHeader(ledger, transactionType);
					}
				case TransactionTypes.GLStandardJournal:
					{
						return IsGJLCompatibleWithTransactionHeader(ledger, transactionType);
					}
				case TransactionTypes.GLAutoJournal:
					{
						return IsAJLCompatibleWithTransactionHeader(ledger, transactionType);
					}
				case TransactionTypes.GLReversingJournal:
					{
						return IsRJLCompatibleWithTransactionHeader(ledger, transactionType);
					}
				case TransactionTypes.GLNoteJournal:
					{
						return IsNJLCompatibleWithTransactionHeader(ledger, transactionType);
					}
			}
			return false;
		}

		static bool IsCSTCompatibleWithTransactionHeader(string ledger, string transactionType)
		{
			return (ledger == LedgerTypes.AccountsPayable && (
						transactionType == TransactionTypes.Invoice ||
						transactionType == TransactionTypes.CreditNote ||
						transactionType == TransactionTypes.AdjustmentNote))
				|| (ledger == LedgerTypes.JobCosting && transactionType == TransactionTypes.JobRevenueJournal);
		}

		static bool IsREVCompatibleWithTransactionHeader(string ledger, string transactionType)
		{
			return (ledger == LedgerTypes.AccountsReceivable && (
						transactionType == TransactionTypes.Invoice ||
						transactionType == TransactionTypes.CreditNote ||
						transactionType == TransactionTypes.AdjustmentNote))
				|| (ledger == LedgerTypes.JobCosting && (
						transactionType == TransactionTypes.Journal ||
						transactionType == TransactionTypes.JobRevenueJournal));
		}

		static bool IsUCTCompatibleWithTransactionHeader(string ledger, string transactionType)
		{
			return ledger == LedgerTypes.UnapprovedPayableTransactions && (
						transactionType == TransactionTypes.UAInvoice ||
						transactionType == TransactionTypes.UACreditNote);
		}

		static bool IsDPYCompatibleWithTransactionHeader(string ledger, string transactionType)
		{
			return ledger == LedgerTypes.CashBook && transactionType == TransactionTypes.DirectPayment;
		}

		static bool IsDRCCompatibleWithTransactionHeader(string ledger, string transactionType)
		{
			return ledger == LedgerTypes.CashBook && transactionType == TransactionTypes.DirectReceipt;
		}

		static bool IsGJLCompatibleWithTransactionHeader(string ledger, string transactionType)
		{
			return ledger == LedgerTypes.General && transactionType == TransactionTypes.GLStandardJournal;
		}

		static bool IsAJLCompatibleWithTransactionHeader(string ledger, string transactionType)
		{
			return ledger == LedgerTypes.General && transactionType == TransactionTypes.GLAutoJournal;
		}

		static bool IsRJLCompatibleWithTransactionHeader(string ledger, string transactionType)
		{
			return ledger == LedgerTypes.General && transactionType == TransactionTypes.GLReversingJournal;
		}

		static bool IsNJLCompatibleWithTransactionHeader(string ledger, string transactionType)
		{
			return ledger == LedgerTypes.General && transactionType == TransactionTypes.GLNoteJournal;
		}
	}
}
