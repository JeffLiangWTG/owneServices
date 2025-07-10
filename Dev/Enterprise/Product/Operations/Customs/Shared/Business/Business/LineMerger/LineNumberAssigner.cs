using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ILineNumberAssigner
	{
		void Execute();
	}

	public class LineNumberAssigner : ILineNumberAssigner
	{
		public LineNumberAssigner(CusEntryHeader entryHeader)
		{
			if (entryHeader == null)
			{
				throw new ArgumentNullException(nameof(entryHeader));
			}
			this.EntryHeader = entryHeader;
		}

		protected CusEntryHeader EntryHeader { get; }

		protected BaseJobDeclaration Declaration
		{
			get { return EntryHeader.Declaration; }
		}

		public void Execute()
		{
			var sortDictionary = new Dictionary<ZGuid, SortInfo>();
			SortInvoiceLines(sortDictionary);
			EntryHeader.MergedLines.Sort(GetEntryLineComparerBeforeLineNumbering(EntryHeader));
			ExecuteCore();

			foreach (CusEntryLine entryLine in EntryHeader.MergedLines)
			{
				if (entryLine.InvoiceLines.Count > 1)
				{
					if (sortDictionary.TryGetValue(entryLine.PK, out var sortInformation))
					{
						entryLine.InvoiceLines.Sort(sortInformation);
					}
				}
			}
		}

		protected virtual IComparer<CusEntryLine> GetEntryLineComparerBeforeLineNumbering(CusEntryHeader entry)
		{
			return new EntryLineComparerAccordingToInvoiceLineOrder();
		}

		void SortInvoiceLines(Dictionary<ZGuid, SortInfo> sortDictionary)
		{
			foreach (CusEntryLine entryLine in EntryHeader.MergedLines)
			{
				if (entryLine.InvoiceLines.Count > 1)
				{
					var sortInformation = entryLine.InvoiceLines.SortInformation;
					if (sortInformation != null)
					{
						sortDictionary[entryLine.PK] = sortInformation;
					}
					entryLine.InvoiceLines.Sort((IComparer<BaseJobComInvoiceLine>)new BaseJobComInvoiceLine.LineComparer());
				}
			}
		}

		protected virtual void ExecuteCore()
		{
			if (ShouldCompletelyReassignNumbers)
			{
				EntryHeader.CH_HighestLineNumber = 0;
			}

			lastLineNumber = EntryHeader.CH_HighestLineNumber;

			foreach (CusEntryLine entryLine in EntryHeader.MergedLines)
			{
				if (entryLine.CL_LineNumber == 0
					|| entryLine.CL_LineNumber > EntryHeader.CH_HighestLineNumber
					|| ShouldCompletelyReassignNumbers)
				{
					AssignLineNumber(entryLine);
				}
			}
		}

		protected ZShort lastLineNumber { get; set; }

		bool ShouldCompletelyReassignNumbers => EntryHeader.ShouldCompletelyReassignNumbers;

		protected virtual void AssignLineNumber(CusEntryLine entryLine)
		{
			lastLineNumber++;
			entryLine.CL_LineNumber = lastLineNumber;
		}

		protected class EntryLineComparerAccordingToInvoiceLineOrder : IComparer<CusEntryLine>
		{
			public int Compare(CusEntryLine x, CusEntryLine y)
			{
				var invoiceLineX = x.InvoiceLines.Count > 0 ? x.InvoiceLines[0] : x.RandomLine;
				var invoiceLineY = y.InvoiceLines.Count > 0 ? y.InvoiceLines[0] : y.RandomLine;

				return InvoiceLineComparer.Compare(invoiceLineX, invoiceLineY);
			}

			BaseJobComInvoiceLine.LineComparer InvoiceLineComparer
			{
				get
				{
					if (fInvoiceLineComparer == null)
					{
						fInvoiceLineComparer = new BaseJobComInvoiceLine.LineComparer();
					}
					return fInvoiceLineComparer;
				}
			}
			BaseJobComInvoiceLine.LineComparer fInvoiceLineComparer;
		}
	}
}
