using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	/// <summary>
	/// Used to sort invoice lines before merge. If x is merged and y is not merged, x should come first
	/// If both are merged and x's entry is lodged and y's entry is not lodged, then x should come first
	/// If both are merged and lodged and x's entry is active, y's entry is not active, then x should come first
	/// if both are merged, lodged and active, x's merge key changes, but y's merge key is the same, then x should come last
	/// If order is not determined after this, then it uses LineNo + InvoiceNo to determine the order
	/// </summary>
	public class InvoiceLineComparerForMerge : IComparer<BaseJobComInvoiceLine>
	{
		public InvoiceLineComparerForMerge(EntryCreationStrategy strategy, ReadOnlyBusinessObjectFactory cleanFactory)
		{
			this.strategy = strategy;
			normalComparer = new BaseJobComInvoiceLine.LineComparer();
			this.cleanFactory = cleanFactory;
		}
		readonly BaseJobComInvoiceLine.LineComparer normalComparer;
		readonly EntryCreationStrategy strategy;
		readonly ReadOnlyBusinessObjectFactory cleanFactory;

		public int Compare(BaseJobComInvoiceLine x, BaseJobComInvoiceLine y)
		{
			if (x == y)
			{
				return 0;
			}

			CusEntryLine xEntryLine = strategy.GetExistingEntryLine(x);
			CusEntryLine yEntryLine = strategy.GetExistingEntryLine(y);

			int result = 0;

			if (xEntryLine != null && yEntryLine != null)
			{
				CusEntryHeader xEntryHeader = xEntryLine.Header;
				CusEntryHeader yEntryHeader = yEntryLine.Header;

				//AU entries withdrawn should remain active so that any subsequent merge still links invoices to the entry until replacement entry comes in place
				if ((!xEntryHeader.NeedToMaintainLinesDuringMerge || !xEntryHeader.IsActive || xEntryHeader.HasBeenWithdrawn) &&
					 yEntryHeader.NeedToMaintainLinesDuringMerge && yEntryHeader.IsActive && !yEntryHeader.HasBeenWithdrawn)
				{
					result = 1;
				}
				else if (xEntryHeader.NeedToMaintainLinesDuringMerge && xEntryHeader.IsActive && !xEntryHeader.HasBeenWithdrawn &&
					(!yEntryHeader.NeedToMaintainLinesDuringMerge || !yEntryHeader.IsActive || yEntryHeader.HasBeenWithdrawn))
				{
					result = -1;
				}
				else if (xEntryHeader.NeedToMaintainLinesDuringMerge && xEntryHeader.IsActive && !xEntryHeader.HasBeenWithdrawn &&
				 yEntryHeader.NeedToMaintainLinesDuringMerge && yEntryHeader.IsActive && !yEntryHeader.HasBeenWithdrawn)
				{
					//check if x and y lines have merge key changes
					//the one with merge key change should come last
					bool xHasMergeKeyChange = strategy.HasMergeKeyChangeSinceLastSaving(x, cleanFactory);
					bool yHasMergeKeyChange = strategy.HasMergeKeyChangeSinceLastSaving(y, cleanFactory);

					if (xHasMergeKeyChange && !yHasMergeKeyChange)
					{
						result = 1;
					}
					else if (!xHasMergeKeyChange && yHasMergeKeyChange)
					{
						result = -1;
					}
				}

				if (result == 0)
				{
					result = xEntryLine.CL_LineNumber.CompareTo(yEntryLine.CL_LineNumber);
				}
			}
			else if (xEntryLine != null)
			{
				result = -1;
			}
			else if (yEntryLine != null)
			{
				result = 1;
			}

			if (result == 0)
			{
				result = normalComparer.Compare(x, y);
			}

			return result;
		}
	}
}
