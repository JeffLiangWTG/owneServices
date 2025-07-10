using System.Collections.Generic;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffHolidayCollection : GlbStaffResourceTimeCollection<GlbStaffHoliday>
	{
		public GlbStaffHolidayCollection(GlbStaff staff)
			: base(staff)
		{
		}

		protected override IEnumerable<string> RecordTypes
		{
			get { yield return GlbStaffHolidayLookups.RecordTypes.Leave; }
		}
	}
}
