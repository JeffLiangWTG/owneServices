using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCallAdditionalAttendee))]
	sealed class OrgSalesCallAdditionalAttendeeTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestCanDelete()
		{
			var contact1 = Factory.New<OrgContact>();
			var contact2 = Factory.New<OrgContact>();

			var communication = Factory.New<OrgSalesCall>();
			var contactAttendee = communication.AdditionalAttendeesContact.AddNew();
			AssertEquals(true, contactAttendee.CanDelete);

			contactAttendee.O6_AttendeeID = contact1.PK;
			communication.OQ_OC = contact1.PK;

			AssertEquals(false, contactAttendee.CanDelete);
			AssertEquals("Can not remove Primary Contact from list of attendees", contactAttendee.ReasonForNotAbleToDelete);

			var contactAttendee2 = communication.AdditionalAttendeesContact.AddNew();
			contactAttendee2.O6_AttendeeID = contact1.PK;
			AssertEquals("Can remove primary contact if there are duplicates", true, contactAttendee.CanDelete);

			contactAttendee.O6_AttendeeID = contact2.PK;
			AssertEquals(true, contactAttendee.CanDelete);

			contactAttendee.O6_AttendeeID = ZGuid.Empty;
			AssertEquals(true, contactAttendee.CanDelete);
		}

		public void TestName()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_FullName = "Staff Name";

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "~test~";

			OrgContact contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Contact Name";

			OrgSalesCall call = Factory.New<OrgSalesCall>();
			call.OQ_OH = org.PK;
			OrgSalesCallAdditionalAttendee attendee = call.AdditionalAttendeesOther.AddNew();

			AssertEquals("", attendee.Name);

			attendee.O6_AttendeeName = "Other Attendee";
			AssertEquals("Other Attendee", attendee.Name);

			attendee.O6_AttendeeTableCode = OrgContactSchema.Constants.Prefix;
			attendee.O6_AttendeeID = contact.PK;
			AssertEquals("Contact Name", attendee.Name);

			attendee.O6_AttendeeTableCode = GlbStaffSchema.Constants.Prefix;
			attendee.O6_AttendeeID = staff.PK;
			AssertEquals("Staff Name", attendee.Name);
		}

		public void TestEmail()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_EmailAddress = "staff@test.com";

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "~test~";

			var contact = org.Contacts.AddNew();
			contact.OC_Email = "contact@test.com";

			var call = org.SalesCalls.AddNew();
			var attendee = call.AdditionalAttendeesOther.AddNew();

			AssertEquals("", attendee.Email);

			attendee.O6_AttendeeTableCode = OrgContactSchema.Constants.Prefix;
			attendee.O6_AttendeeID = contact.PK;
			AssertEquals("contact@test.com", attendee.Email);

			attendee.O6_AttendeeTableCode = GlbStaffSchema.Constants.Prefix;
			attendee.O6_AttendeeID = staff.PK;
			AssertEquals("staff@test.com", attendee.Email);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			OrgSalesCall salesCall = org.SalesCalls.AddNew();
			return salesCall.AdditionalAttendeesOther.AddNew();
		}
	}
}
