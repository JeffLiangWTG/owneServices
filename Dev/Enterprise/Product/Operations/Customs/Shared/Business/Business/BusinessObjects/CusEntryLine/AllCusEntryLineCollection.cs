using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface IAllCusEntryLineCollection<out TCusEntryLine> : IBusinessObjectCollection<TCusEntryLine>
		where TCusEntryLine : CusEntryLine
	{
		CusEntryHeader EntryHeader { get; }
		TCusEntryLine FindByLineNumber(ZInt lineNumber);
		new TCusEntryLine this[int index] { get; }
	}

	public class AllCusEntryLineCollection<TCusEntryLine> : DependentBusinessObjectCollection<TCusEntryLine, CusEntryHeader>, IAllCusEntryLineCollection<TCusEntryLine>
		where TCusEntryLine : CusEntryLine
	{
		public AllCusEntryLineCollection(CusEntryHeader entryHeader) : base(entryHeader)
		{
			EntryHeader = entryHeader;
		}

		public CusEntryHeader EntryHeader { get; }

		public IEnumerator<TCusEntryLine> GetEnumerator() => Elements.Cast<TCusEntryLine>().GetEnumerator();

		public TCusEntryLine FindByLineNumber(ZInt lineNumber)
		{
			foreach (TCusEntryLine line in this)
			{
				if (line.CL_LineNumber == lineNumber)
				{
					return line;
				}
			}
			return null;
		}

		protected override bool AllowNewCore => false;
	}
}
