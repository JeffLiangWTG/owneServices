using System.Collections.Generic;

namespace Enterprise.Customs.US.Business
{
	class EntrySummaryEntryLineComparerForNumbering : Comparer<IEntryLine>, IComparer<Customs.Business.CusEntryLine>
	{
		public override int Compare(IEntryLine x, IEntryLine y)
		{
			if (x == y)
			{
				return 0;
			}

			var xToCompare = x;
			var yToCompare = y;

			if (xToCompare == null && yToCompare != null)
			{
				return -1;
			}
			else if (xToCompare != null && yToCompare == null)
			{
				return 1;
			}

			var xParent = xToCompare.ParentLine;
			var yParent = yToCompare.ParentLine;

			//Parent before its children
			if (xParent != null && xParent == y) //y is x's parent
			{
				return 1;
			}
			else if (yParent != null && x == yParent)//x is y's parent
			{
				return -1;
			}

			xToCompare = GetEntryLineToCompareWith(x, y);
			yToCompare = GetEntryLineToCompareWith(y, x);

			var xLine = xToCompare.FirstInvoiceLineAfterSortedOnInvoiceLineNo;
			var yLine = yToCompare.FirstInvoiceLineAfterSortedOnInvoiceLineNo;

			if (xLine != null && yLine != null)
			{
				if (xLine == yLine)
				{
					if (xToCompare.US_SupAdditionalLine != yToCompare.US_SupAdditionalLine) // Additional Supplementary Line 1
					{
						return xToCompare.US_SupAdditionalLine ? -1 : 1;
					}
					else if (xToCompare.US_SupAdditionalLine2 != yToCompare.US_SupAdditionalLine2) // Additional Supplementary Line 2
					{
						return xToCompare.US_SupAdditionalLine2 ? -1 : 1;
					}
					else if (xToCompare.US_SupAdditionalLine3 != yToCompare.US_SupAdditionalLine3) // Additional Supplementary Line 3
					{
						return xToCompare.US_SupAdditionalLine3 ? -1 : 1;
					}
					else if (xToCompare.US_SupAdditionalLine4 != yToCompare.US_SupAdditionalLine4) // Additional Supplementary Line 4
					{
						return xToCompare.US_SupAdditionalLine4 ? -1 : 1;
					}
					else if (xToCompare.US_SupAdditionalLine5 != yToCompare.US_SupAdditionalLine5) // Additional Supplementary Line 5
					{
						return xToCompare.US_SupAdditionalLine5 ? -1 : 1;
					}
					else if (xToCompare.US_SupLine != yToCompare.US_SupLine) // Supplementary Line
					{
						return xToCompare.US_SupLine ? -1 : 1;
					}
				}
				return LineComparer.Compare(xLine, yLine);
			}

			return xToCompare.PK.CompareTo(yToCompare.PK);
		}

		IEntryLine GetEntryLineToCompareWith(IEntryLine entryLine, IEntryLine lineToCompareWith)
		{
			IEntryLine result;
			var parentLine = entryLine.ParentLine;
			if (parentLine == null || parentLine == lineToCompareWith.ParentLine)
			{
				result = entryLine;
			}
			else
			{
				result = parentLine;
			}

			return result;
		}

		int IComparer<Customs.Business.CusEntryLine>.Compare(Customs.Business.CusEntryLine x, Customs.Business.CusEntryLine y)
		{
			return Compare(x as IEntryLine, y as IEntryLine);
		}

		LineComparer LineComparer
		{
			get { return lineComparer ?? (lineComparer = new LineComparer()); }
		}
		LineComparer lineComparer;
	}
}
