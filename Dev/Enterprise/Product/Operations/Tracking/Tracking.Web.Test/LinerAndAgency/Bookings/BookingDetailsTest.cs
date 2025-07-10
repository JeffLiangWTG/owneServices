using System.Web.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.LinerAndAgency.Testing
{
	[HttpContextEnabledTest]
	sealed class BookingDetailsTest : BookingBasePageTest
	{
		public void TestDuplicateBooking()
		{
			var testPage = (TestBookingDetails)TestPage;
			testPage.OnDuplicateBookingClickForTest();
			var newBookingPK = GetRedirectReferencePK();
			var newBooking = Factory.Load<TrackingLinerAndAgencyBooking>(newBookingPK);
			AssertEquals(testPage.SiteUser.ContactAndCompanyReference, newBooking.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		public void TestReverseBooking()
		{
			var testPage = (TestBookingDetails)TestPage;
			testPage.OnReverseBookingClickForTest();

			var newAgencyBookingPK = GetRedirectReferencePK();
			var newAgencyBooking = Factory.Load<TrackingLinerAndAgencyBooking>(newAgencyBookingPK);
			AssertEquals(testPage.SiteUser.ContactAndCompanyReference, newAgencyBooking.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		bool initialIsWeb;

		protected override void SetUp()
		{
			base.SetUp();
			initialIsWeb = Globals.IsWeb;
			Globals.IsWeb = true;
		}

		protected override void TearDown()
		{
			base.TearDown();
			Globals.IsWeb = initialIsWeb;
		}

		public void TestBookingtDetailsComponentsVisibiltyWhenUserIsShipmentQuickViewUser()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			using (TestBookingDetails page = new TestBookingDetails())
			{
				AssertNotNull("TestOrg", helper.TestOrg);
				AssertNotNull("TestUser", helper.TestSiteUser);
				AssertEquals("QuickViewUser is logged in", true, page.SiteUser.IsShipmentQuickViewUser);

				var testBooking = Factory.NewWithValidTestData<TrackingLinerAndAgencyBooking>();
				page.TestBooking = testBooking;
				page.SetupPageForTest();
				page.ForTest_RunOnLoad();
				page.ForTest_RunOnPreBind();

				Assert(!page.DuplicateBookingForTest.Visible);
				Assert(!page.ReverseBookingForTest.Visible);
				Assert(!page.EditBookingForTest.Visible);
				Assert(!page.CancelBookingForTest.Visible);

				if (page.PacksGridGridForTest is ZGrid)
				{
					Assert(!((ZGrid)page.PacksGridGridForTest).ShouldShowControl);
				}

				if (page.ContainerGridForTest is ZGrid)
				{
					Assert(!((ZGrid)page.ContainerGridForTest).ShouldShowControl);
				}

				if (page.BookedContainersGridForTest is ZGrid)
				{
					Assert(!((ZGrid)page.BookedContainersGridForTest).ShouldShowControl);
				}
			}
		}

		public void TestBookingDetailsPageAddFile()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			using (TestBookingDetails page = new TestBookingDetails())
			{
				var testBooking = Factory.NewWithValidTestData<TrackingLinerAndAgencyBooking>();
				page.TestBooking = testBooking;
				page.SetupPageForTest();
				page.ForTest_SetupDocumentsGrid();
				page.Controls.Add(page.eDocsAddNewLinkForTest);
				page.ForTest_RunOnLoad();

				var dataSourceofAddFilePage = page.Session[page.WebInterfacesHelperForTest.Key.ToString()];
				AssertNotNull(dataSourceofAddFilePage);
				Assert("The type of Add New File page's datasource should be LinerAndAgencyBaseWebInterfacesHelper", dataSourceofAddFilePage is LinerAndAgencyBaseWebInterfacesHelper);
				Assert("The type of Add New File page's datasource should not be BookingDetail Page's datasource", !(dataSourceofAddFilePage is TrackingLinerAndAgencyBooking));
			}
		}

		protected override WebSecurityRight SiteUserSecurityRight => null;

		protected override string GetExpectedPageName() => WebTracker.Pages.LinerAndAgencyBookingDetails;

		protected override Control GetNewControl()
		{
			var page = new TestBookingDetails();
			page.SetupPageForTest();
			page.TestBooking = Factory.NewWithValidTestData<TrackingLinerAndAgencyBooking>();

			return page;
		}
	}
}
