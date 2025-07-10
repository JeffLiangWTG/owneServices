using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	public sealed class AccTransactionLinesCompatibilityMatrixTestHelper
	{
		public bool IsHeaderCompatibleWithLine(string ledger, string transactionType)
		{
			return GetCompatibleLineType(ledger, transactionType) != null;
		}

		public string GetCompatibleLineType(string ledger, string transactionType)
		{
			return LineHeaderCompatibilityMatrix.FirstOrDefault(x => x.Value.Any(y => y.Item1 == ledger && y.Item2 == transactionType)).Key;
		}

		public Tuple<string, string> GetCompatibleLedgerTransactionType(string lineType)
		{
			var result = Tuple.Create(string.Empty, string.Empty);
			List<Tuple<string, string>> compatibleTuples;
			if (LineHeaderCompatibilityMatrix.TryGetValue(lineType, out compatibleTuples))
			{
				result = compatibleTuples.First();
			}
			return result;
		}

		public Dictionary<string, List<Tuple<string, string>>> LineHeaderCompatibilityMatrix
		{
			get
			{
				if (lineHeaderCompatibilityMatrix == null)
				{
					lineHeaderCompatibilityMatrix = InitializeLineHeaderCompatibilityMatrix();
				}
				return lineHeaderCompatibilityMatrix;
			}
		}

		Dictionary<string, List<Tuple<string, string>>> lineHeaderCompatibilityMatrix;

		Dictionary<string, List<Tuple<string, string>>> InitializeLineHeaderCompatibilityMatrix()
		{
			var validLineHeaderPairs = new Dictionary<string, List<Tuple<string, string>>>();

			validLineHeaderPairs.Add(TransactionLineTypes.Cost, new List<Tuple<string, string>>
			{
				Tuple.Create(LedgerTypes.AccountsPayable, TransactionTypes.Invoice),
				Tuple.Create(LedgerTypes.AccountsPayable, TransactionTypes.CreditNote),
				Tuple.Create(LedgerTypes.AccountsPayable, TransactionTypes.AdjustmentNote),
				Tuple.Create(LedgerTypes.JobCosting, TransactionTypes.JobRevenueJournal)
			});

			validLineHeaderPairs.Add(TransactionLineTypes.Revenue, new List<Tuple<string, string>>
			{
				Tuple.Create(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice),
				Tuple.Create(LedgerTypes.AccountsReceivable, TransactionTypes.CreditNote),
				Tuple.Create(LedgerTypes.AccountsReceivable, TransactionTypes.AdjustmentNote),
				Tuple.Create(LedgerTypes.JobCosting, TransactionTypes.Journal),
				Tuple.Create(LedgerTypes.JobCosting, TransactionTypes.JobRevenueJournal),
			});

			validLineHeaderPairs.Add(TransactionLineTypes.UnapprovedCost, new List<Tuple<string, string>>
			{
				Tuple.Create(LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UAInvoice),
				Tuple.Create(LedgerTypes.UnapprovedPayableTransactions, TransactionTypes.UACreditNote),
			});

			validLineHeaderPairs.Add(TransactionTypes.DirectPayment, new List<Tuple<string, string>>
			{
				Tuple.Create(LedgerTypes.CashBook, TransactionTypes.DirectPayment)
			});

			validLineHeaderPairs.Add(TransactionTypes.DirectReceipt, new List<Tuple<string, string>>
			{
				Tuple.Create(LedgerTypes.CashBook, TransactionTypes.DirectReceipt)
			});

			validLineHeaderPairs.Add(TransactionTypes.GLStandardJournal, new List<Tuple<string, string>>
			{
				Tuple.Create(LedgerTypes.General, TransactionTypes.GLStandardJournal)
			});

			validLineHeaderPairs.Add(TransactionTypes.GLReversingJournal, new List<Tuple<string, string>>
			{
				Tuple.Create(LedgerTypes.General, TransactionTypes.GLReversingJournal)
			});

			validLineHeaderPairs.Add(TransactionTypes.GLAutoJournal, new List<Tuple<string, string>>
			{
				Tuple.Create(LedgerTypes.General, TransactionTypes.GLAutoJournal)
			});

			validLineHeaderPairs.Add(TransactionTypes.GLNoteJournal, new List<Tuple<string, string>>
			{
				Tuple.Create(LedgerTypes.General, TransactionTypes.GLNoteJournal)
			});

			validLineHeaderPairs.Add(TransactionLineTypes.Accrual, Enumerable.Empty<Tuple<string, string>>().ToList());
			validLineHeaderPairs.Add(TransactionLineTypes.WIP, Enumerable.Empty<Tuple<string, string>>().ToList());

			return validLineHeaderPairs;
		}
	}
}
