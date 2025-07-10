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
	sealed class EditWarehouseReceiveForTest : EditWarehouseReceive
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public BusinessObject GetNewDataSourceForTest()
		{
			return GetNewDataSource();
		}

		public EditWarehouseReceiveForTest()
		{
			IsCreateNewAppInstanceIfNullForTest = true;
			SiteUser.LoginSupportForTest("EDICUS");

			UnauthorisedDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			AuthorisedContent = new System.Web.UI.HtmlControls.HtmlGenericControl();
			WhsReceiveLabel = new ZTextLabel();
			SaveReceive = new Button();
			CancelReceive = new Button();
			NotFoundError = new System.Web.UI.HtmlControls.HtmlGenericControl();
			ReceiveContents = new System.Web.UI.HtmlControls.HtmlGenericControl();
			WarehouseDropDown = new ZGuidDropDownList();

			WhsReceiveInventoryGrid = new ZDataGrid();
			WhsReceiveInventoryGrid.BindTo = "Lines";
			WhsReceiveInventoryGrid.AllowEdit = true;
			WhsReceiveInventoryGrid.AllowDelete = true;
			Controls.Add(WhsReceiveInventoryGrid);
			SetupReceiveInventoryGrid();
		}

		public new void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
		}

		public ZDataGrid WhsReceiveInventoryGridForTest
		{
			get { return WhsReceiveInventoryGrid; }
		}

		protected override void WhsReceiveInventoryGrid_ItemDataBound(object sender, DataGridItemEventArgs e)
		{
			ItemDataBoundWasCalled = true;
			base.WhsReceiveInventoryGrid_ItemDataBound(sender, e);
		}

		protected override BusinessObject GetNewDataSource()
		{
			return ReceiveForTest;
		}

		public TrackingWhsReceive ReceiveForTest
		{
			get
			{
				if (fReceiveForTest == null)
				{
					fReceiveForTest = TrackingHelper.Get(Factory.New<WhsReceive>());
				}
				return fReceiveForTest;
			}
		}
		TrackingWhsReceive fReceiveForTest;

		public bool ItemDataBoundWasCalled;

		public int GetColumnIndexForTest(ZString columnHeader)
		{
			int columnIndex = -1;

			for (int i = 0; i < WhsReceiveInventoryGrid.Columns.Count; i++)
			{
				if (WhsReceiveInventoryGrid.Columns[i].HeaderText == columnHeader)
				{
					columnIndex = i;
					break;
				}
			}

			return columnIndex;
		}
	}
}
