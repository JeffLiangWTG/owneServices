using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class EntryLineStatusFilterCollection : SubsetBusinessObjectCollection<CusEntryLine>
	{
		public EntryLineStatusFilterCollection(CusEntryHeader entryHeader, params string[] statusFilters)
			: base((BusinessObjectCollection)entryHeader.AllEntryLines)
		{
			EntryHeader = entryHeader;
			StatusFilters = statusFilters;
			Rebuild();
		}

		public CusEntryLine FindByLineNumber(ZInt lineNumber)
		{
			foreach (CusEntryLine line in this)
			{
				if (line.CL_LineNumber == lineNumber)
				{
					return line;
				}
			}
			return null;
		}

		protected readonly CusEntryHeader EntryHeader;
		protected readonly string[] StatusFilters;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var entryLine = element as CusEntryLine;
			return entryLine != null && StatusFilters != null && StatusFilters.Contains(entryLine.CL_CustomsPostedStatus.ToString());
		}

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);

			var entryLine = (CusEntryLine)child;
			if (EntryHeader != null)
			{
				entryLine.CL_CH = EntryHeader.PK;
				entryLine.CL_ClusterKey = EntryHeader.CH_ClusterKey;
			}

			if (StatusFilters != null && !StatusFilters.Contains(entryLine.CL_CustomsPostedStatus.ToString()))
			{
				entryLine.CL_CustomsPostedStatus = StatusFilters[0];
			}
		}
	}
}
