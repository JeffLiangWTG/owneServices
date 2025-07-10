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
	sealed class BillOfLadingDetailsTest : BillOfLadingBasePageTest
	{
		public void TestDuplicateBillOfLading()
		{
			var testPage = (TestBillOfLadingDetails)TestPage;
			testPage.OnDuplicateBillOfLadingClickForTest();
			var newAgencyBookingPK = GetRedirectReferencePK();
			var newAgencyBooking = Factory.Load<TrackingLinerAndAgencyBooking>(newAgencyBookingPK);
			AssertEquals(testPage.SiteUser.ContactAndCompanyReference, newAgencyBooking.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		public void TestReverseBillOfLading()
		{
			var testPage = (TestBillOfLadingDetails)TestPage;
			testPage.OnReverseBillOfLadingClickForTest();

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

		public void TestBillOLadingDetailsComponentsVisibiltyWhenUserIsShipmentQuickViewUser()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			using (TestBillOfLadingDetails page = new TestBillOfLadingDetails())
			{
				AssertNotNull("TestOrg", helper.TestOrg);
				AssertNotNull("TestUser", helper.TestSiteUser);
				AssertEquals("QuickViewUser is logged in", true, page.SiteUser.IsShipmentQuickViewUser);

				TrackingBillOfLading testBillOfLading = Factory.NewWithValidTestData<TrackingBillOfLading>();
				page.TestBillOfLading = testBillOfLading;
				page.SetupPageForTest();
				page.ForTest_RunOnLoad();
				page.ForTest_RunOnPreBind();

				Assert(!page.DuplicateBillOfLadingForTest.Visible);
				Assert(!page.ReverseBillOfLadingForTest.Visible);
				Assert(!page.EditButtonForTest.Visible);

				if (page.PacksGridGridForTest is ZGrid)
				{
					Assert(!((ZGrid)page.PacksGridGridForTest).ShouldShowControl);
				}

				if (page.ContainerGridForTest is ZGrid)
				{
					Assert(!((ZGrid)page.ContainerGridForTest).ShouldShowControl);
				}
			}
		}

		protected override WebSecurityRight SiteUserSecurityRight => null;

		protected override string GetExpectedPageName() => WebTracker.Pages.BillOfLadingDetails;

		protected override Control GetNewControl()
		{
			var page = new TestBillOfLadingDetails();
			page.SetupPageForTest();
			page.TestBillOfLading = Factory.NewWithValidTestData<TrackingBillOfLading>();

			return page;
		}
	}
}
