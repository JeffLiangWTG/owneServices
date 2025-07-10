using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public class GlbTimeAllocationCollection : GlbStaffResourceTimeCollection<GlbTimeAllocation>
	{
		public GlbTimeAllocationCollection(GlbStaff staff)
			: base(staff)
		{
		}

		protected override IEnumerable<string> RecordTypes
		{
			get { yield return GlbStaffHolidayLookups.RecordTypes.TimeAllocation; }
		}
	}
}
