using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Web;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Login;
using Enterprise.ZArchitecture.Web.GUI.Login.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using static Enterprise.Integration.Customs.CA;
using USCustoms = Enterprise.Customs.US.Business;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class TrackingLoginHelperTest : OrgContactLoginHelperTest
	{
		public void TestNewFailedLoginLogCreatedIfStoreFailedLoginsIsTrue()
		{
			SetupStoreFailedLoginsAndLoginWithInvalidPassword(true);

			AssertLoginFailedLogExists("Company Code: XXXXX, E-mail: mehmeh@meh.com.au, IsLocked: False");
		}

		void AssertLoginFailedLogExists(string expectedInfo)
		{
			var logQuery = new ZQuery(StmALogSchema.SL_Parent, Env.Registry.WebBranch);
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.MiscellaneousEventCode);
			var logs = Factory.Load<StmALog>(logQuery);
			AssertEquals(1, logs.Length);
			var log = logs[0];
			AssertNotNull(log);

			var dataQuery = new ZQuery(StmDataSchema.SD_Owner, log.PK);
			var data = Factory.Load<StmData>(dataQuery);
			AssertEquals(1, data.Length);
			var decoder = new TwoWayEncoder(Env.Registry.WebBranch);
			var actualInfo = decoder.Decrypt(data[0].SD_BinaryValue.ToUTF8());
			AssertEquals(expectedInfo, actualInfo);
		}

		public void TestNoFailedLoginLogCreatedIfStoreFailedLoginsIsFalse()
		{
			SetupStoreFailedLoginsAndLoginWithInvalidPassword(false);

			var logQuery = new ZQuery(StmALogSchema.SL_Parent, Env.Registry.WebBranch);
			logQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.MiscellaneousEventCode);
			var logs = Factory.Load<StmALog>(logQuery);
			AssertEquals(0, logs.Length);
		}

		void SetupStoreFailedLoginsAndLoginWithInvalidPassword(bool storeFailedLogins)
		{
			var org = CreateNewCompany();
			var contact = CreateNewContact(org);
			Factory.Save();

			var page = GetNewPageWithRequest();
			var helper = GetNewHelper(page);
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource",
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
			helper.SetParamsValueForTest("UserEmail", contact.OC_Email);
			helper.SetParamsValueForTest("UserPassword", contact.PasswordForTesting + "Wrong");

			var cachedStoreFailedLogins =
				WebDataRegistry.Instance.StoreFailedLogins.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			try
			{
				WebDataRegistry.Instance.StoreFailedLogins.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
					storeFailedLogins);
				helper.OnPageLoad();
				AssertNotNull("SiteUser", page.SiteUser);
				AssertEquals("User should not be logged in", false, page.SiteUser.IsLoggedIn);
			}
			finally
			{
				WebDataRegistry.Instance.StoreFailedLogins.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
					cachedStoreFailedLogins);
			}
		}

		public void TestAccountLocked()
		{
			var org = CreateNewCompany();
			var contact = CreateNewContact(org);
			var person = Factory.NewWithValidTestData<GlbPerson>();
			contact.OC_PER = person.PK;
			var loginFailureLog = Factory.NewWithValidTestData<StmLoginFailureLog>();
			loginFailureLog.SFL_IsLockOut = true;
			loginFailureLog.SFL_LoginName = contact.OC_Email + " " + org.OH_Code;
			loginFailureLog.SFL_TableCode = OrgContactSchema.Constants.Prefix;
			loginFailureLog.SFL_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			WebDataRegistry.Instance.WebLoginLockoutMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 60);
			using (WebDataRegistry.Instance.StoreFailedLogins.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
						true))
			{
				var helper = GetNewHelper(TestPage);
				typeof(ZPage).InvokeMember("LoadOrCreateDataSource",
					BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, TestPage, null);

				helper.SetParamsValueForTest("CompanyCode", org.OH_Code);
				helper.SetParamsValueForTest("UserEmail", contact.OC_Email);
				helper.SetParamsValueForTest("UserPassword", contact.PasswordForTesting);

				helper.OnPageLoad();
				AssertNotNull("SiteUser", TestPage.SiteUser);
				AssertEquals("User should not be logged in", false, TestPage.SiteUser.IsLoggedIn);
				AssertEquals("Account Locked", ((TrackingLoginManager)TestPage.DataSource).LoginErrorMsg);

				AssertLoginFailedLogExists("Company Code: XXXXX, E-mail: mehmeh@meh.com.au, IsLocked: True");
			}
		}

		public void TestGetCustomLoginPageUrl()
		{
			TrackingLoginHelper loginHelper = (TrackingLoginHelper)GetNewHelper(TestPage);
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource",
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, TestPage, null);
			Assert("No URL by default", loginHelper.GetCustomLoginPageUrl().IsEmpty);

			WebDataRegistry.Instance.CustomLoginPageURL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "www.yahoo.com");
			AssertEquals("Should be URL without parameters", "www.yahoo.com", loginHelper.GetCustomLoginPageUrl());
			((TrackingLoginManager)TestPage.DataSource).UserName = "George.Bush@whitehouse.usa";
			AssertEquals("Should be URL with encoded parameters",
				"www.yahoo.com?UserEmail=George.Bush%40whitehouse.usa", loginHelper.GetCustomLoginPageUrl());
		}

		public void TestGetCustomShipmentQuickViewPageUrl()
		{
			TrackingLoginHelper loginHelper = (TrackingLoginHelper)GetNewHelper(TestPage);
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource",
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, TestPage, null);
			Assert("No URL by default", loginHelper.GetCustomShipmentQuickViewPageUrl().IsEmpty);

			WebDataRegistry.Instance.CustomQuickViewPageURL.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				"www.yahoo.com");
			Assert("QuickViewNumber is empty by default",
				((TrackingLoginManager)TestPage.DataSource).QuickViewNumber.IsEmpty);
			Assert("Should be no URL because there are no QuickView parameters",
				loginHelper.GetCustomShipmentQuickViewPageUrl().IsEmpty);

			((TrackingLoginManager)TestPage.DataSource).UserName = "George.Bush@whitehouse.usa";
			Assert("Should be no URL because there are no QuickView parameters",
				loginHelper.GetCustomShipmentQuickViewPageUrl().IsEmpty);

			((TrackingLoginManager)TestPage.DataSource).QuickViewNumber = "1234";
			AssertEquals("Should be URL with parameters", "www.yahoo.com?QuickViewNumber=1234",
				loginHelper.GetCustomShipmentQuickViewPageUrl());

			((TrackingLoginManager)TestPage.DataSource).QuickViewNumber = ZString.Empty;
			Assert("Should be no URL because there are no QuickView parameters",
				loginHelper.GetCustomShipmentQuickViewPageUrl().IsEmpty);
			((TrackingLoginManager)TestPage.DataSource).QuickViewErrorMsg = "Error Message";
			AssertEquals("Should be URL with encoded parameters", "www.yahoo.com?QuickViewErrorMsg=Error%20Message",
				loginHelper.GetCustomShipmentQuickViewPageUrl());
		}

		public void TestDefaultUrlRemovesUserSpecificQueryParameters()
		{
			var loginHelper = (TestTrackingLoginHelper)GetNewHelper(TestPage);
			var global = (GlobalForLoginHelperTest)TestPage.AppInstance;

			global.DefaultPageForTesting = "/some/relative/path/page.aspx?Mode=Show";
			AssertEquals("/some/relative/path/page.aspx", loginHelper.DefaultUrlExposed);

			global.DefaultPageForTesting = "/some/relative/path/page.aspx?Mode=Show&Another=Parameter";
			AssertEquals("/some/relative/path/page.aspx?Another=Parameter", loginHelper.DefaultUrlExposed);

			global.DefaultPageForTesting = "/some/relative/path/page.aspx?Another=Parameter&Mode=Show";
			AssertEquals("/some/relative/path/page.aspx?Another=Parameter", loginHelper.DefaultUrlExposed);

			global.DefaultPageForTesting = "http:/www.site.com/some/absolute/path/page.aspx?Mode=Show";
			AssertEquals("http:/www.site.com/some/absolute/path/page.aspx", loginHelper.DefaultUrlExposed);

			global.DefaultPageForTesting =
				"http:/www.site.com/some/absolute/path/page.aspx?Mode=Show&Another=Parameter";
			AssertEquals("http:/www.site.com/some/absolute/path/page.aspx?Another=Parameter",
				loginHelper.DefaultUrlExposed);

			global.DefaultPageForTesting =
				"http:/www.site.com/some/absolute/path/page.aspx?Another=Parameter&Mode=Show";
			AssertEquals("http:/www.site.com/some/absolute/path/page.aspx?Another=Parameter",
				loginHelper.DefaultUrlExposed);

			global.DefaultPageForTesting = "/some/relative/path/page.aspx?Mode=Show&";
			AssertEquals("/some/relative/path/page.aspx", loginHelper.DefaultUrlExposed);
		}

		public void TestGetShipmentQuickViewUrlForCFSShipment()
		{
			var shipment = Factory.NewWithValidTestData<TrackingShipment>();
			shipment.JS_HouseBill = "01234";
			shipment.JS_UniqueConsignRef = "23456";
			shipment.JS_IsCFSRegistered = true;
			shipment.JS_IsForwardRegistered = false;
			Factory.Save();

			var loginHelper = (TrackingLoginHelper)GetNewHelper(TestPage);

			string expectedShipmentURL =
				string.Format("{0}?Ref={1}", "/CFSShipments/CFSShipmentDetails.aspx", shipment.PK);
			AssertEquals("Expecting CFS Shipment redirect URL", expectedShipmentURL,
				loginHelper.GetShipmentQuickViewURL(shipment.JS_HouseBill));
			AssertEquals("Expecting CFS Shipment redirect URL", expectedShipmentURL,
				loginHelper.GetShipmentQuickViewURL(shipment.JS_UniqueConsignRef));
		}

		public void TestGetShipmentQuickViewUrlForBooking()
		{
			var shipment = Factory.NewWithValidTestData<TrackingShipment>();
			shipment.JS_HouseBill = "01234";
			shipment.JS_UniqueConsignRef = "23456";
			shipment.JS_IsBooking = true;
			shipment.JS_IsForwardRegistered = false;
			Factory.Save();

			var loginHelper = (TrackingLoginHelper)GetNewHelper(TestPage);

			var expectedShipmentURL = $"/Bookings/BookingDetails.aspx?Ref={shipment.PK}";
			AssertEquals("Expecting Booking redirect URL", expectedShipmentURL,
				loginHelper.GetShipmentQuickViewURL(shipment.JS_HouseBill));
			AssertEquals("Expecting Booking redirect URL", expectedShipmentURL,
				loginHelper.GetShipmentQuickViewURL(shipment.JS_UniqueConsignRef));
		}

		public void TestGetShipmentQuickViewURL()
		{
			TrackingShipment shipment = Factory.NewWithValidTestData<TrackingShipment>();
			shipment.JS_HouseBill = "01234";
			shipment.JS_UniqueConsignRef = "23456";
			Factory.Save();

			TrackingLoginHelper loginHelper = (TrackingLoginHelper)GetNewHelper(TestPage);

			string expectedShipmentURL = string.Format("{0}?Ref={1}", "/Shipments/ShipmentDetails.aspx", shipment.PK);
			AssertEquals("Expecting Shipment redirect URL", expectedShipmentURL,
				loginHelper.GetShipmentQuickViewURL(shipment.JS_HouseBill));
			AssertEquals("Expecting Shipment redirect URL", expectedShipmentURL,
				loginHelper.GetShipmentQuickViewURL(shipment.JS_UniqueConsignRef));

			BaseJobDeclaration declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_HouseBill = "12345";
			declaration.JE_DeclarationReference = "34567";
			Factory.Save();

			string expectedDeclarationURL =
				string.Format("{0}?Ref={1}", "/Declaration/DeclarationDetails.aspx", declaration.PK);
			AssertEquals("Expecting Declaration redirect URL", expectedDeclarationURL,
				loginHelper.GetShipmentQuickViewURL(declaration.JE_HouseBill));
			AssertEquals("Expecting Declaration redirect URL", expectedDeclarationURL,
				loginHelper.GetShipmentQuickViewURL(declaration.JE_DeclarationReference));

			TrackingShipment shipment1 = Factory.NewWithValidTestData<TrackingShipment>();
			shipment1.JS_HouseBill = "12345";
			Factory.Save();

			AssertEquals("Should be the same Housebill number", declaration.JE_HouseBill, shipment1.JS_HouseBill);
			string expectedShipmentURL1 = string.Format("{0}?Ref={1}", "/Shipments/ShipmentDetails.aspx", shipment1.PK);
			AssertEquals("Expecting Shipment redirect URL", expectedShipmentURL1,
				loginHelper.GetShipmentQuickViewURL(declaration.JE_HouseBill));

			#region TestLinerAndAgencyBillOfLading

			var linerAndAgencyBillOfLadingWFI = Factory.NewWithValidTestData<TrackingShipment>();
			linerAndAgencyBillOfLadingWFI.JS_HouseBill = "2345600";
			linerAndAgencyBillOfLadingWFI.JS_IsShipping = true;
			linerAndAgencyBillOfLadingWFI.JS_ShipmentStatus = ShipmentStatusList.Codes.WebFwdInstruction;
			Factory.Save();

			string expectedLinerAndAgencyBillOfLadingURL = string.Format("{0}?Ref={1}",
				"/LinerAndAgency/BillsOfLading/BillOfLadingDetails.aspx", linerAndAgencyBillOfLadingWFI.PK);
			AssertEquals("Expecting Shipment Bills Of Lading redirect URL", expectedLinerAndAgencyBillOfLadingURL,
				loginHelper.GetShipmentQuickViewURL(linerAndAgencyBillOfLadingWFI.JS_HouseBill));

			var linerAndAgencyBillOfLadingCNF = Factory.NewWithValidTestData<TrackingShipment>();
			linerAndAgencyBillOfLadingCNF.JS_HouseBill = "3456700";
			linerAndAgencyBillOfLadingCNF.JS_IsShipping = true;
			linerAndAgencyBillOfLadingCNF.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Factory.Save();

			expectedLinerAndAgencyBillOfLadingURL = string.Format("{0}?Ref={1}",
				"/LinerAndAgency/BillsOfLading/BillOfLadingDetails.aspx", linerAndAgencyBillOfLadingCNF.PK);
			AssertEquals("Expecting Shipment Bills Of Lading redirect URL", expectedLinerAndAgencyBillOfLadingURL,
				loginHelper.GetShipmentQuickViewURL(linerAndAgencyBillOfLadingCNF.JS_HouseBill));

			var linerAndAgencyBillOfLadingNeg = Factory.NewWithValidTestData<TrackingShipment>();
			linerAndAgencyBillOfLadingNeg.JS_HouseBill = "4567800";
			linerAndAgencyBillOfLadingNeg.JS_IsShipping = false; // Turn off IsShipping for negative test case
			linerAndAgencyBillOfLadingNeg.JS_ShipmentStatus = ShipmentStatusList.Codes.Confirmed;
			Factory.Save();

			expectedShipmentURL = string.Format("{0}?Ref={1}", "/Shipments/ShipmentDetails.aspx",
				linerAndAgencyBillOfLadingNeg.PK);
			AssertEquals("Expecting Shipment redirect URL", expectedShipmentURL,
				loginHelper.GetShipmentQuickViewURL(linerAndAgencyBillOfLadingNeg.JS_HouseBill));

			#endregion

			#region TestLinerAndAgencyBookings

			var linerAndAgencyBookingsWEB = Factory.NewWithValidTestData<TrackingShipment>();
			linerAndAgencyBookingsWEB.JS_HouseBill = "5678900";
			linerAndAgencyBookingsWEB.JS_IsShipping = true;
			linerAndAgencyBookingsWEB.JS_ShipmentStatus = ShipmentStatusList.Codes.WebBooking;
			Factory.Save();

			string expectedLinerAndAgencyBookings = string.Format("{0}?Ref={1}",
				"/LinerAndAgency/Bookings/BookingDetails.aspx", linerAndAgencyBookingsWEB.PK);
			AssertEquals("Expecting Shipment Bookings redirect URL", expectedLinerAndAgencyBookings,
				loginHelper.GetShipmentQuickViewURL(linerAndAgencyBookingsWEB.JS_HouseBill));

			var linerAndAgencyBookingsBKD = Factory.NewWithValidTestData<TrackingShipment>();
			linerAndAgencyBookingsBKD.JS_HouseBill = "6789000";
			linerAndAgencyBookingsBKD.JS_IsShipping = true;
			linerAndAgencyBookingsBKD.JS_ShipmentStatus = ShipmentStatusList.Codes.Booked;
			Factory.Save();

			expectedLinerAndAgencyBookings = string.Format("{0}?Ref={1}",
				"/LinerAndAgency/Bookings/BookingDetails.aspx", linerAndAgencyBookingsBKD.PK);
			AssertEquals("Expecting Shipment Bookings redirect URL", expectedLinerAndAgencyBookings,
				loginHelper.GetShipmentQuickViewURL(linerAndAgencyBookingsBKD.JS_HouseBill));

			var linerAndAgencyBookingsWTL = Factory.NewWithValidTestData<TrackingShipment>();
			linerAndAgencyBookingsWTL.JS_HouseBill = "7890100";
			linerAndAgencyBookingsWTL.JS_IsShipping = true;
			linerAndAgencyBookingsWTL.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
			Factory.Save();

			expectedLinerAndAgencyBookings = string.Format("{0}?Ref={1}",
				"/LinerAndAgency/Bookings/BookingDetails.aspx", linerAndAgencyBookingsWTL.PK);
			AssertEquals("Expecting Shipment Bookings redirect URL", expectedLinerAndAgencyBookings,
				loginHelper.GetShipmentQuickViewURL(linerAndAgencyBookingsWTL.JS_HouseBill));

			var linerAndAgencyBookingsNeg = Factory.NewWithValidTestData<TrackingShipment>();
			linerAndAgencyBookingsNeg.JS_HouseBill = "8901200";
			linerAndAgencyBookingsNeg.JS_IsShipping = false; // Turn off IsShipping for negative test case
			linerAndAgencyBookingsNeg.JS_ShipmentStatus = ShipmentStatusList.Codes.WaitListed;
			Factory.Save();

			expectedShipmentURL = string.Format("{0}?Ref={1}", "/Shipments/ShipmentDetails.aspx",
				linerAndAgencyBookingsNeg.PK);
			AssertEquals("Expecting Shipment redirect URL", expectedShipmentURL,
				loginHelper.GetShipmentQuickViewURL(linerAndAgencyBookingsNeg.JS_HouseBill));

			#endregion
		}

		public void TestGetShipmentQuickViewURL_SearchShipmentByMasterBill()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "123456";

			var shipment = Factory.NewWithValidTestData<TrackingShipment>();
			shipment.Consols.Add(consol);

			Factory.Save();

			var loginHelper = (TrackingLoginHelper)GetNewHelper(TestPage);

			string expectedShipmentURL = string.Format("{0}?Ref={1}", "/Shipments/ShipmentDetails.aspx", shipment.PK);
			AssertEquals("Searching by valid MasterBill, should return Shipment Redirect URL", expectedShipmentURL,
				loginHelper.GetShipmentQuickViewURL(consol.JK_MasterBillNum));
			AssertEquals("Searching by invalid MasterBill, should return Shipment Redirect URL", string.Empty,
				loginHelper.GetShipmentQuickViewURL("fake master bill"));

			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			Factory.Save();
			AssertEquals("Searching by valid MasterBill but not a direct shipment, should return empty", string.Empty,
				loginHelper.GetShipmentQuickViewURL(consol.JK_MasterBillNum));

			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			Factory.Save();

			AssertEquals("Searching by valid MasterBill, should return Shipment Redirect URL", expectedShipmentURL,
				loginHelper.GetShipmentQuickViewURL(consol.JK_MasterBillNum));
			AssertEquals("Searching by invalid MasterBill, should return empty", string.Empty,
				loginHelper.GetShipmentQuickViewURL("POLT123456"));
			AssertEquals("Searching by invalid MasterBill, should return empty", string.Empty,
				loginHelper.GetShipmentQuickViewURL("fake master bill"));

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "POLT987654";
			Factory.Save();

			AssertEquals("Searching by valid MasterBill (including SCAC), should return Shipment redirect URL",
				expectedShipmentURL, loginHelper.GetShipmentQuickViewURL(consol.JK_MasterBillNum));
			AssertEquals("Searching by valid MasterBill (excluding), should return Shipment redirect URL",
				expectedShipmentURL, loginHelper.GetShipmentQuickViewURL("987654"));

			consol.JK_MasterBillNum = "987654";
			Factory.Save();

			AssertEquals("Searching by valid MasterBill (excluding SCAC), should return Shipment redirect URL",
				expectedShipmentURL, loginHelper.GetShipmentQuickViewURL(consol.JK_MasterBillNum));
			AssertEquals("Searching by valid MasterBill (including SCAC), should return Shipment redirect URL",
				expectedShipmentURL, loginHelper.GetShipmentQuickViewURL("POLT987654"));
			AssertEquals("Searching by invalid MasterBill (including SCAC), should return empty", string.Empty,
				loginHelper.GetShipmentQuickViewURL("POLT98765X"));
			AssertEquals("Searching by invalid MasterBill (including SCAC), should return empty", string.Empty,
				loginHelper.GetShipmentQuickViewURL("POLT98765"));
			AssertEquals("Searching by invalid MasterBill (excluding SCAC), should return empty", string.Empty,
				loginHelper.GetShipmentQuickViewURL("98765X"));
			AssertEquals("Searching by invalid MasterBill, should return empty", string.Empty,
				loginHelper.GetShipmentQuickViewURL("X"));
			AssertEquals("Searching by invalid MasterBill, should return empty", string.Empty,
				loginHelper.GetShipmentQuickViewURL("54"));

			consol.JK_MasterBillNum = "POLTX";
			Factory.Save();
			AssertEquals("Searching by invalid MasterBill, should return empty", string.Empty,
				loginHelper.GetShipmentQuickViewURL(string.Empty));
			AssertEquals("Searching by valid MasterBill (excluding SCAC), should return Shipment redirect URL",
				expectedShipmentURL, loginHelper.GetShipmentQuickViewURL("X"));
		}

		public void TestGetShipmentQuickViewURL_SearchDeclarationByMasterBill()
		{
			var loginHelper = (TrackingLoginHelper)GetNewHelper(TestPage);
			var testUsCompany = Factory.NewWithValidTestData<GlbCompany>();
			testUsCompany.GC_RN_NKCountryCode = "US";
			var testUsBranch = testUsCompany.Branches.AddNew();
			testUsBranch.GB_Code = "BR1";
			var testUsBranch2 = testUsCompany.Branches.AddNew();
			testUsBranch2.GB_Code = "BR2";

			var usDeclaration1 = Factory.NewWithValidTestData<USCustoms.JobDeclaration>();
			usDeclaration1.JE_TransportMode = "AIR";
			usDeclaration1.JE_MasterBill = "987654";
			usDeclaration1.JE_GB = testUsBranch.PK;
			usDeclaration1.JE_MasterBillIssuerSCAC = "POLT";

			var usDeclaration2 = Factory.NewWithValidTestData<USCustoms.JobDeclaration>();
			usDeclaration2.JE_TransportMode = "SEA";
			usDeclaration2.JE_MasterBill = "987654";
			usDeclaration2.JE_GB = testUsBranch.PK;
			usDeclaration2.JE_MasterBillIssuerSCAC = "POLT";

			var usDeclaration3 = Factory.NewWithValidTestData<USCustoms.JobDeclaration>();
			usDeclaration3.JE_TransportMode = "SEA";
			usDeclaration3.JE_MasterBill = "987654";
			usDeclaration3.JE_GB = testUsBranch.PK;
			usDeclaration3.JE_MasterBillIssuerSCAC = "SCAC";

			var usDeclaration4 = Factory.NewWithValidTestData<USCustoms.JobDeclaration>();
			usDeclaration4.JE_TransportMode = "SEA";
			usDeclaration4.JE_MasterBill = "SCA2987654";
			usDeclaration4.JE_GB = testUsBranch2.PK;
			usDeclaration4.JE_MasterBillIssuerSCAC = "SCAC";

			var usDeclaration5 = Factory.NewWithValidTestData<USCustoms.JobDeclaration>();
			usDeclaration5.JE_TransportMode = "SEA";
			usDeclaration5.JE_MasterBill = "1234596";
			usDeclaration5.JE_GB = testUsBranch.PK;
			usDeclaration5.JE_MasterBillIssuerSCAC = "SCAC";
			Factory.Save();

			string expectedDeclarationURL1 =
				string.Format("{0}?Ref={1}", "/Declaration/DeclarationDetails.aspx", usDeclaration1.PK);
			string expectedDeclarationURL2 =
				string.Format("{0}?Ref={1}", "/Declaration/DeclarationDetails.aspx", usDeclaration2.PK);
			string expectedDeclarationURL3 =
				string.Format("{0}?Ref={1}", "/Declaration/DeclarationDetails.aspx", usDeclaration3.PK);
			string expectedDeclarationURL4 =
				string.Format("{0}?Ref={1}", "/Declaration/DeclarationDetails.aspx", usDeclaration4.PK);

			AssertEquals("Searching by valid MasterBill, should return Declaration redirect URL",
				expectedDeclarationURL1, loginHelper.GetShipmentQuickViewURL("987654"));
			AssertEquals(
				"Searching by valid MasterBill, should return empty as the matching shoud be on SCAC + Masterbill",
				string.Empty, loginHelper.GetShipmentQuickViewURL("123456"));
			AssertEquals(
				"Searching by valid SCAC + MasterBill, should match only SEA/RAI job excluding AIR with MasterBillIssuerSCAC",
				expectedDeclarationURL2, loginHelper.GetShipmentQuickViewURL("POLT987654"));
			AssertEquals(
				"Searching by valid SCAC + MasterBill, should match only SEA/RAI job excluding AIR with MasterBillIssuerSCAC",
				expectedDeclarationURL2, loginHelper.GetShipmentQuickViewURL("polt987654"));
			AssertEquals("Searching by valid SCAC + MasterBill, should match", expectedDeclarationURL3,
				loginHelper.GetShipmentQuickViewURL("SCAC987654"));
			AssertEquals("Searching by valid SCAC + MasterBill, should match", expectedDeclarationURL3,
				loginHelper.GetShipmentQuickViewURL("scac987654"));

			AssertEquals("Searching by invalid MasterBill, should return empty", string.Empty,
				loginHelper.GetShipmentQuickViewURL("NotAValid MasterBillNumber"));
			AssertEquals("Searching by invalid SCAC + valid MasterBill, should return empty", string.Empty,
				loginHelper.GetShipmentQuickViewURL("FAKE SCAC" + usDeclaration1.JE_MasterBill));
			AssertEquals("Searching by invalid MasterBill, should return empty", string.Empty,
				loginHelper.GetShipmentQuickViewURL("X"));

			var anotherUSDeclaration = Factory.NewWithValidTestData<USCustoms.JobDeclaration>();
			anotherUSDeclaration.JE_TransportMode = "RAI";
			anotherUSDeclaration.JE_MasterBill = "987654";
			anotherUSDeclaration.JE_GB = testUsBranch.PK;
			anotherUSDeclaration.JE_MasterBillIssuerSCAC = "POLT";
			Factory.Save();

			AssertEquals("More than one declaration with that MasterBill, should return empty for security reason",
				string.Empty, loginHelper.GetShipmentQuickViewURL("POLT987654"));

			var yetAnotherUSDeclaration = Factory.NewWithValidTestData<USCustoms.JobDeclaration>();
			yetAnotherUSDeclaration.JE_TransportMode = "ROA";
			yetAnotherUSDeclaration.JE_MasterBill = "456";
			yetAnotherUSDeclaration.JE_GB = testUsBranch.PK;
			yetAnotherUSDeclaration.JE_MasterBillIssuerSCAC = "SCAC";

			string expectedDeclarationURL5 = string.Format("{0}?Ref={1}", "/Declaration/DeclarationDetails.aspx",
				yetAnotherUSDeclaration.PK);

			Factory.Save();

			AssertEquals("More than one declaration with that MasterBill, should return empty for security reason",
				expectedDeclarationURL5, loginHelper.GetShipmentQuickViewURL("SCAC456"));
		}

		public void TestGetShipmentQuickViewAdditionalReferences()
		{
			var shipment1 = Factory.NewWithValidTestData<TrackingShipment>();
			shipment1.JS_HouseBill = "01234";

			var shipment2 = Factory.NewWithValidTestData<TrackingShipment>();
			shipment2.JS_BookingReference = "BookingRef";
			shipment2.JS_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);

			var shipment3 = Factory.NewWithValidTestData<TrackingShipment>();
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "OrderRef";
			order.JD_JS = shipment3.PK;

			var shipment4 = Factory.NewWithValidTestData<TrackingShipment>();
			var item = shipment4.DocsAndCartage.OrderItems.AddNew();
			item.JT_OrderReference = "OrderItemRef";

			var shipment5 = Factory.NewWithValidTestData<TrackingShipment>();
			shipment5.JS_BookingReference = "BookingRef";
			shipment5.JS_SystemCreateTimeUtc = ZDateTime.Now;

			Factory.Save();

			TrackingLoginHelper loginHelper = (TrackingLoginHelper)GetNewHelper(TestPage);

			string expectedShipment1URL = string.Format("{0}?Ref={1}", "/Shipments/ShipmentDetails.aspx", shipment1.PK);
			AssertEquals("Expecting Shipment redirect URL", expectedShipment1URL,
				loginHelper.GetShipmentQuickViewURL(shipment1.JS_HouseBill));

			string expectedShipment2URL = string.Format("{0}?Ref={1}", "/Shipments/ShipmentDetails.aspx", shipment2.PK);
			string expectedShipment5URL = string.Format("{0}?Ref={1}", "/Shipments/ShipmentDetails.aspx", shipment5.PK);
			WebDataRegistry.Instance.WebTrackerQuickViewByAdditionalReferences.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, false);
			AssertEquals("Expecting empty redirect URL", string.Empty,
				loginHelper.GetShipmentQuickViewURL(shipment2.JS_BookingReference));
			WebDataRegistry.Instance.WebTrackerQuickViewByAdditionalReferences.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, true);
			Assert("Precondition: Shipment5 created later than Shipment2",
				shipment2.JS_SystemCreateTimeUtc < shipment5.JS_SystemCreateTimeUtc);
			AssertEquals("Expecting Shipment5 redirect URL - created later", expectedShipment5URL,
				loginHelper.GetShipmentQuickViewURL(shipment2.JS_BookingReference));

			string expectedShipment3URL = string.Format("{0}?Ref={1}", "/Shipments/ShipmentDetails.aspx", shipment3.PK);
			AssertEquals("Expecting Shipment3 redirect URL - order ref", expectedShipment3URL,
				loginHelper.GetShipmentQuickViewURL("OrderRef"));

			string expectedShipment4URL = string.Format("{0}?Ref={1}", "/Shipments/ShipmentDetails.aspx", shipment4.PK);
			AssertEquals("Expecting Shipment4 redirect URL - order item ref", expectedShipment4URL,
				loginHelper.GetShipmentQuickViewURL("OrderItemRef"));
		}

		public void TestShipmentQuickViewFromParams()
		{
			TrackingShipment shipment = Factory.NewWithValidTestData<TrackingShipment>();
			shipment.JS_UniqueConsignRef = "23456";
			Factory.Save();

			typeof(ZPage).InvokeMember("LoadOrCreateDataSource",
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, TestPage, null);
			TrackingLoginManager loginMan = TestPage.DataSource as TrackingLoginManager;
			AssertNotNull("LoginManager", loginMan);
			TrackingLoginHelper helper = (TrackingLoginHelper)GetNewHelper(TestPage);
			helper.SetParamsValueForTest("QuickViewNumber", shipment.JS_UniqueConsignRef);
			// Incomplete login details should not affect result
			helper.SetParamsValueForTest("CompanyCode", "EDITEST");
			helper.SetParamsValueForTest("UserEmail", "test@cargowise.com");

			WebDataRegistry.Instance.WebTrackerShipmentQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			helper.OnPageLoad();
			AssertNotNull("SiteUser", TestPage.SiteUser);
			AssertEquals("User should not be logged in", false, TestPage.SiteUser.IsLoggedIn);
			AssertEquals("Response should not be redirected", true,
				string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));

			WebDataRegistry.Instance.WebTrackerShipmentQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			helper.OnPageLoad();
			AssertNotNull("SiteUser", TestPage.SiteUser);
			AssertEquals("User should be logged in", true, TestPage.SiteUser.IsLoggedIn);
			AssertEquals("OrgCode should be correct", GlbCompany.CurrentCompany.OrgProxy.OH_Code,
				TestPage.SiteUser.LoggedInOrganisation.OH_Code);
			AssertEquals("Contact Email should be empty because it is a temporary contact for superuser login",
				ZString.Empty, TestPage.SiteUser.LoggedInWebContact.OC_Email);
			string expectedUrl = string.Format("{0}?Ref={1}", "/Shipments/ShipmentDetails.aspx", shipment.PK);
			AssertEquals("Response should be redirected to the expected Url", expectedUrl,
				HttpContext.Current.Response.RedirectLocation);
		}

		public void TestShipmentQuickViewParams_SkipIfReturnUrl()
		{
			var shipment = Factory.NewWithValidTestData<TrackingShipment>();
			shipment.JS_UniqueConsignRef = "23456";
			Factory.Save();

			typeof(ZPage).InvokeMember("LoadOrCreateDataSource",
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, TestPage, null);
			var loginMan = TestPage.DataSource as TrackingLoginManager;
			var helper = (TrackingLoginHelper)GetNewHelper(TestPage);
			helper.SetParamsValueForTest("QuickViewNumber", shipment.JS_UniqueConsignRef);
			helper.SetParamsValueForTest("ReturnUrl", "SomePageWithAuthentication.aspx");

			using (WebDataRegistry.Instance.WebTrackerShipmentQuickView.SetTemporaryValue(Guid.Empty, Guid.Empty,
						Guid.Empty, true))
			{
				helper.OnPageLoad();
				Assert("QuickViewNumber should be empty", loginMan.QuickViewNumber.IsEmpty);
				Assert("Request should not be redirected", !HttpContext.Current.Response.IsRequestBeingRedirected);
			}
		}

		public void TestShipmentQuickViewWithNoOrgProxy()
		{
			var shipment = Factory.NewWithValidTestData<TrackingShipment>();
			shipment.JS_UniqueConsignRef = "23456";
			Factory.Save();

			typeof(ZPage).InvokeMember("LoadOrCreateDataSource",
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, TestPage, null);
			var loginMan = TestPage.DataSource as TrackingLoginManager;
			var helper = (TrackingLoginHelper)GetNewHelper(TestPage);
			helper.SetParamsValueForTest("QuickViewNumber", shipment.JS_UniqueConsignRef);

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
			AssertNull(GlbCompany.CurrentCompany.OrgProxy);
			helper.OnPageLoad();
			Assert(!TestPage.SiteUser.IsLoggedIn);
			AssertNull(HttpContext.Current.Response.RedirectLocation);
		}

		public void TestGetShipmentPk()
		{
			TrackingShipment shipment = Factory.NewWithValidTestData<TrackingShipment>();
			shipment.JS_UniqueConsignRef = "23456";
			Factory.Save();
			TrackingLoginHelper helper = (TrackingLoginHelper)GetNewHelper(TestPage);
			AssertEquals(shipment.PK, helper.GetShipmentPk(shipment.JS_UniqueConsignRef));
		}

		public void TestAreShipmentAndContainerRelated()
		{
			TrackingShipment shipmentWithContainer = Factory.NewWithValidTestData<TrackingShipment>();
			shipmentWithContainer.JS_UniqueConsignRef = "23456";

			TrackingShipment shipmentWithoutContainer = Factory.NewWithValidTestData<TrackingShipment>();
			shipmentWithoutContainer.JS_UniqueConsignRef = "12345";

			Factory.Save();

			TrackingConsol consol = Factory.NewWithValidTestData<TrackingConsol>();
			Factory.Save();

			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			container.JC_ContainerNum = "TGHU216203";
			container.JC_JK = consol.PK;
			Factory.Save();

			JobConShipLink jobConShipLink = Factory.NewWithValidTestData<JobConShipLink>();
			jobConShipLink.JN_JS = shipmentWithContainer.PK;
			jobConShipLink.JN_JK = consol.PK;
			Factory.Save();

			TrackingLoginHelper helper = (TrackingLoginHelper)GetNewHelper(TestPage);
			AssertEquals(true,
				helper.AreShipmentAndContainerRelated(container.JC_ContainerNum, shipmentWithContainer.PK));
			AssertEquals(false,
				helper.AreShipmentAndContainerRelated(container.JC_ContainerNum, shipmentWithoutContainer.PK));
		}

		public void TestContainerQuickViewShowDetailsOfLatestJobRelatedToContainer()
		{
			var shipmentWithContainer1 = Factory.NewWithValidTestData<TrackingShipment>();
			shipmentWithContainer1.JS_UniqueConsignRef = "23456";

			var shipmentWithContainer2 = Factory.NewWithValidTestData<TrackingShipment>();
			shipmentWithContainer2.JS_UniqueConsignRef = "12345";

			var consol1 = Factory.NewWithValidTestData<TrackingConsol>();
			var consol2 = Factory.NewWithValidTestData<TrackingConsol>();

			var container1 = Factory.NewWithValidTestData<TrackingContainer>();
			container1.JC_ContainerNum = "TGHU216203";
			container1.JC_JK = consol1.PK;
			container1.JC_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);

			var container2 = Factory.NewWithValidTestData<TrackingContainer>();
			container2.JC_ContainerNum = "TGHU216203";
			container2.JC_JK = consol2.PK;
			container2.JC_SystemCreateTimeUtc = ZDateTime.Now;

			var jobConShipLink1 = Factory.NewWithValidTestData<JobConShipLink>();
			jobConShipLink1.JN_JS = shipmentWithContainer1.PK;
			jobConShipLink1.JN_JK = consol1.PK;

			var jobConShipLink2 = Factory.NewWithValidTestData<JobConShipLink>();
			jobConShipLink2.JN_JS = shipmentWithContainer2.PK;
			jobConShipLink2.JN_JK = consol2.PK;

			Factory.Save();

			var queryContainer = new ZQuery(JobContainerSchema.JC_ContainerNum, "TGHU216203");
			queryContainer.OrderBy = JobContainerSchema.JC_SystemCreateTimeUtc.Name + OrderByClause.Descending;

			IList containerWithSameName = Factory.Load<TrackingContainer>(queryContainer);
			AssertEquals(2, containerWithSameName.Count);

			using (TestPage.Factory.EnableTableHitQueryCollection(new[] { JobContainerSchema.Constants.TableName }))
			{
				typeof(ZPage).InvokeMember("LoadOrCreateDataSource",
					BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, TestPage, null);
				var loginMan = TestPage.DataSource as TrackingLoginManager;

				AssertNotNull("LoginManager", loginMan);
				var helper = (TrackingLoginHelper)GetNewHelper(TestPage);
				helper.SetParamsValueForTest("ContainerQuickViewNumber", "TGHU216203");
				helper.SetParamsValueForTest("CompanyCode", "EDITEST");
				helper.SetParamsValueForTest("UserEmail", "test@cargowise.com");

				WebDataRegistry.Instance.WebTrackerContainerQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				helper.OnPageLoad();
				AssertNotNull("SiteUser", TestPage.SiteUser);
				AssertEquals("User should be logged in", true, TestPage.SiteUser.IsLoggedIn);
				AssertEquals("OrgCode should be correct", GlbCompany.CurrentCompany.OrgProxy.OH_Code,
					TestPage.SiteUser.LoggedInOrganisation.OH_Code);
				AssertEquals("Contact Email should be empty because it is a temporary contact for superuser login",
					ZString.Empty, TestPage.SiteUser.LoggedInWebContact.OC_Email);

				string expectedUrl = string.Format("{0}?Ref={1}", "/Containers/ContainerDetails.aspx", container2.PK);
				AssertEquals("Response should be redirected to the expected Url", expectedUrl,
					HttpContext.Current.Response.RedirectLocation);

				var tableSelect =
					TestPage.Factory.TableSelects.Single(x => x.TableName == JobContainerSchema.Constants.TableName);
				foreach (var query in tableSelect.Queries)
				{
					AssertContains("Query needs to have ORDER BY", $"ORDER BY {JobContainerSchema.Constants.JC_SystemCreateTimeUtc}{OrderByClause.Descending}", query.Query);
				}
			}
		}

		public void TestLinerAndAgencyContainerQuickViewFromParams()
		{
			var shipmentWithContainer = Factory.NewWithValidTestData<AgencyShipment>();
			shipmentWithContainer.JS_UniqueConsignRef = "23456";
			shipmentWithContainer.JS_IsShipping = true;

			Factory.Save();

			var container = Factory.NewWithValidTestData<LinerAndAgencyContainer>();
			container.JC_ContainerNum = "TGHU216203";
			container.JC_JS_FCLBookingOnlyLink = shipmentWithContainer.PK;

			Factory.Save();

			typeof(ZPage).InvokeMember("LoadOrCreateDataSource",
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, TestPage, null);
			var loginMan = TestPage.DataSource as TrackingLoginManager;

			AssertNotNull("LoginManager", loginMan);
			var helper = (TrackingLoginHelper)GetNewHelper(TestPage);
			helper.SetParamsValueForTest("ContainerQuickViewNumber", container.JC_ContainerNum);
			// Incomplete login details should not affect result
			helper.SetParamsValueForTest("CompanyCode", "EDITEST");
			helper.SetParamsValueForTest("UserEmail", "test@cargowise.com");

			WebDataRegistry.Instance.WebTrackerContainerQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			helper.OnPageLoad();
			AssertNotNull("SiteUser", TestPage.SiteUser);
			AssertEquals("User should not be logged in", false, TestPage.SiteUser.IsLoggedIn);
			AssertEquals("Response should not be redirected", true,
				string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));

			WebDataRegistry.Instance.WebTrackerContainerQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			helper.OnPageLoad();
			AssertNotNull("SiteUser", TestPage.SiteUser);
			AssertEquals("User should be logged in", true, TestPage.SiteUser.IsLoggedIn);
			AssertEquals("OrgCode should be correct", GlbCompany.CurrentCompany.OrgProxy.OH_Code,
				TestPage.SiteUser.LoggedInOrganisation.OH_Code);
			AssertEquals("Contact Email should be empty because it is a temporary contact for superuser login",
				ZString.Empty, TestPage.SiteUser.LoggedInWebContact.OC_Email);
			string expectedUrl = string.Format("{0}?Ref={1}",
				"/LinerAndAgency/LinerAndAgencyContainers/LinerAndAgencyContainerDetails.aspx", container.PK);
			AssertEquals("Response should be redirected to the expected Url", expectedUrl,
				HttpContext.Current.Response.RedirectLocation);
		}

		public void TestContainerQuickViewFromParams()
		{
			TrackingContainer container = Factory.NewWithValidTestData<TrackingContainer>();
			container.JC_ContainerNum = "TGHU216203";
			Factory.Save();

			typeof(ZPage).InvokeMember("LoadOrCreateDataSource",
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, TestPage, null);
			TrackingLoginManager loginMan = TestPage.DataSource as TrackingLoginManager;

			AssertNotNull("LoginManager", loginMan);
			TrackingLoginHelper helper = (TrackingLoginHelper)GetNewHelper(TestPage);
			helper.SetParamsValueForTest("ContainerQuickViewNumber", container.JC_ContainerNum);
			// Incomplete login details should not affect result
			helper.SetParamsValueForTest("CompanyCode", "EDITEST");
			helper.SetParamsValueForTest("UserEmail", "test@cargowise.com");

			WebDataRegistry.Instance.WebTrackerContainerQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			helper.OnPageLoad();
			AssertNotNull("SiteUser", TestPage.SiteUser);
			AssertEquals("User should not be logged in", false, TestPage.SiteUser.IsLoggedIn);
			AssertEquals("Response should not be redirected", true,
				string.IsNullOrEmpty(HttpContext.Current.Response.RedirectLocation));

			WebDataRegistry.Instance.WebTrackerContainerQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			helper.OnPageLoad();
			AssertNotNull("SiteUser", TestPage.SiteUser);
			AssertEquals("User should be logged in", true, TestPage.SiteUser.IsLoggedIn);
			AssertEquals("OrgCode should be correct", GlbCompany.CurrentCompany.OrgProxy.OH_Code,
				TestPage.SiteUser.LoggedInOrganisation.OH_Code);
			AssertEquals("Contact Email should be empty because it is a temporary contact for superuser login",
				ZString.Empty, TestPage.SiteUser.LoggedInWebContact.OC_Email);
			string expectedUrl = string.Format("{0}?Ref={1}", "/Containers/ContainerDetails.aspx", container.PK);
			AssertEquals("Response should be redirected to the expected Url", expectedUrl,
				HttpContext.Current.Response.RedirectLocation);
		}

		public void TestContainerQuickViewParams_SkipIfReturnUrl()
		{
			var container = Factory.NewWithValidTestData<TrackingContainer>();
			container.JC_ContainerNum = "TGHU216203";
			Factory.Save();

			typeof(ZPage).InvokeMember("LoadOrCreateDataSource",
				BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, TestPage, null);
			var loginMan = TestPage.DataSource as TrackingLoginManager;
			var helper = (TrackingLoginHelper)GetNewHelper(TestPage);
			helper.SetParamsValueForTest("ContainerQuickViewNumber", container.JC_ContainerNum);
			helper.SetParamsValueForTest("ReturnUrl", "SomePageWithAuthentication.aspx");

			using (WebDataRegistry.Instance.WebTrackerContainerQuickView.SetTemporaryValue(Guid.Empty, Guid.Empty,
						Guid.Empty, true))
			{
				helper.OnPageLoad();
				Assert("ContainerQuickViewNumber should be empty", loginMan.ContainerQuickViewNumber.IsEmpty);
				Assert("Request should not be redirected", !HttpContext.Current.Response.IsRequestBeingRedirected);
			}
		}

		public void TestGetShipmentQuickViewCanadianReferences()
		{
			TrackingLoginHelper loginHelper = (TrackingLoginHelper)GetNewHelper(TestPage);
			var cachedValue = WebDataRegistry.Instance.WebTrackerUseCanadianReferencesQuickView.Value;

			#region Find Shipment using CCN CusEntryNumber

			var shipment = Factory.NewWithValidTestData<TrackingShipment>();
			var cusEntryNumberShipment = Factory.NewWithValidTestData<CusEntryNumber>();
			cusEntryNumberShipment.CE_EntryIsSystemGenerated = false;
			cusEntryNumberShipment.CE_ParentID = shipment.PK;
			cusEntryNumberShipment.CE_ParentTable = shipment.TableName;
			cusEntryNumberShipment.CE_EntryNum = "1234561";
			cusEntryNumberShipment.CE_ParentTable = JobShipmentSchema.Constants.TableName;
			cusEntryNumberShipment.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			cusEntryNumberShipment.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			Factory.Save();

			WebDataRegistry.Instance.WebTrackerUseCanadianReferencesQuickView.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, true);
			string expectedShipmentURL = string.Format("{0}?Ref={1}", "/Shipments/ShipmentDetails.aspx", shipment.PK);
			AssertEquals("Expecting Shipment redirect URL", expectedShipmentURL,
				loginHelper.GetShipmentQuickViewURL(cusEntryNumberShipment.CE_EntryNum));
			WebDataRegistry.Instance.WebTrackerUseCanadianReferencesQuickView.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, false);
			AssertEquals("Since Canadian References were disabled, Declaration should not be found", string.Empty,
				loginHelper.GetShipmentQuickViewURL(cusEntryNumberShipment.CE_EntryNum));

			#endregion

			#region Find Declaration Using CCN CusEntryNumber

			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var cusEntryNumberDeclaration = Factory.NewWithValidTestData<CusEntryNumber>();
			cusEntryNumberDeclaration.CE_ParentID = declaration1.PK;
			cusEntryNumberDeclaration.CE_ParentTable = declaration1.TableName;
			cusEntryNumberDeclaration.CE_EntryNum = "1234562";
			cusEntryNumberDeclaration.CE_ParentTable = JobDeclarationSchema.Constants.TableName;
			cusEntryNumberDeclaration.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			cusEntryNumberDeclaration.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			Factory.Save();

			WebDataRegistry.Instance.WebTrackerUseCanadianReferencesQuickView.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, true);
			string expectedDeclarationURL =
				string.Format("{0}?Ref={1}", "/Declaration/DeclarationDetails.aspx", declaration1.PK);
			AssertEquals("Expecting Declaration redirect URL", ZString.Empty,
				loginHelper.GetShipmentQuickViewURL("123 4562"));
			AssertEquals("Expecting Declaration redirect URL", expectedDeclarationURL,
				loginHelper.GetShipmentQuickViewURL("1234562"));
			WebDataRegistry.Instance.WebTrackerUseCanadianReferencesQuickView.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, false);
			AssertEquals("Since Canadian References were disabled, Declaration should not be found", string.Empty,
				loginHelper.GetShipmentQuickViewURL(cusEntryNumberDeclaration.CE_EntryNum));

			#endregion

			#region Find Declaration Using CCN CusAddInfo

			var declaration2 = Factory.New(ObjectFactory.GetType<IJobDeclaration>());
			var ccNumber = (ICargoControlNumber)Factory.New(ObjectFactory.GetType<ICargoControlNumber>());
			ccNumber.B7_ParentID = declaration2.PK;
			ccNumber.B7_ParentTableCode = declaration2.TablePrefix;
			var ccn = "1234563";
			ccNumber.B7_AddInfoData = "CCNInfoNumber=" + ccn;
			ccNumber.B7_Type = Customs.Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.CACCN;
			Factory.Save();

			WebDataRegistry.Instance.WebTrackerUseCanadianReferencesQuickView.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, true);
			expectedDeclarationURL =
				string.Format("{0}?Ref={1}", "/Declaration/DeclarationDetails.aspx", declaration2.PK);
			AssertEquals("Expecting Declaration redirect URL", expectedDeclarationURL,
				loginHelper.GetShipmentQuickViewURL(ccn));
			WebDataRegistry.Instance.WebTrackerUseCanadianReferencesQuickView.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, false);
			AssertEquals("Since Canadian References were disabled, Declaration should not be found", string.Empty,
				loginHelper.GetShipmentQuickViewURL(ccn));

			#endregion

			#region Find Declaration Using Transaction Number

			var declaration3 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var cusEntryNumberTransaction = Factory.NewWithValidTestData<CusEntryNumber>();
			cusEntryNumberTransaction.CE_ParentID = declaration3.PK;
			cusEntryNumberTransaction.CE_ParentTable = declaration3.TableName;
			cusEntryNumberTransaction.CE_EntryNum = "1234564";
			cusEntryNumberTransaction.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
			cusEntryNumberTransaction.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			cusEntryNumberTransaction.CE_EntryType = CusEntryNumber.EntryType.CATransactionNumber;
			Factory.Save();

			WebDataRegistry.Instance.WebTrackerUseCanadianReferencesQuickView.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, true);
			expectedDeclarationURL =
				string.Format("{0}?Ref={1}", "/Declaration/DeclarationDetails.aspx", declaration3.PK);
			AssertEquals("Expecting Declaration redirect URL", expectedDeclarationURL,
				loginHelper.GetShipmentQuickViewURL(cusEntryNumberTransaction.CE_EntryNum));
			WebDataRegistry.Instance.WebTrackerUseCanadianReferencesQuickView.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, false);
			AssertEquals("Since Canadian References were disabled, Declaration should not be found", string.Empty,
				loginHelper.GetShipmentQuickViewURL(cusEntryNumberTransaction.CE_EntryNum));

			#endregion

			WebDataRegistry.Instance.WebTrackerUseCanadianReferencesQuickView.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, cachedValue);
		}
		public void TestGetShipmentQuickViewURL_Performance()
		{
			using (WebDataRegistry.Instance.WebTrackerUseCanadianReferencesQuickView.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (TestPage.Factory.EnableTableHitQueryCollection(new[] { JobShipmentSchema.Constants.TableName, JobDeclarationSchema.Constants.TableName }))
			{
				var loginHelper = (TrackingLoginHelper)GetNewHelper(TestPage);

				var result = loginHelper.GetShipmentQuickViewURL("Test");

				var queries = TestPage.Factory.TableSelects.Single(x => x.TableName == JobShipmentSchema.Constants.TableName).Queries.Select(x => x.Query);

				var mainQueries = queries.Where(x => x.Contains($"{nameof(CommonShipment.JS_HouseBill)} = 'Test'") && x.Contains($"{nameof(CommonShipment.JS_UniqueConsignRef)} = 'Test'"));
				var canadaQueries = queries.Where(x => x.Contains("dbo.CusEntryNum"));

				Assert("Canadian references should be fetched separately for better execution plan", mainQueries.All(x => !canadaQueries.Contains(x)));

				var declarationQueries = TestPage.Factory.TableSelects.Single(x => x.TableName == JobDeclarationSchema.Constants.TableName).Queries.Select(x => x.Query);

				var mainDeclarationQueries = declarationQueries.Where(x => x.Contains($"{nameof(BaseJobDeclaration.JE_HouseBill)} = 'Test'") && x.Contains($"{nameof(BaseJobDeclaration.JE_DeclarationReference)} = 'Test'"));
				var declarationCanadaQueries = declarationQueries.Where(x => x.Contains("dbo.CusEntryNum"));

				Assert("Canadian references should be fetched separately for better execution plan", mainDeclarationQueries.All(x => !declarationCanadaQueries.Contains(x)));
			}
		}

		public void TestGetShipmentQuickViewWithMultipleResults()
		{
			var shipment1 = Factory.NewWithValidTestData<TrackingShipment>();
			shipment1.JS_SystemCreateTimeUtc = DateTime.UtcNow.AddHours(-1);
			shipment1.JS_HouseBill = "123";

			Factory.Save();

			var shipment2 = Factory.NewWithValidTestData<TrackingShipment>();
			shipment2.JS_SystemCreateTimeUtc = DateTime.UtcNow;
			shipment2.JS_HouseBill = shipment1.JS_HouseBill;

			Factory.Save();

			using (TestPage.Factory.EnableTableHitQueryCollection(new[] { JobShipmentSchema.Constants.TableName }))
			{
				var helper = (TrackingLoginHelper)GetNewHelper(TestPage);
				var result = helper.GetShipmentQuickViewURL(shipment1.JS_HouseBill);
				var expectedUrl = $"/Shipments/ShipmentDetails.aspx?Ref={shipment2.PK}";
				AssertEquals(expectedUrl, result);

				var tableSelect =
					TestPage.Factory.TableSelects.Single(x => x.TableName == JobShipmentSchema.Constants.TableName);
				var tableSelectQuery = tableSelect.Queries.Single();
				AssertContains("Query needs to have ORDER BY", $"ORDER BY {JobShipmentSchema.Constants.JS_SystemCreateTimeUtc}{OrderByClause.Descending}", tableSelectQuery.Query);
			}
		}

		(ZPage, TestTrackingLoginHelper, OrgContact) SetupForRedirectViaLoginRouter()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var loginContact = CreateNewContact("user1", "user@1.com", "1234", org);
			loginContact.SupersedeWebAccess();
			Factory.Save();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var activeContact = CreateNewContact("user2", "user@2.com", "1234", org2);
			activeContact.OC_PER = loginContact.OC_PER;
			Factory.Save();

			var page = GetNewPageWithRequest();
			typeof(ZPage).InvokeMember("LoadOrCreateDataSource", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, page, null);

			var helper = (TestTrackingLoginHelper)GetNewHelper(page);
			helper.OnPageLoad();

			return (page, helper, loginContact);
		}

		public void TestRedirectViaLoginRouter()
		{
			using (WebDataRegistry.Instance.WebTrackerUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://webtracker.com"))
			{
				(ZPage page, TestTrackingLoginHelper helper, OrgContact loginContact) = SetupForRedirectViaLoginRouter();
				helper.RedirectViaLoginRouter(loginContact);
				AssertEquals("Should not sign user in", false, page.SiteUser.IsLoggedIn);
				AssertStartsWith("TrackingSupersededLoginRoutingDescriptor is triggered", "/webapp/Login/LoginSuperseded.aspx", page.Response.RedirectLocation);
			}
		}

		public void TestRedirectViaLoginRouter_WithRememberMeOptionOn()
		{
			using (WebDataRegistry.Instance.WebTrackerUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://webtracker.com"))
			{
				(ZPage page, TestTrackingLoginHelper helper, OrgContact loginContact) = SetupForRedirectViaLoginRouter();
				Assert("Application cookie should not exist", !page.AppInstance.ApplicationCookie.CookieExist());

				helper.LoginManForTesting.UserName = "test@test.com";
				helper.LoginManForTesting.CompanyCode = "TST";
				helper.LoginManForTesting.Password = "test";
				helper.LoginManForTesting.RememberMe = true;
				helper.RedirectViaLoginRouter(loginContact);
				var cookie = page.AppInstance.ApplicationCookie;
				Assert("Application cookie should be created", cookie.CookieExist());
				AssertEquals("Should save user email to cookie", "test@test.com", cookie.GetUserEmail());
				AssertEquals("Should save empty company code to cookie", "TST", cookie.GetCompanyCode());
				AssertEquals("Should save password to cookie", "test", cookie.GetUserPassword());
			}
		}

		public void TestRedirectViaLoginRouter_WithRememberMeOptionOff()
		{
			using (WebDataRegistry.Instance.WebTrackerUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://webtracker.com"))
			{
				(ZPage page, TestTrackingLoginHelper helper, OrgContact loginContact) = SetupForRedirectViaLoginRouter();
				Assert("Application cookie should not exist", !page.AppInstance.ApplicationCookie.CookieExist());

				helper.LoginManForTesting.UserName = "test@test.com";
				helper.LoginManForTesting.CompanyCode = "TST";
				helper.LoginManForTesting.Password = "test";
				helper.LoginManForTesting.RememberMe = false;
				helper.RedirectViaLoginRouter(loginContact);
				Assert("Application cookie should not exist", !page.AppInstance.ApplicationCookie.CookieExist());
			}
		}

		#region Setup

		protected override LoginManager GetPageNewDataSource()
		{
			return new TrackingLoginManager();
		}

		protected override WebApplicationLoginHelper GetNewHelper(ZPage page)
		{
			return new TestTrackingLoginHelper(page);
		}

		protected override WebApplicationLoginHelper GetNewHelper(ZPage page, bool isAutoMaticallyHookupOnLoad)
		{
			return new TestTrackingLoginHelper(page, isAutoMaticallyHookupOnLoad);
		}

		protected override void SetupLoginDataFromParamsForTest(OrgContactLoginHelper helper)
		{
			((TestTrackingLoginHelper)helper).SetupLoginDataFromParamsForTest();
		}

		PageForLoginHelperTest TestPage => testPage ?? (testPage = new PageForLoginHelperTest());

		PageForLoginHelperTest testPage;

		#endregion
	}
}
