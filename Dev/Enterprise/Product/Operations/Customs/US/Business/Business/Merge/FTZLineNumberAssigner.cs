using System.Collections.Generic;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.US.Business
{
	class FTZLineNumberAssigner : ILineNumberAssigner
	{
		public FTZLineNumberAssigner(CusEntryHeader entry)
		{
			this.entry = entry;
		}

		readonly CusEntryHeader entry;

		void ILineNumberAssigner.Execute()
		{
			if (entry != null)
			{
				entry.MergedLines.Sort(new FTZEntryLineComparerForLineNumbering());

				AssignLineNumbers();
			}
		}

		void AssignLineNumbers()
		{
			Bill bill = null;

			short lastLineNumber = 0;
			foreach (CusEntryLine entryLine in entry.MergedLines)
			{
				if (bill == null || bill != entryLine.FTZBill)
				{
					bill = entryLine.FTZBill;
					lastLineNumber = 0;
				}

				if (entryLine.ParentLine != null && entryLine.IsSecondaryTariffLine)
				{
					entryLine.CL_LineNumber = entryLine.ParentLine.CL_LineNumber;
				}
				else
				{
					lastLineNumber++;
					entryLine.CL_LineNumber = lastLineNumber;
				}
			}
		}
	}

	class FTZEntryLineComparerForLineNumbering : IComparer<CusEntryLine>
	{
		public int Compare(CusEntryLine x, CusEntryLine y)
		{
			if (x == y)
			{
				return 0;
			}

			int result = 0;

			var billX = x.FTZBill;
			var billY = y.FTZBill;

			if (billX != null && billY != null && billX != billY)
			{
				result = billX.CU_BillNum.ToUpper().CompareTo(billY.CU_BillNum.ToUpper());

				if (result == 0)
				{
					result = billX.PK.CompareTo(billY.PK);
				}
			}

			if (result == 0)
			{
				result = new EntrySummaryEntryLineComparerForNumbering().Compare(x, y);
			}

			return result;
		}
	}
}
