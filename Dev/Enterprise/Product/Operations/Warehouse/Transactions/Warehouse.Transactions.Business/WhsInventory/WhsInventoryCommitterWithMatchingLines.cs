using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsInventoryCommitterWithMatchingLines<T> : WhsInventoryCommitter<T>
		where T : BusinessObject, ILineWithMatchingLines<T>
	{
		public WhsInventoryCommitterWithMatchingLines(T transactionLine)
			: base(transactionLine, throwExceptionIfHasMatchingLines: false)
		{
			if (!transactionLine.IsMainTransactionLine())
			{
				throw new ArgumentException("Matching Transaction lines should not be used in inventory committer.");
			}
		}

		#region UncommitOverPickedOrNonMatchingInventory

		protected override void UncommitOverPickedOrNonMatchingInventoryCore()
		{
			base.UncommitOverPickedOrNonMatchingInventoryCore();

			UncommitNonMatchingInventory(TransactionLine);

			var matchingLines = TransactionLine.MatchingLines.ToArray();
			foreach (var line in matchingLines)
			{
				UncommitNonMatchingInventory(line);
				UncommitOverPickedInventory(line);
				ReallocateUncommittedQtyToMainLine(line);
			}
		}

		#region UncommitNonMatchingInventory

		void UncommitNonMatchingInventory(T transactionLine)
		{
			var pickLines = transactionLine.PickLines;
			var pickLinesGrouped = new Dictionary<WhsDocketLine, ZDecimal>();

			for (var i = pickLines.Count - 1; i >= 0; i--)
			{
				var pickLine = pickLines[i];
				var inventoryLine = pickLine.InventoryLine;
				if (!pickLine.IsPickedFromPutawayLocation && DoesPickLineNotMatchTransactionLine(transactionLine, pickLine))
				{
					pickLine.Delete();
				}
				else if (!pickLinesGrouped.ContainsKey(inventoryLine))
				{
					pickLinesGrouped.Add(inventoryLine, pickLine.WZ_Units);
				}
				else
				{
					pickLinesGrouped[inventoryLine] += pickLine.WZ_Units;
				}
			}

			if (pickLinesGrouped.Count > 1) // only 1 inventory can be linked to 1 transaction line
			{
				var inventoryLinePK = GetInventoryLinePKWithMaxCommitQty(pickLinesGrouped);
				for (var i = pickLines.Count - 1; i >= 0; i--)
				{
					if (pickLines[i].WZ_WE_InventoryLine != inventoryLinePK)
					{
						pickLines[i].Delete();
					}
				}
			}
		}

		static ZGuid GetInventoryLinePKWithMaxCommitQty(IEnumerable<KeyValuePair<WhsDocketLine, ZDecimal>> pickLinesGrouped)
		{
			var firstPickLineGroup = pickLinesGrouped.First();
			var inventoryLinePK = firstPickLineGroup.Key.PK;
			var maxValue = firstPickLineGroup.Value;
			foreach (var pickLineGroup in pickLinesGrouped)
			{
				if (pickLineGroup.Value > maxValue)
				{
					inventoryLinePK = pickLineGroup.Key.PK;
					maxValue = pickLineGroup.Value;
				}
			}

			return inventoryLinePK;
		}

		#endregion

		#region ReallocateUncommittedQtyToMainLine

		void ReallocateUncommittedQtyToMainLine(T line)
		{
			var qtyUncommitted = line.TransactionQty - line.GetQtyCommittedToThisLine();
			if (qtyUncommitted > 0)
			{
				line.TransactionQty -= qtyUncommitted;
				TransactionLine.TransactionQty += qtyUncommitted;

				if (line.TransactionQty <= 0m)
				{
					line.Delete();
				}
			}
		}

		#endregion

		#endregion

		#region CommitInventory

		#region AllocateStockToExistingPickLines

		protected override decimal AllocateStockToExistingPickLinesCore(decimal quantityToCommit)
		{
			decimal quantityCommitted = 0m;

			foreach (var line in TransactionLine.MatchingLines)
			{
				var qtyAllocated = AllocateStockToPickLines(line, quantityToCommit);
				line.TransactionQty += qtyAllocated;
				TransactionLine.TransactionQty -= qtyAllocated;
				quantityToCommit -= qtyAllocated;
				quantityCommitted += qtyAllocated;

				if (quantityToCommit == 0m)
				{
					break; // allocation fulfilled.
				}
			}

			return quantityCommitted;
		}

		#endregion

		#region AllocateStockToNewPickLines

		protected override void CreateAndLinkPickLine(WhsInventoryView inventory, decimal quantityToCommit, ZString pickedBy)
		{
			// only 1 pickline per transaction line is allowed
			if (TransactionLine.PickLines.Count == 0)
			{
				base.CreateAndLinkPickLine(inventory, quantityToCommit, pickedBy);
			}
			else
			{
				var matchingLine = TransactionLine.SplitIntoMatchingLine(quantityToCommit);

				CreatePickLine(matchingLine, inventory, quantityToCommit, pickedBy);
				ReAssignReservedPickLinesToMatchingLine(matchingLine);
			}
		}

		protected override void SetPerPackageQuantityCore()
		{
			foreach (ILineWithCommittedPickLines line in TransactionLine.MatchingLines)
			{
				line.PerPackageQty = TransactionLine.PerPackageQty;
			}
		}

		void ReAssignReservedPickLinesToMatchingLine(T matchingLine)
		{
			if (TransactionLine.TransactionQty < TransactionLine.ReservedQuantity
				|| TransactionLine.ReservedPickLines.GroupBy(line => line.WZ_WE_TransactionLine).Any(grouping => grouping.Count() > 1))
			{
				ReAssignReservedPickLinesToMatchingLineCore(matchingLine);
			}
		}

		void ReAssignReservedPickLinesToMatchingLineCore(T matchingLine)
		{
			var pickLinesToReAssign = TransactionLine.ReservedPickLines
				.Where(line => matchingLine.TransactionQty >= line.ReservedQuantity)
				.OrderByDescending(line => line.ReservedQuantity);

			var qtyToReAssign = matchingLine.TransactionQty;
			foreach (var pickLine in pickLinesToReAssign)
			{
				if (qtyToReAssign >= pickLine.ReservedQuantity)
				{
					pickLine.WZ_WE_InventoryLine = matchingLine.PK;
					qtyToReAssign -= pickLine.ReservedQuantity;
				}

				if (qtyToReAssign == 0)
				{
					break;
				}
			}
		}

		#endregion

		#endregion

		// interfaces

		#region ICommittedInventoryStrategy

		protected override ZDecimal GetTotalTransactionQtyForMainTransactionLine()
		{
			return TransactionLine.GetTransactionQtyIncludingMatchingLines();
		}

		protected override ZDecimal GetTotalQtyCommittedToMainTransactionLine()
		{
			return TransactionLine.GetQtyCommittedIncludingMatchingLines();
		}

		#endregion
	}
}
