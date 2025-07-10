using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Tracking.Web.Bookings;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class BookingDetailsTest : BasePageWithAuthorisationTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.BookingDetails;
		}

		public void TestDuplicateBooking()
		{
			var testPage = (TestBookingDetails)TestPage;
			var user = testPage.SiteUser;
			AssertNotNull(user);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "X";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			SetupSecurity(WebSecurityRightsList.WebBookingsSelectSchedules, org, contact, true);

			Factory.Save();

			var webBooking = (TrackingBooking)testPage.DataSource;

			user.Login(org.OH_Code, "user@user.com", "password");

			testPage.SetupPageForTesting();
			testPage.OnDuplicateBookingClickForTest();

			var newBookingPK = GetRedirectReferencePK();
			var newBooking = webBooking.Factory.Load<ForwardingShipment>(newBookingPK);
			AssertEquals("user@user.com (X)", newBooking.Logs.AutoCreatedLogDefaultSL_Reference);

			var storedWebBooking = (TrackingBooking)testPage.LoadDataSourceForTest(newBookingPK);
			AssertEquals(typeof(TrackingScheduleChooser), storedWebBooking.QuotedBooking.ScheduleChooser.GetType());
		}

		public void TestReverseBooking()
		{
			var testPage = (TestBookingDetails)TestPage;
			var user = testPage.SiteUser;
			AssertNotNull(user);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "X";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			SetupSecurity(WebSecurityRightsList.WebBookingsSelectSchedules, org, contact, true);

			Factory.Save();

			var webBooking = (TrackingBooking)testPage.DataSource;

			user.Login(org.OH_Code, "user@user.com", "password");

			testPage.SetupPageForTesting();
			testPage.OnReverseBookingClickForTest();

			var newBookingPK = GetRedirectReferencePK();
			var newBooking = webBooking.Factory.Load<ForwardingShipment>(newBookingPK);
			AssertEquals("user@user.com (X)", newBooking.Logs.AutoCreatedLogDefaultSL_Reference);

			var storedWebBooking = (TrackingBooking)testPage.LoadDataSourceForTest(newBookingPK);
			AssertEquals(typeof(TrackingScheduleChooser), storedWebBooking.QuotedBooking.ScheduleChooser.GetType());
		}

		public void TestScheduleChoserVisibilityDependsOnRights()
		{
			TestBookingDetails testPage = TestPage as TestBookingDetails;
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

			testPage.SetupPageForTesting();

			testPage.OnPreRenderForTest();
			Assert(testPage.SchedulesPanelForTest.Visible);

			org.SecurityRights.RemoveAndDeleteAll();
			SetupSecurity(WebSecurityRightsList.WebBookingsSelectSchedules, org, contact, false);
			Factory.Save();
			user.OnSecurityRightsChangedForTest();

			testPage.OnPreRenderForTest();
			Assert(!testPage.SchedulesPanelForTest.Visible);
		}

		public void TestButtonsWhenNoDataSource()
		{
			var page = new TestBookingDetails();
			page.ShouldCreateDataSource = false;
			page.SetupPageForTesting();
			AssertNull(page.DataSource);

			page.SetupAuthorisedContentForTesting(true);
			AssertEquals(false, page.DuplicateBookingForTest.Enabled);
			AssertEquals(false, page.ReverseBookingForTest.Enabled);
		}

		public void TestBookingDetailsComponentsVisibiltyWhenUserIsShipmentQuickViewUser()
		{
			using (var page = new TestBookingDetails())
			{
				page.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
				page.SetupPageForTesting();
				AssertEquals("QuickViewUser is logged in", true, page.SiteUser.IsShipmentQuickViewUser);

				page.OnLoadForTest();
				AssertQuickViewVisibility(page, false);

				page.SiteUser.LoginSupportForTest("TST");
				page.SetupPageForTesting();
				AssertEquals("Regular is logged in", false, page.SiteUser.IsShipmentQuickViewUser);
				page.OnLoadForTest();
				AssertQuickViewVisibility(page, true);
			}
		}

		void AssertQuickViewVisibility(TestBookingDetails page, bool expectedVisible)
		{
			AssertEquals(expectedVisible, page.EditBookingForTest.Visible);
			AssertEquals(expectedVisible, page.CancelBookingForTest.Visible);
			AssertEquals(expectedVisible, page.DuplicateBookingForTest.Visible);
			AssertEquals(expectedVisible, page.ReverseBookingForTest.Visible);
			AssertEquals(expectedVisible, page.DocsMenuForTest.Visible);

			AssertEquals(expectedVisible, page.ConsignorConsigneeRowForTest.Visible);
			AssertEquals(expectedVisible, page.ConsignorConsigneeContactRowForTest.Visible);
			AssertEquals(expectedVisible, page.ForAuthenticatedUserOnlyForTest.Visible);
			AssertEquals(expectedVisible, page.PickupAddressRowForTest.Visible);
			AssertEquals(expectedVisible, page.DeliveryAddressRowForTest.Visible);
			AssertEquals(expectedVisible, page.PickupAgentRowForTest.Visible);
			AssertEquals(expectedVisible, page.DeliveryAgentRowForTest.Visible);

			AssertEquals(expectedVisible, page.PackLinesGridForTest.ShouldShowControl);
			AssertEquals(expectedVisible, page.AttachedOrdersGridForTest.ShouldShowControl);
			AssertEquals(expectedVisible, page.ContainersDataGridForTest.ShouldShowControl);
			AssertEquals(expectedVisible, page.ReferenceDataGridForTest.ShouldShowControl);

			AssertEquals(expectedVisible, page.DetailsGoodsDescriptionForTest.Visible);
			AssertEquals(expectedVisible, page.ForAuthenticatedUserOnly2ForTest.Visible);
			AssertEquals(expectedVisible, page.PaymentTermsRowForTest.Visible);
			AssertEquals(expectedVisible, page.BillingPartyRowForTest.Visible);
			AssertEquals(expectedVisible, page.MarksAndNumbersRowForTest.Visible);
			AssertEquals(expectedVisible, page.SpecialInstructionsRowForTest.Visible);
		}

		public void TestButtonsWhenNotAuthorised()
		{
			var page = new TestBookingDetails();
			page.SetupPageForTesting();
			AssertNotNull(page.DataSource);

			page.SetupAuthorisedContentForTesting(false);

			AssertEquals(false, page.EditBookingForTest.Enabled);
			AssertEquals(false, page.CancelBookingForTest.Enabled);
			AssertEquals(false, page.DuplicateBookingForTest.Enabled);
			AssertEquals(false, page.ReverseBookingForTest.Enabled);
		}

		public void TestButtonsWhenAuthorisedWithoutAddEdit()
		{
			var page = new TestBookingDetails();
			page.SetupPageForTesting();
			AssertNotNull(page.DataSource);

			page.SetupAuthorisedContentForTesting(true);

			AssertEquals(false, page.EditBookingForTest.Enabled);
			AssertEquals(false, page.CancelBookingForTest.Enabled);
			AssertEquals(false, page.DuplicateBookingForTest.Enabled);
			AssertEquals(false, page.ReverseBookingForTest.Enabled);
			var expectedToolTip = "You are not authorized to use this function. Please contact your system administrator to request access rights.";
			AssertEquals(expectedToolTip, page.EditBookingForTest.ToolTip);
			AssertEquals(expectedToolTip, page.CancelBookingForTest.ToolTip);
			AssertEquals(expectedToolTip, page.DuplicateBookingForTest.ToolTip);
			AssertEquals(expectedToolTip, page.ReverseBookingForTest.ToolTip);
		}

		public void TestButtonsWhenAuthorisedWithAddEdit()
		{
			var page = new TestBookingDetails();
			var helper = new TestHelper(page.Factory);

			var orgRight = helper.TestOrg.SecurityRights.AddNew();
			orgRight.OX_Granted = true;
			orgRight.OX_SecurityItemName = WebSecurityRightsList.WebBookingsAddEdit.Code;
			var viewRight = helper.TestOrg.SecurityRights.AddNew();
			viewRight.OX_Granted = true;
			viewRight.OX_SecurityItemName = WebSecurityRightsList.WebBookingsView.Code;

			var userRight = helper.TestContact.SecurityRightsForBindingOnly.AddNew();
			userRight.OZ_OX = orgRight.PK;
			userRight.OZ_Granted = true;
			var userViewRight = helper.TestContact.SecurityRightsForBindingOnly.AddNew();
			userViewRight.OZ_OX = orgRight.PK;
			userViewRight.OZ_Granted = true;

			page.Factory.Save();

			page.SiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			page.SetupPageForTesting();
			AssertNotNull(page.DataSource);

			page.SetupAuthorisedContentForTesting(true);

			AssertEquals(true, page.DuplicateBookingForTest.Enabled);
			AssertEquals(true, page.ReverseBookingForTest.Enabled);
			AssertEquals(true, page.DuplicateBookingForTest.Enabled);
			AssertEquals(true, page.ReverseBookingForTest.Enabled);
			var expectedToolTip = string.Empty;
			AssertEquals(expectedToolTip, page.EditBookingForTest.ToolTip);
			AssertEquals(expectedToolTip, page.CancelBookingForTest.ToolTip);
			AssertEquals(expectedToolTip, page.DuplicateBookingForTest.ToolTip);
			AssertEquals(expectedToolTip, page.ReverseBookingForTest.ToolTip);
		}

		public void TestShowContainersGrid()
		{
			TestBookingDetails testPage = TestPage as TestBookingDetails;
			AssertNotNull("TestPage should be proper type", testPage);
			testPage.SetupPageForTesting();

			TrackingBooking webBooking = testPage.DataSource as TrackingBooking;
			AssertNotNull("DataSource should be TrackingBooking", webBooking);

			webBooking.Mode = Core.Constants.RateMode.FCL;
			WebDataRegistry.Instance.DisableBookingContainersGrid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			testPage.SetupPageForTest();
			AssertEquals(testPage.FCLForTest.Visible, true);
			AssertEquals(testPage.notFCLForTest.Visible, false);

			WebDataRegistry.Instance.DisableBookingContainersGrid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			testPage.SetupPageForTest();
			AssertEquals(testPage.FCLForTest.Visible, false);
			AssertEquals(testPage.notFCLForTest.Visible, false);

			webBooking.Mode = Core.Constants.RateMode.LCL;
			WebDataRegistry.Instance.DisableBookingContainersGrid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			testPage.SetupPageForTest();
			AssertEquals(testPage.FCLForTest.Visible, false);
			AssertEquals(testPage.notFCLForTest.Visible, true);

			WebDataRegistry.Instance.DisableBookingContainersGrid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			testPage.SetupPageForTest();
			AssertEquals(testPage.FCLForTest.Visible, false);
			AssertEquals(testPage.notFCLForTest.Visible, true);
		}

		public void TestAdditionalTermsVisiblity()
		{
			TestBookingDetails testPage = TestPage as TestBookingDetails;
			AssertNotNull("TestPage should be proper type", testPage);
			testPage.SetupPageForTesting();

			TrackingBooking webBooking = testPage.DataSource as TrackingBooking;
			AssertNotNull("DataSource should be TrackingBooking", webBooking);

			webBooking.Origin = "USORD";
			webBooking.Destination = "USLAX";
			Assert("Should be Domestic", webBooking.IsDomesticFreight);

			testPage.SetupPageForTest();
			Assert("Additional Terms should NOT be visible for Domestic Bookings", !testPage.AdditionalTermsForTest.Visible);
			AssertEquals("Term should be called Payment Term", "Payment Term:", testPage.PayTermLabelForTest.Text);

			webBooking.Destination = "AUSYD";
			Assert("Should NOT be Domestic", !webBooking.IsDomesticFreight);
			testPage.SetupPageForTest();
			Assert("Additional Terms should be visible for non-Domestic Bookings", testPage.AdditionalTermsForTest.Visible);
			AssertEquals("Term should be called Incoterm", "Incoterm:", testPage.PayTermLabelForTest.Text);
		}

		public void TestAttachedOrdersVisibility()
		{
			var testPage = TestPage as TestBookingDetails;
			testPage.SetupPageForTesting();
			var user = testPage.SiteUser;
			AssertNotNull(user);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "X";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			SetupSecurity(WebSecurityRightsList.WebOrdersView, org, contact, true);

			var booking = testPage.DataSource as TrackingBooking;
			booking.AttachedOrderLinks.AddNew();

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

		[HttpContextEnabledTest]
		public void TestCancelBooking_SaveFailed()
		{
			var testPage = TestPage as TestBookingDetails;
			testPage.SetupPageForTesting();

			testPage.DataSource.Factory.Saving += (_) => throw new Exception();

			testPage.OnCancelBookingClickForTest();

			var (key, value) = HttpContext.Current.Response.GetHeaderForTest(0);

			Assert(testPage.ZClientScript.IsStartupScriptRegistered(typeof(TestBookingDetails), "There was a problem while saving your changes. Please try again."));
			AssertEquals("Refresh", key);
			AssertEquals(string.Format("0; url={0}", string.Format("{0}?Ref={1}", testPage.AppInstance.BookingDetailsPage, ((TrackingBooking)testPage.DataSource).BookingPK)), value);
		}

		#region Overrides

		protected override void AssertAuthorisedContent(BooleanRegistryItem useModule, OrgSecurityContacts contactSecurity)
		{
			base.AssertAuthorisedContent(useModule, contactSecurity);

			AssertEquals("Cancel Button Enabled", useModule.Value && contactSecurity.OZ_Granted, ((TestBookingDetails)TestPage).CancelBookingForTest.Enabled);
			AssertEquals("Edit Button Enabled", useModule.Value && contactSecurity.OZ_Granted, ((TestBookingDetails)TestPage).EditBookingForTest.Enabled);
			AssertEquals("Consignor Label", "ShippingConsignor:", ((TestBookingDetails)TestPage).ConsignorLabelForTest.Text);
			AssertEquals("Consignor Contact Label", "ShippingConsignor Contact:", ((TestBookingDetails)TestPage).ConsignorContactLabelForTest.Text);
		}

		protected override Control GetNewControl()
		{
			return new TestBookingDetails();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebForwardingBookingsModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebBookingsView; }
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerBooking }; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			((TestBookingDetails)TestPage).SetupPageForTesting();
		}
		#endregion

		#region Test Booking

		public class TestBookingDetails : BookingDetails
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}

			protected override Uri RequestUrl
			{
				get { return new Uri("http://www.test.com/ediWeb/test.aspx?data=xyz"); }
			}

			public void SetupAuthorisedContentForTesting(bool isAuthorized)
			{
				base.SetupAuthorisedContent(isAuthorized);
			}

			public void SetupPageForTesting()
			{
				CancelBooking = new Button();
				EditBooking = new Button();
				DuplicateBooking = new Button();
				ReverseBooking = new Button();
				ContainersDataGrid = new ZGrid();
				PackLinesGrid = new ZGrid();
				notFCL = new HtmlGenericControl();
				ContainersDataGrid = new ZGrid();
				BookingCancelledDiv = new HtmlGenericControl();
				ConsignorLabel = new Label();
				ConsignorContactLabel = new Label();
				WarehouseRecRow = new HtmlTableRow();
				CustomsEntryRow = new HtmlTableRow();
				ScheduleChooser = new WebScheduleChooserControl();
				SchedulesPanel = new ZCollapsablePanel();
				Controls.Add(new HtmlForm());
				AdditionalTermsLabel = new Label();
				PayTermLabel = new Label();
				UnauthorisedDiv = new HtmlGenericControl();
				OrderReferencesLabel = new Label();
				OrderReferencesTextBox = new ZTextLabel();
				DocsMenu = new ZDocumentsMenu();
				AttachedOrdersGrid = new ZGrid();
				DocumentsGrid = new ZGrid();
				ConsignorConsigneeRow = new HtmlTableRow();
				ConsignorConsigneeContactRow = new HtmlTableRow();
				ForAuthenticatedUserOnly = new HtmlTable();
				PickupAddressRow = new HtmlTableRow();
				DeliveryAddressRow = new HtmlTableRow();
				PickupAgentRow = new HtmlTableRow();
				DeliveryAgentRow = new HtmlTableRow();
				PackLinesGrid = new ZGrid();
				ContainersDataGrid = new ZGrid();
				ReferenceDataGrid = new ZGrid();
				UnauthorisedLabel = new ZTextLabel();
				AuthorisedContent = new HtmlGenericControl();
				DetailsGoodsDescription = new HtmlTable();
				ForAuthenticatedUserOnly2 = new HtmlTable();
				PaymentTermsRow = new HtmlTableRow();
				BillingPartyRow = new HtmlTableRow();
				MarksAndNumbersRow = new HtmlTableRow();
				SpecialInstructionsRow = new HtmlTableRow();

				FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ShippingConsignor");
				AssertEquals("Registry value should be set", "ShippingConsignor", FreightDataRegistry.Instance.ConsignorShipperTerminology.Value);

				LoadOrCreateDataSource();
			}

			public bool ShouldCreateDataSource = true;

			protected override BusinessObject GetNewDataSource()
			{
				return ShouldCreateDataSource ? new TrackingBooking(Factory, SiteUser) : null;
			}

			public void SetupPageForTest()
			{
				SetupPage();
			}

			public object LoadDataSourceForTest(ZGuid indexer) => LoadDataSource(indexer);

			public void OnLoadForTest() => OnLoad(EventArgs.Empty);

			public void OnPreRenderForTest() => OnPreRender(EventArgs.Empty);

			public void OnCancelBookingClickForTest() => CancelBooking_Click(null, EventArgs.Empty);

			public void OnDuplicateBookingClickForTest() => DuplicateBooking_Click(null, EventArgs.Empty);

			public void OnReverseBookingClickForTest() => ReverseBooking_Click(null, EventArgs.Empty);

			public ZCollapsablePanel SchedulesPanelForTest
			{
				get { return SchedulesPanel; }
			}

			public Control DetailsGoodsDescriptionForTest => DetailsGoodsDescription;

			public Control ForAuthenticatedUserOnly2ForTest => ForAuthenticatedUserOnly2;

			public Control PaymentTermsRowForTest => PaymentTermsRow;

			public Control BillingPartyRowForTest => BillingPartyRow;

			public Control MarksAndNumbersRowForTest => MarksAndNumbersRow;

			public Control SpecialInstructionsRowForTest => SpecialInstructionsRow;

			public Control DocsMenuForTest => DocsMenu;

			public Control ConsignorConsigneeRowForTest => ConsignorConsigneeRow;

			public Control ConsignorConsigneeContactRowForTest => ConsignorConsigneeContactRow;

			public Control ForAuthenticatedUserOnlyForTest => ForAuthenticatedUserOnly;

			public Control PickupAddressRowForTest => PickupAddressRow;

			public Control DeliveryAddressRowForTest => DeliveryAddressRow;

			public Control PickupAgentRowForTest => PickupAgentRow;

			public Control DeliveryAgentRowForTest => DeliveryAgentRow;

			public ZGrid PackLinesGridForTest => PackLinesGrid;

			public ZGrid AttachedOrdersGridForTest => AttachedOrdersGrid;

			public ZGrid ContainersDataGridForTest => ContainersDataGrid;

			public ZGrid ReferenceDataGridForTest => ReferenceDataGrid;

			public Control FCLForTest
			{
				get { return ContainersDataGrid; }
			}

			public HtmlGenericControl notFCLForTest
			{
				get { return notFCL; }
			}

			public Button DuplicateBookingForTest
			{
				get { return DuplicateBooking; }
			}

			public Button ReverseBookingForTest
			{
				get { return ReverseBooking; }
			}

			public Button CancelBookingForTest
			{
				get { return CancelBooking; }
			}

			public Button EditBookingForTest
			{
				get { return EditBooking; }
			}

			public Control AdditionalTermsForTest
			{
				get { return AdditionalTermsLabel; }
			}

			public Label PayTermLabelForTest
			{
				get { return PayTermLabel; }
			}

			public Label ConsignorLabelForTest
			{
				get { return ConsignorLabel; }
			}

			public Label ConsignorContactLabelForTest
			{
				get { return ConsignorContactLabel; }
			}
		}
		#endregion
	}
}
