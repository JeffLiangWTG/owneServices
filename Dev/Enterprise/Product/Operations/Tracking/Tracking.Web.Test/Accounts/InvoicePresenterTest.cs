using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class InvoicePresenterTest : TestCaseWithFactory
	{
		#region TestGetColumns

		public void TestGetColumns()
		{
			ZDataGrid chargesGrid = new ZDataGrid();

			InvoicePresenter presenter = new InvoicePresenter(null);
			ZGrid testGrid = new ZGrid();
			presenter.SetupGrid(testGrid);
			DataGridColumn[] columns = testGrid.ColumnProvider.AllColumns.ToArray();

			AssertColumns(columns);
			ZLinkButtonColumn invoiceLinkColumn = columns[0] as ZLinkButtonColumn;
			AssertNotNull("Should be InvoiceLinkColumn", invoiceLinkColumn);
			Assert("Should be no ClientClickHandler", string.IsNullOrEmpty(invoiceLinkColumn.ClientClickHandler));
		}

		public void TestGetSearchResultColumns()
		{
			ZDataGrid chargesGrid = new ZDataGrid();

			InvoicePresenter presenter = new InvoicePresenter(null);
			DataGridColumn[] columns = presenter.GetSearchResultColumns();

			AssertSearchResultColumns(columns);
			ZLinkButtonColumn invoiceLinkColumn = columns[0] as ZLinkButtonColumn;
			AssertNotNull("Should be InvoiceLinkColumn", invoiceLinkColumn);
			Assert("Should be no ClientClickHandler", string.IsNullOrEmpty(invoiceLinkColumn.ClientClickHandler));
		}

		public void TestGetColumnsForQuickView()
		{
			ZDataGrid chargesGrid = new ZDataGrid();

			TrackingSiteUser user = new TrackingSiteUser();
			user.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);

			InvoicePresenter presenter = new InvoicePresenter(user);
			ZGrid testGrid = new ZGrid();
			presenter.SetupGrid(testGrid);
			DataGridColumn[] columns = testGrid.ColumnProvider.AllColumns.ToArray();

			AssertColumns(columns);
			ZLinkButtonColumn invoiceLinkColumn = columns[0] as ZLinkButtonColumn;
			AssertNotNull("Should be InvoiceLinkColumn", invoiceLinkColumn);
			AssertEquals("ClientClickHandler", BasePage.ShipmentQuickViewUserLoginRequest, invoiceLinkColumn.ClientClickHandler);
		}

		#endregion

		#region TestSetupLocalChargesGrid

		public void TestSetupLocalChargesGrid()
		{
			ZGrid localChargesGrid = new ZGrid();

			InvoicePresenter presenter = new InvoicePresenter(null);
			presenter.SetupLocalChargesGrid(localChargesGrid);

			AssertEquals("Grid should contain 5 columns", 5, localChargesGrid.Columns.Count);

			AssertEquals("First column should be Description", "Description", localChargesGrid.Columns[0].HeaderText);
			AssertEquals("Second column should be Cur", "Cur", localChargesGrid.Columns[1].HeaderText);
			AssertEquals("Third column should be Ex. Tax", "Ex. Tax", localChargesGrid.Columns[2].HeaderText);
			AssertEquals("Fourth column should be Tax", "Tax", localChargesGrid.Columns[3].HeaderText);
			AssertEquals("Fifth column should be Total Amount", "Total Amount", localChargesGrid.Columns[4].HeaderText);
		}

		#endregion

		[HttpContextEnabledTest]
		public void TestSearchResultsDataGrid_ItemCommand_Async()
		{
			AssertDataGridItemCommand(false, true);
		}

		[HttpContextEnabledTest]
		public void TestSearchResultsDataGrid_ItemCommand_QuickUser()
		{
			AssertDataGridItemCommand(true);
		}

		[HttpContextEnabledTest]
		public void TestSearchResultsDataGrid_ItemCommand()
		{
			AssertDataGridItemCommand(false);
		}

		void AssertDataGridItemCommand(bool isQuickUser, bool isAsyncPostback = false)
		{
			var user = new TrackingSiteUser();
			if (isQuickUser)
			{
				user.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			}
			else
			{
				var testLoggedInOrg = Factory.New<OrgHeader>();
				testLoggedInOrg.OH_Code = "XXXXX";
				user.LoginSupportForTest(testLoggedInOrg.OH_Code);
			}

			var page = new ZAjaxPageForTest();
			var localChargesGrid = new ZTestGrid
			{
				Page = page
			};
			page.IsAsyncPostBack = isAsyncPostback;

			var invoicePK = ZGuid.NewZGuid();
			localChargesGrid.ViewState_Exposed.Add("DataKeys", new ArrayList { invoicePK });

			var presenter = new InvoicePresenter(user);
			var row = new DataGridItem(0, 0, ListItemType.Item);
			var args = new DataGridCommandEventArgs(row, localChargesGrid, new CommandEventArgs("ViewInvoice", null));
			presenter.SearchResultsDataGrid_ItemCommand(localChargesGrid, args);

			var expectedUrl = InvoiceRequestHandler.RequestHelper.GetHandlerUrl(invoicePK);

			var isUpdatePanelRedirected = page.TryGetUpdatePanelRedirectUrl("ViewInvoice", out var updatePanelRedirectUrl);
			var isRequestRedirected = HttpContext.Current.Response.IsRequestBeingRedirected;
			var requestRedirectUrl = HttpContext.Current.Response.RedirectLocation;

			if (!isQuickUser)
			{
				if (isAsyncPostback)
				{
					Assert(isUpdatePanelRedirected);
					AssertEquals(expectedUrl, updatePanelRedirectUrl);
				}
				else
				{
					Assert(isRequestRedirected);
					AssertEquals(string.Format("/webapp/{0}", expectedUrl), requestRedirectUrl);
				}
			}
			else
			{
				Assert(!isRequestRedirected && !isUpdatePanelRedirected);
			}
		}

		#region Implementation

		void AssertColumns(DataGridColumn[] columns)
		{
			AssertNotNull("Should be not null", columns);
			AssertEquals("Should contain 10 columns", 10, columns.Length);

			AssertColumnsBase(columns);
		}

		void AssertSearchResultColumns(DataGridColumn[] columns)
		{
			AssertNotNull("Should be not null", columns);
			AssertEquals("Should contain 13 columns", 13, columns.Length);

			AssertColumnsBase(columns);
			AssertEquals("Eleventh column should be Consignor", "Consignor", columns[10].HeaderText);
			AssertEquals("Twelfth column should be Consignee", "Consignee", columns[11].HeaderText);
			AssertEquals("Thertinth column should be Last Requested", "Last Requested", columns[12].HeaderText);
		}

		void AssertColumnsBase(DataGridColumn[] columns)
		{
			AssertEquals("First column should be Invoice #", "Invoice #", columns[0].HeaderText);
			AssertEquals("Second column should be Issuer", "Issuer", columns[1].HeaderText);
			AssertEquals("Third column should be Type", "Type", columns[2].HeaderText);
			AssertEquals("Fourth column should be Terms", "Terms", columns[3].HeaderText);
			AssertEquals("Fifth column should be Inv. Date", "Inv. Date", columns[4].HeaderText);
			AssertEquals("Sixth column should be Due Date", "Due Date", columns[5].HeaderText);
			AssertEquals("Seventh column should be Currency", "Currency", columns[6].HeaderText);
			AssertEquals("Eighth column should be Amount", "Amount", columns[7].HeaderText);
			AssertEquals("Nineth column should be Outstanding Amt.", "Outstanding Amt.", columns[8].HeaderText);
			AssertEquals("Tenth column should be Paid Date", "Paid Date", columns[9].HeaderText);
		}

		protected override void SetUp()
		{
			base.SetUp();
		}

		#endregion

		class ZTestGrid : ZGrid
		{
			public StateBag ViewState_Exposed
			{
				get { return base.ViewState; }
			}
		}

		class ZAjaxPageForTest : ZTestPage, IZAjaxPage
		{
			public ZAjaxPageForTest()
			{
				updatePanelRedirects = new Dictionary<string, string>();
			}

			readonly Dictionary<string, string> updatePanelRedirects;

			public AJAXManager AJAX => ajax ?? (ajax = new AJAXManager());
			AJAXManager ajax;

			public bool IsAsyncPostBack { get; set; }

			public string AsyncPostBackSourceElementID => string.Empty;

			public void UpdatePanelRedirect(string key, string redirectUrl)
			{
				updatePanelRedirects.Add(key, redirectUrl);
			}

			public bool TryGetUpdatePanelRedirectUrl(string key, out string redirectUrl)
			{
				return updatePanelRedirects.TryGetValue(key, out redirectUrl);
			}
		}
	}
}
