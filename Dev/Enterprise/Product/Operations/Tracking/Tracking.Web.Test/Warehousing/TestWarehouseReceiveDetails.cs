using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TestWarehouseReceiveDetails : WarehouseReceiveDetails
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		protected override Uri RequestUrl => new Uri("http://www.test.com/cwWeb/test.aspx?data=xyz");

		public void SetupPageForTesting()
		{
			CancelReceive = new Button();
			EditReceive = new Button();
			WhsReceiveInventoryGrid = new ZGrid();
			WhsReceiveContainersGrid = new ZGrid();
			AdditionalDetailPanel = new ZCollapsablePanel();
			ReceiveContents = new HtmlGenericControl();
			NotFoundError = new HtmlGenericControl();
			NotFoundLabel = new ZTextLabel();
			EditButtonDiv = new HtmlGenericControl();
			CanEditCancelLabel = new ZTextLabel();
			DocumentsGrid = new ZGrid();
			LoadOrCreateDataSource();
		}

		protected override BusinessObject GetNewDataSource() => TrackingHelper.Get(Factory.New<WhsReceive>());

		public void OnLoadForTest() => OnLoad(EventArgs.Empty);

		public Button CancelReceiveForTest => CancelReceive;

		public Button EditReceiveForTest => EditReceive;

		public void OnDuplicateReceiveClickForTest() => DuplicateReceive_Click(null, EventArgs.Empty);
	}
}
