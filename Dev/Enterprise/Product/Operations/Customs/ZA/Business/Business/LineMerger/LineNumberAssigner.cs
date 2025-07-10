using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business
{
	class LineNumberAssigner : Customs.Business.LineNumberAssigner
	{
		public LineNumberAssigner(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		protected override void ExecuteCore()
		{
			var alreadyUsedEntryLineNumber = new List<ZShort>();
			foreach (CusEntryLine line in EntryHeader.MergedLines)
			{
				var invoiceLineTargetEntryLineNumber = line?.RandomLine?.JI_TargetEntryLineNumber ?? ZShort.Zero;
				if (!invoiceLineTargetEntryLineNumber.IsEmpty && !alreadyUsedEntryLineNumber.Contains(invoiceLineTargetEntryLineNumber))
				{
					line.CL_LineNumber = invoiceLineTargetEntryLineNumber;
					alreadyUsedEntryLineNumber.Add(line.CL_LineNumber);
				}
				else
				{
					line.CL_LineNumber = ZShort.Zero;
				}
			}

			var unassignedEntryLines = EntryHeader.MergedLines.OfType<CusEntryLine>()?.Where(x => x.CL_LineNumber.IsEmpty)?.ToList();
			if ((unassignedEntryLines?.Count ?? 0) > 0)
			{
				var maxAssignedTargetEntryLineNumber = alreadyUsedEntryLineNumber.Max();
				unassignedEntryLines.Sort(new EntryLineComparerAccordingToInvoiceLineOrder());
				foreach (var unassignedEntryLine in unassignedEntryLines)
				{
					unassignedEntryLine.CL_LineNumber = ++maxAssignedTargetEntryLineNumber;
				}
			}
		}

		protected override IComparer<Customs.Business.CusEntryLine> GetEntryLineComparerBeforeLineNumbering(Customs.Business.CusEntryHeader entry)
		{
			return new ZAEntryLineComparer();
		}

		class ZAEntryLineComparer : IComparer<Customs.Business.CusEntryLine>
		{
			public int Compare(Customs.Business.CusEntryLine x, Customs.Business.CusEntryLine y)
			{
				var invoiceLineX = x.InvoiceLines.Count > 0 ? x.InvoiceLines[0] : x.RandomLine;
				var invoiceLineY = y.InvoiceLines.Count > 0 ? y.InvoiceLines[0] : y.RandomLine;

				if (invoiceLineX == null && invoiceLineY != null)
				{
					return -1;
				}
				else if (invoiceLineX != null && invoiceLineY == null)
				{
					return 1;
				}
				else if (invoiceLineX == null && invoiceLineY == null)
				{
					return 0;
				}
				else
				{
					var targetLineNumberX = (invoiceLineX as JobComInvoiceLine)?.JI_TargetEntryLineNumber ?? ZShort.Zero;
					var targetLineNumberY = (invoiceLineY as JobComInvoiceLine)?.JI_TargetEntryLineNumber ?? ZShort.Zero;
					if (targetLineNumberX != 0 && targetLineNumberY == 0)
					{
						return -1;
					}
					else if (targetLineNumberX == 0 && targetLineNumberY != 0)
					{
						return 1;
					}
					else
					{
						return InvoiceLineComparer.Compare(invoiceLineX, invoiceLineY);
					}
				}
			}

			BaseJobComInvoiceLine.LineComparer InvoiceLineComparer => invoiceLineComparer ?? (invoiceLineComparer = new BaseJobComInvoiceLine.LineComparer());
			BaseJobComInvoiceLine.LineComparer invoiceLineComparer;
		}
	}

	class LineNumberAssignerForIMX : Customs.Business.LineNumberAssigner
	{
		public LineNumberAssignerForIMX(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}
		protected override void AssignLineNumber(Customs.Business.CusEntryLine entryLine)
		{
			entryLine.CL_LineNumber = entryLine.RandomLine.JI_PreviousEntryLineNumber;
		}
	}
}
