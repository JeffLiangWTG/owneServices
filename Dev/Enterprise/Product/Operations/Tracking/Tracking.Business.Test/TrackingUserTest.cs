using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Moq;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingUserTest : OrgContactWebUserTest
	{
		#region setup

		new TrackingSiteUser User
		{
			get { return base.User as TrackingSiteUser; }
		}

		protected override WebUser GetNewWebUser()
		{
			return new TrackingSiteUser();
		}

		DocumentFactory MasterFactory
		{
			get
			{
				if (masterFactory == null)
				{
					masterFactory = new DocumentFactoryProvider().GetFactory(Factory);
				}
				return masterFactory;
			}
		}
		DocumentFactory masterFactory;

		#endregion

		#region Events

		public void TestCanViewEvents()
		{
			TestUserSecurity(delegate
			{ return User.CanViewEvents; }, WebSecurityRightsList.WebEventsView);
		}

		#endregion

		#region Milestones

		public void TestEstimatedMilestonesRights()
		{
			TestUserSecurity(delegate
			{ return User.CanUpdateEstimatedMilestones; }, WebSecurityRightsList.WebEstimatedMilestonesUpdate);
		}

		public void TestActualMilestonesRights()
		{
			TestUserSecurity(delegate
			{ return User.CanUpdateActualMilestones; }, WebSecurityRightsList.WebActualMilestonesUpdate);
		}

		#endregion

		public void TestAvailabilityForShipmentQuickUser()
		{
			OrgHeader testOrg = OrgHeader.LoadFromCode(Factory, "DEMORG");
			AssertNotNull("testOrg", testOrg);
			testOrg.MiscServ.OM_CMClientPortalHomePage = "http://www.portal.com";
			testOrg.OH_IsWarehouseClient = true;
			Factory.Save();

			User.LoginSupportForTest("DEMORG");
			Assert("Should be LoggedIn", User.IsLoggedIn);
			Assert("IsShipmentQuickViewUser", User.IsShipmentQuickViewUser);

			Assert(!User.CanViewQuotations);
			Assert(!User.CanViewAccounts);

			Assert(User.CanViewBookings);
			Assert(User.CanViewCFSShipments);
			Assert(!User.CanEditBookings);
			Assert(!User.CanEditContainerNumbers);
			Assert(!User.CanBookSailings);

			Assert(!User.CanViewCartage);

			Assert(!User.CanViewISF);
			Assert(!User.CanEditISF);
			Assert(!User.CanSendISF);
			Assert(!User.CanDeleteISF);

			Assert(User.CanViewOrders);
			Assert(!User.CanEditOrders);

			Assert(!User.CanViewInventory);

			Assert(User.CanViewWarehouseOrders);
			Assert(!User.CanEditWarehouseOrders);
			Assert(!User.CanViewWarehouseReceipts);

			Assert(!User.CanViewWarehouseProducts);

			Assert(User.CanViewTrackingContainers);
			Assert(!User.CanEditTrackingContainers);
			Assert(!User.CanEditContainersClientRef);
			Assert(!User.CanEditContainersRequiredDeliveryDate);
			Assert(!User.CanEditContainersActualDeliveryDate);
			Assert(!User.CanEditContainersConfirmedDeliveryDate);
			Assert(!User.CanEditContainersEstimatedDehireDate);
			Assert(!User.CanEditContainersPickupDate);
			Assert(!User.CanEditContainersActualDehireDate);
			Assert(!User.CanEditContainersSequence);

			Assert(!User.CanViewReports(WebReportModes.All));

			Assert(User.CanViewLinerAndAgencyBookings);
			Assert(!User.CanEditLinerAndAgencyBookings);

			Assert(User.CanViewLinerAndAgencyBillsOfLading);
			Assert(!User.CanEditLinerAndAgencyFwdInstructions);

			Assert(!User.CanViewDocuments);
			Assert(!User.CanViewDocuments);

			Assert(!User.CanAddDeliveryRequest);
			Assert(!User.CanEditDeliveryRequest);
		}

		#region Reports

		public void TestCanViewReportsForQuickViewUser()
		{
			bool cachedForwardingReportsModule = WebDataRegistry.Instance.UseWebForwardingReportsModule.Value;
			bool cachedLinerAndAgencyReportsModule = WebDataRegistry.Instance.UseWebLinerAndAgencyReportsModule.Value;
			bool cachedCustomsReportsModule = WebDataRegistry.Instance.UseWebCustomsReportsModule.Value;
			bool cachedTransportReportsModule = WebDataRegistry.Instance.UseWebTransportReportsModule.Value;
			bool cachedWarehouseReportsModule = WebDataRegistry.Instance.UseWebWarehouseReportsModule.Value;

			OrgHeader testOrg = OrgHeader.LoadFromCode(Factory, "DEMORG");
			AssertNotNull(testOrg);
			Factory.Save();

			User.LoginSupportForTest("DEMORG");
			Assert("Should be LoggedIn", User.IsLoggedIn);

			Assert("IsShipmentQuickViewUser", User.IsShipmentQuickViewUser);
			Assert(!User.LoggedInUser.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebReports));

			WebDataRegistry.Instance.UseWebForwardingReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.UseWebLinerAndAgencyReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.UseWebCustomsReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.UseWebTransportReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.UseWebWarehouseReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!User.CanViewReports(WebReportModes.All));
			Assert(!User.CanViewReports(WebReportModes.Customs));
			Assert(!User.CanViewReports(WebReportModes.Freight));
			Assert(!User.CanViewReports(WebReportModes.LinerAgency));
			Assert(!User.CanViewReports(WebReportModes.Transport));
			Assert(!User.CanViewReports(WebReportModes.Warehouse));

			WebDataRegistry.Instance.UseWebForwardingReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WebDataRegistry.Instance.UseWebLinerAndAgencyReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WebDataRegistry.Instance.UseWebCustomsReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WebDataRegistry.Instance.UseWebTransportReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WebDataRegistry.Instance.UseWebWarehouseReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert(!User.CanViewReports(WebReportModes.All));
			Assert(!User.CanViewReports(WebReportModes.Customs));
			Assert(!User.CanViewReports(WebReportModes.Freight));
			Assert(!User.CanViewReports(WebReportModes.LinerAgency));
			Assert(!User.CanViewReports(WebReportModes.Transport));
			Assert(!User.CanViewReports(WebReportModes.Warehouse));

			SetOrgRight(User.LoggedInUser, WebSecurityRightsList.WebReports, true);
			Assert(!User.LoggedInUser.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebReports));

			Assert(!User.CanViewReports(WebReportModes.All));
			Assert(!User.CanViewReports(WebReportModes.Customs));
			Assert(!User.CanViewReports(WebReportModes.Freight));
			Assert(!User.CanViewReports(WebReportModes.LinerAgency));
			Assert(!User.CanViewReports(WebReportModes.Transport));
			Assert(!User.CanViewReports(WebReportModes.Warehouse));

			WebDataRegistry.Instance.UseWebForwardingReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedForwardingReportsModule);
			WebDataRegistry.Instance.UseWebLinerAndAgencyReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedLinerAndAgencyReportsModule);
			WebDataRegistry.Instance.UseWebCustomsReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedCustomsReportsModule);
			WebDataRegistry.Instance.UseWebTransportReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedTransportReportsModule);
			WebDataRegistry.Instance.UseWebWarehouseReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedWarehouseReportsModule);
		}

		public void TestCanViewReports()
		{
			bool cachedForwardingReportsModule = WebDataRegistry.Instance.UseWebForwardingReportsModule.Value;
			bool cachedLinerAndAgencyReportsModule = WebDataRegistry.Instance.UseWebLinerAndAgencyReportsModule.Value;
			bool cachedCustomsReportsModule = WebDataRegistry.Instance.UseWebCustomsReportsModule.Value;
			bool cachedTransportReportsModule = WebDataRegistry.Instance.UseWebTransportReportsModule.Value;
			bool cachedWarehouseReportsModule = WebDataRegistry.Instance.UseWebWarehouseReportsModule.Value;

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_Code = "TESTORG";
			AssertNotNull(testOrg);
			OrgContact testContact = testOrg.Contacts.AddNew();
			testContact.OC_Email = "test@testcompany.com";
			testContact.SetHashedPassword("testpassword");
			testContact.OC_WebAccessEnabled = true;
			SetOrgRight(testContact, WebSecurityRightsList.WebReports, false);
			Factory.Save();

			User.Login("TESTORG", "test@testcompany.com", "testpassword");
			Assert("Should be LoggedIn", User.IsLoggedIn);

			Assert(!User.IsShipmentQuickViewUser);
			Assert(!User.LoggedInUser.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebReports));

			WebDataRegistry.Instance.UseWebForwardingReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.UseWebLinerAndAgencyReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.UseWebCustomsReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.UseWebTransportReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			WebDataRegistry.Instance.UseWebWarehouseReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert(!User.CanViewReports(WebReportModes.All));
			Assert(!User.CanViewReports(WebReportModes.Customs));
			Assert(!User.CanViewReports(WebReportModes.Freight));
			Assert(!User.CanViewReports(WebReportModes.LinerAgency));
			Assert(!User.CanViewReports(WebReportModes.Transport));
			Assert(!User.CanViewReports(WebReportModes.Warehouse));

			WebDataRegistry.Instance.UseWebForwardingReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WebDataRegistry.Instance.UseWebLinerAndAgencyReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WebDataRegistry.Instance.UseWebCustomsReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WebDataRegistry.Instance.UseWebTransportReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			WebDataRegistry.Instance.UseWebWarehouseReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert(!User.CanViewReports(WebReportModes.All));
			Assert(!User.CanViewReports(WebReportModes.Customs));
			Assert(!User.CanViewReports(WebReportModes.Freight));
			Assert(!User.CanViewReports(WebReportModes.LinerAgency));
			Assert(!User.CanViewReports(WebReportModes.Transport));
			Assert(!User.CanViewReports(WebReportModes.Warehouse));

			SetOrgRight(testContact, WebSecurityRightsList.WebReports, true);
			Factory.Save();
			User.Login("TESTORG", "test@testcompany.com", "testpassword");
			Assert("Should be LoggedIn", User.IsLoggedIn);

			Assert(User.LoggedInUser.SecurityRightsForBindingOnly.IsRightGranted(WebSecurityRightsList.WebReports));

			Assert(User.CanViewReports(WebReportModes.All));
			Assert(User.CanViewReports(WebReportModes.Customs));
			Assert(User.CanViewReports(WebReportModes.Freight));
			Assert(User.CanViewReports(WebReportModes.LinerAgency));
			Assert(User.CanViewReports(WebReportModes.Transport));
			Assert(User.CanViewReports(WebReportModes.Warehouse));

			WebDataRegistry.Instance.UseWebForwardingReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedForwardingReportsModule);
			WebDataRegistry.Instance.UseWebLinerAndAgencyReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedLinerAndAgencyReportsModule);
			WebDataRegistry.Instance.UseWebCustomsReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedCustomsReportsModule);
			WebDataRegistry.Instance.UseWebTransportReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedTransportReportsModule);
			WebDataRegistry.Instance.UseWebWarehouseReportsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedWarehouseReportsModule);
		}

		void SetOrgRight(OrgContact contact, WebSecurityRight securityRight, bool isGranted)
		{
			OrgSecurity requestedOrgRight = null;
			OrgHeader contactOrg = contact.ParentOrg;
			foreach (OrgSecurity orgRight in contactOrg.SecurityRights)
			{
				if (orgRight.OX_SecurityItemName == securityRight.Code)
				{
					requestedOrgRight = orgRight;
					break;
				}
			}
			if (requestedOrgRight == null)
			{
				requestedOrgRight = contactOrg.SecurityRights.AddNew();
				requestedOrgRight.OX_SecurityItemName = securityRight.Code;
			}
			requestedOrgRight.OX_Granted = isGranted;

			OrgSecurityContacts requestedUserRight = null;
			foreach (OrgSecurityContacts userRight in contact.SecurityRightsForBindingOnly)
			{
				if (userRight.OZ_OX == requestedOrgRight.PK)
				{
					requestedUserRight = userRight;
					break;
				}
			}
			if (requestedUserRight == null)
			{
				requestedUserRight = contact.SecurityRightsForBindingOnly.AddNew();
				requestedUserRight.OZ_OX = requestedOrgRight.PK;
			}
			requestedUserRight.OZ_Granted = isGranted;
		}

		#endregion

		#region GetClientPortalUrl

		[HttpContextEnabledTest]
		public void TestGetClientPortalUrl()
		{
			string originalValueOfRegistry = WebDataRegistry.Instance.WebTrackerSharedSecret.Value;
			bool originalWebInventory = WebDataRegistry.Instance.UseWebWarehouseInventoryModule.Value;

			WebDataRegistry.Instance.UseWebCFSShipmentsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			try
			{
				WebDataRegistry.Instance.WebTrackerSharedSecret.SetValue(
					Guid.Empty, Guid.Empty, Guid.Empty, "5239erjie5t45342ijodesio"); // random string

				string expectedURL = "";
				string actualURL = User.GetClientPortalUrl();
				AssertEquals("Client Portal URL", expectedURL, actualURL);

				OrgHeader testOrg = OrgHeader.LoadFromCode(Factory, "DEMORG");
				AssertNotNull("testOrg", testOrg);
				testOrg.MiscServ.OM_CMClientPortalHomePage = "http://www.portal.com";
				testOrg.OH_IsWarehouseClient = true;
				Factory.Save();

				User.LoginSupportForTest("DEMORG");
				Assert("Should be LoggedIn", User.IsLoggedIn);
				actualURL = User.GetClientPortalUrl();
				expectedURL = "http://www.portal.com?OrgCode=DEMORG&OrgName=Demo%20Organisation&Email=&ContactName=CWSupport&SessionID=DummySession&Hash=58af66e981cd769f0523c8d751c657a9&WhsInventory=N&WhsOrders=Y&WhsReceipts=N&WhsProducts=N&Shipments=Y&CFSShipments=Y&ISF=N&Bookings=Y&Orders=Y&Containers=Y&Quotes=N&Reports=N&Accounts=N";
				AssertEquals("Client Portal URL", expectedURL, actualURL);

				OrgContact contact = testOrg.Contacts.AddNew();
				contact.OC_ContactName = "user";
				contact.OC_Email = "user@cargowise.com";
				contact.OC_WebAccessEnabled = true;
				contact.SetHashedPassword("pass");
				Factory.Save();

				User.Login("DEMORG", "user@cargowise.com", "pass");
				Assert("Should be LoggedIn", User.IsLoggedIn);
				actualURL = User.GetClientPortalUrl();
				expectedURL = "http://www.portal.com?OrgCode=DEMORG&OrgName=Demo%20Organisation&Email=user%40cargowise.com&ContactName=user&SessionID=DummySession&Hash=e35b27430203770edf7c86e417589646&WhsInventory=Y&WhsOrders=Y&WhsReceipts=Y&WhsProducts=Y&Shipments=Y&CFSShipments=Y&ISF=N&Bookings=Y&Orders=Y&Containers=Y&Quotes=Y&Reports=Y&Accounts=Y";
				AssertEquals("Client Portal URL", expectedURL, actualURL);

				SetOrgRight(User.LoggedInUser, WebSecurityRightsList.WebShipmentsView, false);
				User.OnSecurityRightsChangedForTest();
				Factory.Save();
				actualURL = User.GetClientPortalUrl();
				expectedURL = "http://www.portal.com?OrgCode=DEMORG&OrgName=Demo%20Organisation&Email=user%40cargowise.com&ContactName=user&SessionID=DummySession&Hash=e35b27430203770edf7c86e417589646&WhsInventory=Y&WhsOrders=Y&WhsReceipts=Y&WhsProducts=Y&Shipments=N&CFSShipments=Y&ISF=N&Bookings=Y&Orders=Y&Containers=Y&Quotes=Y&Reports=Y&Accounts=Y";
				AssertEquals("Client Portal URL", expectedURL, actualURL);

				SetOrgRight(User.LoggedInUser, WebSecurityRightsList.WebShipmentsView, true);
				User.OnSecurityRightsChangedForTest();
				Factory.Save();
				WebDataRegistry.Instance.UseWebWarehouseInventoryModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				actualURL = User.GetClientPortalUrl();
				expectedURL = "http://www.portal.com?OrgCode=DEMORG&OrgName=Demo%20Organisation&Email=user%40cargowise.com&ContactName=user&SessionID=DummySession&Hash=e35b27430203770edf7c86e417589646&WhsInventory=N&WhsOrders=Y&WhsReceipts=Y&WhsProducts=Y&Shipments=Y&CFSShipments=Y&ISF=N&Bookings=Y&Orders=Y&Containers=Y&Quotes=Y&Reports=Y&Accounts=Y";
				AssertEquals("Client Portal URL", expectedURL, actualURL);

				WebDataRegistry.Instance.UseWebWarehouseInventoryModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				SetupWarehouseListTestData(testOrg);
				actualURL = User.GetClientPortalUrl();
				expectedURL = "http://www.portal.com?OrgCode=DEMORG&OrgName=Demo%20Organisation&Email=user%40cargowise.com&ContactName=user&SessionID=DummySession&Hash=e35b27430203770edf7c86e417589646&Whs=Warehouse%20One&WhsInventory=Y&WhsOrders=Y&WhsReceipts=Y&WhsProducts=Y&Shipments=Y&CFSShipments=Y&ISF=N&Bookings=Y&Orders=Y&Containers=Y&Quotes=Y&Reports=Y&Accounts=Y";

				AssertEquals("Client Portal URL", expectedURL, actualURL);

				CreateInventory("Z&Z", "Warehouse & Ampersand Character", testOrg, "R3");
				Factory.Save();
				actualURL = User.GetClientPortalUrl();
				expectedURL = "http://www.portal.com?OrgCode=DEMORG&OrgName=Demo%20Organisation&Email=user%40cargowise.com&ContactName=user&SessionID=DummySession&Hash=e35b27430203770edf7c86e417589646&Whs=Warehouse%20%26%20Ampersand%20Character%2CWarehouse%20One&WhsInventory=Y&WhsOrders=Y&WhsReceipts=Y&WhsProducts=Y&Shipments=Y&CFSShipments=Y&ISF=N&Bookings=Y&Orders=Y&Containers=Y&Quotes=Y&Reports=Y&Accounts=Y";
				AssertEquals("Client Portal URL", expectedURL, actualURL);
			}
			finally
			{
				WebDataRegistry.Instance.WebTrackerSharedSecret.SetValue(
					Guid.Empty, Guid.Empty, Guid.Empty, originalValueOfRegistry);
				WebDataRegistry.Instance.UseWebWarehouseInventoryModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalWebInventory);
			}
		}

		#endregion

		#region Accounts

		public void TestCanViewAccounts()
		{
			TestUserSecurity(delegate
			{ return User.CanViewAccounts; }, WebSecurityRightsList.WebInvoicingAndStatements);
		}

		#endregion

		#region Quotations

		public void TestCanViewQuotations()
		{
			TestUserSecurity(delegate
			{ return User.CanViewQuotations; }, WebSecurityRightsList.WebQuotes);
		}

		#endregion

		#region CFSShipment

		public void TestCanViewCFSShipment()
		{
			WebDataRegistry.Instance.UseWebCFSShipmentsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestUserSecurity(delegate
			{ return User.CanViewCFSShipments; }, WebSecurityRightsList.WebCFSShipmentView);
			WebDataRegistry.Instance.UseWebCFSShipmentsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		#endregion

		#region Declarations

		public void TestCanViewDeclarations() => TestUserSecurity(() => User.CanViewDeclarations, WebSecurityRightsList.WebDeclarationView);

		#endregion

		#region Shipments

		public void TestCanViewShipments()
		{
			TestUserSecurity(delegate
			{ return User.CanViewShipments; }, WebSecurityRightsList.WebShipmentsView);
		}

		#endregion

		#region Bookings

		public void TestCanViewBookings()
		{
			TestUserSecurity(delegate
			{ return User.CanViewBookings; }, WebSecurityRightsList.WebBookingsView);
		}

		public void TestCanEditBookings()
		{
			TestUserSecurity(delegate
			{ return User.CanEditBookings; }, WebSecurityRightsList.WebBookingsAddEdit);
		}

		public void TestCanEditContainerNumbers()
		{
			TestUserSecurity(delegate
			{ return User.CanEditContainerNumbers; }, WebSecurityRightsList.WebContainersAddEditNumber);
		}

		#endregion

		#region Orders

		public void TestCanViewOrders()
		{
			TestUserSecurity(delegate
			{ return User.CanViewOrders; }, WebSecurityRightsList.WebOrdersView);
		}

		public void TestCanEditOrders()
		{
			TestUserSecurity(delegate
			{ return User.CanEditOrders; }, WebSecurityRightsList.WebOrdersAddEdit);
		}

		#endregion

		#region Shipments

		public void TestCanAddDeliveryRequest()
		{
			TestUserSecurity(() => User.CanAddDeliveryRequest, WebSecurityRightsList.WebShipmentDeliveryAdd);
		}

		public void TestCanEditDeliveryRequest()
		{
			TestUserSecurity(() => User.CanEditDeliveryRequest, WebSecurityRightsList.WebShipmentDeliveryEdit);
		}

		#endregion

		#region IsWarehouseClient

		public void TestIsWarehouseClient()
		{
			Assert("SiteUser should not be logged in", !User.IsLoggedIn);

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			testOrg.OH_Code = "XXXYYYZZZ";
			OrgContact testContact = testOrg.Contacts.AddNew();
			testContact.OC_Email = "test@testcompany.com";
			testContact.SetHashedPassword("testpassword");
			testContact.OC_WebAccessEnabled = true;

			testOrg.OH_IsWarehouseClient = false;
			Factory.Save();

			User.Login("XXXYYYZZZ", "test@testcompany.com", "testpassword");
			Assert("User should not be a warehouse client", !User.IsWarehouseClient);

			testOrg.OH_IsWarehouseClient = true;
			Factory.Save();

			User.Login("XXXYYYZZZ", "test@testcompany.com", "testpassword");
			Assert("User should be a warehouse client", User.IsWarehouseClient);

			User.Logout();
		}

		#endregion

		#region IsForwarder

		public void TestIsForwarder()
		{
			Assert("SiteUser should not be logged in", !User.IsLoggedIn);

			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			testOrg.OH_Code = "XXXYYYZZZ";
			OrgContact testContact = testOrg.Contacts.AddNew();
			testContact.OC_Email = "test@testcompany.com";
			testContact.SetHashedPassword("testpassword");
			testContact.OC_WebAccessEnabled = true;

			testOrg.OH_IsForwarder = false;
			testOrg.OH_IsConsignor = false;
			Factory.Save();

			User.Login("XXXYYYZZZ", "test@testcompany.com", "testpassword");
			AssertEquals("User should not be a Forwarder", false, User.IsForwarder);

			testOrg.OH_IsForwarder = true;
			Factory.Save();

			User.Login("XXXYYYZZZ", "test@testcompany.com", "testpassword");
			Assert("User should be a forwarder", User.IsForwarder);

			User.LoggedInOrganisation.OH_IsForwarder = false;
			User.LoggedInOrganisation.OH_IsConsignor = true;
			Assert("User should be a forwarder", User.IsForwarder);

			User.LoggedInOrganisation.OH_IsForwarder = false;
			User.LoggedInOrganisation.OH_IsConsignor = false;
			AssertEquals("User should not be a forwarder", false, User.IsForwarder);

			User.Logout();
		}

		#endregion

		#region Inventory

		public void TestCanViewInventory()
		{
			TestUserSecurityForWarehouse(delegate
			{ return User.CanViewInventory; }, WebSecurityRightsList.WebInventoryView);
		}
		#endregion

		#region WarehouseOrders

		public void TestCanViewWarehouseOrders()
		{
			TestUserSecurityForWarehouse(delegate
			{ return User.CanViewWarehouseOrders; }, WebSecurityRightsList.WebWarehouseOrdersView);
		}

		public void TestCanEditWarehouseOrders()
		{
			TestUserSecurityForWarehouse(delegate
			{ return User.CanEditWarehouseOrders; }, WebSecurityRightsList.WebWarehouseOrdersAddEdit);
		}

		#endregion

		#region WarehouseReceipts

		public void TestCanViewWarehouseReceipts()
		{
			TestUserSecurityForWarehouse(delegate
			{ return User.CanViewWarehouseReceipts; }, WebSecurityRightsList.WebWarehouseReceiptsView);
		}

		public void TestCanEditWarehouseReceipts()
		{
			TestUserSecurityForWarehouse(delegate
			{ return User.CanEditWarehouseReceipts; }, WebSecurityRightsList.WebWarehouseReceiptsAddEdit);
		}

		#endregion

		#region WarehouseProducts

		public void TestCanViewWarehouseProducts()
		{
			TestUserSecurityForWarehouse(delegate
			{ return User.CanViewWarehouseProducts; }, WebSecurityRightsList.WebWarehouseProductsView);
		}

		#endregion

		#region Tracking Containers

		public void TestCanViewTrackingContainers()
		{
			TestUserSecurity(delegate
			{ return User.CanViewTrackingContainers; }, WebSecurityRightsList.WebContainers);
		}

		public void TestCanEditTrackingContainers()
		{
			TestUserSecurity(delegate
			{ return User.CanEditTrackingContainers; }, WebSecurityRightsList.WebContainersEdit);
		}

		public void TestCanEditContainersClientRef()
		{
			TestUserSecurity(delegate
			{ return User.CanEditContainersClientRef; }, WebSecurityRightsList.WebContainersEditClientRef);
		}

		public void TestCanEditContainersRequiredDeliveryDate()
		{
			TestUserSecurity(delegate
			{ return User.CanEditContainersRequiredDeliveryDate; }, WebSecurityRightsList.WebContainersReqDeliveryDate);
		}

		public void TestCanEditContainersActualDeliveryDate()
		{
			TestUserSecurity(delegate
			{ return User.CanEditContainersActualDeliveryDate; }, WebSecurityRightsList.WebContainersActDeliveryDate);
		}

		public void TestCanEditContainersConfirmedDeliveryDate()
		{
			TestUserSecurity(delegate
			{ return User.CanEditContainersConfirmedDeliveryDate; }, WebSecurityRightsList.WebContainersConDeliveryDate);
		}

		public void TestCanEditContainersEstimatedDehireDate()
		{
			TestUserSecurity(delegate
			{ return User.CanEditContainersEstimatedDehireDate; }, WebSecurityRightsList.WebContainersEstDehireDate);
		}

		public void TestCanEditContainersPickupDate()
		{
			TestUserSecurity(delegate
			{ return User.CanEditContainersPickupDate; }, WebSecurityRightsList.WebContainersPickup);
		}

		public void TestCanEditContainersActualDehireDate()
		{
			TestUserSecurity(delegate
			{ return User.CanEditContainersActualDehireDate; }, WebSecurityRightsList.WebContainersActualDehire);
		}
		#endregion

		#region LinerAndAgency

		public void TestCanViewLinerAndAgencyBookings()
		{
			TestUserSecurity(delegate
			{ return User.CanViewLinerAndAgencyBookings; }, WebSecurityRightsList.WebLinerAndAgencyBookingsView);
		}

		public void TestCanEditLinerAndAgencyBookings()
		{
			TestUserSecurity(delegate
			{ return User.CanEditLinerAndAgencyBookings; }, WebSecurityRightsList.WebLinerAndAgencyBookingsAddEdit);
		}

		public void TestCanViewLinerAndAgencyBillsOfLading()
		{
			TestUserSecurity(delegate
			{ return User.CanViewLinerAndAgencyBillsOfLading; }, WebSecurityRightsList.WebLinerAndAgencyBillsOfLadingView);
		}

		public void TestCanEditLinerAndAgencyInstructions()
		{
			TestUserSecurity(delegate
			{ return User.CanEditLinerAndAgencyFwdInstructions; }, WebSecurityRightsList.WebLinerAndAgencyFwdInstructionsEdit);
		}

		public void TestCanEditLinerAndAgencyContainerNumbers()
		{
			TestUserSecurity(delegate
			{ return User.CanEditLinerAndAgencyContainerNumbers; }, WebSecurityRightsList.WebLinerAndAgencyContainersAddEditNumber);
		}

		#endregion

		#region TestSuperUserCanAccessAllTabs

		public void TestSuperUserCanAccessAllTabs()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			testOrg.OH_Code = "XXXYYYZZZ";
			Factory.Save();

			User.LoginSupportForTest("XXXYYYZZZ");
			AssertEquals("User should be logged in", true, User.IsLoggedIn);
			AssertEquals("User should be super user", true, User.IsSuperUser);

			AssertEquals("CanEditBookings - Should always be true for Superuser", true, User.CanEditBookings);
			AssertEquals("CanBookSailings - Should always be true for Superuser", true, User.CanBookSailings);
			AssertEquals("CanEditOrders - Should always be true for Superuser", true, User.CanEditOrders);
			AssertEquals("CanEditWarehouseOrders - Should always be true for Superuser", true, User.CanEditWarehouseOrders);
			AssertEquals("CanViewAccounts - Should always be true for Superuser", true, User.CanViewAccounts);
			AssertEquals("CanViewBookings - Should always be true for Superuser", true, User.CanViewBookings);
			AssertEquals("CanViewCFSShipments - Should always be true for Superuser", true, User.CanViewCFSShipments);
			AssertEquals("CanViewInventory - Should always be true for Superuser", true, User.CanViewInventory);
			AssertEquals("CanViewOrders - Should always be true for Superuser", true, User.CanViewOrders);
			AssertEquals("CanViewQuotations - Should always be true for Superuser", true, User.CanViewQuotations);
			AssertEquals("CanViewWarehouseOrders - Should always be true for Superuser", true, User.CanViewWarehouseOrders);
			AssertEquals("CanEditContainerNumbers - Should always be true for Superuser", true, User.CanEditContainerNumbers);
		}

		#endregion

		#region TestIsShipmentQuickViewUser

		public void TestIsShipmentQuickViewUser()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			testOrg.OH_Code = "XXXYYYZZZ";
			testOrg.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			User.LoginSupportForTest("XXXYYYZZZ");
			AssertEquals("User should be logged in", true, User.IsLoggedIn);
			AssertEquals("User should be super user", true, User.IsSuperUser);
			AssertEquals("User should not be ShipmentQuickView user", false, User.IsShipmentQuickViewUser);
			Assert("RelatedBranch should not be null", !User.BranchPKForLoginForTest.IsEmpty);

			User.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			AssertEquals("User should be logged in", true, User.IsLoggedIn);
			AssertEquals("User should be super user", true, User.IsSuperUser);
			AssertEquals("User should be ShipmentQuickView user", true, User.IsShipmentQuickViewUser);
			Assert("RelatedBranch should be null", User.BranchPKForLoginForTest.IsEmpty);
		}

		#endregion

		#region TestCanViewDocument

		public void TestCanViewDocument()
		{
			var testOrg = MasterFactory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			testOrg.OH_Code = "XXXYYYZZZ";

			var docType = MasterFactory.NewWithValidTestData<RefDocType>();
			docType.RT_DocType = "XZY";
			docType.RT_ReferenceType = "ALL";

			var related = MasterFactory.NewWithValidTestData<StorageMain>();
			related.SM_Type = "DEC";
			related.SM_ParentFK = ZGuid.NewZGuid();

			var allowedDoc = related.Documents.AddNew();
			allowedDoc.SC_DocType = docType.RT_DocType;
			allowedDoc.SC_IsPublished = true;

			MasterFactory.Save();

			User.LoginSupportForTest("XXXYYYZZZ");
			AssertEquals("User should be logged in", true, User.IsLoggedIn);
			AssertEquals("User should be super user", true, User.IsSuperUser);

			AssertNotNull(allowedDoc.DocType);
			AssertEquals("Should have permissions", true, User.CanViewDocument(MasterFactory, allowedDoc.DocType));

			docType.RT_DocType = "ZZZ";
			AssertNull(allowedDoc.DocType);
			AssertEquals("Should not have permissions", false, User.CanViewDocument(MasterFactory, allowedDoc.DocType));
		}

		#endregion

		public void TestCanAccessGlowTrackingPortal()
		{
			var originalShowValue = WebDataRegistry.Instance.ShowNewTrackingPortal.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var originalPortalsUrl = GlowRegistry.Instance.GlowPortalsUri.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			try
			{
				WebDataRegistry.Instance.ShowNewTrackingPortal.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glow");
				var factory = new BusinessObjectFactory();
				var org = factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "X";
				var contact = org.Contacts.AddNew();
				contact.OC_Email = "user@user.com";
				contact.SetHashedPassword("password");
				contact.OC_WebAccessEnabled = true;

				var orgRight = org.SecurityRights.AddNew();
				orgRight.OX_Granted = true;
				orgRight.OX_SecurityItemName = WebSecurityRightsList.TrackingPortal.Code;
				var userRight = contact.SecurityRightsForBindingOnly.AddNew();
				userRight.OZ_OX = orgRight.PK;
				userRight.OZ_Granted = true;
				factory.Save();

				User.Login(org.OH_Code, "user@user.com", "password");
				Assert("All conditions met", !User.CanAccessGlowTrackingPortal); // Temporarily forced to be false - WI00238192

				WebDataRegistry.Instance.ShowNewTrackingPortal.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Assert("Registry - ShowNewTrackingPortal false", !User.CanAccessGlowTrackingPortal);
				WebDataRegistry.Instance.ShowNewTrackingPortal.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
				Assert("No glow portal registry value", !User.CanAccessGlowTrackingPortal);
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://glow");

				orgRight.OX_Granted = false;
				userRight.OZ_Granted = false;
				factory.Save();
				User.OnSecurityRightsChangedForTest();
				Assert("No tracking portal security right", !User.CanAccessGlowTrackingPortal);
				orgRight.OX_Granted = true;
				userRight.OZ_Granted = true;
				factory.Save();
				User.OnSecurityRightsChangedForTest();

				User.Logout();
				Assert("Not logged in", !User.CanAccessGlowTrackingPortal);

				User.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
				Assert("No shipment quick view users", !User.CanAccessGlowTrackingPortal);
			}
			finally
			{
				WebDataRegistry.Instance.ShowNewTrackingPortal.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalShowValue);
				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalPortalsUrl);
			}
		}

		public void TestRecordsLoginAttemptOnFailure_Multiple()
		{
			var otherCompany = CreateNewCompany();
			otherCompany.OH_Code = "YYYYY";
			var otherContact = CreateNewContact("contact on other org", Email, Password, otherCompany);
			Factory.Save();

			var contactUsername = string.Empty;
			var mockLoginAttemptRecorder = new Mock<IOrgContactLoginAttemptRecorder>();
			mockLoginAttemptRecorder
				.Setup(x => x.RecordLoginAttempt(string.Empty, Email, null))
				.Callback<string, string, byte[]>((companyCode, username, hash) => contactUsername = username);
			ObjectFactory.Substitute(mockLoginAttemptRecorder.Object);

			User.Login(string.Empty, Email, "wrongPassword");

			mockLoginAttemptRecorder.Verify(x => x.RecordLoginAttempt(string.Empty, Email, null), Times.Once);
			AssertEquals("Contact's login attempts recorded", Email, contactUsername);
		}

		public void TestGetTransactionCompanies()
		{
			User.Login(Company.OH_Code, Email, Password);

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var company3 = Factory.NewWithValidTestData<GlbCompany>();
			var otherOrg = Factory.NewWithValidTestData<OrgHeader>();

			var companyData1 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData1.OB_GC = company1.PK;
			companyData1.OB_OH = User.LoggedInOrganisation.PK;
			companyData1.OB_IsDebtor = true;
			var companyData2 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData2.OB_GC = company2.PK;
			companyData2.OB_OH = User.LoggedInOrganisation.PK;
			companyData2.OB_IsDebtor = false;
			var companyData3 = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData3.OB_GC = company3.PK;
			companyData3.OB_OH = otherOrg.PK;
			companyData3.OB_IsDebtor = true;
			Factory.Save();

			var transactionCompanies = User.GetTransactionCompanies(Factory);

			Assert("Not all companies are fetched", transactionCompanies.Count == 1);
			Assert("Only those companies are fetched where current org is debtor", transactionCompanies[0].PK == company1.PK);
		}

		public void TestGetTransactionCompanies_NotLoggedIn()
		{
			var transactionCompanies = User.GetTransactionCompanies(Factory);

			Assert("Empty collection if no logged in user", transactionCompanies.Count == 0);
		}

		#region Implementation

		delegate bool CheckAccess();

		void TestUserSecurity(CheckAccess checkAccess, params WebSecurityRight[] rights)
		{
			Assert("SiteUser should not be logged in", !User.IsLoggedIn);
			Assert("Access should be denied when not logged in", !checkAccess());

			OrgHeader testOrg = Helper.TestOrg;
			testOrg.OH_Code = "XXXYYYZZZ";
			testOrg.OH_IsForwarder = true; // For Liner And Agency Booking
			OrgContact testContact = Helper.TestContact;
			testContact.OC_Email = "test@testcompany.com";
			testContact.SetHashedPassword("testpassword");
			testContact.OC_WebAccessEnabled = true;

			List<OrgSecurityContacts> contactSecurity = new List<OrgSecurityContacts>();
			foreach (WebSecurityRight right in rights)
			{
				OrgSecurity orgRight = testOrg.SecurityRights.AddNew();
				orgRight.OX_Granted = true;
				orgRight.OX_SecurityItemName = right.Code;
				OrgSecurityContacts userRight = testContact.SecurityRightsForBindingOnly.AddNew();
				userRight.OZ_OX = orgRight.PK;
				userRight.OZ_Granted = true;
				contactSecurity.Add(userRight);
			}

			Factory.Save();

			User.Login("XXXYYYZZZ", "test@testcompany.com", "testpassword");

			AssertEquals("Logged in user should be TestContact", testContact.PK, User.LoggedInUser.PK);

			AssertAccessAndSecurity(true, checkAccess, true, rights);

			foreach (OrgSecurityContacts security in contactSecurity)
			{
				security.OZ_Granted = false;
			}

			Factory.Save();

			User.Login("XXXYYYZZZ", "test@testcompany.com", "testpassword");
			AssertEquals("Logged in user should be TestContact", testContact.PK, User.LoggedInUser.PK);

			AssertAccessAndSecurity(false, checkAccess, false, rights);

			foreach (OrgSecurityContacts security in contactSecurity)
			{
				security.OZ_Granted = true;
			}

			Factory.Save();
		}

		void TestUserSecurityForWarehouse(CheckAccess checkAccess, params WebSecurityRight[] rights)
		{
			Helper.TestOrg.OH_IsWarehouseClient = true;
			TestUserSecurity(checkAccess, rights);

			User.Login("XXXYYYZZZ", "test@testcompany.com", "testpassword");
			Assert("Should be Warehouse Client", User.LoggedInOrganisation.OH_IsWarehouseClient);
			AssertAccessAndSecurity(true, checkAccess, true, rights);

			User.LoggedInOrganisation.OH_IsWarehouseClient = false;
			AssertAccessAndSecurity(false, checkAccess, true, rights);
		}

		void AssertAccessAndSecurity(bool expectedHasAccess, CheckAccess checkAccess, bool expectedHasRights, params WebSecurityRight[] rights)
		{
			Assert("SiteUser should be logged in", User.IsLoggedIn);

			ZString assertionMessage = expectedHasRights ?
				"SiteUser should have security rights granted" :
				"SiteUser should NOT have security rights granted";

			foreach (WebSecurityRight right in rights)
			{
				AssertEquals(assertionMessage, expectedHasRights, User.LoggedInUser.SecurityRightsForBindingOnly.IsRightGranted(right));
			}

			assertionMessage = expectedHasAccess ?
				"Access should be allowed" :
				"Access should NOT be allowed";

			AssertEquals(assertionMessage, expectedHasAccess, checkAccess());
		}

		void SetupWarehouseListTestData(OrgHeader client)
		{
			var inventory1 = CreateInventory("WH1", "Warehouse One", client, "R1");
			var inventory2 = CreateInventory("WH2", "Warehouse Two", Factory.NewWithValidTestData<OrgHeader>(), "R2");

			WhsWarehouse warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			warehouse.WW_WarehouseCode = "XXX";

			Factory.Save();
		}

		WhsInventoryView CreateInventory(string whsCode, string whsName, OrgHeader whsClient, string reference)
		{
			var warehouse = WarehouseTestHelper.CreateWarehouse(whsName, whsCode, "A");
			var product = WarehouseTestHelper.CreateProduct(whsClient, whsCode + "O1");
			return WarehouseTestHelper.CreateWhsReceiveWithInventory(whsClient, warehouse, reference, product, 1m).Inventory[0];
		}

		WhsTestHelperFunctions WarehouseTestHelper
		{
			get { return warehouseTestHelper ?? (warehouseTestHelper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions warehouseTestHelper;

		#endregion
	}
}
