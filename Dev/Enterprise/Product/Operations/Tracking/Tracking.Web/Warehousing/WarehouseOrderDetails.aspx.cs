using System;
using System.Web;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// Summary description for WarehouseOrderDetails.
	/// </summary>
	public partial class WarehouseOrderDetails : WarehousingBasePage
	{
		#region Overrides

		protected override string GetPageName()
		{
			return WebTracker.Pages.WarehouseOrderDetails;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.WarehouseOrderDetailsPage;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			SetupPage();
		}

		protected void GetOrSetDetalisationMode()
		{
			if (Request.Params["Mode"] == null || !bool.TryParse(Request.Params["Mode"], out showDetails))
			{
				showDetails = WebDataRegistry.Instance.GetShowDetailedOrderLines(SiteUser.LoggedInUser.PK);
			}
			else
			{
				bool initialValue = WebDataRegistry.Instance.GetShowDetailedOrderLines(SiteUser.LoggedInUser.PK);
				if (initialValue != showDetails)
				{
					WebDataRegistry.Instance.SetShowDetailedOrderLines(SiteUser.LoggedInUser.PK, showDetails);
				}
			}
		}

		protected void SetupPage()
		{
			bool orderIsValid = (Order != null);

			OrderContents.Visible = orderIsValid;
			NotFoundError.Visible = !orderIsValid;
			NotFoundLabel.Text = Res.GetString("72a23d3e-debf-4ac9-b963-56b5e620fce0", "Order was not found in the database or you don't have rights to view it.");
			EditButtonDiv.Visible = !IsShownInPopup;
			if (orderIsValid)
			{
				var canEditMessage = string.Empty;
				if (Order.WhsOrder.IsFinalised)
				{
					canEditMessage = Res.GetString("1afcb716-c5a2-4bb0-b212-0bb80f81d7aa", "This order has been dispatched and cannot be edited or canceled");
				}
				else if (Order.WhsOrder.IsAttachedToPick)
				{
					canEditMessage = Res.GetString("e291bd92-5e85-413d-881a-d70f8d6bd1bc", "This order has already been processed and cannot be edited or canceled");
				}

				CancelOrder.Text = Order.IsCancelled ? Res.GetString("dc395556-2d94-4fb0-b642-469676884553", "Re-Activate Order") : Res.GetString("0099f2d3-4733-4cbe-a804-3abb53594c26", "Cancel Order");
				CanEditCancelLabel.Visible = !string.IsNullOrEmpty(canEditMessage);
				CanEditCancelLabel.Text = canEditMessage;
				DocumentsGrid.Visible = SiteUser.CanViewDocuments;
				new ZWebControlBinder(Order).Bind(OrderStatusControls.Controls);

				ShowLinesDetail.NavigateUrl = string.Format((NoResString)"{0}?Ref={1}&Mode={2}", AppInstance.WarehouseOrderDetailsPage, Order.WhsOrder.PK, true);
				ShowLinesSummary.NavigateUrl = string.Format((NoResString)"{0}?Ref={1}&Mode={2}", AppInstance.WarehouseOrderDetailsPage, Order.WhsOrder.PK, false);
			}

			WhsOrderLineDetailsContainer.Visible = showDetails;
			WhsOrderLineSummaryContainer.Visible = !showDetails;

			if (SiteUser.IsShipmentQuickViewUser)
			{
				SetUpPageForQuickViewUser();
			}
		}

		void SetUpPageForQuickViewUser()
		{
			EditButtonDiv.Visible = false;
			CneeArea.Visible = false;
			CneeAddressArea.Visible = false;
			TransportCoArea.Visible = false;
			ShowLinesDetail.Visible = false;
			ShowLinesSummary.Visible = false;
			WhsOrderLineDetailsContainer.Visible = false;
			WhsOrderLineSummaryContainer.Visible = true;

			DocumentsGrid.Visible = false;
			ChargesGrid.Visible = false;
			NotesPanel.Visible = false;
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewWarehouseOrders; }
		}

		protected override void SetupAuthorisedContent(bool isAuthorised)
		{
			base.SetupAuthorisedContent(isAuthorised);
			if (Order != null)
			{
				bool canEdit = isAuthorised && SiteUser.CanEditWarehouseOrders;
				CancelOrder.Enabled = canEdit && Order.CanCancelDocket;
				EditOrder.Enabled = canEdit && Order.CanEdit;
			}
		}

		#endregion Overrides

		#region BusinessObject

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			TrackingWhsOrder fOrder = null;
			if (OrderPK.IsValid)
			{
				fOrder = TrackingWhsOrder.FromPKFilteredByContact(Factory, OrderPK, SiteUser);
			}
			else
			{
				fOrder = TrackingHelper.Get(Factory.New<WhsOrder>());
				fOrder.SiteUser = SiteUser;
				fOrder.WhsOrder.WD_OH_Client = SiteUser.LoggedInOrganisation.PK;
			}
			if (fOrder != null)
			{
				fOrder.Logs.AutoCreatedLogDefaultSL_Reference = SiteUser.ContactAndCompanyReference;
			}
			return fOrder;
		}

		protected
#if DEBUG
 virtual
#endif
 TrackingWhsOrder Order
		{
			get { return DataSource as TrackingWhsOrder; }
		}

		protected override void OnDataSourceFactorySaved()
		{
			base.OnDataSourceFactorySaved();
			RedirectToOrderDetailsPage();
		}

		protected void RedirectToOrderDetailsPage()
		{
			Response.Redirect(string.Format("{0}?{1}={2}", AppInstance.WarehouseOrderDetailsPage, RefParameterName, Order.WhsOrder.PK));
		}

		protected ZGuid OrderPK
		{
			get { return GetGuidFromParameter(RefParameterName); }
		}

		#endregion BusinessObject

		#region Grid setup

		bool showDetails = true;

		protected override void OnPreBind()
		{
			base.OnPreBind();
			WhsOrderReferencesGrid.Visible = Order.WhsOrder.References != null && Order.WhsOrder.References.Count > 0;
			SetupDocumentsGrid(DocumentsGrid);
			SetupAdditionalInformationTable(Order.GetAdditionalInformationFields(), AdditionalDetailTable, AdditionalDetailPanel, Order);
		}

		protected override void SetupGrids()
		{
			SetupReferenceGrid();
			SetupOrderLinesGrid();
			SetupChargesGrid();
		}

		#region Charges Grid

		void SetupChargesGrid()
		{
			if (((TrackingSiteUser)SiteUser).CanViewAccounts)
			{
				ChargesGrid.Visible = true;
				new InvoicePresenter(SiteUser).SetupGrid(ChargesGrid);
			}
			else
			{
				ChargesGrid.Visible = false;
			}
		}

		#endregion Charges Grid

		protected void SetupReferenceGrid()
		{
			WhsOrderReferencesGrid.ColumnProvider = new WarehouseDocketReferenceColumnProvider();
		}

		protected void SetupOrderLinesGrid()
		{
			WhsOrderLinesGrid.ColumnProvider = new TrackingWhsOrderDetailsColumnProvider();
			SetupOrderLinesSummaryGrid();

			string buttonText;

			if (showDetails)
			{
				WhsOrderLinesGrid.Menu.Controls.AddAt(0, DetailedExportButton);
				DetailedExportButton.Click += DetailedExportButton_Click;
				WhsOrderLinesGrid.RegisterPostBackControl(DetailedExportButton);
				buttonText = Res.GetString("d788ad96-4dc4-4425-9848-d9a1e0bf67c3", "Export to Excel (Summary)");
			}
			else
			{
				DetailedExportButton.Visible = false;
				buttonText = Res.GetString("abbf3bc6-6610-4bb2-b064-62be69a695fd", "Export to Excel");
			}

			WhsOrderLinesGrid.SetButtonText(WhsOrderLinesGrid.ExportToExcelButton, buttonText);
			WhsOrderLinesGrid.ExportToExcelButton.ToolTip = buttonText;
		}

		protected void SetupOrderLinesSummaryGrid()
		{
			WhsOrderSummaryLinesGrid.ColumnProvider = new WarehouseOrderLinesSummaryColumnProvider();
		}

		#endregion Grid setup

		#region AutoGenerated

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
			GetOrSetDetalisationMode();
		}

		LinkButton DetailedExportButton
		{
			get
			{
				if (detailedExportButton == null)
				{
					var text = Res.GetString("2acaf839-e845-46e3-ab2b-0cabd79b4b8e", "Export to Excel (Details)");
					return detailedExportButton = new LinkButton
					{
						ID = "DetailedExportButton",
						Visible = true,
						Text = text,
						ToolTip = text,
						CssClass = "OnDemandMenuButton" // Is a CSS Class
					};
				}
				return detailedExportButton;
			}
		}
		LinkButton detailedExportButton;

		void DetailedExportButton_Click(object sender, EventArgs e)
		{
			if (WhsOrderLinesGrid != null)
			{
				var orderLines = WhsOrderLinesGrid.DataSource as TrackingWhsOrderLineCollection;
				if (orderLines != null)
				{
					var collection = new TrackingWhsReleaseLineCollection(Factory);
					foreach (TrackingWhsOrderLine orderline in orderLines)
					{
						collection.AddRange(orderline.ReleaseDetails);
					}
					WhsOrderLinesGrid.ExportIntoExcel(collection, TrackingOrderLineTemplate.GetWhsOrderLinesExcelExportColumns(collection));
				}
			}
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			WhsOrderLabel.BindTo = null;
			CanEditCancelLabel.BindTo = null;
			NotFoundLabel.BindTo = null;
			WarehouseLabel.BindTo = null;
			Warehouse.BindTo = TrackingWhsOrder.WrapperSchema.WD_WW_Whs;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZGuid)(((TrackingWhsOrder)(null)).WhsOrder.WD_WW_Whs)));
			OrderNumberLabel.BindTo = null;
			OrderNumber.BindTo = TrackingWhsOrder.WrapperSchema.WD_ExternalReference;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingWhsOrder)(null)).WhsOrder.WD_ExternalReference)));
			OrderStatusLabel.BindTo = null;
			OrderStatus.BindTo = TrackingWhsOrder.WrapperSchema.WD_DocketStatusDescription;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingWhsOrder)(null)).WhsOrder.WD_DocketStatusDescription)));
			RequiredDateLabel.BindTo = null;
			RequiredByDate.BindTo = TrackingWhsOrder.WrapperSchema.TrackingRequiredDate;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingWhsOrder)(null)).TrackingRequiredDate)));
			TotalUnitsLabel.BindTo = null;
			TotalUnits.BindTo = TrackingWhsOrder.WrapperSchema.WD_TotalUnits;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((INumericZType)(((TrackingWhsOrder)(null)).WhsOrder.WD_TotalUnits)));
			ConsigneeLabel.BindTo = null;

			TransportRef.BindTo = TrackingWhsOrder.WrapperSchema.WD_TransportReference;
			TransportRef.DataNavigateUrlFormatString = "{0}"; // Request parameter
			TransportRef.DataNavigateUrlFields = new string[1] { TrackingWhsOrder.WrapperSchema.WD_TransportCoUrl };
			TransportRef.Target = "_blank"; // HTML target
			TransportRef.IsExternalHyperLink = true;

			WhsOrderLineDetails.BindTo = null;
			this.DataSourceAssemblyName = "Enterprise.Tracking.Business";
			this.DataSourceTypeName = "Enterprise.Tracking.Business.TrackingWhsOrder";
		}
		#endregion

		#endregion

		#region Event Handlers

		protected void DuplicateOrder_Click(object sender, EventArgs e)
		{
			if (Order != null)
			{
				var newOrder = ((ITemplateCopyable)Order).TemplateCopy() as TrackingWhsOrder;
				if (newOrder != null)
				{
					newOrder.SiteUser = SiteUser;
					newOrder.WhsOrder.WD_OH_Client = SiteUser.LoggedInOrganisation.PK;
					UpdateAutoCreatedLogReferenceAndSaveDataSource(newOrder.PK, newOrder);
					HttpContext.Current.Response.Redirect(string.Format((NoResString)"{0}?Ref={1}&{2}={3}", AppInstance.WarehouseEditOrders, newOrder.PK, DataSourceInSessionParameterName, DataSourceInSessionParameterValue)); // Redirection path
				}
			}
		}

		protected void EditOrder_Click(object sender, EventArgs e)
		{
			if (Order != null)
			{
				HttpContext.Current.Response.Redirect(string.Format("{0}?{1}={2}", AppInstance.WarehouseEditOrders, RefParameterName, Order.WhsOrder.PK));
			}
		}

		protected void CancelOrder_Click(object sender, EventArgs e)
		{
			NotificationFlags.DisplayAll = false;

			if (Order != null)
			{
				Order.WhsOrder.WD_DocketStatus = Order.WhsOrder.WD_DocketStatus == DocketStatus.Codes.Cancelled ? DocketStatus.Codes.Entered : DocketStatus.Codes.Cancelled;
				Order.Factory.Save();

				var eventLogHelper = new EventLogHelper();
				eventLogHelper.CreateLogForEdit(SiteUser, Order);
				eventLogHelper.CreateLogForCancel(SiteUser, Order);
			}

			SetupPage();
		}

		#endregion
	}
}
