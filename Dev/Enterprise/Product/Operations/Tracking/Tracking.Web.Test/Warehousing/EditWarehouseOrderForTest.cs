using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class EditWarehouseOrderForTest : EditWarehouseOrder
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public EditWarehouseOrderForTest()
		{
			IsCreateNewAppInstanceIfNullForTest = true;
			SiteUser.LoginSupportForTest("EDICUS");

			UnauthorisedDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			AuthorisedContent = new System.Web.UI.HtmlControls.HtmlGenericControl();
			WhsOrderLabel = new ZTextLabel();
			SaveOrder = new Button();
			CancelOrder = new Button();
			NotFoundError = new System.Web.UI.HtmlControls.HtmlGenericControl();
			OrderContents = new System.Web.UI.HtmlControls.HtmlGenericControl();
			WarehouseDropDown = new ZGuidDropDownList();

			WhsOrderLinesGrid = new ZDataGrid();
			WhsOrderLinesGrid.BindTo = "Lines";
			WhsOrderLinesGrid.AllowEdit = true;
			WhsOrderLinesGrid.AllowDelete = true;
			Controls.Add(WhsOrderLinesGrid);
			SetupOrderLinesGrid();

			WhsOrderReferencesGrid = new ZDataGrid();
			WhsOrderReferencesGrid.BindTo = "WhsOrder.References";
			WhsOrderLinesGrid.AllowEdit = true;
			WhsOrderLinesGrid.AllowDelete = true;
			SetupReferenceGrid();
			SaveErrorMessage = new ZTextLabel();
		}

		public void SetupOrderLinesGridForTesting() => SetupOrderLinesGrid();

		public new void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		public void OnPreBindForTesting()
		{
			OnPreBind();
		}

		public BusinessObject GetBusinessObjectToValidate()
		{
			return BusinessObjectToValidate;
		}

		public WarehouseOrderLineGridAddOn LinesGridAddOnForTest => (WarehouseOrderLineGridAddOn)LinesGridAddOn;

		public ZDataGrid WhsOrderLinesGridForTest
		{
			get { return WhsOrderLinesGrid; }
		}

		public ZHyperLinkColumn CrossDocksLinkColumnForTest => CrossDocksLinkColumn;

		protected override void WhsOrderLinesGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			ItemDataBoundWasCalled = true;
			base.WhsOrderLinesGrid_ItemDataBound(sender, e);
		}

		public void FireSave_Click()
		{
			SaveOrder_Click(this, null);
		}

		public Button SaveButtonForTest
		{
			get { return SaveOrder; }
		}

		public string RowErrorMessageWantToAdd { set; get; }

		protected override bool DataSourceFactorySave()
		{
			if (!string.IsNullOrEmpty(RowErrorMessageWantToAdd))
			{
				OrderForTest.AddRowError(RowErrorMessageWantToAdd);
				return false;
			}
			else
			{
				return base.DataSourceFactorySave();
			}
		}

		protected override BusinessObject GetNewDataSource()
		{
			DataSourceIndexer = OrderForTest.PK;

			return OrderForTest;
		}

		public TrackingWhsOrder OrderForTest
		{
			get
			{
				if (fOrderForTest == null)
				{
					fOrderForTest = TrackingHelper.Get(Factory.New<WhsOrder>());
				}
				return fOrderForTest;
			}
		}
		TrackingWhsOrder fOrderForTest;

		public bool ItemDataBoundWasCalled;

		public int GetColumnIndexForTest(ZString columnHeader)
		{
			int columnIndex = -1;

			for (int i = 0; i < WhsOrderLinesGrid.Columns.Count; i++)
			{
				if (WhsOrderLinesGrid.Columns[i].HeaderText == columnHeader)
				{
					columnIndex = i;
					break;
				}
			}

			return columnIndex;
		}
	}
}
