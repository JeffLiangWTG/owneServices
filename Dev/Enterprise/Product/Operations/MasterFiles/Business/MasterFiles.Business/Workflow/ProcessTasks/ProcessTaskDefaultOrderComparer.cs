using System.Collections;
using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	class ProcessTaskDefaultOrderComparer : IComparer<ProcessTask>, IComparer
	{
		public int Compare(ProcessTask t1, ProcessTask t2)
		{
			var r = t1.P9_Sequence.CompareTo(t2.P9_Sequence);
			if (r != 0)
			{
				return r;
			}
			r = t1.P9_Description.CompareTo(t2.P9_Description);
			if (r != 0)
			{
				return r;
			}
			r = t1.P9_GS_NKAssignedStaffMember.CompareTo(t2.P9_GS_NKAssignedStaffMember);
			if (r != 0)
			{
				return r;
			}
			return t1.PK.CompareTo(t2.PK);
		}

		public int Compare(object x, object y) => Compare((ProcessTask)x, (ProcessTask)y);
	}
}
