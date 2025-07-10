using System.Collections.Generic;
using CargoWise.Definitions;

namespace Enterprise.MasterFiles.Business
{
	public class GlbStaffHolidayIncBMSLeaveCollection : GlbStaffResourceTimeCollection<GlbStaffHoliday>
	{
		public GlbStaffHolidayIncBMSLeaveCollection(GlbStaff staff)
			: base(staff)
		{
		}

		protected override IEnumerable<string> RecordTypes
		{
			get
			{
				yield return GlbStaffHolidayLookups.RecordTypes.Leave;
				yield return StaffHolidayRecordTypeCodes.BufferManagementLeave;
			}
		}
	}
}
