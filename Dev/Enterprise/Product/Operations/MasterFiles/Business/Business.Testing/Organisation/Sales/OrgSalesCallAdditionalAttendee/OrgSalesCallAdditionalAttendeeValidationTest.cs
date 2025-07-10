using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSalesCallAdditionalAttendeeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckO6_AttendeeID()
		{
			var call = Factory.NewWithValidTestData<OrgSalesCall>();
			var collection = new OrgSalesCallAdditionalAttendeeContactCollection(call);

			Factory.Save();

			var attendee = collection.AddNew();
			attendee.O6_OQ = call.PK;
			attendee.O6_AttendeeTableCode = OrgContactSchema.Constants.Prefix;

			attendee.O6_AttendeeID = Guid.Empty;
			AssertHasErrors(attendee.O6_AttendeeIDInfo);

			attendee.O6_AttendeeTableCode = string.Empty;
			attendee.Validation.ValidateO6_AttendeeID();
			AssertNoErrors(attendee.O6_AttendeeIDInfo);

			attendee.O6_AttendeeTableCode = GlbStaffSchema.Constants.Prefix;
			attendee.Validation.ValidateO6_AttendeeID();
			AssertHasErrors(attendee.O6_AttendeeIDInfo);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			attendee.O6_AttendeeID = staff.PK;
			Factory.Save();

			attendee.Validation.ValidateO6_AttendeeID();
			AssertEquals(false, attendee.O6_AttendeeIDInfo.HasError("This Staff is inactive - it may not be used."));
			AssertEquals(false, attendee.O6_AttendeeIDInfo.HasWarning("This Staff is inactive."));

			staff.GS_IsActive = false;

			attendee.Validation.ValidateO6_AttendeeID();
			AssertEquals(false, attendee.O6_AttendeeIDInfo.HasError("This Staff is inactive - it may not be used."));
			AssertEquals(true, attendee.O6_AttendeeIDInfo.HasWarning("This Staff is inactive."));

			attendee = Factory.NewWithValidTestData<OrgSalesCallAdditionalAttendee>();
			attendee.O6_OQ = call.PK;
			attendee.O6_AttendeeTableCode = GlbStaffSchema.Constants.Prefix;
			attendee.O6_AttendeeID = staff.PK;
			AssertEquals(true, attendee.O6_AttendeeIDInfo.HasError("This Staff is inactive - it may not be used."));
			AssertEquals(false, attendee.O6_AttendeeIDInfo.HasWarning("This Staff is inactive."));

			var attendee2 = collection.AddNew();
			attendee2.O6_AttendeeID = attendee.O6_AttendeeID;
			AssertHasErrors(attendee2.O6_AttendeeIDInfo);

			attendee2.O6_AttendeeID = Factory.NewWithValidTestData<GlbStaff>().PK;
			AssertNoErrors(attendee2.O6_AttendeeIDInfo);
		}
	}
}
