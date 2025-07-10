using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesCallAdditionalAttendeeStaffCollection : OrgSalesCallAdditionalAttendeeCollection
	{
		public OrgSalesCallAdditionalAttendeeStaffCollection(OrgSalesCall parent) : base(parent)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			return new ZQuery(OrgSalesCallAdditionalAttendeeSchema.O6_AttendeeTableCode, GlbStaffSchema.Constants.Prefix);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			OrgSalesCallAdditionalAttendee attendee = (OrgSalesCallAdditionalAttendee)child;
			attendee.O6_AttendeeTableCode = GlbStaffSchema.Constants.Prefix;
			attendee.O6_ReceiverReminder = true;
		}
	}
}
