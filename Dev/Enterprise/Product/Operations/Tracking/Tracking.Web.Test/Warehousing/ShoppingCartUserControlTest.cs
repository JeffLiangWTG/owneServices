using System;
using System.Linq;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class ShoppingCartUserControlTest : TestCaseWithFactory
	{
		#region TestNewButtonClickEvent

		public void TestNewButtonClickEvent()
		{
			using (var control = new ShoppingCartUserControlForTest())
			{
				newButtonClickedCounter = 0;
				control.InitializeComponentExposed();
				control.NewButtonClicked += new NewButtonClickedHandler(control_NewButtonClicked);
				control.NewButton_ClickExposed();
				AssertEquals(1, newButtonClickedCounter);
			}
		}

		void control_NewButtonClicked()
		{
			newButtonClickedCounter++;
		}

		int newButtonClickedCounter;

		#endregion

		#region TestClearButtonClickEvent

		public void TestClearButtonClickEvent()
		{
			using (var control = new ShoppingCartUserControlForTest())
			{
				clearButtonClickedCounter = 0;
				control.InitializeComponentExposed();
				control.ClearButtonClicked += new ClearButtonClickedHanlder(control_ClearButtonClicked);
				control.ClearButton_ClickExposed();
				AssertEquals(1, clearButtonClickedCounter);
			}
		}

		void control_ClearButtonClicked()
		{
			clearButtonClickedCounter++;
		}

		int clearButtonClickedCounter;

		#endregion

		#region TestCssStyles

		public void TestCssStyles()
		{
			using (var control = new ShoppingCartUserControlForTest())
			{
				AssertEquals(CssConstants.DetailsTable, control.TableCssExposed);
				AssertEquals(CssConstants.DetailsCell, control.ItemCssExposed);
				AssertEquals(CssConstants.DetailsAlternatingCell, control.AlternatingCssExposed);
				AssertEquals(CssConstants.DetailsSelectedCell, control.SelectedCssExposed);
				AssertEquals(CssConstants.ResultsTablePager, control.PagerCssExposed);
				AssertEquals(CssConstants.DetailsHeader, control.HeaderCssExposed);
			}
		}

		#endregion

		#region TestGridDatesColumns

		public void TestGridDatesColumns()
		{
			var userContact = Factory.NewWithValidTestData<OrgContact>();
			userContact.OC_Email = "test@test.com";
			userContact.OC_WebAccessEnabled = true;
			userContact.SetHashedPassword("test");

			var contactOrg = userContact.ParentOrg;
			contactOrg.MiscServ.OM_IMUseExpiryDate = true;
			contactOrg.MiscServ.OM_IMUsePackingDate = true;

			Factory.Save();

			using (var control = new ShoppingCartUserControlForTest())
			{
				var siteUser = ((ZPage)control.Page).SiteUser;
				siteUser.Login(userContact.OrganisationCode, "test@test.com", "test");
				Assert(siteUser.IsLoggedIn);

				control.SetupGridColumnsExposed();
				var dateColumns = control.OrderLinesGrid.Columns.OfType<ZDateTimeColumn>();
				AssertEquals(2, dateColumns.Count());

				var packingDateColumn = dateColumns.FirstOrDefault(d => d.BindTo == TrackingWhsOrderLine.WrapperSchema.WE_PackingDate);
				var expiryDateColumn = dateColumns.FirstOrDefault(d => d.BindTo == TrackingWhsOrderLine.WrapperSchema.WE_ExpiryDate);
				AssertNotNull(packingDateColumn);
				AssertNotNull(expiryDateColumn);

				AssertEquals("Packing Date", packingDateColumn.HeaderText);
				AssertEquals("Expiry Date", expiryDateColumn.HeaderText);
			}
		}

		public void TestGridSerialNumberColumn_SerialNumberEnabledAndClientUsesSerialNumber()
		{
			var userContact = Factory.NewWithValidTestData<OrgContact>();
			userContact.OC_Email = "test@test.com";
			userContact.OC_WebAccessEnabled = true;
			userContact.SetHashedPassword("test");

			var contactOrg = userContact.ParentOrg;
			contactOrg.MiscServ.OM_IMUseSerialNumber = true;
			Factory.Save();

			using (var control = new ShoppingCartUserControlForTest())
			{
				var siteUser = ((ZPage)control.Page).SiteUser;
				siteUser.Login(userContact.OrganisationCode, "test@test.com", "test");
				Assert(siteUser.IsLoggedIn);

				control.SetupGridColumnsExposed();
				AssertEquals(true, control.OrderLinesGrid.Columns.OfType<ZTemplateColumn>().Any(column => column.HeaderText == "Serial Number"));
			}
		}

		#endregion

		#region TestGridBindToDecimals

		public void TestGridBindToDecimals()
		{
			TrackingWhsOrder order = TrackingHelper.Get(Factory.NewWithValidTestData<WhsOrder>());
			TrackingWhsOrderLineCollection orderLines = new TrackingWhsOrderLineCollection(order);
			OrgSupplierPart supplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			supplierPart.OP_PartNum = "super mega part";
			supplierPart.OP_CountDecimalPlaces = 5;
			TrackingWhsOrderLine orderLine = orderLines.AddNew();
			orderLine.WhsOrderLine.WE_OP = supplierPart.PK;
			var control = new ShoppingCartUserControlForTest();
			control.SetupGridColumnsExposed();
			control.OrderLinesGrid.Bind(orderLines);

			AssertEquals(1, control.OrderLinesGrid.Items.Count);
			DataGridItem item = control.OrderLinesGrid.Items[0];
			AssertNotNull(item);
			ZNumericTextBox numericTextBox = item.Cells[2].Controls[0] as ZNumericTextBox;
			AssertNotNull(numericTextBox);
			AssertEquals(5, numericTextBox.Decimals);
		}

		#endregion

		#region InventoryShoppingCartUserControlForTest

		class ShoppingCartUserControlForTest : ShoppingCartUserControl
		{
			public ShoppingCartUserControlForTest()
			{
				this.Page = new TrackingTestPage();
				this.NewButton = new Button();
				this.ClearButton = new Button();
				this.OrderLinesGrid = new ZDataGrid();
			}

			public void InitializeComponentExposed()
			{
				InitializeComponent();
			}

			public void NewButton_ClickExposed()
			{
				NewButton_Click(this, EventArgs.Empty);
			}

			public void ClearButton_ClickExposed()
			{
				ClearButton_Click(this, EventArgs.Empty);
			}

			public string TableCssExposed
			{
				get
				{
					return TableCss;
				}
			}

			public string ItemCssExposed
			{
				get
				{
					return ItemCss;
				}
			}

			public string AlternatingCssExposed
			{
				get
				{
					return AlternatingCss;
				}
			}

			public string SelectedCssExposed
			{
				get
				{
					return SelectedCss;
				}
			}

			public string PagerCssExposed
			{
				get
				{
					return PagerCss;
				}
			}

			public string HeaderCssExposed
			{
				get
				{
					return HeaderCss;
				}
			}

			public void SetupGridColumnsExposed()
			{
				SetupGridColumns();
			}
		}

		#endregion
	}
}
