using System;
using System.Web;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Tracking.Web.Base;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using WTG.WebSecurityRight;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class DefaultPageTest : BaseTrackingPageTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.Default;
		}

		protected override bool IsInWebRootDirectory
		{
			get { return true; }
		}

		public void TestRegistryItemIsActivated()
		{
			Assert(!testPage.RegistryItemIsActivated(null));
			WebDataRegistry.Instance.UseWebForwardingShipmentsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!testPage.RegistryItemIsActivated(WebDataRegistry.Instance.UseWebForwardingShipmentsModule));

			WebDataRegistry.Instance.UseWebForwardingShipmentsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert(testPage.RegistryItemIsActivated(WebDataRegistry.Instance.UseWebForwardingShipmentsModule));
		}

		public void TestGetFirstAvailablePageUrl()
		{
			ZWebTestHelper helper = new ZWebTestHelper(Factory);
			Factory.Save();

			OrgHeader org = helper.TestOrg;
			org.OH_IsWarehouseClient = ZBool.False;
			OrgSecurity orgRight = org.SecurityRights[0];

			WebDataRegistry.Instance.UseWebForwardingShipmentsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.UseWebForwardingOrdersModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.UseWebLinerAndAgencyBookingsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.UseWebLinerAndAgencyBillsOfLadingModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.UseWebDeclarationModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.UseWebWarehouseInventoryModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WebDataRegistry.Instance.UseWebWarehouseOrdersModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			//BooleanRegistryItem TestRegistryItem = new BooleanRegistryItem(OrgRight.OX_SecurityItemName, (NoResString)"TestCategory", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.System, true);
			WebSecurityRight testWebRight = new WebSecurityRight(orgRight.OX_SecurityItemName, (NoResString)orgRight.OX_SecurityItemName, WebSecurityApplication.EdiWebTracker);
			Assert(testPage.PageAvailable(WebDataRegistry.Instance.UseWebWarehouseInventoryModule, testWebRight));

			orgRight.OX_Granted = true;
			Factory.Save();

			helper.TestSiteUser.Login(org.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			testPage.SetUser(helper.TestSiteUser);

			Assert("Contact should have security rights", helper.TestContact.SecurityRightsForBindingOnly.IsRightGranted(testWebRight));
			Assert("Org should have security right granted", helper.TestOrg.SecurityRights.IsRightGranted(testWebRight));
			Assert(testPage.PageAvailable(WebDataRegistry.Instance.UseWebWarehouseInventoryModule, testWebRight));

			AssertNotContains("Warehousing", testPage.GetFirstAvailablePageUrl());
		}

		public void TestPageAvailable()
		{
			ZWebTestHelper helper = new ZWebTestHelper(Factory);
			Factory.Save();

			OrgSecurity orgRight = helper.TestOrg.SecurityRights[0];
			BooleanRegistryItem testRegistryItem = new BooleanRegistryItem(orgRight.OX_SecurityItemName, (NoResString)"TestCategory", (NoResString)"TestCaption", (NoResString)"TestHint", RegistryStorageFlags.System, true);
			WebSecurityRight testWebRight = new WebSecurityRight(orgRight.OX_SecurityItemName, (NoResString)orgRight.OX_SecurityItemName, WebSecurityApplication.EdiWebTracker);
			Assert(testPage.PageAvailable(testRegistryItem, testWebRight));

			orgRight.OX_Granted = true;
			Factory.Save();
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			testPage.SetUser(helper.TestSiteUser);

			Assert("Contact should have security rights", helper.TestContact.SecurityRightsForBindingOnly.IsRightGranted(testWebRight));
			Assert("Org should have security right granted", helper.TestOrg.SecurityRights.IsRightGranted(testWebRight));
			Assert(testPage.PageAvailable(testRegistryItem, testWebRight));

			orgRight.ContactSecurityRights.ToString(); // Hack

			orgRight.OX_Granted = false;
			Factory.Save();

			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			testPage.SetUser(helper.TestSiteUser);
			Assert("Org security right should now be removed", !helper.TestOrg.SecurityRights.IsRightGranted(testWebRight));
			Assert("Contact should have also lost security rights", !helper.TestContact.SecurityRightsForBindingOnly.IsRightGranted(testWebRight));
			Assert(!testPage.PageAvailable(testRegistryItem, testWebRight));
		}

		public void TestOnLoadRedirectsToShipments()
		{
			WebDataRegistry.Instance.UseWebForwardingShipmentsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.True);
			testPage.OnLoadForTest();
			Assert("Request should be redirected", HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertEquals("Should be redirected to Shipments page", testPage.AppInstance.ShipmentsPage, HttpContext.Current.Response.RedirectLocation);
		}

		public void TestOnLoadRedirectsToShipments_NoRights()
		{
			using (WebDataRegistry.Instance.UseWebForwardingShipmentsModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var helper = new TestHelper(Factory);
				var orgRight = helper.TestOrg.SecurityRights.AddNew();
				orgRight.OX_Granted = true;
				orgRight.OX_SecurityItemName = WebSecurityRightsList.WebShipmentsView.Code;
				var contactRight = helper.TestContact.SecurityRightsForBindingOnly.AddNew();
				contactRight.OZ_OX = orgRight.PK;
				contactRight.OZ_Granted = false;
				Factory.Save();

				helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
				testPage.SetUser(helper.TestSiteUser);

				testPage.OnLoadForTest();

				Assert("Request should be redirected", HttpContext.Current.Response.IsRequestBeingRedirected);
				AssertNotEquals("Should NOT be redirected to Shipments page", testPage.AppInstance.ShipmentsPage, HttpContext.Current.Response.RedirectLocation);
			}
		}

		public void TestOnLoadRedirectsToOrders()
		{
			WebDataRegistry.Instance.UseWebForwardingShipmentsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.False);
			WebDataRegistry.Instance.UseWebForwardingOrdersModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.True);
			testPage.OnLoadForTest();
			Assert("Request should be redirected", HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertEquals("Should be redirected to Orders page", testPage.AppInstance.OrdersPage, HttpContext.Current.Response.RedirectLocation);
		}

		public void TestOnLoadRedirectsToBookings()
		{
			WebDataRegistry.Instance.UseWebForwardingShipmentsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.False);
			WebDataRegistry.Instance.UseWebForwardingOrdersModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.False);
			WebDataRegistry.Instance.UseWebForwardingBookingsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.True);
			WebDataRegistry.Instance.UseWebDeclarationModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.False);
			WebDataRegistry.Instance.UseWebWarehouseOrdersModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.False);
			WebDataRegistry.Instance.UseWebWarehouseInventoryModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.False);
			WebDataRegistry.Instance.UseWebLinerAndAgencyBookingsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.False);
			WebDataRegistry.Instance.UseWebLinerAndAgencyBillsOfLadingModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.False);
			WebDataRegistry.Instance.UseWebForwardingFlightsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.False);

			var testOrg = OrgHeader.LoadFromCode(Factory, "DEMORG");
			AssertNotNull("testOrg", testOrg);
			testPage.SiteUser.LoginSupportForTest("DEMORG");
			SetupNavigationBar();
			testPage.OnLoadForTest();

			Assert("Request should be redirected", HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertEquals("Should be redirected to Bookings page", testPage.AppInstance.BookingsPage, HttpContext.Current.Response.RedirectLocation);
		}

		public void TestOnLoadRedirectsToShipmentsWhenNoCLientPortalHomePageSetForOrg()
		{
			WebDataRegistry.Instance.UseWebForwardingShipmentsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZBool.True);
			string originalValueOfRegistry = WebDataRegistry.Instance.WebTrackerSharedSecret.Value;
			try
			{
				WebDataRegistry.Instance.WebTrackerSharedSecret.SetValue(
					Guid.Empty, Guid.Empty, Guid.Empty, "5239erjie5t45342ijodesio"); // random string

				testPage.OnLoadForTest();
				Assert("Request should be redirected", HttpContext.Current.Response.IsRequestBeingRedirected);
				AssertEquals("Should be redirected to Shipments page", testPage.AppInstance.ShipmentsPage, HttpContext.Current.Response.RedirectLocation);
			}
			finally
			{
				WebDataRegistry.Instance.WebTrackerSharedSecret.SetValue(
					Guid.Empty, Guid.Empty, Guid.Empty, originalValueOfRegistry);
			}
		}

		public void TestOnLoadRedirectsToClientPortal()
		{
			string originalValueOfRegistry = WebDataRegistry.Instance.WebTrackerSharedSecret.Value;
			try
			{
				WebDataRegistry.Instance.WebTrackerSharedSecret.SetValue(
					Guid.Empty, Guid.Empty, Guid.Empty, "5239erjie5t45342ijodesio"); // random string

				OrgHeader testOrg = OrgHeader.LoadFromCode(Factory, "DEMORG");
				AssertNotNull("testOrg", testOrg);
				testOrg.MiscServ.OM_CMClientPortalHomePage = "http://www.portal.com";
				testOrg.OH_IsWarehouseClient = true;
				Factory.Save();

				testPage.SiteUser.LoginSupportForTest("DEMORG");
				Assert("Should be LoggedIn", testPage.SiteUser.IsLoggedIn);
				string expectedURL = "http://www.portal.com?OrgCode=DEMORG&OrgName=Demo%20Organisation&Email=&ContactName=CWSupport&SessionID=DummySession&Hash=58af66e981cd769f0523c8d751c657a9&WhsInventory=N&WhsOrders=Y&WhsReceipts=N&WhsProducts=N&Shipments=Y&CFSShipments=Y&ISF=N&Bookings=Y&Orders=Y&Containers=Y&Quotes=N&Reports=N&Accounts=N";

				testPage.OnLoadForTest();
				Assert("Request should be redirected", HttpContext.Current.Response.IsRequestBeingRedirected);
				AssertEquals("Should be redirected to portal page", expectedURL, HttpContext.Current.Response.RedirectLocation);
			}
			finally
			{
				WebDataRegistry.Instance.WebTrackerSharedSecret.SetValue(
					Guid.Empty, Guid.Empty, Guid.Empty, originalValueOfRegistry);
			}
		}

		#region Implementation

		void SetupNavigationBar()
		{
			var pageHeader = new PageHeaderWithNavigation();
			var navBar = new ZNavigationBar();
			testPage.Controls.Clear();
			testPage.Controls.Add(pageHeader);
			pageHeader.SetupNavigationBar(navBar);
			testPage.Controls.Add(navBar);
		}

		protected override System.Web.UI.Control GetNewControl()
		{
			return new DefaultForTest();
		}

		DefaultForTest testPage
		{
			get { return this.Control as DefaultForTest; }
		}

		class DefaultForTest : _Default
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}

			public override WebUser CurrentUser
			{
				get
				{
					return fCurrentUser;
				}
			}
			WebUser fCurrentUser;

			public void SetUser(WebUser user)
			{
				fCurrentUser = user;
			}

			public void OnLoadForTest()
			{
				OnLoad(EventArgs.Empty);
			}
		}

		#endregion

	}
}
