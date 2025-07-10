using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.HtmlControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Web.Testing;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Base
{
	[HttpContextEnabledTest]
	class PageHeaderWithNavigationTest : TrackingWebControlTest
	{
		protected override System.Web.UI.Control GetNewControl()
		{
			return new DummyPageHeaderWithNavigation();
		}

		public void TestGetNavigationBar()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			testOrg.OH_Code = "XXXYYYZZZ";
			OrgContact testContact = testOrg.Contacts.AddNew();
			testContact.OC_Email = "test@cargowise.com";
			testContact.SetHashedPassword("test");
			testContact.OC_WebAccessEnabled = true;
			testOrg.OH_IsWarehouseClient = true;
			testOrg.OH_IsForwarder = true;

			WebDataRegistry.Instance.WebTrackerSiteTermsAndConditions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Some Terms and Conditions");

			Factory.Save();

			Page.SiteUser.Login("XXXYYYZZZ", "test@cargowise.com", "test");
			Assert("WebUser should be logged in", Page.SiteUser.IsLoggedIn);

			AssertNotNull("Control should not be null", NavigationControl);
			ZNavigationBar navBar = new ZNavigationBar();
			NavigationControl.SetupNavigationBar(navBar);

			AssertNotNull(navBar);

			AssertEquals("Should contain 9 top level elements", 9, navBar.NavigationElements.Count);

			// FORWARDING
			AssertEquals("First menu item should be Forwarding", "Forwarding", navBar.NavigationElements[0].Title);
			AssertEquals("Forwarding menu item should not have a URL", "#", navBar.NavigationElements[0].URL);
			AssertEquals("Forwarding submenu items", 12, navBar.NavigationElements[0].SubMenuItems.Count);

			AssertEquals("Forwarding should link to Shipments", "Shipments", navBar.NavigationElements[0].SubMenuItems[0].Title);
			AssertEquals("Shipments URL", "/Shipments/Shipments.aspx", navBar.NavigationElements[0].SubMenuItems[0].URL);

			AssertEquals("Forwarding should link to Bookings", "Bookings", navBar.NavigationElements[0].SubMenuItems[1].Title);
			AssertEquals("Bookings URL", "/Bookings/Bookings.aspx", navBar.NavigationElements[0].SubMenuItems[1].URL);

			AssertEquals("Forwarding should link to Orders", "Orders", navBar.NavigationElements[0].SubMenuItems[2].Title);
			AssertEquals("Orders URL", "/Orders/Orders.aspx", navBar.NavigationElements[0].SubMenuItems[2].URL);

			AssertEquals("Forwarding should link to Containers", "Containers", navBar.NavigationElements[0].SubMenuItems[3].Title);
			AssertEquals("Containers URL", "/Containers/Containers.aspx", navBar.NavigationElements[0].SubMenuItems[3].URL);

			AssertEquals("Forwarding should link to Spot Quotes", "Spot Quotes", navBar.NavigationElements[0].SubMenuItems[4].Title);
			AssertEquals("Quotations URL", "/Quotes/Quotations.aspx", navBar.NavigationElements[0].SubMenuItems[4].URL);

			AssertEquals("Forwarding should link to MAWBs", "MAWBs", navBar.NavigationElements[0].SubMenuItems[5].Title);
			AssertEquals("MAWB URL", "/AWB/MAWB/MAWBs.aspx", navBar.NavigationElements[0].SubMenuItems[5].URL);

			AssertEquals("Forwarding should link to HAWBs", "HAWBs", navBar.NavigationElements[0].SubMenuItems[6].Title);
			AssertEquals("HAWB URL", "/AWB/HAWB/HAWBs.aspx", navBar.NavigationElements[0].SubMenuItems[6].URL);

			AssertEquals("Forwarding should link to Reports", "Reports", navBar.NavigationElements[0].SubMenuItems[7].Title);
			AssertEquals("Reports URL", "/Reports/Reports.aspx?ContentType=Freight", navBar.NavigationElements[0].SubMenuItems[7].URL);

			AssertEquals("Forwarding should link to Flights", "Flights", navBar.NavigationElements[0].SubMenuItems[8].Title);
			AssertEquals("Flights URL", "/Schedules/FlightSchedules.aspx", navBar.NavigationElements[0].SubMenuItems[8].URL);

			AssertEquals("Forwarding should link to Sailings", "Sailings", navBar.NavigationElements[0].SubMenuItems[9].Title);
			AssertEquals("Sailings URL", "/Schedules/SailingSchedules.aspx", navBar.NavigationElements[0].SubMenuItems[9].URL);

			AssertEquals("Forwarding should link to Road", "Road", navBar.NavigationElements[0].SubMenuItems[10].Title);
			AssertEquals("Flights URL", "/Schedules/RoadSchedules.aspx", navBar.NavigationElements[0].SubMenuItems[10].URL);

			AssertEquals("Forwarding should link to Rail", "Rail", navBar.NavigationElements[0].SubMenuItems[11].Title);
			AssertEquals("Rail URL", "/Schedules/RailSchedules.aspx", navBar.NavigationElements[0].SubMenuItems[11].URL);

			// CFS
			AssertEquals("First menu item should be CFS", "CFS", navBar.NavigationElements[1].Title);
			AssertEquals("CFS menu item should not have a URL", "#", navBar.NavigationElements[1].URL);
			AssertEquals("CFS submenu items", 1, navBar.NavigationElements[1].SubMenuItems.Count);

			AssertEquals("CFS should link to Shipments", "Shipments", navBar.NavigationElements[1].SubMenuItems[0].Title);
			AssertEquals("Shipments URL", "/CFSShipments/CFSShipments.aspx", navBar.NavigationElements[1].SubMenuItems[0].URL);

			// LinerAndAgency
			AssertEquals("Second menu item should be Liner & Agency", "Liner & Agency", navBar.NavigationElements[2].Title);
			AssertEquals("Liner & Agency menu should not have URL", "#", navBar.NavigationElements[2].URL);
			AssertEquals("Liner & Agency should have 4 submenu items", 4, navBar.NavigationElements[2].SubMenuItems.Count);

			AssertEquals("Liner & Agency should link to Bookings", "Bookings", navBar.NavigationElements[2].SubMenuItems[0].Title);
			AssertEquals("Bookings URL", "/LinerAndAgency/Bookings/Bookings.aspx", navBar.NavigationElements[2].SubMenuItems[0].URL);

			AssertEquals("Liner & Agency should link to BillsOfLading", "Bills Of Lading", navBar.NavigationElements[2].SubMenuItems[1].Title);
			AssertEquals("BillsOfLading URL", "/LinerAndAgency/BillsOfLading/BillsOfLading.aspx", navBar.NavigationElements[2].SubMenuItems[1].URL);

			AssertEquals("Liner & Agency should link to Containers", "Containers", navBar.NavigationElements[2].SubMenuItems[2].Title);
			AssertEquals("Container URL", "/LinerAndAgency/LinerAndAgencyContainers/LinerAndAgencyContainers.aspx", navBar.NavigationElements[2].SubMenuItems[2].URL);

			AssertEquals("Liner & Agency should link to Reports", "Reports", navBar.NavigationElements[2].SubMenuItems[3].Title);
			AssertEquals("Reports URL", "/Reports/Reports.aspx?ContentType=LinerAgency", navBar.NavigationElements[2].SubMenuItems[3].URL);

			//Customs
			AssertEquals("Second menu item should be Customs", "Customs", navBar.NavigationElements[3].Title);
			AssertEquals("Customs menu should not have URL", "#", navBar.NavigationElements[3].URL);
			AssertEquals("Customs should have 2 submenu items", 3, navBar.NavigationElements[3].SubMenuItems.Count);

			AssertEquals("Forwarding should link to Declarations", "Declarations", navBar.NavigationElements[3].SubMenuItems[0].Title);
			AssertEquals("Shipments URL", "/Declaration/Declarations.aspx", navBar.NavigationElements[3].SubMenuItems[0].URL);

			AssertEquals("Forwarding should link to ISF", "ISF", navBar.NavigationElements[3].SubMenuItems[1].Title);
			AssertEquals("Shipments URL", "/ImporterSecurityFiling/ImporterSecurityFiling.aspx", navBar.NavigationElements[3].SubMenuItems[1].URL);

			AssertEquals("Warehouse should link to Reports", "Reports", navBar.NavigationElements[3].SubMenuItems[2].Title);
			AssertEquals("Reports URL", "/Reports/Reports.aspx?ContentType=Customs", navBar.NavigationElements[3].SubMenuItems[2].URL);

			// WAREHOUSING
			AssertEquals("Third menu item should be Warehousing", "Warehouse", navBar.NavigationElements[4].Title);
			AssertEquals("Warehouse menu should not have URL", "#", navBar.NavigationElements[4].URL);
			AssertEquals("Warehouse should have 5 submenu items", 5, navBar.NavigationElements[4].SubMenuItems.Count);

			AssertEquals("Warehouse should link to Inventory", "Inventory", navBar.NavigationElements[4].SubMenuItems[0].Title);
			AssertEquals("Inventory URL", "/Warehousing/Inventory.aspx", navBar.NavigationElements[4].SubMenuItems[0].URL);

			AssertEquals("Warehouse should link to Warehouse Orders", "Orders", navBar.NavigationElements[4].SubMenuItems[1].Title);
			AssertEquals("Orders URL", "/Warehousing/WarehouseOrders.aspx", navBar.NavigationElements[4].SubMenuItems[1].URL);

			AssertEquals("Warehouse should link to Warehouse Receipts", "Receipts", navBar.NavigationElements[4].SubMenuItems[2].Title);
			AssertEquals("Receipts URL", "/Warehousing/WarehouseReceipts.aspx", navBar.NavigationElements[4].SubMenuItems[2].URL);

			AssertEquals("Warehouse should link to Products", "Products", navBar.NavigationElements[4].SubMenuItems[3].Title);
			AssertEquals("Products URL", "/Warehousing/OrgSupplierParts.aspx", navBar.NavigationElements[4].SubMenuItems[3].URL);

			AssertEquals("Warehouse should link to Reports", "Reports", navBar.NavigationElements[4].SubMenuItems[4].Title);
			AssertEquals("Reports URL", "/Reports/Reports.aspx?ContentType=Warehouse", navBar.NavigationElements[4].SubMenuItems[4].URL);

			AssertEquals("Transport item", "Port Transport", navBar.NavigationElements[5].Title);
			AssertEquals("User menu should not have URL", "#", navBar.NavigationElements[5].URL);
			AssertEquals("Warehouse should link to Transport Jobs", "Transport Jobs", navBar.NavigationElements[5].SubMenuItems[0].Title);
			AssertEquals("Products URL", "/Cartage/Cartages.aspx", navBar.NavigationElements[5].SubMenuItems[0].URL);
			AssertEquals("Warehouse should link to Reports", "Reports", navBar.NavigationElements[5].SubMenuItems[1].Title);
			AssertEquals("Reports URL", "/Reports/Reports.aspx?ContentType=Transport", navBar.NavigationElements[5].SubMenuItems[1].URL);

			AssertEquals("Third menu item should be Accounts", "Accounts", navBar.NavigationElements[6].Title);
			AssertEquals("Accounts URL", "/Accounts/Transactions.aspx", navBar.NavigationElements[6].URL);
			AssertEquals("Accounts should not have any submenu items", 0, navBar.NavigationElements[6].SubMenuItems.Count);

			AssertEquals("Fourth menu item should be User", "User", navBar.NavigationElements[7].Title);
			AssertEquals("User menu should not have URL", "#", navBar.NavigationElements[7].URL);
			AssertEquals("User should have 2 submenu items", 3, navBar.NavigationElements[7].SubMenuItems.Count);

			AssertEquals("User should link to Password", "Password", navBar.NavigationElements[7].SubMenuItems[0].Title);
			AssertEquals("Password URL", "/Admin/ChangePassword.aspx", navBar.NavigationElements[7].SubMenuItems[0].URL);

			AssertEquals("User should link to Log Off", "Log Off", navBar.NavigationElements[7].SubMenuItems[1].Title);
			AssertEquals("Log Off URL", "/Logout.aspx?ClearSaved=1", navBar.NavigationElements[7].SubMenuItems[1].URL);

			AssertEquals("Warehouse should link to Reports", "Reports", navBar.NavigationElements[7].SubMenuItems[2].Title);
			AssertEquals("Reports URL", "/Reports/Reports.aspx?ContentType=All", navBar.NavigationElements[7].SubMenuItems[2].URL);

			AssertEquals("This menu item should be Terms & Conditions", "Terms & Conditions", navBar.NavigationElements[8].Title);
			AssertEquals("Terms & Conditions URL", "/Terms/ViewTermsAndConditions.aspx", navBar.NavigationElements[8].URL);
			AssertEquals("Terms & Conditions should not have any submenu items", 0, navBar.NavigationElements[8].SubMenuItems.Count);
		}

		public void TestNavigationSecurityRights()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			testOrg.OH_Code = "XXXYYYZZZ";
			OrgContact testContact = testOrg.Contacts.AddNew();
			testContact.OC_Email = "test@cargowise.com";
			testContact.SetHashedPassword("test");
			testContact.OC_WebAccessEnabled = true;
			testOrg.OH_IsWarehouseClient = true;
			testOrg.OH_IsForwarder = true;

			Factory.Save();

			Page.SiteUser.Login("XXXYYYZZZ", "test@cargowise.com", "test");
			Assert("WebUser should be logged in", Page.SiteUser.IsLoggedIn);

			AssertNotNull("Control should not be null", NavigationControl);
			ZNavigationBar navBar = new ZNavigationBar();
			NavigationControl.SetupNavigationBar(navBar);

			AssertNotNull(navBar);
			AssertPageSecurityRelation(navBar);
		}

		protected virtual void AssertPageSecurityRelation(ZNavigationBar navigrationBar)
		{
			foreach (NavigationElement element in navigrationBar.NavigationElements)
			{
				foreach (NavigationElement subElement in element.SubMenuItems)
				{
					AssertPageSecurityRelation(subElement);
				}
			}
		}

		protected virtual void AssertPageSecurityRelation(NavigationElement element)
		{
			if (!string.IsNullOrEmpty(element.PagePath) && element.PagePath != "#")
			{
				if (element.SecurityRight != null)
				{
					if (!ExpectedSecurityRights.ContainsKey(element.Title))
					{
						Assert(string.Format("Element {0} security right is not being tested", element.Title), false);
					}
					AssertEquals(ExpectedSecurityRights[element.Title][element.PagePath], element.SecurityRight);
				}
				else
				{
					if (ExpectedSecurityRights.ContainsKey(element.Title))
					{
						AssertNull(string.Format("Element {0} is missing security right", element.Title), ExpectedSecurityRights[element.Title][element.PagePath]);
					}
				}
			}
			Assert(true);
		}

		protected virtual Dictionary<string, Dictionary<string, WebSecurityRight>> ExpectedSecurityRights
		{
			get
			{
				if (expectedSecurityRights == null)
				{
					expectedSecurityRights = new Dictionary<string, Dictionary<string, WebSecurityRight>>();
					expectedSecurityRights.Add("Shipments", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["Shipments"].Add(TrackingConstants.RelativePath.ShipmentsPage, WebSecurityRightsList.WebShipmentsView);
					expectedSecurityRights["Shipments"].Add(TrackingConstants.RelativePath.CFSShipmentsPage, WebSecurityRightsList.WebCFSShipmentView);

					expectedSecurityRights.Add("Bookings", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["Bookings"].Add(TrackingConstants.RelativePath.BookingsPage, WebSecurityRightsList.WebBookingsView);
					expectedSecurityRights["Bookings"].Add(TrackingConstants.RelativePath.LinerAndAgencyBookingsPage, WebSecurityRightsList.WebLinerAndAgencyBookingsView);

					expectedSecurityRights.Add("Orders", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["Orders"].Add(TrackingConstants.RelativePath.OrdersPage, WebSecurityRightsList.WebOrdersView);
					expectedSecurityRights["Orders"].Add(TrackingConstants.RelativePath.WarehouseOrdersPage, WebSecurityRightsList.WebWarehouseOrdersView);

					expectedSecurityRights.Add("Containers", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["Containers"].Add(TrackingConstants.RelativePath.ContainersPage, WebSecurityRightsList.WebContainers);
					expectedSecurityRights["Containers"].Add(TrackingConstants.RelativePath.LinerAndAgencyContainersPage, WebSecurityRightsList.WebLinerAndAgencyContainers);

					expectedSecurityRights.Add("Spot Quotes", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["Spot Quotes"].Add(TrackingConstants.RelativePath.QuotationsPage, WebSecurityRightsList.WebQuotes);

					expectedSecurityRights.Add("Reports", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["Reports"].Add(TrackingConstants.RelativePath.ReportsPage, WebSecurityRightsList.WebReports);

					expectedSecurityRights.Add("Bills Of Lading", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["Bills Of Lading"].Add(TrackingConstants.RelativePath.LinerAndAgencyBillsOfLadingPage, WebSecurityRightsList.WebLinerAndAgencyBillsOfLadingView);

					expectedSecurityRights.Add("Declarations", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["Declarations"].Add(TrackingConstants.RelativePath.DeclarationModulePage, WebSecurityRightsList.WebDeclarationView);

					expectedSecurityRights.Add("ISF", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["ISF"].Add(TrackingConstants.RelativePath.ISFPage, WebSecurityRightsList.WebISFView);

					expectedSecurityRights.Add("Inventory", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["Inventory"].Add(TrackingConstants.RelativePath.InventoryPage, WebSecurityRightsList.WebInventoryView);

					expectedSecurityRights.Add("Receipts", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["Receipts"].Add(TrackingConstants.RelativePath.WarehouseReceiptsPage, WebSecurityRightsList.WebWarehouseReceiptsView);

					expectedSecurityRights.Add("Products", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["Products"].Add(TrackingConstants.RelativePath.ProductProfilesPage, WebSecurityRightsList.WebWarehouseProductsView);

					expectedSecurityRights.Add("Transport Jobs", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["Transport Jobs"].Add(TrackingConstants.RelativePath.CartagePage, WebSecurityRightsList.WebCartageView);

					expectedSecurityRights.Add("Accounts", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["Accounts"].Add(TrackingConstants.RelativePath.TransactionsPage, WebSecurityRightsList.WebInvoicingAndStatements);

					expectedSecurityRights.Add("MAWBs", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["MAWBs"].Add(TrackingConstants.RelativePath.MAWBPage, WebSecurityRightsList.WebMAWBView);

					expectedSecurityRights.Add("HAWBs", new Dictionary<string, WebSecurityRight>());
					expectedSecurityRights["HAWBs"].Add(TrackingConstants.RelativePath.HAWBPage, WebSecurityRightsList.WebHAWBView);
				}
				return expectedSecurityRights;
			}
		}

		Dictionary<string, Dictionary<string, WebSecurityRight>> expectedSecurityRights;

		public void TestWarehouseMenuItemIsHiddenFromNonWarehouseUsers()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			testOrg.OH_Code = "XXXYYYZZZ";
			OrgContact testContact = testOrg.Contacts.AddNew();
			testContact.OC_Email = "test@cargowise.com";
			testContact.SetHashedPassword("test");
			testContact.OC_WebAccessEnabled = true;
			testOrg.OH_IsWarehouseClient = true;

			Factory.Save();

			Page.SiteUser.Login("XXXYYYZZZ", "test@cargowise.com", "test");
			Assert("WebUser should be logged in", Page.SiteUser.IsLoggedIn);

			ZNavigationBar navBar = new ZNavigationBar();
			NavigationControl.SetupNavigationBar(navBar);

			AssertNotNull(navBar);
			AssertNavigationBarContents("Warehouse", true, navBar);

			Page.SiteUser.Logout();
			testOrg.OH_IsWarehouseClient = false;
			navBar = new ZNavigationBar();
			Factory.Save();

			Page.SiteUser.Login("XXXYYYZZZ", "test@cargowise.com", "test");
			Assert("WebUser should be logged in", Page.SiteUser.IsLoggedIn);

			NavigationControl.SetupNavigationBar(navBar);

			AssertNotNull(navBar);
			AssertNavigationBarContents("Warehouse", false, navBar);
		}

		public void TestLinerAgencyMenuItemIsHiddenFromNonForwardereUsers()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			testOrg.OH_Code = "XXXYYYZZZ";
			OrgContact testContact = testOrg.Contacts.AddNew();
			testContact.OC_Email = "test@cargowise.com";
			testContact.SetHashedPassword("test");
			testContact.OC_WebAccessEnabled = true;
			testOrg.OH_IsForwarder = true;

			Factory.Save();

			Page.SiteUser.Login("XXXYYYZZZ", "test@cargowise.com", "test");
			Assert("WebUser should be logged in", Page.SiteUser.IsLoggedIn);

			ZNavigationBar navBar = new ZNavigationBar();
			NavigationControl.SetupNavigationBar(navBar);

			AssertNotNull(navBar);
			AssertNavigationBarContents("Liner & Agency", true, navBar);

			Page.SiteUser.Logout();
			testOrg.OH_IsForwarder = false;
			navBar = new ZNavigationBar();
			Factory.Save();

			Page.SiteUser.Login("XXXYYYZZZ", "test@cargowise.com", "test");
			Assert("WebUser should be logged in", Page.SiteUser.IsLoggedIn);

			NavigationControl.SetupNavigationBar(navBar);

			AssertNotNull(navBar);
			AssertNavigationBarContents("Liner & Agency", false, navBar);
		}

		void AssertNavigationBarContents(ZString elementTitle, bool expectedResult, ZNavigationBar navBar)
		{
			bool isInNavBar = false;

			foreach (NavigationElement element in navBar.NavigationElements)
			{
				if (element.Title == elementTitle)
				{
					isInNavBar = true;
				}
			}

			AssertEquals("Element '" + elementTitle + "' is in navigation bar", expectedResult, isInNavBar);
		}

		public void TestChangePasswordMenuItem()
		{
			OrgContact contact1 = Factory.NewWithValidTestData<OrgContact>();
			contact1.Header.OH_Code = "XXX";
			contact1.Header.OH_IsWarehouseClient = true;
			contact1.OC_Email = "test@cargowise.com";
			contact1.OC_WebAccessEnabled = true;
			contact1.SetHashedPassword("test");

			OrgContact contact2 = Factory.NewWithValidTestData<OrgContact>();
			contact2.Header.OH_Code = "YYY";
			contact2.Header.OH_IsWarehouseClient = true;
			contact2.OC_Email = "test@cargowise.com";
			contact2.OC_WebAccessEnabled = true;
			contact2.SetHashedPassword("test");

			Factory.Save();

			OrgContactWebUser siteUser = (OrgContactWebUser)Page.SiteUser;
			Page.SiteUser.Login("", "test@cargowise.com", "test");
			Assert("WebUser should be logged in", siteUser.IsLoggedIn);
			AssertEquals("Should be two related orgs", 2, siteUser.AllUserRelatedOrgs.Count);

			AssertNotNull("Control should not be null", NavigationControl);
			ZNavigationBar navBar = new ZNavigationBar();
			NavigationControl.SetupNavigationBar(navBar);

			AssertNotNull(navBar);

			AssertEquals("Should contain 7 top level elements", 7, navBar.NavigationElements.Count);

			AssertEquals("Fith menu item should be User", "User", navBar.NavigationElements[6].Title);
			AssertEquals("User menu should not have URL", "#", navBar.NavigationElements[6].URL);
			AssertEquals("User should have 4 submenu items", 4, navBar.NavigationElements[6].SubMenuItems.Count);

			AssertEquals("User should link to Company", "Company", navBar.NavigationElements[6].SubMenuItems[0].Title);
			string expectedUrl = string.Format("/{0}?ReturnUrl={1}", TrackingConstants.RelativePath.SwitchCompanyPage, HttpContext.Current.Server.UrlEncode(HttpContext.Current.Request.Url.PathAndQuery));
			AssertEquals("Company URL", expectedUrl, navBar.NavigationElements[6].SubMenuItems[0].URL);

			AssertEquals("User should link to Password", "Password", navBar.NavigationElements[6].SubMenuItems[1].Title);
			AssertEquals("Password URL", "/Admin/ChangePassword.aspx", navBar.NavigationElements[6].SubMenuItems[1].URL);

			AssertEquals("User should link to Log Off", "Log Off", navBar.NavigationElements[6].SubMenuItems[2].Title);
			AssertEquals("Log Off URL", "/Logout.aspx?ClearSaved=1", navBar.NavigationElements[6].SubMenuItems[2].URL);
		}
		PageHeaderWithNavigation NavigationControl
		{
			get { return Control as PageHeaderWithNavigation; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			NavigationControl.pagehead = new HtmlGenericControl { ID = "pagehead" };
			NavigationControl.pagehead.Controls.Add(new EDITracBanner { ID = "EDITracBanner1" });
			NavigationControl.menu = new HtmlGenericControl { ID = "menu" };

			NavigationControl.Controls.Add(NavigationControl.pagehead);
			NavigationControl.Controls.Add(NavigationControl.menu);
		}

		class DummyPageHeaderWithNavigation : PageHeaderWithNavigation
		{
			protected override ZNavigationBar LoadNavigationBar(ZWebResource navigationResource)
			{
				return new ZNavigationBar();
			}
		}
	}
}
