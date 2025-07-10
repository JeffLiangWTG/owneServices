using System;
using System.Web;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Orders
{
	/// <summary>
	/// Summary description for OrderDetails.
	/// </summary>
	public partial class OrderDetails : BasePageWithAuthorisation
	{
		#region DataSource

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			TrackingOrder order;

			ZGuid orderPK = GetGuidFromParameter(RefParameterName);
			if (orderPK.IsEmpty && Request.Params["Number"] != null)
			{
				order = TrackingOrder.FromNumberFilteredByContact(Factory, new ZString(Request.Params["Number"]), SiteUser);
			}
			else
			{
				order = TrackingOrder.FromPKFilteredByContact(Factory, orderPK, SiteUser);
			}
			return order;
		}

		protected
#if DEBUG
 virtual
#endif
 TrackingOrder CurrentOrder
		{
			get { return DataSource as TrackingOrder; }
		}

		#endregion

		#region OnLoad

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewOrders; }
		}

		protected void SetupPage()
		{
			ViewShipmentDetailsButton.Visible = CurrentOrder.ShipOrDec != null;
			OrderCancelledDiv.Visible = !IsActive;
			CancelOrder.Text = IsCancelled ? Res.GetString("ca94577d-a697-4626-8632-2f4b65876d56", "Re-Activate Order") : Res.GetString("7713d955-ada0-4058-83df-87b7c8741285", "Cancel Order");
			DocumentsGrid.Visible = SiteUser.CanViewDocuments;
			ViewShipmentDetailsButton.Text = Res.GetString("716500e5-a6c1-4830-95b2-48d276a717a4", "View") + " " + (IsAttachedToBooking ? Res.GetString("51bc7790-33c0-44b8-94a2-bee90fc0d366", "Booking") : Res.GetString("11e7376a-3110-4c63-9cf1-56cdb2b9bd4d", "Shipment")) + " " + Res.GetString("5848616b-bb42-4b0b-a762-c3b72cef79cc", "Details");
		}

		protected override void SetupAuthorisedContent(bool isAuthorised)
		{
			base.SetupAuthorisedContent(isAuthorised);
			if (isAuthorised)
			{
				if (CurrentOrder != null)
				{
					SetupPage();

					var isActive = IsActive;
					EditOrder.Enabled = SiteUser.CanEditOrders && CurrentOrder.ShipOrDec == null && !IsCancelled && isActive;
					CancelOrder.Enabled = SiteUser.CanEditOrders && CurrentOrder.ShipOrDec == null && isActive;

					var tooltip = !isActive ? Res.GetString("cd9461c1-6893-4a63-ba5e-d57fc04ecceb", "This order has been deactivated.") : string.Empty;
					EditOrder.ToolTip = CancelOrder.ToolTip = tooltip;
				}
				else
				{
					NotFoundError.Visible = true;
					OrderNotFoundLabel.Text = Res.GetString("b77ac5c1-0c8f-4e05-bce9-ae616c0bbd82", "The Order was not found in the database.");
					OrderContents.Visible = false;
				}
			}
			else
			{
				EditOrder.Enabled = false;
				CancelOrder.Enabled = false;
			}
		}

		#endregion OnLoad

		#region OnPreBind

		protected override void OnPreBind()
		{
			base.OnPreBind();

			if (CurrentOrder.Shipment != null)
			{
				DetailsPanel.Label = Res.GetString("11e7376a-3110-4c63-9cf1-56cdb2b9bd4d", "Shipment");
			}
			else if (CurrentOrder.Declaration != null)
			{
				DetailsPanel.Label = Res.GetString("015c13a7-a895-478c-801c-d0f2866b473c", "Declaration");
			}
			else
			{
				DetailsPanel.Label = Res.GetString("dde2d282-03f0-4178-bbf4-5edc642127bc", "Planning");
			}

			PlannedPacksPanel.Visible = CurrentOrder.ShipOrDec != null;

			SetupPlanningDetails();
			SetupShipmentDetails();
			SetupOrderLinesGrid();
			SetupDocumentsGrid(DocumentsGrid);
			SetupVoyagesGrid(CurrentOrder.PlannedVoyages);

			CustomLabelInfoList customLabels = CurrentOrder.GetAdditionalInformationFields();
			SetupAdditionalInformationTable(customLabels, AdditionalDetailTable, AdditionalDetailPanel);

			if (SiteUser.IsShipmentQuickViewUser)
			{
				SetUpPageForQuickViewUser();
			}
		}

		void SetUpPageForQuickViewUser()
		{
			EditButtonDiv.Visible = false;
			AuthArea1.Visible = false;
			AuthArea2.Visible = false;
			SendingAgentArea.Visible = false;
			ReceivingAgentArea.Visible = false;
			DocumentsGrid.Visible = false;
			NotesPanel.Visible = false;
		}

		#endregion

		#region SetupPlanning/Shipment/Declaration

		protected void SetupPlanningDetails()
		{
			if (CurrentOrder.ShipOrDec == null)
			{
				if (CurrentOrder.IsSeaTransport)
				{
					PlannedContainersGrid.Visible = true;
					SetupPlannedContainersGrid(PlannedContainersGrid);
				}
			}
			else
			{
				PlannedContainersGrid.Visible = false;
			}
		}

		protected void SetupShipmentDetails()
		{
			ContainersGrid.Visible = false;
			TransportPanel.Visible = false;

			if (CurrentOrder.Shipment != null)
			{
				if (CurrentOrder.IsSeaTransport)
				{
					ContainersGrid.Visible = true;
					SetupContainersGrid(ContainersGrid);
				}

				TransportPanel.Visible = true;
				SetupTransportGrid();

				if (!Page.IsPostBack)
				{
					ConfigurationHelper.ConfigureLegendLabels(PendingLegendLabel, OverdueLegendLabel, CompletedLegendLabel, CompletedLateLegendLabel);
				}
			}
		}

		#endregion

		#region Grids

		protected void SetupOrderLinesGrid()
		{
			OrderLinesGrid.ColumnProvider = new ForwardingOrderLineColumnProvider(CurrentOrder);
		}

		protected void SetupPlannedContainersGrid(ZGrid plannedContainersGrid)
		{
			plannedContainersGrid.ColumnProvider = new ForwardingOrderPlannedContainerColumnProvider();
		}

		protected void SetupContainersGrid(ZGrid containersGrid)
		{
			containersGrid.ColumnProvider = new ForwardingOrderContainerColumnProvider();
		}

		protected void SetupTransportGrid()
		{
			TransportGrid.ColumnProvider = new TrackingTransportDetailsGridColumnProvider();
		}

		protected void SetupVoyagesGrid(PlannedVoyagesCollection plannedVoyages)
		{
			PlannedVoyagesGrid.Visible = (plannedVoyages.Count > 0 && CurrentOrder.Shipment == null);
			if (!PlannedVoyagesGrid.Visible)
			{
				return;
			}
			PlannedVoyagesGrid.ColumnProvider = new ForwardingOrderPlannedVoyageColumnProvider();
		}

		#endregion

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
			PageTitleLabel.BindTo = "JD_OrderNumberAndSplit";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).JD_OrderNumberAndSplit)));
			OrderCancelledLabel.BindTo = null;
			GoodsDescLabel.BindTo = "JD_OrderGoodsDescription";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).JD_OrderGoodsDescription)));
			OrderDateLabel.BindTo = "JD_OrderDate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingOrder)(null)).JD_OrderDate)));
			BookingConfRefLabel.BindTo = "JD_BookingConfRef";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).JD_BookingConfRef)));
			IncoTermsLabel.BindTo = "JD_IncoTerm";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).JD_IncoTerm)));
			SupplierLabel.BindTo = "SupplierPK";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZGuid)(((TrackingOrder)(null)).SupplierPK)));
			SendingAgent.BindTo = "JD_OH_SendingAgent";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZGuid)(((TrackingOrder)(null)).JD_OH_SendingAgent)));
			ReceivingAgent.BindTo = "JD_OH_ReceivingAgent";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZGuid)(((TrackingOrder)(null)).JD_OH_ReceivingAgent)));
			PortOfLoading.BindTo = "JD_RL_NKPortOfLoading";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingOrder)(null)).JD_RL_NKPortOfLoading)));
			PortOfDischarge.BindTo = "JD_RL_NKPortOfDischarge";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingOrder)(null)).JD_RL_NKPortOfDischarge)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingOrder)(null)).ETAWithSuppression)));
			PlanningHouseBillLabel.BindTo = "JD_Waybill";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).JD_Waybill)));
			PlanningMasterBillLabel.BindTo = "JD_MasterWaybill";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).JD_MasterWaybill)));
			PacksLabel.BindTo = "ShipDecPacksWithUnits";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).ShipDecPacksWithUnits)));
			VolumeLabel.BindTo = "ShipDecActualVolumeWithUnits";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).ShipDecActualVolumeWithUnits)));
			WeightLabel.BindTo = "ShipDecActualWeightWithUnits";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).ShipDecActualWeightWithUnits)));
			PlannedPacksLabel.BindTo = "JD_PacksWithUnits";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).JD_PacksWithUnits)));
			PlannedVolumeLabel.BindTo = "JD_ActualVolumeWithUnits";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).JD_ActualVolumeWithUnits)));
			PlannedWeightLabel.BindTo = "JD_ActualWeightWithUnits";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingOrder)(null)).JD_ActualWeightWithUnits)));

			ReqExWorks.BindTo = "JD_ExWorksRequiredBy";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingOrder)(null)).JD_ExWorksRequiredBy)));
			ReqInStore.BindTo = "JD_DeliveryRequiredBy";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingOrder)(null)).JD_DeliveryRequiredBy)));

			OrderNotFoundLabel.BindTo = null;

			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((TrackingOrder)null).Shipment.TransportsInLegOrder);

			this.DataSourceAssemblyName = "Enterprise.Tracking.Business";
			this.DataSourceTypeName = "Enterprise.Tracking.Business.TrackingOrder";
		}
		#endregion

		#endregion

		#region Event Handlers

		protected void EditOrder_Click(object sender, EventArgs e)
		{
			Response.Redirect(String.Format("{0}?{1}={2}", AppInstance.EditOrderPage, RefParameterName, CurrentOrder.PK));
		}

		protected void CancelOrder_Click(object sender, EventArgs e)
		{
			NotificationFlags.DisplayAll = false;
			if (IsCancelled)
			{
				CurrentOrder.JD_OrderStatus = Constants.OrderStatus.Incomplete;
			}
			else
			{
				CurrentOrder.JD_OrderStatus = Constants.OrderStatus.Cancelled;
			}

			SaveCancelWithRefreshOnFailure(string.Format((NoResString)"{0}?Ref={1}", AppInstance.OrderDetailsPage, CurrentOrder.PK)); // Redirection path
#if DEBUG
			if (!Globals.IsTest)
#endif
			{
				OnLoad(new EventArgs());
			}
		}

		protected void ViewShipmentDetailsButton_Click(object sender, EventArgs e)
		{
			if (CurrentOrder != null && CurrentOrder.ShipOrDec != null)
			{
				string queryString = string.Format((NoResString)"?Ref={0}&Table={1}", CurrentOrder.ShipOrDec.PersistentBizOPK, ((BusinessObject)CurrentOrder.ShipOrDec).TableName); // Request parameters
				if (IsAttachedToBooking)
				{
					Response.Redirect(AppInstance.BookingDetailsPage + queryString);
				}

				Response.Redirect(AppInstance.ShipmentPage + queryString);
			}
		}

		#endregion EventHandlers

		protected bool IsActive => CurrentOrder != null && !CurrentOrder.JD_IsCancelled;

		protected bool IsCancelled
		{
			get
			{
				return CurrentOrder != null & CurrentOrder.JD_OrderStatus == Constants.OrderStatus.Cancelled;
			}
		}

		protected bool IsAttachedToBooking
		{
			get
			{
				if (CurrentOrder != null)
				{
					if (CurrentOrder.Shipment != null)
					{
						if (!CurrentOrder.Shipment.JS_IsForwardRegistered && CurrentOrder.Shipment.JS_IsBooking)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.OrderDetails;
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.OrderDetailsPage;
		}

		protected void DuplicateOrder_Click(object sender, EventArgs e)
		{
			if (CurrentOrder != null)
			{
				var newOrder = ((ITemplateCopyable)CurrentOrder).TemplateCopy() as TrackingOrder;
				if (newOrder != null)
				{
					newOrder.JD_IsCancelled = false;
					UpdateAutoCreatedLogReferenceAndSaveDataSource(newOrder.PK, newOrder);
					HttpContext.Current.Response.Redirect(string.Format((NoResString)"{0}?Ref={1}&{2}={3}", AppInstance.EditOrderPage, newOrder.PK, ZPage.DataSourceInSessionParameterName, ZPage.DataSourceInSessionParameterValue)); // Redirection path
				}
			}
		}
	}
}
