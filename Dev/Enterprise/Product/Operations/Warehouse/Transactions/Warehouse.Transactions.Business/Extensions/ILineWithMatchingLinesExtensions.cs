using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public delegate ZDecimal GetQtyDelegate<T>(T line) where T : ILineWithMatchingLines<T>;

	public static class ILineWithMatchingLinesExtensions
	{
		#region GetQtyCommitted

		public static ZDecimal GetQtyCommittedIncludingMatchingLines<T>(this T mainTransactionLine)
			where T : ILineWithMatchingLines<T>
		{
			return mainTransactionLine.GetQtyIncludingMatchingLines(l => l.GetQtyCommittedToThisLine());
		}

		#endregion

		#region GetTransactionQtyIncludingMatchingLines

		public static ZDecimal GetTransactionQtyIncludingMatchingLines<T>(this T mainTransactionLine)
			where T : ILineWithMatchingLines<T>
		{
			return mainTransactionLine.GetQtyIncludingMatchingLines(l => l.TransactionQty);
		}

		#endregion

		#region GetQtyIncludingMatchingLines

		public static ZDecimal GetQtyIncludingMatchingLines<T>(this T mainTransactionLine, GetQtyDelegate<T> getQty)
			where T : ILineWithMatchingLines<T>
		{
			if (!mainTransactionLine.IsMainTransactionLine())
			{
				throw new InvalidOperationException("Should only call GetQtyIncludingMatchingLines() on Main Transaction Line.");
			}

			return getQty(mainTransactionLine) + mainTransactionLine.MatchingLines.Sum(l => getQty(l));
		}

		#endregion

		#region IsMainTransactionLine

		public static bool IsMainTransactionLine<T>(this ILineWithMatchingLines<T> transactionLine)
			where T : ILineWithCommittedPickLines
		{
			return transactionLine.MatchingLinePK.IsEmpty;
		}

		#endregion

		#region CreateMatchingLineWithQuantity

		public static T CreateMatchingLineWithQuantity<T>(this T mainTransactionLine, ZDecimal qtyForMatchingLine)
			where T : BusinessObject, ILineWithMatchingLines<T>
		{
			if (!mainTransactionLine.IsMainTransactionLine())
			{
				throw new InvalidOperationException("Should only call CreateMatchingLine() on Main Transaction Line.");
			}

			if (!mainTransactionLine.ParentDocketPK.IsValid)
			{
				throw new ArgumentException("ParentDocketPK should be a valid Guid.", nameof(mainTransactionLine.ParentDocketPK));
			}

			if (qtyForMatchingLine < 0m)
			{
				throw new ArgumentException("qtyForMatchingLine cannot be Negative.", nameof(qtyForMatchingLine));
			}

			if (qtyForMatchingLine == 0m)
			{
				throw new ArgumentException("qtyForMatchingLine cannot be Zero.", nameof(qtyForMatchingLine));
			}

			var matchingLine = (T)mainTransactionLine.Clone();
			matchingLine.MatchingLinePK = mainTransactionLine.PK;
			matchingLine.ParentDocketPK = mainTransactionLine.ParentDocketPK;

			var lineWithPickAndPutawayDetails = mainTransactionLine as ILineWithPickAndPutawayDetails;
			if (lineWithPickAndPutawayDetails != null)
			{
				var matchingLineWithPutawayDetails = (ILineWithPickAndPutawayDetails)matchingLine;
				matchingLineWithPutawayDetails.PutawayBy = lineWithPickAndPutawayDetails.PutawayBy;
			}

			matchingLine.TransactionQty = qtyForMatchingLine;

			return matchingLine;
		}

		#endregion

		#region SplitIntoMatchingLine

		public static T SplitIntoMatchingLine<T>(this T mainTransactionLine, ZDecimal qtyForMatchingLine)
			where T : BusinessObject, ILineWithMatchingLines<T>
		{
			if (qtyForMatchingLine >= mainTransactionLine.TransactionQty)
			{
				throw new ArgumentException("qtyForMatchingLine must be less than the Transaction Qty.", nameof(qtyForMatchingLine));
			}

			var matchingLine = mainTransactionLine.CreateMatchingLineWithQuantity(qtyForMatchingLine);

			mainTransactionLine.TransactionQty -= qtyForMatchingLine;

			return matchingLine;
		}

		#endregion
	}
}
