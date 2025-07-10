using System;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class WhsOrderLineAllocationZPageTest : ZPageTestCase
	{
		#region TestAddCrossDock

		public void TestAddCrossDock()
		{
			var data = new TestDataSimpleEnvironment(TestPage.Factory);
			TestPage.SetupPageForTesting();
			TestPage.OrderForTest.WhsOrder.WD_OH_Client = data.Org1.PK;
			TestPage.OrderForTest.WhsOrder.WD_WW_Whs = data.Whs1.PK;
			TestPage.OrderLineForTest.WhsOrderLine.WE_OP = data.Part1.PK;
			TestPage.OrderLineForTest.WhsOrderLine.WE_TransactionQuantity = 10m;

			var receive = TestPage.Factory.New<WhsReceive>();
			receive.WD_OH_Client = data.Org1.PK;
			receive.WD_WW_Whs = data.Whs1.PK;

			var testInventory = receive.Lines.AddNew().Inventory[0];
			testInventory.WI_OP = data.Part1.PK;
			testInventory.WI_InDocketLineUnits = 10m;
			testInventory.WI_TotalUnits = 10m;
			Factory.Save();

			AssertEquals(0, TestPage.OrderLineForTest.WhsOrderLine.ReservedPickLines.Count);
			TestPage.AddCrossDock(testInventory.PK);
			AssertEquals(1, TestPage.OrderLineForTest.WhsOrderLine.ReservedPickLines.Count);
		}

		#endregion

		#region TestOrderLinesGridSetup

		public void TestCrossDockGridSetup()
		{
			AssertColumnIsInGrid("Receipt Ref", 0, typeof(ZTextEditColumn));
			AssertColumnIsInGrid("ETA/Arrival", 1, typeof(ZDateTimeColumn));
			AssertColumnIsInGrid("Status", 2, typeof(ZTextEditColumn));
			AssertColumnIsInGrid("Available Pick Qty", 3, typeof(ZCalcEditColumn));
		}

		void AssertColumnIsInGrid(ZString headerText, int columnIndex, Type columnType)
		{
			if (columnIndex < TestPage.CrossDocksGridForTesting.Columns.Count)
			{
				bool columnIsInGrid = (TestPage.CrossDocksGridForTesting.Columns[columnIndex].HeaderText.Equals(headerText));
				Assert(ZString.Format("Column '{0}' is not in the grid at index {1} as expected.", headerText, columnIndex), columnIsInGrid);
				AssertEquals(ZString.Format("Column '{0}' type", headerText), columnType, TestPage.CrossDocksGridForTesting.Columns[columnIndex].GetType());
			}
			else
			{
				Fail(ZString.Format("The expected index {0} of column '{1}' is out of range.", columnIndex, headerText));
			}
		}

		#endregion

		public void TestAddOrderLineDetailsNullRef()
		{
			TestPage.AddOrderLineDetails_Exposed(null, null);

			var e = new ZTextIFramePopup.GetAdditionalParametersEventArgs(null);
			TestPage.AddOrderLineDetails_Exposed(new object(), e);

			e = new ZTextIFramePopup.GetAdditionalParametersEventArgs(new System.Collections.Specialized.NameValueCollection());
			TestPage.AddOrderLineDetails_Exposed(new object(), e);

			var orderLine = TestPage.OrderLineForTest;
			TestPage.OrderLineForTest = null;
			TestPage.AddOrderLineDetails_Exposed(new object(), e);

			TestPage.OrderLineForTest = orderLine;

			AssertEquals(true, true);  //[ExpectNoExceptions]
		}

		public void TestSetupCustomAttributes_WithSerialNumberEnabledAndClientUsesSerialNumber()
		{
			TestSetupCustomAttributes_WithSerialNumberCore(clientUsesSerialNumber: true);
		}

		public void TestSetupCustomAttributes_ClientDoesNotUseSerialNumber()
		{
			TestSetupCustomAttributes_WithSerialNumberCore(clientUsesSerialNumber: false);
		}

		void TestSetupCustomAttributes_WithSerialNumberCore(bool clientUsesSerialNumber)
		{
			var testLoggedInOrg = Factory.New<OrgHeader>();
			testLoggedInOrg.OH_Code = "ABC";
			testLoggedInOrg.MiscServ.OM_IMUseSerialNumber = clientUsesSerialNumber;
			Factory.Save();

			var whsOrderLineAllocationPage = new WhsOrderLineAllocationForTest(testLoggedInOrg.OH_Code);
			whsOrderLineAllocationPage.SetupCustomAttributes();
			AssertEquals(clientUsesSerialNumber, whsOrderLineAllocationPage.IsSerialNumberEnabledForTest);
			if (clientUsesSerialNumber)
			{
				AssertEquals("Serial Number", whsOrderLineAllocationPage.SerialNumberCaptionForTest);
			}
		}

		public void TestSave_ClickNullRef()
		{
			AssertNoExceptionThrown(() => TestPage.Save_Click(new object(), new EventArgs()));
		}

		#region Implementation

		WhsOrderLineAllocationForTest TestPage => Page as WhsOrderLineAllocationForTest;

		protected override ZPage GetNewZPage()
		{
			return new WhsOrderLineAllocationForTest("EDICUS");
		}

		#endregion

		#region WhsOrderLineAllocationForTest

		public class WhsOrderLineAllocationForTest : WhsOrderLineAllocation
		{
			protected override ZGlobal GetNewTestGlobal()
			{
				return new TestGlobal();
			}

			public WhsOrderLineAllocationForTest(string orgCode)
			{
				IsCreateNewAppInstanceIfNullForTest = true;
				SiteUser.LoginSupportForTest(orgCode);

				UnauthorisedDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();
				UnauthorisedLabel = new ZTextLabel();
				AuthorisedContent = new System.Web.UI.HtmlControls.HtmlGenericControl();
				NotFoundError = new System.Web.UI.HtmlControls.HtmlGenericControl();
				OrderLineContents = new System.Web.UI.HtmlControls.HtmlGenericControl();
				CustomAttr1 = new ZTextLabel();
				CustomAttr2 = new ZTextLabel();
				CustomAttr3 = new ZTextLabel();
				SerialNumber = new ZTextLabel();
				SerialNumberLabel = new Label();
				InventorySelector = new ZGuidFindBox();
				CustomAttr12Row = new System.Web.UI.HtmlControls.HtmlTableRow();
				CustomAttr3Row = new System.Web.UI.HtmlControls.HtmlTableRow();

				CrossDocksGrid = new ZDataGrid();
				CrossDocksGrid.BindTo = "WhsOrderLine.ReservedPickLines";
				Controls.Add(CrossDocksGrid);

				SetupCrossDockGrid();
			}

			public bool IsSerialNumberEnabledForTest => SerialNumber.Visible && SerialNumberLabel.Visible && CustomAttr3Row.Visible;

			public string SerialNumberCaptionForTest => SerialNumberLabel.Text;

			public TrackingWhsOrder OrderForTest => orderForTest ?? (orderForTest = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>()));
			TrackingWhsOrder orderForTest;

			public void SetupPageForTesting()
			{
				fOrderLine = TrackingHelper.Get(Factory.New<WhsOrderLine>());
				fOrderLine.WhsOrderLine.WE_WD = OrderForTest.PK;
			}
			TrackingWhsOrderLine fOrderLine;

			public TrackingWhsOrderLine OrderLineForTest
			{
				get => OrderLine;
				set => fOrderLine = value;
			}

			protected override TrackingWhsOrderLine OrderLine => fOrderLine;

			public ZDataGrid CrossDocksGridForTesting => CrossDocksGrid;

			public new void AddCrossDock(ZGuid inventoryPK)
			{
				base.AddCrossDock(inventoryPK);
			}

			public void AddOrderLineDetails_Exposed(object sender, ZTextIFramePopup.GetAdditionalParametersEventArgs e)
			{
				AddOrderLineDetails(sender, e);
			}
		}
		#endregion
	}
}
