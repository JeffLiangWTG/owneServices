using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// Summary description for ShipmentDetails.
	/// </summary>
	public partial class EditContainer : BasePageWithAuthorisation
	{
		#region DataSource

		protected override BusinessObject GetNewDataSource()
		{
			return TrackingContainer.FromPKFilteredBySiteUser(Factory, GetGuidFromParameter(RefParameterName), SiteUser);
		}

		protected TrackingContainer CurrentContainer
		{
			get { return DataSource as TrackingContainer; }
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		#endregion

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DisabledSubmitButtons.Add(SaveContainer.ClientID);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			bool isDataSourceValid = (CurrentContainer != null);

			ContainerContents.Visible = isDataSourceValid;
			NotFoundError.Visible = !isDataSourceValid;
			OrdersPanel.Visible = SiteUser?.CanViewOrders ?? false;

			if (!isDataSourceValid)
			{
				NotFoundLabel.Text = Res.GetString("0f26158a-84b9-4d1d-8a42-3e587da64846", "Container was not found in the database or you don't have rights to view it.");
			}
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser != null && SiteUser.CanEditTrackingContainers; }
		}

		protected override void SetupAuthorisedContent(bool isAuthorised)
		{
			base.SetupAuthorisedContent(isAuthorised);
			if (SiteUser != null)
			{
				Sequence.Enabled = isAuthorised && SiteUser.CanEditContainersSequence;
				RequiredDelivery.Enabled = isAuthorised && SiteUser.CanEditContainersRequiredDeliveryDate;
				EmptyReady.Enabled = isAuthorised && SiteUser.CanEditContainersEstimatedDehireDate;
				ConfirmedDelivery.Enabled = isAuthorised && SiteUser.CanEditContainersConfirmedDeliveryDate;
				EmptyPickup.Enabled = isAuthorised && SiteUser.CanEditContainersPickupDate;
				ActualDelivery.Enabled = isAuthorised && SiteUser.CanEditContainersActualDeliveryDate;
				ActualDehire.Enabled = isAuthorised && SiteUser.CanEditContainersActualDehireDate;
			}
		}

		#region Setting up grids

		/// <summary>
		/// Set up all grids on the page
		/// </summary>
		protected override void SetupGrids()
		{
		}

		protected override void OnPreBind()
		{
			base.OnPreBind();

			//these grids require DataSource to be loaded before grid's construction.
			//Data source have to be loaded after ViewState's construction

			SetupOrdersGrid(OrdersGrid);
			SetupDocumentsGrid(DocumentsGrid);
		}

		protected void SetupOrdersGrid(ZDataGrid ordersGrid)
		{
			ordersGrid.AutoGenerateColumns = false;

			ZHyperLinkColumn orderNumberColumn = new ZHyperLinkColumn(Res.GetString("017908b8-f3cb-420a-b470-3e865fce3bfd", "Order #"), TrackingOrderLine.Schema.OrderNumber);
			orderNumberColumn.DataNavigateUrlFormatString = AppInstance.OrderDetailsPage + "?" + RefParameterName + "={0}";
			orderNumberColumn.DataNavigateUrlFields = new string[] { TrackingOrderLine.Schema.OrderPK };
			ordersGrid.Columns.Add(orderNumberColumn);

			ZHyperLinkColumn shipmentNumberColumn = new ZHyperLinkColumn(Res.GetString("af02a644-92d6-45fe-b525-ed4b7d2bfe9d", "Shipment #"), TrackingOrderLine.Schema.ShipmentNumber);
			shipmentNumberColumn.DataNavigateUrlFormatString = AppInstance.ShipmentDetailsPage + "?" + RefParameterName + "={0}";
			shipmentNumberColumn.DataNavigateUrlFields = new string[] { TrackingOrderLine.Schema.ShipmentPK };
			ordersGrid.Columns.Add(shipmentNumberColumn);

			ordersGrid.Columns.Add(new ZTextEditColumn(Res.GetString("1fb4248e-7185-4639-b1d7-e067829d727d", "House Bill"), TrackingOrderLine.Schema.HouseBillNumber));
			ordersGrid.Columns.Add(new ZTextEditColumn(Res.GetString("857c4c08-5852-4991-8ac7-609bc3a13ecc", "Supplier"), TrackingOrderLine.Schema.SupplierName));
			ordersGrid.Columns.Add(new ZTextEditColumn(Res.GetString("869f597d-a636-4560-b5df-a22fa820d605", "Product"), TrackingOrderLine.Schema.ProductNumber));
			ordersGrid.Columns.Add(new ZTextEditColumn(Res.GetString("1f5a0eb4-d905-4f51-a02f-68ff39774908", "Supplier Part"), TrackingOrderLine.Schema.CustomAttribute1));
			ordersGrid.Columns.Add(new ZTextEditColumn(Res.GetString("f5378450-a95d-459b-a31f-8ecc4fa74ea2", "Description"), TrackingOrderLine.Schema.Description));
			ordersGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("187a34a5-6bed-4b24-b080-fc13dac59da7", "Container Qty"), TrackingOrderLine.Schema.ContainerQuantity));
			ordersGrid.Columns.Add(new ZCalcEditColumn(Res.GetString("35bde3d3-6aaf-4753-80fe-a451743497ac", "Packs"), TrackingOrderLine.Schema.Packs));
		}

		#endregion

		protected void SaveChanges_Click(object sender, EventArgs e)
		{
			this.OrdersGrid.ExportIntoExcel();
		}

		protected void SaveContainer_Click(object sender, EventArgs e)
		{
			SaveDataSourceFactory();
			Response.Redirect(string.Format("{0}?{1}={2}", AppInstance.ContainerDetailsPage, RefParameterName, DataSource.PK));
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.EditContainerPage;
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.EditContainer;
		}

		protected void DropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			Bind();
		}

		#region Autogenerated

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.DataSourceAssemblyName = "Enterprise.Tracking.Business";
			this.DataSourceTypeName = "Enterprise.Tracking.Business.TrackingContainer";
		}
		#endregion

		protected void Page_Load(object sender, EventArgs e)
		{
		}

		#endregion
	}
}
