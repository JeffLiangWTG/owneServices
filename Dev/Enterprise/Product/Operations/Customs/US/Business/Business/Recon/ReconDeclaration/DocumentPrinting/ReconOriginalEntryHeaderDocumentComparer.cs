using System.Collections.Generic;

namespace Enterprise.Customs.US.Business
{
	public class ReconOriginalEntryHeaderDocumentComparer : IComparer<ReconOriginalEntryHeader>
	{
		#region IComparer<ReconOriginalEntryHeader> Members

		public int Compare(ReconOriginalEntryHeader x, ReconOriginalEntryHeader y)
		{
			return x.CH_OrigEntryReference.CompareTo(y.CH_OrigEntryReference);
		}

		#endregion
	}
}
