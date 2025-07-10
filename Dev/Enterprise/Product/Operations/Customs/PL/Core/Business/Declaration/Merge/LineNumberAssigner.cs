using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

class LineNumberAssigner(Declaration.CusEntryHeader entryHeader) : Customs.Business.LineNumberAssigner(entryHeader)
{
	protected override void ExecuteCore()
	{
		var maxLineNumber = ZShort.Zero;
		foreach (var (entryLine, targetEntryLineNumber) in EntryHeader.MergedLines.Cast<Declaration.CusEntryLine>()
			.Select(entryLine => (entryLine, entryLine?.RandomLine?.JI_TargetEntryLineNumber ?? 0))
			.Where(tuple => !tuple.Item2.IsEmpty))
		{
			entryLine!.CL_LineNumber = targetEntryLineNumber;
			if (targetEntryLineNumber > maxLineNumber)
			{
				maxLineNumber = targetEntryLineNumber;
			}
		}

		var lineNumberComparer = new EntryLineComparerAccordingToInvoiceLineOrder();
		foreach (var unassignedEntryLine in EntryHeader.MergedLines.Cast<Declaration.CusEntryLine>()
			.Where(entryLine => entryLine?.RandomLine?.JI_TargetEntryLineNumber.IsEmpty ?? false)
			.OrderBy(x => x, lineNumberComparer))
		{
			unassignedEntryLine.CL_LineNumber = ++maxLineNumber;
		}
	}

	protected override IComparer<Customs.Business.CusEntryLine> GetEntryLineComparerBeforeLineNumbering(Customs.Business.CusEntryHeader entry)
	{
		var baseComparer = base.GetEntryLineComparerBeforeLineNumbering(entry);
		return new PLEntryLineComparer(baseComparer);
	}

	class PLEntryLineComparer(IComparer<Customs.Business.CusEntryLine> baseComparer) : IComparer<Customs.Business.CusEntryLine>
	{
		public int Compare(Customs.Business.CusEntryLine x, Customs.Business.CusEntryLine y)
		{
			var targetEntryLineNumberX = (short?)(x?.RandomLine as JobComInvoiceLine)?.JI_TargetEntryLineNumber ?? 0;
			var targetEntryLineNumberY = (short?)(y?.RandomLine as JobComInvoiceLine)?.JI_TargetEntryLineNumber ?? 0;
			return (targetEntryLineNumberX, targetEntryLineNumberY) switch
			{
				(0, 0) => baseComparer.Compare(x, y),
				(_, 0) => -1,
				(0, _) => 1,
				_ => targetEntryLineNumberX - targetEntryLineNumberY,
			};
		}
	}
}
