using System;
using System.Web.UI.HtmlControls;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.LinerAndAgency.Testing
{
	[HttpContextEnabledTest]
	sealed class EditBookingTest : BookingBasePageTest
	{
		[QueryString("Ref=deadbeef-dead-beef-dead-beefdeadbeef")]
		public void TestBookingPkDoesNotExist()
		{
			using (var page = new EditBookingForTest())
			{
				page.OnLoadForTest();

				AssertEquals(null, page.DataSource);
				AssertEquals("Booking was not found in the database or you do not have rights to access it.", page.NotFoundLabelForTest.Text);
			}
		}

		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.EditLinerAndAgencyBooking;
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new EditBookingForTest();
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebLinerAndAgencyBookingsAddEdit; }
		}

		class EditBookingForTest : EditBooking
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}

			public EditBookingForTest()
			{
				UnauthorisedLabel = new ZTextLabel();
				UnauthorisedDiv = new HtmlGenericControl();
				AuthorisedContent = new HtmlGenericControl();
				Sailing = new ZGuidFindBox();
				NotFoundLabel = new ZTextLabel();
				DataContent = new HtmlGenericControl();
			}

			public ZTextLabel NotFoundLabelForTest => NotFoundLabel;

			public void OnLoadForTest() => base.OnLoad(EventArgs.Empty);
		}
	}
}
