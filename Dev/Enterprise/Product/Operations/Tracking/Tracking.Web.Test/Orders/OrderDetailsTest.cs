using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.Tracking.Web.Orders;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class OrderDetailsTest : BasePageWithAuthorisationTest
	{
		#region Overrides

		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.OrderDetails;
		}

		protected override void AssertAuthorisedContent(BooleanRegistryItem useModule, OrgSecurityContacts contactSecurity)
		{
			base.AssertAuthorisedContent(useModule, contactSecurity);
			AssertEquals("Edit Button Enabled", useModule.Value && contactSecurity.OZ_Granted, ((TestOrderDetails)TestPage).EditOrderButtonForTest.Enabled);
			AssertEquals("Cancel Button Enabled", useModule.Value && contactSecurity.OZ_Granted, ((TestOrderDetails)TestPage).CancelOrderButtonForTest.Enabled);
		}

		protected override Control GetNewControl()
		{
			return new TestOrderDetails();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebForwardingOrdersModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebOrdersView; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			((TestOrderDetails)TestPage).SetupPageForTesting();
		}

		protected override IReadOnlyCollection<ILicenceCheckpoint> ExpectedLicenceCheckPoints
		{
			get { return new ILicenceCheckpoint[] { Environment.Env.Licence.WebTrackerOrderManager }; }
		}

		#endregion

		public void TestDisabledButtonsWhenOrderInactive()
		{
			using (var testOrderDetailsPage = new TestOrderDetails())
			{
				var testOrg = Factory.NewWithValidTestData<OrgHeader>();
				testOrg.OH_Code = "XXXXX";

				Factory.Save();

				testOrderDetailsPage.SiteUser.LoginSupportForTest(testOrg.OH_Code);

				var helper = new TestHelper(Factory);
				var testOrder = helper.CreateOrder();
				testOrder.JD_IsCancelled = true;

				testOrderDetailsPage.SetupPageForTesting();
				testOrderDetailsPage.SetCurrentOrderForTest(testOrder);
				testOrderDetailsPage.SetupAuthorisedContentForTest(true);

				Assert("Order/Cancel Div Is Visible", testOrderDetailsPage.OrderCancelledDivForTest.Visible);

				Assert("Edit Button Disabled", !testOrderDetailsPage.EditOrderButtonForTest.Enabled);
				AssertEquals("Edit Button Has Tooltip", "This order has been deactivated.", testOrderDetailsPage.EditOrderButtonForTest.ToolTip);

				Assert("Cancel Button Disabled", !testOrderDetailsPage.CancelOrderButtonForTest.Enabled);
				AssertEquals("Cancel Button Has Tooltip", "This order has been deactivated.", testOrderDetailsPage.CancelOrderButtonForTest.ToolTip);

				testOrder.JD_IsCancelled = false;
				testOrderDetailsPage.SetupAuthorisedContentForTest(true);

				Assert("Order/Cancel Div Is Hidden", !testOrderDetailsPage.OrderCancelledDivForTest.Visible);

				Assert("Edit Button Enabled", testOrderDetailsPage.EditOrderButtonForTest.Enabled);
				AssertEquals("Edit Button Empty Tooltip", string.Empty, testOrderDetailsPage.EditOrderButtonForTest.ToolTip);

				Assert("Cancel Button Enabled", testOrderDetailsPage.CancelOrderButtonForTest.Enabled);
				AssertEquals("Cancel Button Empty Tooltip", string.Empty, testOrderDetailsPage.CancelOrderButtonForTest.ToolTip);
			}
		}

		public void TestCancelOrder()
		{
			using (TestOrderDetails testOrderDetailsPage = new TestOrderDetails())
			{
				testOrderDetailsPage.ShouldCreateDataSource = true;

				testOrderDetailsPage.SetupPageForTesting();
				testOrderDetailsPage.SetupOrderLinesGridForTest();

				var testOrder = (TrackingOrder)testOrderDetailsPage.DataSource;

				AssertEquals(Constants.OrderStatus.Incomplete, testOrder.JD_OrderStatus);
				testOrderDetailsPage.CancelOrderButtonClick();

				AssertEquals(Constants.OrderStatus.Cancelled, testOrder.JD_OrderStatus);
				testOrderDetailsPage.CancelOrderButtonClick();

				AssertEquals(Constants.OrderStatus.Incomplete, testOrder.JD_OrderStatus);
			}
		}

		public void TestCancelOrder_SaveFailed()
		{
			using (var testOrderDetailsPage = new TestOrderDetails())
			{
				testOrderDetailsPage.ShouldCreateDataSource = true;
				testOrderDetailsPage.SetupPageForTesting();
				testOrderDetailsPage.SetupOrderLinesGridForTest();

				testOrderDetailsPage.CancelOrderButtonClick();

				var (key, value) = HttpContext.Current.Response.GetHeaderForTest(0);

				Assert(testOrderDetailsPage.ZClientScript.IsStartupScriptRegistered(typeof(TestOrderDetails), "There was a problem while saving your changes. Please try again."));
				AssertEquals("Refresh", key);
				AssertEquals(string.Format("0; url={0}", string.Format("{0}?Ref={1}", testOrderDetailsPage.AppInstance.OrderDetailsPage, testOrderDetailsPage.DataSource.PK)), value);
			}
		}

		public void TestDuplicateOrder()
		{
			using (var testOrderDetailsPage = new TestOrderDetails())
			{
				var helper = new TestHelper(Factory);
				var testOrder = helper.CreateOrder();
				testOrder.JD_IsCancelled = true;

				testOrderDetailsPage.SetCurrentOrderForTest(testOrder);
				testOrderDetailsPage.DuplicateOrderButtonClick();

				var newOrder = (TrackingOrder)testOrderDetailsPage.Session[0];
				Assert("New order is not cancelled", !newOrder.JD_IsCancelled);
				AssertEquals("Should be redirected to Edit order page", string.Format("{0}?Ref={1}&{2}={3}", testOrderDetailsPage.AppInstance.EditOrderPage, newOrder.PK, ZPage.DataSourceInSessionParameterName, ZPage.DataSourceInSessionParameterValue), HttpContext.Current.Response.RedirectLocation);
				AssertEquals(testOrderDetailsPage.SiteUser.ContactAndCompanyReference, newOrder.Logs.AutoCreatedLogDefaultSL_Reference);
			}
		}

		public void TestViewShipmentDetailsButton()
		{
			using (TestOrderDetails testOrderDetailsPage = new TestOrderDetails())
			{
				OrgHeader testOrg = Factory.New<OrgHeader>();
				testOrg.OH_Code = "XXXXX";

				Factory.Save();

				testOrderDetailsPage.SiteUser.LoginSupportForTest(testOrg.OH_Code);

				TrackingOrder testOrder = Factory.New<TrackingOrder>();
				testOrder.BuyerPK = testOrg.PK;

				testOrderDetailsPage.SetCurrentOrderForTest(testOrder);

				AssertNoExceptionThrown(() => testOrderDetailsPage.ViewShipmentDetailsButtonClick());
			}
		}

		public void TestTransportGrid()
		{
			using (TestOrderDetails testOrderDetailsPage = new TestOrderDetails())
			{
				testOrderDetailsPage.SetupTransportGridForTest();
				AssertColumnIsInGrid("Leg", 0, typeof(ZTextEditColumn), testOrderDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Mode", 1, typeof(ZTextEditColumn), testOrderDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Type", 2, typeof(ZDropDownListColumn), testOrderDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Parent", 3, typeof(ZTextEditColumn), testOrderDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Bill", 4, typeof(ZTextEditColumn), testOrderDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Vessel", 5, typeof(ZTextEditColumn), testOrderDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Voyage/Flight", 6, typeof(ZTextEditColumn), testOrderDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Load", 7, typeof(ZTextEditColumn), testOrderDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Discharge", 8, typeof(ZTextEditColumn), testOrderDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Departure", 9, typeof(ZTimelineColumn), testOrderDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Arrival", 10, typeof(ZTimelineColumn), testOrderDetailsPage.TransportGridForTest);
				AssertColumnIsInGrid("Status", 11, typeof(ZTextEditColumn), testOrderDetailsPage.TransportGridForTest);
			}
		}

		#region TestOrderLinesGrid

		internal static void AssertColumnIsInGrid(ZString headerText, int columnIndex, Type columnType, ZDataGrid gridForTesting)
		{
			if (columnIndex < gridForTesting.Columns.Count)
			{
				bool columnIsInGrid = (gridForTesting.Columns[columnIndex].HeaderText.Equals(headerText));
				Assert(ZString.Format("Column '{0}' is not in the grid at index {1} as expected.", headerText, columnIndex), columnIsInGrid);
				AssertEquals(ZString.Format("Column '{0}' type", headerText), columnType, gridForTesting.Columns[columnIndex].GetType());
			}
			else
			{
				Fail(ZString.Format("The expected index {0} of column '{1}' is out of range.", columnIndex, headerText));
			}
		}

		public void TestOrderLinesGrid()
		{
			using (var testDetailsPage = new TestOrderDetails())
			{
				Globals.IsWeb = true;
				try
				{
					OrgHeader testLoggedInOrg = Factory.New<OrgHeader>();
					testLoggedInOrg.OH_Code = "XXXXX";

					OrgHeader testBuyer = Factory.NewWithValidTestData<OrgHeader>();
					Factory.Save();

					testDetailsPage.SiteUser.LoginSupportForTest(testLoggedInOrg.OH_Code);
					testLoggedInOrg = testDetailsPage.SiteUser.LoggedInOrganisation;

					TrackingOrder testOrder = Factory.New<TrackingOrder>();
					testOrder.BuyerPK = testBuyer.PK;

					testDetailsPage.SetCurrentOrderForTest(testOrder);

					GlbCompany.CurrentCompany.OrgProxy.CustomLabels.RemoveAndDeleteAll();
					testLoggedInOrg.CustomLabels.RemoveAndDeleteAll();
					testBuyer.CustomLabels.RemoveAndDeleteAll();

					OrgCustomLabels b_CD4 = testBuyer.CustomLabels.AddNew();
					b_CD4.OT_FieldName = Constants.CustomLabels.OrderLine.CustomDecimal4;
					b_CD4.OT_Caption = "TestBuyer'sCD4";

					GlbCompany.CurrentCompany.OrgProxy.MiscServ.OM_IMPartAttrib1Name = "OrgProxy'sPA1";
					testBuyer.MiscServ.OM_IMPartAttrib1Name = "Buyer'sPA1";

					testDetailsPage.SetupOrderLinesGridForTest();

					foreach (DataGridColumn column in testDetailsPage.OrderLinesGridForTest.Columns)
					{
						Assert("Should not be any Part Attribute columns", column.HeaderText != "OrgProxy'sPA1" && column.HeaderText != "Buyer'sPA1");
					}

					testLoggedInOrg.MiscServ.OM_IMPartAttrib1Name = "LoggedInOrg'sPA1";
					testLoggedInOrg.MiscServ.OM_IMPartAttrib1Type = "MAN";

					testDetailsPage.SetupOrderLinesGridForTest();

					AssertColumnIsInGrid("Line #", 0, typeof(ZCalcEditColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("Part #", 1, typeof(ZHyperLinkColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("Description", 2, typeof(ZTextEditColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("LoggedInOrg'sPA1", 3, typeof(ZTextEditColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("Inner Packs", 4, typeof(ZCalcEditColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("Inner Package Type", 5, typeof(ZDropEditColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("Outer Packs", 6, typeof(ZCalcEditColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("Outer Package Type", 7, typeof(ZDropEditColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("Qty Ordered", 8, typeof(ZCalcEditColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("Qty Invoiced", 9, typeof(ZCalcEditColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("Qty Received", 10, typeof(ZCalcEditColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("Qty Remaining", 11, typeof(ZCalcEditColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("Unit of Qty", 12, typeof(ZTextEditColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("Item Price", 13, typeof(ZCalcEditColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("Total Price", 14, typeof(ZCalcEditColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("Line Status", 15, typeof(ZDropDownListColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("TestBuyer'sCD4 - CA", 16, typeof(ZCalcEditColumn), testDetailsPage.OrderLinesGridForTest);
					AssertColumnIsInGrid("Required In Store Date", 17, typeof(ZDateTimeColumn), testDetailsPage.OrderLinesGridForTest);
				}
				finally
				{
					Globals.IsWeb = false;
				}
			}
		}

		public void TestOrderLinesGrid_WithSerialNumberEnabledAndClientUsesSerialNumber()
		{
			TestOrderLinesGrid_WithSerialNumberCore(clientUsesSerialNumber: true);
		}

		public void TestOrderLinesGrid_ClientDoesNotUseSerialNumber()
		{
			TestOrderLinesGrid_WithSerialNumberCore(clientUsesSerialNumber: false);
		}

		void TestOrderLinesGrid_WithSerialNumberCore(bool clientUsesSerialNumber)
		{
			using (var testDetailsPage = new TestOrderDetails())
			{
				Globals.IsWeb = true;
				try
				{
					var testLoggedInOrg = Factory.New<OrgHeader>();
					testLoggedInOrg.OH_Code = "XXXXX";
					testLoggedInOrg.MiscServ.OM_IMUseSerialNumber = clientUsesSerialNumber;
					Factory.Save();

					testDetailsPage.SiteUser.LoginSupportForTest(testLoggedInOrg.OH_Code);

					var testOrder = Factory.New<TrackingOrder>();
					testDetailsPage.SetCurrentOrderForTest(testOrder);

					testDetailsPage.SetupOrderLinesGridForTest();
					AssertEquals(clientUsesSerialNumber, testDetailsPage.OrderLinesGridForTest.Columns.OfType<ZTemplateColumn>().Any(column => column.HeaderText.Equals("Serial Number")));
				}
				finally
				{
					Globals.IsWeb = false;
				}
			}
		}

		#endregion

		#region TestProductLink

		public void TestProductLink()
		{
			TestOrderDetails testPage = TestPage as TestOrderDetails;

			TestHelper helper = new TestHelper(Factory);
			AssertNotNull("TestOrg", helper.TestOrg);
			AssertNotNull("TestUser", helper.TestSiteUser);

			TrackingOrder testOrder = helper.CreateOrder();
			OrderLine testLine = testOrder.OrderLines.AddNew();
			OrgSupplierPart testPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			testPart.OP_PartNum = "Test_Partnum";
			testLine.JO_Partno = testPart.OP_PartNum;
			testPart.OP_Desc = "Test Part";
			OrgPartRelation testRelation = testPart.RelatedOrganisations.AddNew();
			testRelation.OU_OH = helper.TestOrg.PK;
			testRelation.OU_OP = testPart.PK;
			Factory.Save();

			testPage.SetupPageForTesting();
			testPage.SetCurrentOrderForTest(testOrder);
			testPage.SetupOrderLinesGridForTest();

			testPage.OrderLinesGridForTest.BindTo = "OrderLines";
			testPage.OrderLinesGridForTest.Bind(testOrder);

			string uRL = ((ZHyperlink)testPage.OrderLinesGridForTest.Items[0].Cells[1].Controls[0]).NavigateUrl;
			string expectedPK = testPart.PK.ToString();

			Assert("Hyperlink for product should contain product's PK", uRL.Contains(expectedPK));
		}

		#endregion

		#region TestPlanningDetailsVisibility

		public void TestPlanningDetailsVisibility()
		{
			TestOrderDetails page = (TestOrderDetails)TestPage;
			TrackingOrderForTest order = Factory.New<TrackingOrderForTest>();
			page.SetCurrentOrderForTest(order);

			page.SetupPlanningDetailsForTest();
			Assert("Planned containers panel is visible if no shipment/declaration is attached", page.PlannedContainersPanelForTest.Visible);

			order.SetShipOrDec(Factory.New<TrackingShipment>());

			page.SetupPlanningDetailsForTest();

			Assert("Planned containers panel is not visible if shipment/declaration is attached", !page.PlannedContainersPanelForTest.Visible);
		}

		class TrackingOrderForTest : TrackingOrder
		{
			public TrackingOrderForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public void SetShipOrDec(TrackingShipment shipOrDec)
			{
				this.shipOrDec = shipOrDec;
			}
		}
		#endregion

		#region TestShipmentDetailsVisibility

		public void TestShipmentDetailsVisibility()
		{
			TestOrderDetails page = (TestOrderDetails)TestPage;
			TrackingOrder order = Factory.New<TrackingOrder>();
			page.SetCurrentOrderForTest(order);

			page.SetupShipmentDetailsForTest();
			Assert("Containers panel is not visible if no shipment is attached", !page.ContainersPanelForTest.Visible);
			Assert("Transports panel is not visible if no shipment is attached", !page.TransportPanelForTest.Visible);

			TrackingShipment shipment = Factory.New<TrackingShipment>();
			order.JD_JS = shipment.PK;
			order.JD_TransportMode = Constants.TransportModes.Sea;

			page.SetupShipmentDetailsForTest();

			Assert("Containers panel is visible if shipment is attached", page.ContainersPanelForTest.Visible);
			Assert("Transports panel is visible if shipment is attached", page.TransportPanelForTest.Visible);
		}

		#endregion

		#region TestSetupVoyagesGrid

		public void TestSetupVoyagesGrid()
		{
			TestOrderDetails page = (TestOrderDetails)TestPage;

			TrackingOrder order = Factory.New<TrackingOrder>();
			page.SetCurrentOrderForTest(order);

			page.SetupVoyagesGridForTest(new PlannedVoyagesCollection());
			Assert("Panel is not visible if no voyages are attached", !page.PlannedVoyagesPanelForTest.Visible);

			page.SetupVoyagesGridForTest(new PlannedVoyagesCollection { new PlannedVoyage() });
			Assert("Planned voyages panel should be visible", page.PlannedVoyagesPanelForTest.Visible);

			AssertColumnIsInGrid(ZString.Empty, 0, typeof(ZTextEditColumn), page.PlannedVoyagesGridForTest);
			AssertColumnIsInGrid("Vessel", 1, typeof(ZTextEditColumn), page.PlannedVoyagesGridForTest);
			AssertColumnIsInGrid("Voyage/Flight", 2, typeof(ZTextEditColumn), page.PlannedVoyagesGridForTest);
			AssertColumnIsInGrid("Estimated Departure", 3, typeof(ZDateTimeColumn), page.PlannedVoyagesGridForTest);
			AssertColumnIsInGrid("Estimated Arrival", 4, typeof(ZDateTimeColumn), page.PlannedVoyagesGridForTest);

			TrackingShipment shipment = Factory.New<TrackingShipment>();
			order.JD_JS = shipment.PK;
			page.SetupVoyagesGridForTest(new PlannedVoyagesCollection { new PlannedVoyage() });

			Assert("Planned voyages panel should not be visible if shipment is attached", !page.PlannedVoyagesPanelForTest.Visible);
		}

		#endregion

		#region TestOrderDetails

		public class TestOrderDetails : OrderDetails
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}

			public void SetupPageForTesting()
			{
				EditOrder = new Button();
				ViewShipmentDetailsButton = new Button();
				OrderCancelledDiv = new HtmlGenericControl();
				CancelOrder = new Button();
				DuplicateOrder = new Button();

				PlannedContainersGrid = new ZGrid();

				ContainersGrid = new ZGrid();
				TransportPanel = new ZCollapsablePanel();
				TransportGrid = new ZGrid();

				PendingLegendLabel = new ZTextLabel();
				OverdueLegendLabel = new ZTextLabel();
				CompletedLegendLabel = new ZTextLabel();
				CompletedLateLegendLabel = new ZTextLabel();

				PlannedVoyagesGrid = new ZGrid();
				DocumentsGrid = new ZGrid();

				DetailsPanel = new ZCollapsablePanel();
				PlannedPacksPanel = new ZCollapsablePanel();
				AdditionalDetailTable = new Table();
				AdditionalDetailPanel = new ZCollapsablePanel();

				if (ShouldCreateDataSource)
				{
					LoadOrCreateDataSource();
				}
				else
				{
					fCurrentOrderForTest = Factory.New<TrackingOrder>();
				}
			}

			public bool ShouldCreateDataSource;

			protected override BusinessObject GetNewDataSource()
			{
				return ShouldCreateDataSource ? Factory.New<TrackingOrder>() : null;
			}

			public void SetupAuthorisedContentForTest(bool isAuthorized)
			{
				SetupAuthorisedContent(isAuthorized);
			}

			public void RunOnUnLoadForTest()
			{
				this.OnUnload(new EventArgs());
			}

			public HtmlGenericControl OrderCancelledDivForTest => OrderCancelledDiv;

			public Button EditOrderButtonForTest
			{
				get { return EditOrder; }
			}

			public Button CancelOrderButtonForTest
			{
				get { return CancelOrder; }
			}

			public void CancelOrderButtonClick()
			{
				CancelOrder_Click(this, new EventArgs());
			}

			public void DuplicateOrderButtonClick()
			{
				DuplicateOrder_Click(this, new EventArgs());
			}

			public void ViewShipmentDetailsButtonClick()
			{
				ViewShipmentDetailsButton_Click(this, new EventArgs());
			}

			public void SetupTransportGridForTest()
			{
				TransportGrid = new ZGrid();
				SetupTransportGrid();
			}

			public ZDataGrid TransportGridForTest
			{
				get { return TransportGrid; }
			}

			public void SetupOrderLinesGridForTest()
			{
				this.OrderLinesGrid = new ZGrid();
				this.SetupOrderLinesGrid();
			}

			public ZGrid OrderLinesGridForTest
			{
				get
				{
					return OrderLinesGrid;
				}
				set
				{
					this.OrderLinesGrid = value;
				}
			}

			public void SetCurrentOrderForTest(TrackingOrder order)
			{
				fCurrentOrderForTest = order;
			}
			TrackingOrder fCurrentOrderForTest;

			protected override TrackingOrder CurrentOrder
			{
				get
				{
					if (fCurrentOrderForTest != null)
					{
						return fCurrentOrderForTest;
					}
					else
					{
						return base.CurrentOrder;
					}
				}
			}

			public void SetupPlanningDetailsForTest()
			{
				SetupPlanningDetails();
			}

			public Control PlannedContainersPanelForTest
			{
				get { return PlannedContainersGrid; }
			}

			public void SetupShipmentDetailsForTest()
			{
				SetupShipmentDetails();
			}

			public Control ContainersPanelForTest
			{
				get { return ContainersGrid; }
			}

			public ZCollapsablePanel TransportPanelForTest
			{
				get { return TransportPanel; }
			}

			public Control PlannedVoyagesPanelForTest
			{
				get { return PlannedVoyagesGrid; }
			}

			public ZDataGrid PlannedVoyagesGridForTest
			{
				get { return PlannedVoyagesGrid; }
			}

			public void SetupVoyagesGridForTest(PlannedVoyagesCollection voyages)
			{
				SetupVoyagesGrid(voyages);
			}
		}
		#endregion
	}
}
