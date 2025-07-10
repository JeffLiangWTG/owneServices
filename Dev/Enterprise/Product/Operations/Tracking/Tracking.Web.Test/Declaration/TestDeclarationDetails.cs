using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using CargoWise.EntityFramework;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Testing
{
	sealed class TestDeclarationDetails : DeclarationDetails
	{
		protected override ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}

		public void SetUpPageForTest()
		{
			UnauthorisedDiv = new HtmlGenericControl();
			AuthorisedContent = new HtmlGenericControl();
			UnauthorisedLabel = new ZTextLabel();
			NotFoundError = new HtmlGenericControl();
			StatusHolder = new HtmlGenericControl() { Visible = false };
			CustomsEntriesDataGrid = new ZGrid();
			AreaTransportControl = new HtmlGenericControl();
			TransportsGrid = new ZGrid();
			ChargesGrid = new ZGrid();
			InvoicesGrid = new ZGrid();
			HouseBillsGrid = new ZGrid();
			ReleaseStatusDescText = new ZTextLabel() { Visible = false };
			EstDeliveryLabel = new System.Web.UI.WebControls.Label();
			DeliveryRequiredLabel = new System.Web.UI.WebControls.Label();
			CartageAdvisedLabel = new System.Web.UI.WebControls.Label();
			GoodsDeliveredLabel = new System.Web.UI.WebControls.Label();
			ForAuthentifiedUserOnly = new HtmlGenericControl();
			ForAuthentifiedUserOnly2 = new HtmlTableRow();
			LocalChargesGrid = new ZGrid();
			OrdersGrid = new ZGrid();
			ContainerGrid = new ZGrid();
			DocumentsGrid = new ZGrid();
			DeclarationNotFoundLabel = new ZTextLabel();
		}

		public HtmlGenericControl ForTest_AreaTransportControl
		{
			get { return this.AreaTransportControl; }
		}

		public Control ForTest_AreaTransportGrid
		{
			get { return this.TransportsGrid; }
		}

		public ZGrid ForTest_ConsolPanel
		{
			get { return this.CustomsEntriesDataGrid; }
		}

		public HtmlGenericControl ForTest_StatusHolder
		{
			get { return this.StatusHolder; }
		}

		public ZTextLabel ForTest_ReleaseStatusDescText
		{
			get { return this.ReleaseStatusDescText; }
		}

		public ZGrid TransportGridForTest
		{
			get { return TransportsGrid; }
		}

		public ZGrid OrdersGridForTest
		{
			get { return OrdersGrid; }
		}

		public ZGrid CustomsEntriesDataGridForTest
		{
			get { return CustomsEntriesDataGrid; }
		}

		public ZGrid ContainerGridForTest
		{
			get { return ContainerGrid; }
		}

		public ZGrid InvoicesGriddForTest
		{
			get { return InvoicesGrid; }
		}

		public ZGrid LocalChargesGridForTest
		{
			get { return LocalChargesGrid; }
		}

		public ZGrid ChargesGridForTest
		{
			get { return ChargesGrid; }
		}

		public ZGrid HouseBillsGridForTest => HouseBillsGrid;

		public BusinessObject DataSourceForTest
		{ get; set; }

		protected override BusinessObject GetNewDataSource()
		{
			return DataSourceForTest;
		}

		public TrackingDeclaration TestDeclation
		{
			get { return testDeclaration; }
			set
			{
				testDeclaration = value;
				LoadOrCreateDataSource();
			}
		}
		TrackingDeclaration testDeclaration;

		protected override void SetUpStatusControl(string countryCode, string direction)
		{ }

		public void SetDataSource(BusinessObject newDataSource)
		{
			DataSourceForTest = newDataSource;
			LoadOrCreateDataSource();
		}

		public void ForTest_RunOnLoad()
		{
			this.OnLoad(new EventArgs());
		}

		public void ForTest_RunOnPreBind()
		{
			this.OnPreBind();
		}
	}
}
