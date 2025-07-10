using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class WarehouseOrderDetailsForTest : WarehouseOrderDetails
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public WarehouseOrderDetailsForTest()
		{
			IsCreateNewAppInstanceIfNullForTest = true;

			TrackingSiteUser user = new TrackingSiteUser();
			OrgHeader header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var testContact = header.Contacts.Count > 0 ? header.Contacts[0] : header.Contacts.AddNew();
			if (testContact.OC_Email.IsEmpty)
			{
				testContact.OC_Email = "blah@cargowise.com";
			}

			if (!testContact.HasPassword)
			{
				testContact.SetHashedPassword("test");
			}

			testContact.OC_WebAccessEnabled = true;
			Factory.Save();

			SiteUser.Login(header.OH_Code, testContact.OC_Email, testContact.PasswordForTesting);

			UnauthorisedDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			AuthorisedContent = new System.Web.UI.HtmlControls.HtmlGenericControl();
			WhsOrderLabel = new ZTextLabel();
			EditOrder = new Button();
			CancelOrder = new Button();
			order = TrackingHelper.Get(Factory.New<WhsOrder>());
			NotFoundError = new System.Web.UI.HtmlControls.HtmlGenericControl();
			OrderContents = new System.Web.UI.HtmlControls.HtmlGenericControl();

			WhsOrderLinesGrid = new ZGrid();
			WhsOrderLinesGrid.BindTo = "Lines";
			WhsOrderLinesGrid.AllowEdit = true;
			WhsOrderLinesGrid.AllowDelete = true;
			Controls.Add(WhsOrderLinesGrid);

			WhsOrderSummaryLinesGrid = new ZGrid();
			WhsOrderSummaryLinesGrid.BindTo = "SummaryLines";
			Controls.Add(WhsOrderSummaryLinesGrid);

			NotFoundLabel = new ZTextLabel();
			EditButtonDiv = new System.Web.UI.HtmlControls.HtmlGenericControl();
			CanEditCancelLabel = new ZTextLabel();
			DocumentsGrid = new ZGrid();
			ShowLinesDetail = new HyperLink();
			ShowLinesSummary = new HyperLink();
			WhsOrderLineDetailsContainer = new System.Web.UI.HtmlControls.HtmlGenericControl();
			WhsOrderLineSummaryContainer = new System.Web.UI.HtmlControls.HtmlGenericControl();
			TransportRef = new ZHyperlink() { ID = "TransportRef" };
			Controls.Add(TransportRef);

			SetupOrderLinesGrid();
		}

		public void SetupPageForTesting()
		{
			EditOrder = new Button();
			CancelOrder = new Button();
			order = TrackingHelper.Get(Factory.New<WhsOrder>());
		}
		TrackingWhsOrder order;

		protected override TrackingWhsOrder Order => order;

		public Button EditOrderForTesting => EditOrder;

		public Button CancelOrderForTesting => CancelOrder;

		public ZDataGrid WhsOrderLinesGridForTesting => WhsOrderLinesGrid;

		public ZDataGrid WhsOrderSummaryLinesGridForTesting => WhsOrderSummaryLinesGrid;

		public TrackingWhsOrder TestOrder
		{
			set => order = value;
			get => order;
		}

		public void OnDuplicateOrderClickForTest() => DuplicateOrder_Click(null, EventArgs.Empty);

		public void OnEditOrderClickForTest() => EditOrder_Click(null, EventArgs.Empty);

		public void OnCancelOrderClickForTest() => CancelOrder_Click(null, EventArgs.Empty);
	}
}
