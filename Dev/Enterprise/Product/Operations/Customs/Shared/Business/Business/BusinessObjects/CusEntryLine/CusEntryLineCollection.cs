using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ICusEntryLineCollection<out TCusEntryLine> : IBusinessObjectCollection<TCusEntryLine>
		where TCusEntryLine : CusEntryLine
	{
		int InvoiceLineCount { get; }
		bool AllowNewCore { get; }
		void Sort<T>(IComparer<T> comparison) where T : BusinessObject;
		void CustomSort();
		void Rebuild();
		new TCusEntryLine this[int index] { get; }
		new TCusEntryLine AddNew();
		IComparer GetComparer();
		CusEntryLine FindByLineNumber(ZInt lineNumber);
	}

	public class CusEntryLineCollection<TCusEntryLine> : EntryLineStatusFilterCollection, ICusEntryLineCollection<TCusEntryLine>
		where TCusEntryLine : CusEntryLine
	{
		public CusEntryLineCollection(CusEntryHeader entryHeader)
			: base(entryHeader, EntryLineStatusList.Codes.Active)
		{
		}

		public CusEntryLineCollection(CusEntryHeader entryHeader, string[] statusFilters)
			: base(entryHeader, statusFilters)
		{
		}

		public new TCusEntryLine this[int index] => (TCusEntryLine)Elements[index];

		public new TCusEntryLine AddNew() => (TCusEntryLine)base.AddNew();

		public new TCusEntryLine FindByLineNumber(ZInt lineNumber) => (TCusEntryLine)base.FindByLineNumber(lineNumber);

		public int InvoiceLineCount
		{
			get
			{
				var result = 0;
				foreach (CusEntryLine mergedLine in this)
				{
					result += mergedLine.InvoiceLines.Count;
				}
				return result;
			}
		}

		public IEnumerator<TCusEntryLine> GetEnumerator() => Elements.Cast<TCusEntryLine>().GetEnumerator();

		/// <summary>
		/// Sort by CL_LineNumber. Override GetComparer() if you want different sorting.
		/// </summary>
		public void CustomSort() => Sort(GetComparer());

		protected virtual IComparer GetComparer() => new CusEntryLineComparer();

		IComparer ICusEntryLineCollection<TCusEntryLine>.GetComparer() => GetComparer();

		class CusEntryLineComparer : IComparer
		{
			public int Compare(object x, object y)
			{
				var lineX = (CusEntryLine)x;
				var lineY = (CusEntryLine)y;
				return lineX.CL_LineNumber.CompareTo(lineY.CL_LineNumber);
			}
		}

		protected override bool AllowNewCore => false;

		bool ICusEntryLineCollection<TCusEntryLine>.AllowNewCore => AllowNewCore;
	}
}
