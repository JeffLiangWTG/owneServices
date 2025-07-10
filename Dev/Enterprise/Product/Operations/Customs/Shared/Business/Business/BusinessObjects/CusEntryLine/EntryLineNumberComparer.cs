using System.Collections;

namespace Enterprise.Customs.Business
{
	public class EntryLineNumberComparer : IComparer
	{
		#region IComparer Members

		public int Compare(object x, object y)
		{
			ICusEntryLine lineX = (ICusEntryLine)x;
			ICusEntryLine lineY = (ICusEntryLine)y;
			int result = lineX.CL_LineNumber.CompareTo(lineY.CL_LineNumber);
			return result;
		}

		#endregion
	}
}
