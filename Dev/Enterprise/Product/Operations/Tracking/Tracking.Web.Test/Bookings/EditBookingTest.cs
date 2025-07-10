using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class EditBookingTest : BasePageWithAuthorisationTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.EditBooking;
		}

		public void TestPackLineGridAddOn()
		{
			using (var testPage = new EditBookingForTest())
			{
				testPage.SetupPageForTesting();
				testPage.SetupGridsForTest();

				var packLineGridAddOn = testPage.Controls.OfType<BookingPackLineGridAddOn>().FirstOrDefault();
				AssertNotNull(packLineGridAddOn);
				AssertEquals(testPage.PackLinesGridForTest, packLineGridAddOn.Grid);
			}
		}

		public void TestPanelThirdParyAddress()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "X";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			Factory.Save();

			using (EditBookingForTest testPage = new EditBookingForTest())
			{
				TrackingSiteUser user = testPage.SiteUser;

				AssertNotNull(user);

				user.Login(org.OH_Code, "user@user.com", "password");

				AssertNotNull(user.LoggedInOrganisation);

				testPage.SetupPageForTesting();
				testPage.Session[testPage.DefaultModeKeyForTest] = Constants.RateMode.LCL;
				testPage.SetDefaultModeForTest();

				testPage.BookingForTest.IsDomesticFreight = true;
				testPage.OnLoadForTest();
				Assert(!testPage.ForTestPanelThirdParyAddress.Visible);

				testPage.BookingForTest.IsDomesticFreight = true;
				testPage.BookingForTest.INCO = Constants.DomesticPaymentTerms.CollectThirdParty;
				testPage.OnLoadForTest();
				Assert(testPage.ForTestPanelThirdParyAddress.Visible);
				Assert(!testPage.ForTestPanelThirdParyAddress.RenderCaption);

				testPage.BookingForTest.IsDomesticFreight = false;
				testPage.OnLoadForTest();
				Assert(testPage.ForTestPanelThirdParyAddress.Visible);
				Assert(testPage.ForTestPanelThirdParyAddress.RenderCaption);
			}
		}

		public void TestScheduleChoserVisibilityDependsOnRights()
		{
			EditBookingForTest testPage = TestPage as EditBookingForTest;
			testPage.SetupPageForTesting();
			TrackingSiteUser user = testPage.SiteUser;
			AssertNotNull(user);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "X";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			SetupSecurity(WebSecurityRightsList.WebBookingsSelectSchedules, org, contact, true);

			Factory.Save();

			user.Login(org.OH_Code, "user@user.com", "password");

			testPage.OnLoadForTest();
			Assert(testPage.ScheduleChooserForTest.Visible);

			org.SecurityRights.RemoveAndDeleteAll();
			SetupSecurity(WebSecurityRightsList.WebBookingsSelectSchedules, org, contact, false);
			Factory.Save();
			user.OnSecurityRightsChangedForTest();

			testPage.OnLoadForTest();
			Assert(!testPage.ScheduleChooserForTest.Visible);
		}

		public void TestFCL()
		{
			EditBookingForTest testPage = TestPage as EditBookingForTest;
			testPage.SetupPageForTesting();
			testPage.FCLForTest = null;
			AssertNoExceptionThrown(() => testPage.OnLoadForTest());
		}

		public void TestNonExistentBookingShowsErrorMessage()
		{
			var testPage = new EditBookingWithNullDataSourceForTest();
			testPage.SetupPageForTesting();
			AssertNoExceptionThrown(() => testPage.OnLoadForTest());
			Assert(!testPage.AuthorisedContentForTest.Visible);
			Assert(testPage.NoBookingDivForTest.Visible);
			Assert(testPage.NoBookingLabelForTest.Visible);
			AssertEquals("The booking does not exist.", testPage.NoBookingLabelForTest.Text);
		}

		public void TestOnLoadRedirectsToTermsAndConditions()
		{
			EditBookingForTest testPage = new EditBookingForTest();

			AssertNull("Should be not`redirected", HttpContext.Current.Response.RedirectLocation);
			AssertNull("Flag in Session should be null", testPage.Session[Global.BookingTermsAndConditionsIndexer]);
			Assert("Terms and Conditions Text should be empty", string.IsNullOrEmpty(WebDataRegistry.Instance.BookingTermsAndConditions.Value));
			testPage.Session[Global.BookingTermsAndConditionsIndexer] = true;

			testPage.RedirectToTermsAndConditionsForTest();
			AssertNull("Should be not`redirected", HttpContext.Current.Response.RedirectLocation);
			AssertNull("Flag in Session should be cleared to null", testPage.Session[Global.BookingTermsAndConditionsIndexer]);

			testPage.RedirectToTermsAndConditionsForTest();
			AssertNull("Should be not`redirected", HttpContext.Current.Response.RedirectLocation);

			WebDataRegistry.Instance.BookingTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some Terms and Conditions");
			AssertNull("Flag in Session should be null", testPage.Session[Global.BookingTermsAndConditionsIndexer]);

			testPage.RedirectToTermsAndConditionsForTest();
			string expectedUrl = string.Format("{0}?{1}={2}", testPage.AppInstance.TermsAndConditionsPage, Global.TermsAndConditionsRedirectUrlTag, testPage.AppInstance.EditBookingPage);
			AssertEquals("Response should be redirected to the expected Url", expectedUrl, HttpContext.Current.Response.RedirectLocation);
		}

		public void TestSettingAndSavingModeForTrackingBooking()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "X";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			Factory.Save();

			using (EditBookingForTest testPage = new EditBookingForTest())
			{
				TrackingSiteUser user = testPage.SiteUser;

				AssertNotNull(user);

				user.Login(org.OH_Code, "user@user.com", "password");

				AssertNotNull(user.LoggedInOrganisation);

				testPage.SetupPageForTesting();
				testPage.Session[testPage.DefaultModeKeyForTest] = Constants.RateMode.LCL;
				testPage.SetDefaultModeForTest();

				AssertNotNull(testPage.BookingForTest.Mode);

				AssertEquals(Constants.RateMode.LCL, testPage.BookingForTest.Mode);

				testPage.Session[testPage.DefaultModeKeyForTest] = null;
				AssertNull(testPage.Session[testPage.DefaultModeKeyForTest]);
				testPage.SaveDefaultModeForTest();
				AssertEquals("Mode of DataSource (TrackingBooking) should be saved", testPage.BookingForTest.Mode, testPage.Session[testPage.DefaultModeKeyForTest]);
			}
		}

		public void TestAdditionalTermsVisiblity()
		{
			EditBookingForTest testPage = TestPage as EditBookingForTest;
			AssertNotNull("TestPage should be proper type", testPage);
			testPage.SetupPageForTesting();

			TrackingBooking webBooking = testPage.DataSource as TrackingBooking;
			AssertNotNull("DataSource should be TrackingBooking", webBooking);

			webBooking.Origin = "USORD";
			webBooking.Destination = "USLAX";
			Assert("Should be Domestic", webBooking.IsDomesticFreight);

			testPage.OnLoadForTest();
			Assert("Additional Terms should NOT be visible for Domestic Bookings", !testPage.AdditionalTermsRowForTest.Visible);
			AssertEquals("Term should be called Payment Term", "Payment Term:", testPage.PayTermLabelForTest.Text);

			webBooking.Destination = "AUSYD";
			Assert("Should NOT be Domestic", !webBooking.IsDomesticFreight);
			testPage.OnLoadForTest();
			Assert("Additional Terms should be visible for non-Domestic Bookings", testPage.AdditionalTermsRowForTest.Visible);
			AssertEquals("Term should be called Incoterm", "Incoterm:", testPage.PayTermLabelForTest.Text);
		}

		#region TestSaveDefaultSettingsForDocAddress

		public void TestSaveDefaultSettingsForDocAddress()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "X";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			Factory.Save();

			using (EditBookingForTest testPage = new EditBookingForTest())
			{
				TrackingSiteUser user = testPage.SiteUser;

				AssertNotNull(user);

				user.Login(org.OH_Code, contact.OC_Email, contact.PasswordForTesting);

				AssertNotNull(user.LoggedInOrganisation);

				testPage.SetupPageForTesting();
				TrackingBooking booking = (TrackingBooking)testPage.DataSource;

				AssertSaveDefaultSettingsForDocAddress(testPage, booking.ConsignorPickupAddress, true, org);
				AssertSaveDefaultSettingsForDocAddress(testPage, booking.ConsigneeDeliveryAddress, false, org);
			}
		}

		void AssertSaveDefaultSettingsForDocAddress(EditBookingForTest testPage, JobDocAddress jobDocAddress, bool isConsignor, OrgHeader org)
		{
			testPage.SiteUser.LoggedInOrganisation.OH_IsConsignee = !isConsignor;
			testPage.SiteUser.LoggedInOrganisation.OH_IsConsignor = isConsignor;

			jobDocAddress.OrganisationPK = org.PK;
			jobDocAddress.E2_OA_Address = org.Addresses[0].PK;
			jobDocAddress.ContactPK = org.Contacts[0].PK;

			testPage.SaveDefaultSettingsForDocAddressForTest();

			WebUserDefaultSettingsForDocAddress settings = Env.Registry.GetWebUserDefaultSettingsForDocAddress(testPage.SiteUser.LoggedInUser.PK.ToGuid());

			AssertEquals("OrganisationPK", jobDocAddress.OrganisationPK, settings.OrganisationPK);
			AssertEquals("AddressPK", jobDocAddress.E2_OA_Address, settings.AddressPK);
			AssertEquals("ContactPK", jobDocAddress.ContactPK, settings.ContactPK);
			AssertEquals("DocAddressType", jobDocAddress.DocAddressType.ToString(), settings.DocAddressType);
		}

		#endregion

		public void TestFooterIsSurelyShownWhenFCLModeIsSet()
		{
			WebDataRegistry.Instance.DisableBookingContainersGrid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (EditBookingForTest testPage = TestPage as EditBookingForTest)
			{
				testPage.SetupPageForTesting();

				testPage.ContainersDataGridForTest.ShowFooter = false;
				testPage.BookingForTest.Mode = Constants.RateMode.LCL;

				testPage.OnLoadForTest();

				AssertEquals("Precondition", false, testPage.ContainersDataGridForTest.ShowFooter);
				AssertEquals("Precondition", false, testPage.FCLForTest.Visible);

				testPage.BookingForTest.Mode = Constants.RateMode.FCL;

				testPage.OnLoadForTest();

				AssertEquals("Footer should be shown because mode is set to FCL", true, testPage.ContainersDataGridForTest.ShowFooter);
				AssertEquals("Containers grid should be visible because mode is set to FCL", true, testPage.FCLForTest.Visible);
			}
		}

		public void TestShowContainersGrid()
		{
			EditBookingForTest testPage = TestPage as EditBookingForTest;
			AssertNotNull("TestPage should be proper type", testPage);
			testPage.SetupPageForTesting();

			TrackingBooking webBooking = testPage.DataSource as TrackingBooking;
			AssertNotNull("DataSource should be TrackingBooking", webBooking);

			webBooking.Mode = Constants.RateMode.FCL;
			WebDataRegistry.Instance.DisableBookingContainersGrid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			testPage.OnLoadForTest();
			AssertEquals(testPage.FCLForTest.Visible, true);
			AssertEquals(testPage.notFCLForTest.Visible, false);

			WebDataRegistry.Instance.DisableBookingContainersGrid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			testPage.OnLoadForTest();
			AssertEquals(testPage.FCLForTest.Visible, false);
			AssertEquals(testPage.notFCLForTest.Visible, false);

			webBooking.Mode = Constants.RateMode.LCL;
			WebDataRegistry.Instance.DisableBookingContainersGrid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			testPage.OnLoadForTest();
			AssertEquals(testPage.FCLForTest.Visible, false);
			AssertEquals(testPage.notFCLForTest.Visible, true);

			WebDataRegistry.Instance.DisableBookingContainersGrid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			testPage.OnLoadForTest();
			AssertEquals(testPage.FCLForTest.Visible, false);
			AssertEquals(testPage.notFCLForTest.Visible, true);
		}

		public void TestEditContainerNumbers()
		{
			var testPage = TestPage as EditBookingForTest;
			testPage.SetupPageForTesting();
			var user = testPage.SiteUser;
			AssertNotNull(user);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "X";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "Y";
			var contact2 = org2.Contacts.AddNew();
			contact2.OC_ContactName = "user2";
			contact2.OC_Email = "user2@user.com";
			contact2.SetHashedPassword("password");
			contact2.OC_WebAccessEnabled = true;

			SetupSecurity(WebSecurityRightsList.WebBookingsSelectSchedules, org, contact, true);
			SetupSecurity(WebSecurityRightsList.WebBookingsSelectSchedules, org2, contact2, false);

			Factory.Save();

			user.Login(org.OH_Code, "user@user.com", "password");

			testPage.SetupContainersGridForTest();
			ZTextEditColumn numColumn = testPage.ContainersDataGridForTest.Columns[0] as ZTextEditColumn;
			AssertNotNull(numColumn);
			AssertEquals(null, numColumn.EditItemTemplate);

			user.Logout();
			user.Login(org.OH_Code, "user2@user.com", "password");

			testPage.SetupContainersGridForTest();
			numColumn = testPage.ContainersDataGridForTest.Columns[0] as ZTextEditColumn;
			AssertNotNull(numColumn);
			AssertEquals(true, numColumn.EditItemTemplate is ZTextEditColumnItemTemplate);
		}

		public void TestSetBookingPartyDocumentaryAddress()
		{
			EditBookingForTest testPage = TestPage as EditBookingForTest;
			AssertNotNull("TestPage should be proper type", testPage);

			TrackingSiteUser user = testPage.SiteUser;
			AssertNotNull(user);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "X";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			SetupSecurity(WebSecurityRightsList.WebContainersAddEditNumber, org, contact, true);

			Factory.Save();

			user.Login(org.OH_Code, "user@user.com", "password");
			testPage.SetupPageForTesting();
			TrackingBooking webBooking = testPage.DataSource as TrackingBooking;

			Factory.Save();

			testPage.OnLoadForTest();
			AssertNotNull("DataSource should be TrackingBooking", webBooking);
			AssertEquals(testPage.SiteUser.LoggedInOrganisation.PK.ToString(), webBooking.BookingPartyDocumentaryAddress.OrganisationNameOrPK);

			user.LoggedInUser.SecurityRightsForBindingOnly.RemoveAll();
			webBooking = testPage.DataSource as TrackingBooking;
			testPage.OnLoadForTest();
			AssertEquals(testPage.SiteUser.LoggedInOrganisation.PK.ToString(), webBooking.BookingPartyDocumentaryAddress.OrganisationNameOrPK);
		}

		public void TestMakeBooking_ClickWithNullDataSource()
		{
			EditBookingForTest testPage = TestPage as EditBookingForTest;
			AssertNotNull("TestPage should be proper type", testPage);
			testPage.MakeBooking_Click_ExposedforTesting();
		}

		public void TestMakeBooking_ClickWithNonLinkedOrders()
		{
			var testPage = (EditBookingForTest)TestPage;
			testPage.SetupPageForTesting();
			testPage.LinkPackLinesConfirmationsForTest.ResponseHolder.Value = "Yes";
			testPage.Session["nonLinkedOrders"] = new List<Order> { Factory.NewWithValidTestData<Order>() };

			testPage.MakeBooking_Click_ExposedforTesting();

			var savedBooking = testPage.Session[testPage.DataSource.PK.ToString()];
			AssertEquals(testPage.DataSource, savedBooking);
			Assert(testPage.ZClientScript.IsStartupScriptRegistered(typeof(EditBookingForTest), "OpenPopupScript"));
		}

		[HttpContextEnabledTest]
		public void TestSetupPackLinesGridPaging()
		{
			var testPage = TestPage as EditBookingForTest;
			testPage.SetupPageForTesting();

			using (WebDataRegistry.Instance.BookingPackLinesPageSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0m))
			{
				testPage.SetupGridsForTest();
				AssertEquals(false, testPage.PackLinesGridForTest.AllowPaging);
			}

			using (WebDataRegistry.Instance.BookingPackLinesPageSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10m))
			{
				testPage.SetupGridsForTest();
				AssertEquals(true, testPage.PackLinesGridForTest.AllowPaging);
				AssertEquals(10, testPage.PackLinesGridForTest.PageSize);
			}

			using (WebDataRegistry.Instance.BookingPackLinesPageSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5m))
			{
				testPage.SetupGridsForTest();
				AssertEquals(true, testPage.PackLinesGridForTest.AllowPaging);
				AssertEquals(5, testPage.PackLinesGridForTest.PageSize);
			}

			using (WebDataRegistry.Instance.BookingPackLinesPageSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0m))
			{
				testPage.SetupGridsForTest();
				AssertEquals(false, testPage.PackLinesGridForTest.AllowPaging);
			}
		}

		public void TestAttachedOrdersVisibility()
		{
			var testPage = TestPage as EditBookingForTest;
			testPage.SetupPageForTesting();
			TrackingSiteUser user = testPage.SiteUser;
			AssertNotNull(user);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "X";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			SetupSecurity(WebSecurityRightsList.WebOrdersView, org, contact, true);

			Factory.Save();

			user.Login(org.OH_Code, "user@user.com", "password");

			testPage.OnLoadForTest();

			AssertEquals(true, testPage.AttachedOrdersGridForTest.Visible);

			org.SecurityRights.RemoveAndDeleteAll();
			SetupSecurity(WebSecurityRightsList.WebOrdersView, org, contact, false);
			Factory.Save();
			user.OnSecurityRightsChangedForTest();

			testPage.OnLoadForTest();

			AssertEquals(false, testPage.AttachedOrdersGridForTest.Visible);
		}

		public void TestAddLogForContactCreatedAfterSaveDataSourceFactory()
		{
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testPage = TestPage as EditBookingForTest;
				testPage.SetupPageForTesting();
				SetupBookingForSaving(testPage.BookingForTest);
				testPage.NotificationFlags.DisplayMessageErrors = false;
				var result = testPage.SaveDataSourceFactory();

				Assert(result);
				var logs = testPage.SiteUser.LoggedInOrgContact.Logs;
				Assert("Log module change should show add",
					logs.HasLogWith(x => x.SL_SE_NKEvent == "ADD" && x.SL_Reference == "Booking S00001000"));
			}
		}

		public void TestEditLogForContactCreatedAfterSaveDataSourceFactory()
		{
			using (WebDataRegistry.Instance.WebActivityLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testPage = TestPage as EditBookingForTest;
				testPage.SetupPageForTesting();
				SetupBookingForSaving(testPage.BookingForTest);
				testPage.NotificationFlags.DisplayMessageErrors = false;
				testPage.BookingForTest.Factory.Save();

				testPage.BookingForTest.ConsigneeDeliveryAddress.E2_Address1 = "123 fake";
				var result = testPage.SaveDataSourceFactory();

				Assert(result);
				var logs = testPage.SiteUser.LoggedInOrgContact.Logs;
				Assert("Log module change should show edit",
					logs.HasLogWith(x => x.SL_SE_NKEvent == "EDT" && x.SL_Reference == "Booking S00001000"));
			}
		}

		void SetupBookingForSaving(TrackingBooking booking)
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;

			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			Factory.Save();

			booking.ActualWeight = 10m;
			booking.ActualVolume = 5m;
			booking.QuotedBooking.Booking.CustomsEntryNumberType = string.Empty;

			booking.Mode = Constants.TransportModes.Air;

			booking.ConsigneeOrganisationPK = consignee.PK;
			booking.ConsigneeDeliveryAddress.E2_Address1 = "1234";
			booking.ConsigneeDeliveryAddress.E2_CompanyName = "2323";
			booking.ConsigneeDeliveryAddress.E2_City = "Chicago";
			booking.ConsigneeDeliveryAddress.E2_RN_NKCountryCode = "US";
			booking.ConsigneeDeliveryAddress.E2_State = "IL";
			booking.ConsigneeDeliveryAddress.E2_Postcode = "60084";

			booking.ConsignorOrganisationPK = consignor.PK;
			booking.ConsignorPickupAddress.E2_Address1 = "1234";
			booking.ConsignorPickupAddress.E2_CompanyName = "2323";
			booking.ConsignorPickupAddress.E2_City = "Los Angeles";
			booking.ConsignorPickupAddress.E2_RN_NKCountryCode = "US";
			booking.ConsignorPickupAddress.E2_State = "CA";
			booking.ConsignorPickupAddress.E2_Postcode = "90072";

			booking.QuotedBooking.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;

			booking.Origin = "USORD";
			booking.Destination = "USLAX";
		}

		#region Overrides

		protected override System.Web.UI.Control GetNewControl()
		{
			return new EditBookingForTest();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebForwardingBookingsModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebBookingsAddEdit; }
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return new ILicenceCheckpoint[] { Env.Licence.WebTrackerBooking }; }
		}

		#endregion
	}
}
