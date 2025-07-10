using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Freight.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Bookings.Testing
{
	sealed class WebScheduleChooserControlTest : TestCaseWithFactory
	{
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			testQB = QuotedBooking.New(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK, Factory);
			testBooking = new TrackingBooking(TestQB, null);
			testControl = new WebScheduleChooserControlForTest();
			testControl.CallOnInit();
			testControl.BindTo = "QuotedBooking";
			testControl.Bind(testBooking);
		}

		TrackingBooking TestBooking
		{
			get { return testBooking; }
		}
		TrackingBooking testBooking;

		QuotedBooking TestQB
		{
			get { return testQB; }
		}
		QuotedBooking testQB;

		WebScheduleChooserControlForTest TestControl
		{
			get { return testControl; }
		}
		WebScheduleChooserControlForTest testControl;

		#endregion

		public void TestCarrier()
		{
			AssertCarrier(Core.Constants.RateMode.LSE, "Booking+BindToLists+AirShippingProvider_List", WebModuleIDs.OrgAirCarrierTracking);
			AssertCarrier(Core.Constants.RateMode.ULD, "Booking+BindToLists+AirShippingProvider_List", WebModuleIDs.OrgAirCarrierTracking);
			AssertCarrier(Core.Constants.RateMode.LCL, "Booking+BindToLists+SeaShippingProvider_List", WebModuleIDs.OrgSeaCarrierTracking);
			AssertCarrier(Core.Constants.RateMode.FCL, "Booking+BindToLists+SeaShippingProvider_List", WebModuleIDs.OrgSeaCarrierTracking);
			AssertCarrier(Core.Constants.RateMode.LRO, "Booking+BindToLists+LineHaulShippingProvider_List", WebModuleIDs.OrgRoadCarrierTracking);
			AssertCarrier(Core.Constants.RateMode.FRO, "Booking+BindToLists+LineHaulShippingProvider_List", WebModuleIDs.OrgRoadCarrierTracking);
			AssertCarrier(Core.Constants.RateMode.FTL, "Booking+BindToLists+LineHaulShippingProvider_List", WebModuleIDs.OrgRoadCarrierTracking);
			AssertCarrier(Core.Constants.RateMode.LRA, "Booking+BindToLists+RailShippingProvider_List", WebModuleIDs.OrgRailCarrierTracking);
			AssertCarrier(Core.Constants.RateMode.FRA, "Booking+BindToLists+RailShippingProvider_List", WebModuleIDs.OrgRailCarrierTracking);
		}

		void AssertCarrier(string mode, string expectedBindToList, WebModuleID expectedModuleID)
		{
			testQB.Mode = mode;
			testControl.CallOnLoad();
			testControl.CallOnPreRender();
			AssertEquals(expectedBindToList, testControl.CarrierGetter.BindToList);
			AssertEquals(expectedModuleID, testControl.CarrierGetter.ModuleID);
		}

		public void TestHide()
		{
			WebScheduleChooserControlForTest testCtrl = new WebScheduleChooserControlForTest();
			testCtrl.CallOnLoad();
			Assert(testCtrl.Visible);
			Assert(!testCtrl.HeaderLabelGetter.Visible);
			Assert(!testCtrl.SailingDivGetter.Visible);
			Assert(!testCtrl.SelectScheduleBtnGetter.Visible);

			testCtrl.Visible = false;
			testCtrl.CallOnLoad();
			Assert(!testCtrl.Visible);
			Assert(!testCtrl.HeaderLabelGetter.Visible);
			Assert(!testCtrl.SailingDivGetter.Visible);
			Assert(!testCtrl.SelectScheduleBtnGetter.Visible);

			testCtrl.BindTo = "QuotedBooking";
			testCtrl.Bind(new TrackingBooking(Factory, null));

			testCtrl.CallOnLoad();
			testCtrl.CallOnPreRender();
			Assert(!testCtrl.Visible);
			Assert(testCtrl.HeaderLabelGetter.Visible);
			Assert("The DIV should not be visible because Sailing was not selected", !testCtrl.SailingDivGetter.Visible);
			Assert(testCtrl.SelectScheduleBtnGetter.Visible);

			testCtrl.Visible = true;

			Assert(testCtrl.Visible);
			Assert(testCtrl.HeaderLabelGetter.Visible);
			Assert("The DIV should not be visible because Sailing was not selected", !testCtrl.SailingDivGetter.Visible);
			Assert(testCtrl.SelectScheduleBtnGetter.Visible);
		}

		public void TestSelectScheduleBtnVisibility()
		{
			TestControl.CallOnLoad();

			Assert(TestControl.SelectScheduleBtnGetter.Visible);
			Assert(TestControl.HeaderLabelGetter.Visible);
			Assert(!TestQB.ReadOnly);

			TestBooking.Mode = string.Empty;
			TestQB.ReadOnly = true;
			TestControl.CallOnLoad();
			Assert(TestControl.Visible);
			AssertEquals("Header text should be empty when control is invisible because BookingDetails panel uses it", string.Empty, TestControl.HeaderLabelGetter.Text);

			TestBooking.Mode = Core.Constants.RateMode.LSE;
			TestControl.CallOnLoad();
			TestControl.CallOnPreRender();
			Assert(TestControl.Visible);
			Assert(TestControl.HeaderLabelGetter.Visible);
			Assert(!TestControl.SelectScheduleBtnGetter.Visible);

			testBooking = new TrackingBooking(QuotedBooking.New(ZGuid.Empty, QuotedBooking.CreateNewBooking(Factory).PK, Factory), null);
			TestControl.Bind(TestBooking);

			TestControl.ReadOnly = true;
			TestControl.CallOnLoad();
			TestControl.CallOnPreRender();

			Assert(!TestControl.SelectScheduleBtnGetter.Visible);
			Assert(TestQB.ReadOnly);
		}

		public void TestUpdatesIsDirectBookingPropertyOnBind()
		{
			TestBooking.Mode = Core.Constants.RateMode.AIR;
			TestBooking.QuotedBooking.Booking.JS_IsDirectBooking = false;

			TestControl.SetDirectForTesting(true);
			TestControl.Bind(TestBooking);
			AssertEquals(true, TestBooking.QuotedBooking.Booking.JS_IsDirectBooking);

			TestControl.SetDirectForTesting(false);
			TestControl.Bind(TestBooking);
			AssertEquals(false, TestBooking.QuotedBooking.Booking.JS_IsDirectBooking);
		}

		public void TestSailingSetup()
		{
			TestBooking.Mode = Core.Constants.RateMode.FRA;
			TestControl.CallOnLoad();
			TestControl.CallOnPreRender();

			Assert(!TestControl.SailingDivGetter.Visible);
			Assert(TestControl.CarrierGetter.Visible);
			Assert(TestControl.CFSRefGetter.Visible);
			AssertEquals(TestControl.SelectScheduleBtnGetter.ModuleID, WebModuleIDs.TrackingRailSchedules);

			TestBooking.Mode = Core.Constants.RateMode.LCL;
			JobSailing testSailing = Factory.NewWithValidTestData<JobSailing>();
			TestQB.Booking.JS_JX = testSailing.PK;
			TestQB.Booking.JS_IsDirectBooking = true;

			TestControl.CallOnLoad();
			TestControl.CallOnPreRender();

			Assert(TestControl.SailingDivGetter.Visible);
			Assert(TestControl.CarrierGetter.Visible);
			Assert(TestControl.CFSRefGetter.Visible);
			AssertEquals(TestControl.SelectScheduleBtnGetter.ModuleID, WebModuleIDs.TrackingSailingSchedules);
		}

		[ExpectNoExceptions]
		public void TestBindingNamesAreNotLocalized()
		{
			using (var mockRes = Res.UseMockData())
			{
				mockRes.SetResourceGetter(delegate(string key)
				{ return new ResourceStringData(key, "巴"); });
				testControl = new WebScheduleChooserControlForTest();
				testControl.CallOnInit();
				testControl.BindTo = "QuotedBooking";
				testControl.Bind(testBooking);
				testControl.CallOnLoad();
			}
		}
	}
}
