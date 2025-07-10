using System;
using System.Web;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web.Bookings
{
	public partial class BookingDetails : BasePageWithAuthorisation
	{
		protected override string GetPageName()
		{
			return WebTracker.Pages.BookingDetails;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (SiteUser.IsShipmentQuickViewUser)
			{
				EditBooking.Visible = false;
				CancelBooking.Visible = false;
				DuplicateBooking.Visible = false;
				ReverseBooking.Visible = false;
				DocsMenu.Visible = false;

				ConsignorConsigneeRow.Visible = false;
				ConsignorConsigneeContactRow.Visible = false;
				ForAuthenticatedUserOnly.Visible = false;
				PickupAddressRow.Visible = false;
				DeliveryAddressRow.Visible = false;
				PickupAgentRow.Visible = false;
				DeliveryAgentRow.Visible = false;

				PackLinesGrid.ShouldShowControl = false;
				AttachedOrdersGrid.ShouldShowControl = false;
				ContainersDataGrid.ShouldShowControl = false;
				ReferenceDataGrid.ShouldShowControl = false;

				DetailsGoodsDescription.Visible = false;
				ForAuthenticatedUserOnly2.Visible = false;
				PaymentTermsRow.Visible = false;
				BillingPartyRow.Visible = false;
				MarksAndNumbersRow.Visible = false;
				SpecialInstructionsRow.Visible = false;
			}
		}

		protected void SetupPage()
		{
			NotificationFlags.DisplayAll = false;
			SetupTransportModeHandler();
			ContainersDataGrid.Visible = IsFCL && ShowContainersGrid;
			notFCL.Visible = !IsFCL;
			PackLinesGrid.Visible = ShowGoodsPacksGrid;

			if (OriginDestinationRow != null)
			{
				OriginDestinationRow.Visible = !Booking.IsDomesticFreight;
			}

			if (WarehouseRecRow != null)
			{
				WarehouseRecRow.Visible = !Booking.IsDomesticFreight;
			}

			if (CustomsEntryRow != null)
			{
				CustomsEntryRow.Visible = !Booking.IsDomesticFreight;
			}

			if (InsuranceValueRow != null)
			{
				InsuranceValueRow.Visible = !WebDataRegistry.Instance.DisableBookingInsuranceValue.Value;
			}

			if (AdditionalTermsLabel != null)
			{
				AdditionalTermsLabel.Visible = !Booking.IsDomesticFreight;
			}

			PayTermLabel.Text = Booking.IsDomesticFreight ? Res.GetString("638b3ee3-580c-4d52-ba4f-408f474d0bd5", "Payment Term:") : Res.GetString("3532f0a2-f8e8-889c-4774-668f24b83637", "Incoterm:");

			BookingCancelledDiv.Visible = Booking.IsCancelled;
			CancelBooking.Text = Booking.IsCancelled ? Res.GetString("c93b2b19-fe15-4b3d-b69c-6f83df03656f", "Re-Activate Booking") : Res.GetString("6578c021-2c2c-4d54-8e77-a6fce07839c6", "Cancel Booking");
			string consignorTerminology = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;
			ConsignorLabel.Text = consignorTerminology + ":";
			ConsignorContactLabel.Text = Res.GetString("e229ec08-92a4-495b-aea5-a7609baa3363", "{0} Contact:", consignorTerminology);

			OrderReferencesLabel.Visible = (Booking.AttachedOrderLinks.Count == 0);
			OrderReferencesTextBox.Visible = (Booking.AttachedOrderLinks.Count == 0);
			AttachedOrdersGrid.Visible = !OrderReferencesTextBox.Visible && (SiteUser?.CanViewOrders ?? false);

			DocumentsGrid.Visible = SiteUser.CanViewDocuments;
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (!Globals.IsTest)
			{
				base.OnPreRender(e);
			}
			SchedulesPanel.Label = ((WebScheduleChooserControl)ScheduleChooser).HeaderText;
			SchedulesPanel.Visible = SiteUser != null && SiteUser.IsLoggedIn && SiteUser.CanBookSailings;
		}

		bool IsFCL
		{
			get { return Booking.Mode == Enterprise.Core.Constants.ContainerModes.FCL; }
		}

		bool ShowGoodsPacksGrid
		{
			get { return !WebDataRegistry.Instance.DisableBookingGoodsPacksGrid.Value; }
		}

		bool ShowContainersGrid
		{
			get { return !WebDataRegistry.Instance.DisableBookingContainersGrid.Value; }
		}

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewBookings; }
		}

		protected override void SetupAuthorisedContent(bool isAuthorised)
		{
			base.SetupAuthorisedContent(isAuthorised);
			if (Booking != null && isAuthorised)
			{
				var bookingButtonsEnabled = SiteUser?.CanEditBookings ?? false;
				CancelBooking.Enabled = bookingButtonsEnabled;
				EditBooking.Enabled = bookingButtonsEnabled;
				DuplicateBooking.Enabled = bookingButtonsEnabled;
				ReverseBooking.Enabled = bookingButtonsEnabled;

				if (!bookingButtonsEnabled)
				{
					var toolTip = Res.GetString("d5614d00-4bea-4376-bf24-e0c4a48f7784", "You are not authorized to use this function. Please contact your system administrator to request access rights.");
					CancelBooking.ToolTip = toolTip;
					EditBooking.ToolTip = toolTip;
					DuplicateBooking.ToolTip = toolTip;
					ReverseBooking.ToolTip = toolTip;
				}

				SetupPage();
			}
			else
			{
				CancelBooking.Enabled = false;
				EditBooking.Enabled = false;
				DuplicateBooking.Enabled = false;
				ReverseBooking.Enabled = false;
			}
		}

		#region DataSource

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override BusinessObject GetNewDataSource()
		{
			return TrackingBooking.GetFromRefPK(Factory, GetGuidFromParameter("Ref"), SiteUser);
		}

		protected TrackingBooking Booking
		{
			get { return DataSource != null ? ((TrackingBooking)DataSource) : null; }
		}

		#endregion

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
		/// </summary>l
		void InitializeComponent()
		{
			TitleLabel.BindTo = null;
			Ztextlabel1.BindTo = TrackingBooking.Schema.UniqueConsignRef;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingBooking)(null)).UniqueConsignRef)));

			UnauthorisedLabel.BindTo = null;
			BookingCancelledLabel.BindTo = null;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			Zdatetimelabel1.BindTo = TrackingBooking.Schema.A_BKD;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingBooking)(null)).A_BKD)));

			TransportMode.BindTo = TrackingBooking.Schema.Mode;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingBooking)(null)).Mode)));

			OriginPort.BindTo = TrackingBooking.Schema.Origin;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingBooking)(null)).Origin)));

			DestinationPort.BindTo = TrackingBooking.Schema.Destination;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingBooking)(null)).Destination)));

			ShippersRefTextBox.BindTo = TrackingBooking.Schema.BookingReference;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingBooking)(null)).BookingReference)));

			FinalDestinationETA.BindTo = TrackingBooking.Schema.ETAWithSuppression;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingBooking)(null)).ETAWithSuppression)));

			DescriptionTextBox.BindTo = TrackingBooking.Schema.GoodsDescription;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingBooking)(null)).GoodsDescription)));

			PacksEdit.BindTo = TrackingBooking.Schema.OuterPacks;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((TrackingBooking)(null)).OuterPacks)));

			WeightDropDown.BindTo = TrackingBooking.Schema.OuterPacksPackType;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingBooking)(null)).OuterPacksPackType)));

			PlannedWeightLabel.BindTo = TrackingBooking.Schema.WeightWithUnits;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingBooking)(null)).WeightWithUnits)));

			PlannedVolumeLabel.BindTo = TrackingBooking.Schema.VolumeWithUnits;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingBooking)(null)).VolumeWithUnits)));

			ConsignorTextLabel.BindTo = "ConsignorPickupAddress+" + JobDocAddress.Schema.E2_CompanyName;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingBooking)(null)).ConsignorPickupAddress.E2_CompanyName)));

			ConsigneeTextLabel.BindTo = "ConsigneeDeliveryAddress+" + JobDocAddress.Schema.E2_CompanyName;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingBooking)(null)).ConsigneeDeliveryAddress.E2_CompanyName)));

			WarehouseRecDdateEdit.BindTo = TrackingBooking.Schema.A_RCV;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingBooking)(null)).A_RCV)));

			PickupFromDateTimeLabel.BindTo = TrackingBooking.Schema.EstimatedPickup;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingBooking)(null)).EstimatedPickup)));

			PickupByDateTimeLabel.BindTo = TrackingBooking.Schema.PickupRequiredBy;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingBooking)(null)).PickupRequiredBy)));

			DeliveryOnDateTimeLabel.BindTo = TrackingBooking.Schema.EstimatedDelivery;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingBooking)(null)).EstimatedDelivery)));

			DeliveryByDateTimeLabel.BindTo = TrackingBooking.Schema.DeliveryRequiredBy;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((TrackingBooking)(null)).DeliveryRequiredBy)));

			PayTermCodeLookupLabel.BindTo = TrackingBooking.Schema.INCO;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingBooking)(null)).INCO)));

			ShipperCODAmountBox.BindTo = TrackingBooking.Schema.ShipperCODAmount;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((TrackingBooking)(null)).ShipperCODAmount)));

			ShipperCODTypeLabel.BindTo = TrackingBooking.Schema.ShipperCODPayMethod;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingBooking)(null)).ShipperCODPayMethod)));

			GoodsValueBox.BindTo = TrackingBooking.Schema.GoodsValue;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((TrackingBooking)(null)).GoodsValue)));

			Currency.BindTo = TrackingBooking.Schema.GoodsValueCurr;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingBooking)(null)).GoodsValueCurr)));

			PickupEquipmentNeededDropDownList.BindTo = TrackingBooking.Schema.FCLPickupEquipmentNeeded;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingBooking)(null)).FCLPickupEquipmentNeeded)));

			CartageDropModeCodeLookupLabel.BindTo = TrackingBooking.Schema.FCLDeliveryEquipmentNeeded;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((TrackingBooking)(null)).FCLDeliveryEquipmentNeeded)));

			PickupAddressLabel.BindTo = "ConsignorPickupAddress+AddressAsASingleLine";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingBooking)(null)).ConsignorPickupAddress.AddressAsASingleLine)));

			DeliveryAddressLabel.BindTo = "ConsigneeDeliveryAddress+AddressAsASingleLine";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingBooking)(null)).ConsigneeDeliveryAddress.AddressAsASingleLine)));

			Ztextbox1.BindTo = TrackingBooking.Schema.CustomsEntryNumber;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingBooking)(null)).CustomsEntryNumber)));

			Ztextbox2.BindTo = TrackingBooking.Schema.MarksAndNumbers;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingBooking)(null)).MarksAndNumbers)));

			OrderReferencesTextBox.BindTo = TrackingBooking.Schema.OrderItemsAsString;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingBooking)(null)).OrderItemsAsString)));

			VesselLabel.BindTo = "Vessel";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingBooking)(null)).Vessel)));

			VoyageLabel.BindTo = "VoyageFlightWithSuppression";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingBooking)(null)).VoyageFlightWithSuppression)));

			this.DataSourceAssemblyName = "Enterprise.Freight.Booking.Business";
			this.DataSourceTypeName = "Enterprise.Tracking.Business.TrackingBooking";
		}
		#endregion

		#region Transport Handler & Script

		protected void SetupTransportModeHandler()
		{
			ContainersDataGrid.Enabled = !Booking.IsAir;
		}

		#endregion Transport Handler & Script

		internal protected override ZTemplateColumn MultipleProductsColumn
		{
			get
			{
				ZNewRowColumn result = new ZNewRowColumn((NoResString)"Products");  // Binding name
				result.ItemTemplate = new MultipleProductsTemplate(result);
				result.HeaderText = Res.GetString("b2944b44-6707-48fe-aaa4-b84376dc1f77", "Multiple Products");
				result.Collapsable = true;
				result.ReadOnly = true;
				return result;
			}
		}

		#region Data Grids Setup & Binding

		protected override void SetupGrids()
		{
			base.SetupGrids();

			SetupContainersGrid();
			SetupDocumentsGrid(DocumentsGrid);
			SetupAttachedOrdersGrid();
			SetupReferenceDataGrid();
		}

		protected void SetupAttachedOrdersGrid()
		{
			AttachedOrdersGrid.ColumnProvider = new ForwardingBookingOrderColumnProvider();
		}

		protected void SetupContainersGrid()
		{
			if (ShowContainersGrid)
			{
				ContainersDataGrid.ColumnProvider = new ForwardingBookingContainerColumnProvider();
			}
		}

		void SetupReferenceDataGrid()
		{
			ReferenceDataGrid.ColumnProvider = new ForwardingBookingReferenceColumnProvider();
		}

		protected override void OnPreBind()
		{
			base.OnPreBind();

			SetupPackLinesGrid(PackLinesGrid, false, Booking.IsDomesticFreight);
		}

		#endregion Data Grid Setup

		protected void CancelBooking_Click(object sender, EventArgs e)
		{
			if (SiteUser?.CanEditBookings ?? false)
			{
				Booking.IsCancelled = !Booking.IsCancelled;
				SaveCancelWithRefreshOnFailure(string.Format((NoResString)"{0}?Ref={1}", AppInstance.BookingDetailsPage, Booking.BookingPK)); // Redirection path
			}
		}

		protected void DuplicateBooking_Click(object sender, EventArgs e)
		{
			if (Booking != null && (SiteUser?.CanEditBookings ?? false))
			{
				var newBooking = (TrackingBooking)((ITemplateCopyable)Booking).TemplateCopy();
				if (newBooking != null)
				{
					UpdateAutoCreatedLogReferenceAndSaveDataSource(newBooking.BookingPK, newBooking);
					HttpContext.Current.Response.Redirect(string.Format((NoResString)"{0}?Ref={1}&{2}={3}", AppInstance.EditBookingPage, newBooking.BookingPK, DataSourceInSessionParameterName, DataSourceInSessionParameterValue)); // Redirection path
				}
			}
		}

		protected void ReverseBooking_Click(object sender, EventArgs e)
		{
			if (Booking != null && (SiteUser?.CanEditBookings ?? false))
			{
				var oldBooking = new TrackingBooking(Booking.BookingPK, Factory, SiteUser);
				var newBooking = (TrackingBooking)((ITemplateCopyable)oldBooking).TemplateCopy();
				if (newBooking != null)
				{
					((ITemplateReversible)newBooking).Reverse();
					UpdateAutoCreatedLogReferenceAndSaveDataSource(newBooking.BookingPK, newBooking);
					HttpContext.Current.Response.Redirect(string.Format((NoResString)"{0}?Ref={1}&{2}={3}", AppInstance.EditBookingPage, newBooking.BookingPK, ZPage.DataSourceInSessionParameterName, ZPage.DataSourceInSessionParameterValue)); // Redirection path
				}
			}
		}

		protected override void OnDataSourceFactorySaved()
		{
			base.OnDataSourceFactorySaved();
			SetupPage();
		}

		protected void EditBooking_Click(object sender, EventArgs e)
		{
			if (SiteUser?.CanEditBookings ?? false)
			{
				Response.Redirect(string.Format((NoResString)"{0}?Ref={1}", AppInstance.EditBookingPage, Booking.BookingPK)); // Redirection path
			}
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.BookingDetailsPage;
		}
	}
}
