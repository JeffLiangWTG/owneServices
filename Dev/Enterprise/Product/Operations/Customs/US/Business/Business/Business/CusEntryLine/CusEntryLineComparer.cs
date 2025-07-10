using System.Collections.Generic;

namespace Enterprise.Customs.US.Business
{
	public class CusEntryLineComparer : Comparer<IEntryLine>
	{
		EntrySummaryEntryLineComparerForNumbering Comparer
		{
			get { return comparer ?? (comparer = new EntrySummaryEntryLineComparerForNumbering()); }
		}
		EntrySummaryEntryLineComparerForNumbering comparer;

		public override int Compare(IEntryLine x, IEntryLine y)
		{
			if (x == y)
			{
				return 0;
			}
			int result = 0;
			if (x.IsFTZAdmission)
			{
				result = x.RandomLine.InvoiceNumber.CompareTo(y.RandomLine.InvoiceNumber);
				if (result == 0)
				{
					result = x.RandomLine.JI_LineNo.CompareTo(y.RandomLine.JI_LineNo);
				}
			}
			else
			{
				result = x.CL_LineNumber.CompareTo(y.CL_LineNumber);
				if (result == 0)
				{
					result = Comparer.Compare(x, y);
				}
			}
			return result;
		}
	}
}
