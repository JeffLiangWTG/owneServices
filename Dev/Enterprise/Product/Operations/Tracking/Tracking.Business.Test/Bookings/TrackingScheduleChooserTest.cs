using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(TrackingScheduleChooser))]
	public class TrackingScheduleChooserTest : NonPersistentBusinessObjectTestCase
	{
		public void TestHasNextSailing()
		{
			AssertEquals(false, TestScheduleChooser.HasNextSailing);

			TestScheduleChooser.NextSailingPKForTesting = ZGuid.Empty;
			AssertEquals(false, TestScheduleChooser.HasNextSailing);

			TestScheduleChooser.NextSailingPKForTesting = ZGuid.Invalid;
			AssertEquals(false, TestScheduleChooser.HasNextSailing);

			TestScheduleChooser.NextSailingPKForTesting = new ZGuid("ef3d92b6-4d33-44f9-922e-8cf585783097");
			AssertEquals(true, TestScheduleChooser.HasNextSailing);
		}

		public void TestSetNextSailing()
		{
			var nextSailingPK = new ZGuid("ef3d92b6-4d33-44f9-922e-8cf585783097");
			TestScheduleChooser.NextSailingPKForTesting = nextSailingPK;
			AssertNotEquals(nextSailingPK, TestBooking.QuotedBooking.Booking.JS_JX);

			TestScheduleChooser.SetNextSailing();
			AssertEquals(nextSailingPK, TestBooking.QuotedBooking.Booking.JS_JX);
			AssertEquals(ZGuid.Empty, TestScheduleChooser.NextSailingPKForTesting);
		}

		public void TestClearNextSailing()
		{
			TestScheduleChooser.NextSailingPKForTesting = new ZGuid("ef3d92b6-4d33-44f9-922e-8cf585783097");
			AssertNotEquals(ZGuid.Empty, TestScheduleChooser.NextSailingPKForTesting);

			TestScheduleChooser.ClearNextSailing();
			AssertEquals(ZGuid.Empty, TestScheduleChooser.NextSailingPKForTesting);
		}

		public void TestCanBookSailingSecurity()
		{
			AssertEquals(false, TestScheduleChooser.HasNextSailing);
			var orgSecurity = TestSiteUser.LoggedInOrganisation.SecurityRights.Cast<OrgSecurity>().FirstOrDefault(r => r.OX_SecurityItemName == WebSecurityRightsList.WebBookingsSelectSchedules.Code);
			orgSecurity.OX_Granted = false;
			var contactSecurity = TestSiteUser.LoggedInOrgContact.SecurityRightsForBindingOnly.Cast<OrgSecurityContacts>().FirstOrDefault(r => r.OZ_OX == orgSecurity.PK);
			contactSecurity.OZ_Granted = false;

			var nextSailingPK = new ZGuid("ef3d92b6-4d33-44f9-922e-8cf585783097");
			TestScheduleChooser.CallSetNextSailingForTesting(nextSailingPK);
			AssertEquals(false, TestScheduleChooser.HasNextSailing);

			orgSecurity.OX_Granted = true;
			TestScheduleChooser.CallSetNextSailingForTesting(nextSailingPK);
			AssertEquals(false, TestScheduleChooser.HasNextSailing);

			contactSecurity.OZ_Granted = true;
			TestSiteUser.OnSecurityRightsChangedForTest();
			TestScheduleChooser.CallSetNextSailingForTesting(nextSailingPK);
			AssertEquals(true, TestScheduleChooser.HasNextSailing);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestSiteUser = GetNewSiteUser();

			TestBooking = new TrackingBooking(Factory, TestSiteUser);
			TestBooking.QuotedBooking.TransportMode = TransportModes.Sea;

			TestScheduleChooser = (TrackingScheduleChooser)GetNewBusinessObject();
		}

		TrackingSiteUser GetNewSiteUser()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test@test.com";
			var password = "12345";
			contact.SetHashedPassword(password);
			contact.OC_WebAccessEnabled = true;
			Factory.Save();

			var user = new TrackingSiteUser();
			user.Login(contact.OrganisationCode, contact.OC_Email, password);
			AssertEquals("Precondition: User is logged in", true, user.IsLoggedIn);

			return user;
		}

		protected override BusinessObject GetNewBusinessObject() => new TrackingScheduleChooser(TestBooking.QuotedBooking, TestSiteUser);

		TrackingSiteUser TestSiteUser;
		TrackingScheduleChooser TestScheduleChooser;
		TrackingBooking TestBooking;
	}
}
