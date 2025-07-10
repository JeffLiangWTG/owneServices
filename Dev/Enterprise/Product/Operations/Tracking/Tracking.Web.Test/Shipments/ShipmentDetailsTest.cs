using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Utilities;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class ShipmentDetailsTest : BasePageWithAuthorisationTest
	{
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

		public void TestDuplicateShipment()
		{
			var page = (TestShipmentDetails)TestPage;
			var testShipment = Factory.NewWithValidTestData<TrackingShipment>();
			page.TestShipment = testShipment;
			page.SetupPageForTest();
			page.OnDuplicateShipmentClickForTest();

			var newBookingPK = GetRedirectReferencePK();
			var newBooking = testShipment.Factory.Load<ForwardingShipment>(newBookingPK);
			AssertEquals(page.SiteUser.ContactAndCompanyReference, newBooking.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		public void TestReverseShipment()
		{
			var page = (TestShipmentDetails)TestPage;
			var testShipment = Factory.NewWithValidTestData<TrackingShipment>();
			page.TestShipment = testShipment;
			page.SetupPageForTest();
			page.OnReverseShipmentClickForTest();

			var newBookingPK = GetRedirectReferencePK();
			var newBooking = testShipment.Factory.Load<ForwardingShipment>(newBookingPK);
			AssertEquals(page.SiteUser.ContactAndCompanyReference, newBooking.Logs.AutoCreatedLogDefaultSL_Reference);
		}

		public void TestShipmentDetailsComponentsVisibiltyWhenUserIsShipmentQuickViewUser()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			using (TestShipmentDetails page = new TestShipmentDetails())
			{
				AssertNotNull("TestOrg", helper.TestOrg);
				AssertNotNull("TestUser", helper.TestSiteUser);
				AssertEquals("QuickViewUser is logged in", true, page.SiteUser.IsShipmentQuickViewUser);

				TrackingShipment testShipment = Factory.NewWithValidTestData<TrackingShipment>();
				testShipment.JS_UniqueConsignRef = "test";
				testShipment.ConsigneeDeliveryAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				testShipment.SiteUser = helper.TestSiteUser;
				SetupCharges(testShipment, GlbCompany.CurrentCompany.OrgProxy);
				testShipment.RelatedShipments.AddNew();
				testShipment.JS_JS_ColoadMasterShipment = ZGuid.NewZGuid();
				page.TestShipment = testShipment;
				page.SetupPageForTest();

				SetupSecurity(WebSecurityRightsList.WebInvoicingAndStatements, helper.TestOrg, helper.TestContact, true);
				WebDataRegistry.Instance.UseWebAccountsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				WebDataRegistry.Instance.WebTrackerLocalChargesOnShipmentQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				page.ForTest_RunOnLoad();
				page.ForTest_RunOnPreBind();

				Assert(!page.DuplicateShipmentForTest.Visible);
				Assert(!page.ReverseShipmentForTest.Visible);
				Assert(!page.TransportGridForTest.ShouldShowControl);
				Assert(!page.PackLinesGridForTest.ShouldShowControl);
				Assert(!page.OrdersGridForTest.ShouldShowControl);
				Assert(!page.ContainerGridForTest.ShouldShowControl);
				Assert(!page.ReferenceDataGridForTest.ShouldShowControl);
				Assert(page.ChargesGridForTest.Visible);
				Assert(page.LocalChargesGridForTest.Visible);
				Assert(!page.ChargesGridForTest.ShouldShowControl);
				Assert(!page.LocalChargesGridForTest.ShouldShowControl);
				Assert(!page.CustomsEntriesDataGridForTest.ShouldShowControl);
				Assert(!testShipment.JS_JS_ColoadMasterShipment.IsEmpty);
				AssertEquals(1, testShipment.RelatedShipments.Count);
				Assert(!page.RelatedShipmentsForTest.ShouldShowControl);
			}
		}

		public void TestChargesGrids()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			bool cachedUseWebAccountsModuleValue = WebDataRegistry.Instance.UseWebAccountsModule.Value;
			bool cachedWebTrackerLocalChargesOnShipmentQuickView = WebDataRegistry.Instance.WebTrackerLocalChargesOnShipmentQuickView.Value;
			using (TestShipmentDetails page = new TestShipmentDetails())
			{
				TrackingShipment testShipment = Factory.NewWithValidTestData<TrackingShipment>();
				testShipment.JS_UniqueConsignRef = "test";
				testShipment.ConsigneeDeliveryAddress.E2_OA_Address = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				testShipment.SiteUser = helper.TestSiteUser;
				SetupCharges(testShipment, GlbCompany.CurrentCompany.OrgProxy);
				page.TestShipment = testShipment;
				page.SetupPageForTest();
				Assert(!helper.TestSiteUser.IsShipmentQuickViewUser);
				SetupSecurity(WebSecurityRightsList.WebInvoicingAndStatements, helper.TestOrg, helper.TestContact, false);
				WebDataRegistry.Instance.UseWebAccountsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				WebDataRegistry.Instance.WebTrackerLocalChargesOnShipmentQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Assert(!helper.TestSiteUser.CanViewAccounts);
				page.ForTest_RunOnLoad();
				page.ForTest_RunOnPreBind();
				Assert(!page.ChargesGridForTest.Visible);
				Assert(page.LocalChargesGridForTest.Visible);

				SetupSecurity(WebSecurityRightsList.WebInvoicingAndStatements, helper.TestOrg, helper.TestContact, true);
				Assert(!helper.TestSiteUser.CanViewAccounts);
				page.ForTest_RunOnLoad();
				page.ForTest_RunOnPreBind();
				Assert(!page.ChargesGridForTest.Visible);
				Assert(page.LocalChargesGridForTest.Visible);

				WebDataRegistry.Instance.UseWebAccountsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Assert(helper.TestSiteUser.CanViewAccounts);
				page.ForTest_RunOnLoad();
				page.ForTest_RunOnPreBind();
				Assert(page.ChargesGridForTest.Visible);
				Assert(page.LocalChargesGridForTest.Visible);

				helper.TestSiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

				Assert(!helper.TestSiteUser.CanViewAccounts);
				page.ForTest_RunOnLoad();
				page.ForTest_RunOnPreBind();
				Assert(page.ChargesGridForTest.Visible);
				Assert(!page.LocalChargesGridForTest.Visible);

				WebDataRegistry.Instance.WebTrackerLocalChargesOnShipmentQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				Assert(!helper.TestSiteUser.CanViewAccounts);
				page.ForTest_RunOnLoad();
				page.ForTest_RunOnPreBind();
				Assert(page.ChargesGridForTest.Visible);
				Assert(page.LocalChargesGridForTest.Visible);
			}
			using (TestShipmentDetails page = new TestShipmentDetails())
			{
				TrackingShipment testShipment = Factory.NewWithValidTestData<TrackingShipment>();
				testShipment.JS_UniqueConsignRef = "test2";
				testShipment.SiteUser = helper.TestSiteUser;
				page.TestShipment = testShipment;
				page.SetupPageForTest();
				page.ForTest_RunOnLoad();
				page.ForTest_RunOnPreBind();
				Assert(page.ChargesGridForTest.Visible);
				Assert(!page.LocalChargesGridForTest.Visible);
			}
			WebDataRegistry.Instance.UseWebAccountsModule.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedUseWebAccountsModuleValue);
			WebDataRegistry.Instance.WebTrackerLocalChargesOnShipmentQuickView.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedWebTrackerLocalChargesOnShipmentQuickView);
		}

		void SetupCharges(ITransactionSupport parent, OrgHeader orgHeader)
		{
			JobHeader testJob = Factory.NewJobForTesting<JobHeader>();
			testJob.JH_ParentID = parent.PK;
			testJob.JH_GC = GlbCompany.CurrentCompany.PK;
			testJob.LocalChargesPK = orgHeader.PK;

			AccTransactionHeader invoice = Factory.New<AccTransactionHeader>();
			invoice.AH_TransactionNum = "00001000";
			invoice.AH_TransactionType = "INV";
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_OH = orgHeader.PK;
			invoice.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoice.AH_JH = testJob.PK;
			invoice.AH_InvoiceDate = ZDateTime.Now.AddDays(-2);
			invoice.AH_ConsolidatedInvoiceRef = parent.Reference;

			AccTransactionLines chargeLine1 = Factory.New<AccTransactionLines>();
			chargeLine1.AL_AH = invoice.PK;
			chargeLine1.AL_JH = testJob.PK;
			chargeLine1.AL_GB = invoice.AH_GB;
			chargeLine1.AL_GC = invoice.AH_GC;

			AccTransactionLines chargeLine2 = Factory.New<AccTransactionLines>();
			chargeLine2.AL_AH = invoice.PK;
			chargeLine2.AL_JH = testJob.PK;
			chargeLine2.AL_GB = invoice.AH_GB;
			chargeLine2.AL_GC = invoice.AH_GC;
		}

		public void TestETAAndETDTimeSectionVisibility()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			using (TestShipmentDetails page = new TestShipmentDetails())
			{
				TrackingShipment testShipment = Factory.NewWithValidTestData<TrackingShipment>();
				page.TestShipment = testShipment;
				testShipment.SiteUser = helper.TestSiteUser;
				page.SetupPageForTest();
				page.ForTest_RunOnLoad();

				testShipment.JS_TransportMode = Constants.TransportModes.Air;
				page.ForTest_RunOnPreBind();
				AssertEquals(ZDateTimePickerFormat.Long, page.ETDForTest.DateTimeFormat);

				testShipment.JS_TransportMode = Constants.TransportModes.Sea;
				page.ForTest_RunOnPreBind();
				AssertEquals(ZDateTimePickerFormat.Short, page.ETDForTest.DateTimeFormat);

				testShipment.JS_TransportMode = Constants.TransportModes.Rail;
				page.ForTest_RunOnPreBind();
				AssertEquals(ZDateTimePickerFormat.Long, page.ETDForTest.DateTimeFormat);

				testShipment.JS_TransportMode = Constants.TransportModes.SeaAir;
				page.ForTest_RunOnPreBind();
				AssertEquals(ZDateTimePickerFormat.Short, page.ETDForTest.DateTimeFormat);
			}
		}

		public void TestLoadingMetersVisibility()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			using (TestShipmentDetails page = new TestShipmentDetails())
			{
				TrackingShipment testShipment = Factory.NewWithValidTestData<TrackingShipment>();
				page.TestShipment = testShipment;
				page.SetupPageForTest();

				var enableRoadLoadingMeters = FreightDataRegistry.Instance.EnableRoadLoadingMeters.Value;

				FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				testShipment.JS_TransportMode = Constants.TransportModes.Road;
				page.ForTest_RunOnLoad();
				Assert("Loading Meters should be visible for-Road Shipments", page.LoadingMetersRowForTest.Visible);

				FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				testShipment.JS_TransportMode = Constants.TransportModes.Air;
				page.ForTest_RunOnLoad();
				Assert("Loading Meters should NOT be visible for non Road Shipments", !page.LoadingMetersRowForTest.Visible);

				FreightDataRegistry.Instance.EnableRoadLoadingMeters.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enableRoadLoadingMeters);
			}
		}

		public void TestAdditionalTermsVisibility()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			using (TestShipmentDetails page = new TestShipmentDetails())
			{
				TrackingShipment testShipment = Factory.NewWithValidTestData<TrackingShipment>();
				page.TestShipment = testShipment;
				page.SetupPageForTest();

				testShipment.JS_RL_NKOrigin = "USORD";
				testShipment.JS_RL_NKDestination = "USLAX";

				Assert("Shipment should be domestic", testShipment.IsDomesticFreight);
				page.ForTest_RunOnLoad();

				Assert("Additional Terms should NOT be visible for Domestic Shipments", !page.AdditionalTermsRowForTest.Visible);
				AssertEquals("Term should be called Payment Term", "Payment Term:", page.PayTermLabelForTest.Text);

				testShipment.JS_RL_NKDestination = "AUSYD";

				Assert("Shipment should NOT be domestic", !testShipment.IsDomesticFreight);
				page.ForTest_RunOnLoad();

				Assert("Additional Terms should be visible for non-Domestic Shipments", page.AdditionalTermsRowForTest.Visible);
				AssertEquals("Term should be called Incoterm", "Incoterm:", page.PayTermLabelForTest.Text);
			}
		}

		public void TestMasterAndRelatedShipmentsAreas()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
			using (TestShipmentDetails page = new TestShipmentDetails())
			{
				TrackingShipment testShipment = Factory.NewWithValidTestData<TrackingShipment>();
				page.TestShipment = testShipment;
				page.SetupPageForTest();

				Assert(testShipment.JS_JS_ColoadMasterShipment.IsEmpty);
				AssertEquals(0, testShipment.RelatedShipments.Count);

				page.ForTest_RunOnLoad();

				Assert(!page.ForTest_MasterShipmentArea.Visible);
				Assert(!page.ForTest_RelatedShipmentsArea.Visible);

				testShipment.JS_JS_ColoadMasterShipment = ZGuid.NewZGuid();

				Assert(!testShipment.JS_JS_ColoadMasterShipment.IsEmpty);
				AssertEquals(0, testShipment.RelatedShipments.Count);

				page.ForTest_RunOnLoad();

				Assert(page.ForTest_MasterShipmentArea.Visible);
				Assert(!page.ForTest_RelatedShipmentsArea.Visible);

				testShipment.RelatedShipments.AddNew();

				Assert(!testShipment.JS_JS_ColoadMasterShipment.IsEmpty);
				AssertEquals(1, testShipment.RelatedShipments.Count);

				page.ForTest_RunOnLoad();

				Assert(page.ForTest_MasterShipmentArea.Visible);
				Assert(page.ForTest_RelatedShipmentsArea.Visible);
			}
		}

		#region TestDeliveryConfirmations

		public void TestDeliveryConfirmations()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			using (TestShipmentDetails page = new TestShipmentDetails())
			{
				TrackingShipment testShipment = Factory.NewWithValidTestData<TrackingShipment>();
				page.TestShipment = testShipment;
				page.SetupDeliveryGridForTest();
				page.DeliveryGridForTest.DataBind();
				Assert(!page.DeliveryPanelForTest.Visible);

				testShipment.JS_PackingMode = Constants.ContainerModes.LCL;
				page.DeliveryGridForTest.DataBind();
				Assert("Panel should be invisible because there are no packages", !page.DeliveryPanelForTest.Visible);

				TrackingPackLine line = testShipment.OuterPackLines.AddNew();
				line.JL_PackageCount = 2;
				AssertDeliveryGrid("SiteUser has no rights by default", page, false, false, string.Empty);
				Assert(page.DeliveryPanelForTest.Visible);

				testShipment.JS_PackingMode = Constants.ContainerModes.FCL;
				page.DeliveryGridForTest.DataBind();
				Assert("Panel should not be visible because Shipment is containerised", !page.DeliveryPanelForTest.Visible);

				testShipment.JS_PackingMode = Constants.ContainerModes.LCL;
				foreach (OrgSecurityContacts security in helper.TestSiteUser.LoggedInUser.SecurityRightsForBindingOnly)
				{
					if (security.Security.OX_SecurityItemName == WebSecurityRightsList.WebShipmentDeliveryAdd.Code ||
						security.Security.OX_SecurityItemName == WebSecurityRightsList.WebShipmentDeliveryEdit.Code)
					{
						security.OZ_Granted = true;
					}
				}
				Factory.Save();
				helper.TestSiteUser.OnSecurityRightsChangedForTest();
				AssertDeliveryGrid("SiteUser has rights, no confirmations exist", page, true, true, string.Empty);

				CommonPickupDeliveryConfirm confirmation = page.TestShipment.DeliveryConfirms.AddNew();
				AssertDeliveryGrid("Confirmation exist but is not saved", page, true, true, "Save Delivery Requests");

				Factory.Save();
				AssertDeliveryGrid("Confirmation exist in DB but has no Planned/TransportCompany", page, false, true, "Save Delivery Requests");

				confirmation.EU_TransportCoName = "Some company";
				confirmation.EU_PlannedPickupDeliveryTime = ZDateTime.Now;
				AssertDeliveryGrid("Confirmation exist in DB but has no GoodsSignedBy/ActualTime", page, false, true, "Save Delivery Requests");

				confirmation.EU_GoodsSignForBy = "Me";
				confirmation.EU_PickupDeliveryTime = ZDateTime.Now.AddDays(1);
				AssertDeliveryGrid("Confirmation exist and have complete delivery information", page, false, false, string.Empty);
			}
		}

		void AssertDeliveryGrid(string message, TestShipmentDetails page, bool allowAdd, bool allowEdit, string saveConfirmationsText)
		{
			page.DeliveryGridForTest.DataBind();

			AssertEquals(message + ". Checking DeliveryGrid.AllowAdd", allowAdd, page.DeliveryGridForTest.AllowAdd);
			AssertEquals(message + ". Checking DeliveryGrid.AllowDelete", allowAdd, page.DeliveryGridForTest.AllowDelete);
			AssertEquals(message + ". Checking DeliveryGrid.AllowEdit", allowEdit, page.DeliveryGridForTest.AllowEdit);
			AssertEquals(message + ". Checking SaveConfirmations.Visibile", string.IsNullOrEmpty(saveConfirmationsText), !page.SaveConfirmationsForTest.Visible);

			if (page.SaveConfirmationsForTest.Visible)
			{
				AssertEquals(message + ". Checking SaveConfirmations.Text", saveConfirmationsText, page.SaveConfirmationsForTest.Text);
			}
		}

		#endregion

		#region TestTransportGrid

		public void TestTransportGrid()
		{
			using (TestShipmentDetails testShipmentDetailsPage = new TestShipmentDetails())
			{
				testShipmentDetailsPage.SetupTransportGridForTest();
				AssertColumnIsInGrid("Leg", typeof(ZTextEditColumn), testShipmentDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Mode", typeof(ZTextEditColumn), testShipmentDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Type", typeof(ZDropDownListColumn), testShipmentDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Parent", typeof(ZTextEditColumn), testShipmentDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Bill", typeof(ZTextEditColumn), testShipmentDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Vessel", typeof(ZTextEditColumn), testShipmentDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Voyage/Flight", typeof(ZTextEditColumn), testShipmentDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Load", typeof(ZTextEditColumn), testShipmentDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Discharge", typeof(ZTextEditColumn), testShipmentDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Departure", typeof(ZTimelineColumn), testShipmentDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Arrival", typeof(ZTimelineColumn), testShipmentDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Status", typeof(ZTextEditColumn), testShipmentDetailsPage.TransportGridForTest);
			}
		}

		#endregion

		#region TestOrdersGrid

		public void TestOrdersGrid()
		{
			TestHelper helper = new TestHelper(Factory);
			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);

			using (TestShipmentDetails page = new TestShipmentDetails())
			{
				using (ModuleGridLayoutHelper mglHelper = new ModuleGridLayoutHelper(WebModuleIDs.TrackingOrders, page.Factory))
				{
					string layoutKey = mglHelper.GetGridColumnsLayoutKey();
					try
					{
						HttpContext.Current.Session[layoutKey] = "1,2,3";

						page.SetupOrdersGridForTest();

						AssertColumnIsInGrid("Order #", typeof(ZHyperLinkColumn), page.OrdersGridForTest);
						AssertColumnIsInGrid("Status", typeof(ZTextEditColumn), page.OrdersGridForTest);
						AssertColumnIsInGrid("Packs", typeof(ZTextEditColumn), page.OrdersGridForTest);
						AssertColumnIsInGrid("Order Date", typeof(ZDateTimeColumn), page.OrdersGridForTest);

						List<string> fieldsHeaders = new List<string>();
						foreach (ZTemplateColumn column in page.OrdersGridForTest.Columns)
						{
							fieldsHeaders.Add(column.HeaderText);
						}

						List<DataGridColumn> additionalColumns = mglHelper.GetGridColumns();
						foreach (DataGridColumn column in additionalColumns)
						{
							if (!fieldsHeaders.Contains(column.HeaderText))
							{
								Assert(string.Format("{0} column is not found", column.HeaderText), ColumnExists(page.OrdersGridForTest.Columns, column));
							}
						}
					}
					finally
					{
						HttpContext.Current.Session.Remove(layoutKey);
					}
				}
			}
		}

		public void TestOrdersGridVisibility()
		{
			var page = (TestShipmentDetails)TestPage;
			var testShipment = Factory.NewWithValidTestData<TrackingShipment>();
			page.TestShipment = testShipment;
			page.SetupPageForTest();
			var user = page.SiteUser;
			AssertNotNull(user);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "X";
			var contact = org.Contacts.AddNew();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			SetupSecurity(WebSecurityRightsList.WebOrdersView, org, contact, true);

			Factory.Save();

			user.Login(org.OH_Code, "user@user.com", "password");

			page.ForTest_RunOnLoad();

			AssertEquals(true, page.OrdersGridForTest.Visible);

			org.SecurityRights.RemoveAndDeleteAll();
			SetupSecurity(WebSecurityRightsList.WebOrdersView, org, contact, false);
			Factory.Save();
			user.OnSecurityRightsChangedForTest();

			page.ForTest_RunOnLoad();

			AssertEquals(false, page.OrdersGridForTest.Visible);
		}

		bool ColumnExists(DataGridColumnCollection columns, DataGridColumn checkedColumn)
		{
			foreach (DataGridColumn column in columns)
			{
				if (column.HeaderText == checkedColumn.HeaderText)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		public void TestSetupStatusControlHandlesNonCriticalException()
		{
			using (var page = new TestShipmentDetails())
			{
				page.LoadStatusControlSubstitute = () => { throw new HttpException(); };
				Assert(!page.SetupStatusControlForTesting());

				page.LoadStatusControlSubstitute = () => { throw new NullReferenceException(); };
				Assert(!page.SetupStatusControlForTesting());
			}
		}

		[ExpectException(typeof(OutOfMemoryException))]
		public void TestSetupStatusControlThrowsCriticalException()
		{
			using (var page = new TestShipmentDetails())
			{
				page.LoadStatusControlSubstitute = () => { throw new OutOfMemoryException(); };
				page.SetupStatusControlForTesting();
			}
		}

		public void TestSetupAuthorisedContent()
		{
			LoginSiteUserWithoutRight(WebSecurityRightsList.WebBookingsAddEdit);

			using (var page = new TestShipmentDetails())
			{
				Assert("Precondition", !page.SiteUser.CanEditBookings);

				page.SetupPageForTest();
				page.SetupAuthorisedContentForTesting(true);

				Assert(!page.DuplicateShipmentForTest.Enabled);
				Assert(!page.ReverseShipmentForTest.Enabled);
				var expectedToolTip = "You are not authorized to use this function. Please contact your system administrator to request access rights.";
				AssertEquals(expectedToolTip, page.DuplicateShipmentForTest.ToolTip);
				AssertEquals(expectedToolTip, page.ReverseShipmentForTest.ToolTip);
			}
		}

		public void TestOnLoad_NoShipmentsRight()
		{
			LoginSiteUserWithoutRight(WebSecurityRightsList.WebShipmentsView);

			using (var page = new TestShipmentDetails())
			{
				Assert("Precondition", !page.SiteUser.CanViewShipments);
				var testShipment = Factory.NewWithValidTestData<TrackingShipment>();
				page.TestShipment = testShipment;

				page.SetupPageForTest();
				page.ForTest_RunOnLoad();

				Assert(!page.AuthorisedContentForTest.Visible);
			}
		}

		public void TestGridColumnsWereTranslated()
		{
			var resourceKey = "b7f80b10-4f56-4370-bc66-251d7b9bd82c";
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.French))
			using (var page = new TestShipmentDetails())
			{
				var siteUser = new TrackingSiteUser();
				siteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
				((TestGlobal)((ZPage)page).AppInstance).SetSiteUser(siteUser);

				var method = typeof(Page).GetMethod("SetIntrinsics", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(HttpContext) }, null);
				method.Invoke(page, new object[] { HttpContext.Current });

				var testShipment = Factory.NewWithValidTestData<TrackingShipment>();
				page.TestShipment = testShipment;

				page.ForTest_RunInitializeCulture();
				page.SetupPageForTest();
				page.DeliveryGridForTest.Columns.Add(new ZTextEditColumn(Res.GetString(resourceKey, "Custom Field"), CommonPickupDeliveryConfirm.Schema.TotalDeliveredPackages));
				page.ForTest_RunOnLoad();
				var translateFeedbackManager = page.GetTranslationFeedbackManagerForTest();

				AssertCollectionContains(resourceKey, translateFeedbackManager.GetUsedKeysForTest());
			}
		}

		[HttpContextEnabledTest]
		public void TestDuplicateClickWithoutPermission()
		{
			LoginSiteUserWithoutRight(WebSecurityRightsList.WebBookingsAddEdit);

			using (var page = new TestShipmentDetails())
			{
				Assert("Precondition", !page.SiteUser.CanEditBookings);
				AssertNoBookingCreatedForAction(page, () => page.OnDuplicateShipmentClickForTest());
			}
		}

		[HttpContextEnabledTest]
		public void TestReverseClickWithoutPermission()
		{
			LoginSiteUserWithoutRight(WebSecurityRightsList.WebBookingsAddEdit);

			using (var page = new TestShipmentDetails())
			{
				Assert("Precondition", !page.SiteUser.CanEditBookings);
				AssertNoBookingCreatedForAction(page, () => page.OnReverseShipmentClickForTest());
			}
		}

		void AssertNoBookingCreatedForAction(TestShipmentDetails page, Action clickAction)
		{
			page.SetupPageForTest();
			page.TestShipment = page.Factory.NewWithValidTestData<TrackingShipment>();

			clickAction();

			AssertEquals("Should not have created new bookings", 1, CountNewObjects<CommonShipment>(page.Factory));
		}

		static int CountNewObjects<T>(BusinessObjectFactory factory)
			where T : BusinessObject
		{
			var found = factory.Load<T>(new ZQuery() { FetchOnlyFromLocalCache = true });

			var result = 0;

			for (var i = 0; i < found.Length; i++)
			{
				if (!found[i].IsInDatabase)
				{
					result++;
				}
			}

			return result;
		}

		void LoginSiteUserWithoutRight(WebSecurityRight right)
		{
			var helper = new TestHelper(Factory);

			helper.TestOrg.SecurityRights.RemoveAndDeleteAll();
			var orgRight = helper.TestOrg.SecurityRights.AddNew();
			orgRight.OX_Granted = true;
			orgRight.OX_SecurityItemName = right.Code;

			var userRight = helper.TestContact.SecurityRightsForBindingOnly.AddNew();
			userRight.OZ_OX = orgRight.PK;
			userRight.OZ_Granted = false;

			helper.TestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
		}

		#region Implementation

		void AssertColumnIsInGrid(string headerText, Type columnType, ZGrid gridForTesting)
		{
			bool columnIsInGrid = false;
			foreach (DataGridColumn col in gridForTesting.Columns)
			{
				if (col.HeaderText.Equals(headerText) && columnType.Equals(col.GetType()))
				{
					columnIsInGrid = true;
					break;
				}
			}
			Assert(ZString.Format("Column '{0}' of type '{1}' is not in the grid as expected.", headerText, columnType), columnIsInGrid);
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints => new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerForwarding };

		protected override BooleanRegistryItem UseWebModule => WebDataRegistry.Instance.UseWebForwardingShipmentsModule;

		protected override WebSecurityRight SiteUserSecurityRight => WebSecurityRightsList.WebShipmentsView;

		protected override string GetExpectedPageName() => WebTracker.Pages.ShipmentDetails;

		protected override Control GetNewControl()
		{
			var page = new TestShipmentDetails();
			page.SetupPageForTest();

			return page;
		}

		#endregion
	}
}
