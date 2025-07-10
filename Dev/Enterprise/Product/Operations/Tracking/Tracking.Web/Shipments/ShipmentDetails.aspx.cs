using System;
using System.Collections.Generic;
using System.Web;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Web.Declaration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.Tracking.Web
{
	/// <summary>
	/// Summary description for ShipmentDetails.
	/// </summary>
	public partial class ShipmentDetails : BasePageWithAuthorisation
	{
		#region DataSource

		protected override BusinessObject GetNewDataSource()
		{
			return TrackingShipment.FromPKFilteredBySiteUser(Factory, GetGuidFromParameter("Ref"), SiteUser);
		}

		public TrackingShipment Shipment
		{
			get { return DataSource as TrackingShipment; }
		}

		#endregion

		protected override bool CanAccessAuthorisedContent
		{
			get { return SiteUser.CanViewShipments; }
		}

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			DisabledSubmitButtons.Add(SaveConfirmations.ClientID);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			bool isDataSourceValid = (Shipment != null);

			AuthorisedContent.Visible = isDataSourceValid && CanAccessAuthorisedContent;
			NotFoundError.Visible = !isDataSourceValid;

			if (!isDataSourceValid)
			{
				ShipmentNotFoundLabel.Text = Res.GetString("a9d64813-cdbc-44f6-9bf2-53f25ea9a477", "Shipment was not found in the database or you don't have rights to view it.");
			}
			else
			{
				if (Shipment.JS_IsCancelled)
				{
					AuthorisedContent.Controls.AddAt(0, new ZTextLabel
					{
						CssClass = "SectionTitle",
						Text = Res.GetString("ca5505ed-c18b-4586-87fa-6afcf975ee06", "This shipment has been canceled"),
						ForeColor = System.Drawing.Color.Red
					});
				}

				if (SiteUser.IsShipmentQuickViewUser)
				{
					ForAuthentifiedUserOnly.Visible = false;
					ForAuthentifiedUserOnly2.Visible = false;
					ForAuthentifiedUserOnly3.Visible = false;
					ForAuthentifiedUserOnly4.Visible = false;

					DuplicateShipment.Visible = false;
					ReverseShipment.Visible = false;

					LocalChargesGrid.Visible = LocalChargesVisible;
					ChargesGrid.Visible = LocalChargesVisible;

					AdditionalTermsRow.Visible = false;
					PayTermsRow.Visible = false;

					if (LocalChargesVisible)
					{
						ChargesGrid.ShouldShowControl = false;
						LocalChargesGrid.ShouldShowControl = false;
					}

					TransportGrid.ShouldShowControl = false;
					PackLinesGrid.ShouldShowControl = false;
					OrdersGrid.ShouldShowControl = false;
					ContainerGrid.ShouldShowControl = false;
					ReferenceDataGrid.ShouldShowControl = false;
					CustomsEntriesDataGrid.ShouldShowControl = false;

					if (Shipment.RelatedShipments.Count > 0)
					{
						RelatedShipments.ShouldShowControl = false;
					}
				}
				else
				{
					PayTermsRow.Visible = true;
					PayTermLabel.Text = Shipment.IsDomesticFreight ? Res.GetString("8eae27fd-5dc9-480f-b168-6a23fab540df", "Payment Term:") : Res.GetString("3ae4edad-6a5b-a194-491d-0bbca41ab916", "Incoterm:");
					AdditionalTermsRow.Visible = !Shipment.IsDomesticFreight;

					LoadingMetersRow.Visible = Shipment.IsRoadLoadingMetersEnabled;
				}

				bool hasMasterShipment = !Shipment.JS_JS_ColoadMasterShipment.IsEmpty;
				if (hasMasterShipment)
				{
					MasterLink.NavigateUrl = this.UrlFormatWithAppRoot(TrackingConstants.RelativePath.ShipmentPage) + String.Format((NoResString)"?Ref={0}", Shipment.JS_JS_ColoadMasterShipment.ToString()); // Partial URL
				}
				MasterShipmentArea.Visible = hasMasterShipment;
				RelatedShipments.Visible = Shipment.RelatedShipments.Count > 0;

				SetupPage();
			}

			NotificationFlags.DisplayAll = false;
		}

		protected override void SetupAuthorisedContent(bool isAuthorised)
		{
			base.SetupAuthorisedContent(isAuthorised);

			var bookingButtonsEnabled = isAuthorised && (SiteUser?.CanEditBookings ?? false);
			DuplicateShipment.Enabled = bookingButtonsEnabled;
			ReverseShipment.Enabled = bookingButtonsEnabled;

			if (!bookingButtonsEnabled)
			{
				var toolTip = Res.GetString("3ffd0075-e11e-4623-8ac1-8ed18c704f91", "You are not authorized to use this function. Please contact your system administrator to request access rights.");
				DuplicateShipment.ToolTip = toolTip;
				ReverseShipment.ToolTip = toolTip;
			}
		}

		#region Setting up Status Control

		protected virtual BaseStatusControl LoadStatusControl(string statusControlPath)
		{
			return (BaseStatusControl)Page.LoadControl(statusControlPath);
		}

		BaseStatusControl LoadStatusControl(string countryCode, string direction)
		{
			var statusControlPath = GetStatusControlPath(countryCode, direction);

			try
			{
				return LoadStatusControl(statusControlPath);
			}
			catch (Exception e) when (!e.IsCriticalException()) { }

			return null;
		}

		protected virtual bool SetUpStatusControl(string countryCode, string direction, string bindTo = null)
		{
			var statusControl = LoadStatusControl(countryCode, direction);
			if (statusControl != null)
			{
				statusControl.BindTo = !string.IsNullOrEmpty(bindTo) ? bindTo : ".";
				StatusHolder.Controls.Add(statusControl);
				StatusHolder.Visible = true;

				return true;
			}

			return false;
		}

		void SetupStatusHolder()
		{
			if (Shipment != null)
			{
				bool isExportControlSetup = false;
				var firstExportDeclaration = Shipment.FirstExportDeclaration;
				if (firstExportDeclaration != null && firstExportDeclaration.Branch != null && firstExportDeclaration.Branch.Country != null)
				{
					isExportControlSetup = SetUpStatusControl(firstExportDeclaration.Branch.Country.Code, JobMessageTypeList.Codes.Export, "FirstExportDeclaration");
				}

				if (!isExportControlSetup)
				{
					if (Shipment.Origin != null && Shipment.Origin.Country != null)
					{
						SetUpStatusControl(Shipment.Origin.Country.Code, JobMessageTypeList.Codes.Export);
					}
				}

				bool isImportControlSetup = false;
				var lastImportDeclaration = Shipment.LastImportDeclaration;
				if (lastImportDeclaration != null && lastImportDeclaration.Branch != null && lastImportDeclaration.Branch.Country != null)
				{
					isImportControlSetup = SetUpStatusControl(lastImportDeclaration.Branch.Country.Code, JobMessageTypeList.Codes.Import, "LastImportDeclaration");
				}

				if (!isImportControlSetup)
				{
					if (Shipment.Destination != null && Shipment.Destination.Country != null)
					{
						SetUpStatusControl(Shipment.Destination.Country.Code, JobMessageTypeList.Codes.Import);
					}
				}
			}
		}

		#endregion

		#region Setting up grids

		void SetupPage()
		{
			CodeDescriptionBoolCollection elements = (CodeDescriptionBoolCollection)WebDataRegistry.Instance.ShipmentPageCustomisation.Value;
			ETARow.Visible = !elements.GetBoolFromCode(WebDataRegistry.ShipmentDetailsPageElements.EtdAndEta);
			ETDRow.Visible = !elements.GetBoolFromCode(WebDataRegistry.ShipmentDetailsPageElements.EtdAndEta);
			StorageCommencesRow.Visible = !elements.GetBoolFromCode(WebDataRegistry.ShipmentDetailsPageElements.StorageCommencesDate);
			StorageCommencesParallelRow.Visible = StorageCommencesRow.Visible;
			CartageAdvisedRow.Visible = !elements.GetBoolFromCode(WebDataRegistry.ShipmentDetailsPageElements.CartageAdvisedDate);
			PickupCartageAdvisedRow.Visible = !elements.GetBoolFromCode(WebDataRegistry.ShipmentDetailsPageElements.CartageAdvisedDate);
			TransportGrid.Visible = !elements.GetBoolFromCode(WebDataRegistry.ShipmentDetailsPageElements.TransportGrid);
			PackLinesGrid.Visible = !elements.GetBoolFromCode(WebDataRegistry.ShipmentDetailsPageElements.GoodsPacksGrid);
			OrdersGrid.Visible = !elements.GetBoolFromCode(WebDataRegistry.ShipmentDetailsPageElements.OrdersGrid) && (SiteUser?.CanViewOrders ?? false);
			ContainerGrid.Visible = !elements.GetBoolFromCode(WebDataRegistry.ShipmentDetailsPageElements.ContainersGrid);
			DocumentsGrid.Visible = SiteUser?.CanViewDocuments ?? false;
		}

		protected override void SetupGrids()
		{
			RelatedShipments.ColumnProvider = new TrackingShipmentColumnProvider();
			TransportGrid.ColumnProvider = new TrackingTransportDetailsGridColumnProvider();
			OrdersGrid.ColumnProvider = new TrackingOrderColumnProvider(false);
			ContainerGrid.ColumnProvider = new TrackingContainerDetailsColumnProvider();
			SetupDeliveryGrid();
			SetupReferenceDataGrid();
		}

		protected void SetupReferenceDataGrid()
		{
			ReferenceDataGrid.ColumnProvider = new ForwardingBookingReferenceColumnProvider();
		}

		protected override string GetPageName()
		{
			return WebTracker.Pages.ShipmentDetails;
		}

		#region Application path and formatiing of Urls

		protected string UrlFormatWithAppRoot(string page)
		{
			return AppPath + page;
		}

		protected string AppPath
		{
			get
			{
#if DEBUG
				if (Enterprise.ZArchitecture.Environment.Globals.IsTest)
				{
					return "/";
				}
				else
#endif
				{
					string result = System.Web.HttpContext.Current.Request.ApplicationPath;
					if (result != "/")
					{
						result += "/";
					}

					return result;
				}
			}
		}

		#endregion

		protected override void OnPreBind()
		{
			base.OnPreBind();
			if (Shipment == null)
			{
				ShipmentNotFoundLabel.Text = Res.GetString("a9d64813-cdbc-44f6-9bf2-53f25ea9a477", "Shipment was not found in the database or you don't have rights to view it.");
			}
			else
			{
				OriginRow.Visible = !Shipment.IsDomesticFreight;
				DestinationRow.Visible = !Shipment.IsDomesticFreight;
				CustomsEntriesDataGrid.Visible = !Shipment.IsDomesticFreight;
				ETD.DateTimeFormat = Shipment.IsSea ? ZDateTimePickerFormat.Short : ZDateTimePickerFormat.Long;
				ETA.DateTimeFormat = ETD.DateTimeFormat;
			}
			//these grids require DataSource to be loaded before grid's construction.
			//Data source have to be loaded after ViewState's construction
			SetupDocumentsGrid(DocumentsGrid);
			SetupChargesGrid();
			SetupCustomsEntriesDataGrid();
			SetupStatusHolder();
			SetupPackLinesGrid(PackLinesGrid, true);
		}

		public new TrackingSiteUser SiteUser
		{
			get { return WebEnv.AppInstance.SiteUser as TrackingSiteUser; }
		}

		#region Delivery Grid

		protected void SetupDeliveryGrid()
		{
			DeliveryGrid.Columns.Clear();

			ZBindToChecker.CheckBindTo((CommonPickupDeliveryConfirm)((TrackingShipment)null).DeliveryConfirms[0]);

			ZBindToChecker.CheckBindTo((ZInt)((CommonPickupDeliveryConfirm)null).TotalDeliveredPackages);
			DeliveryGrid.Columns.Add(new ZTextEditColumn(Res.GetString("57dec0b1-cce0-4375-b88d-01955954d464", "Pieces Delivered"), CommonPickupDeliveryConfirm.Schema.TotalDeliveredPackages) { ReadOnly = true });

			ZBindToChecker.CheckBindTo((ZDateTime)((CommonPickupDeliveryConfirm)null).EU_RequestedPickupDeliveryTime);
			DeliveryGrid.Columns.Add(new ZDateTimeColumn(Res.GetString("18e0e5d6-cc28-4cd8-90ec-bb1b213fa715", "Requested By"), CommonPickupDeliveryConfirm.Schema.EU_RequestedPickupDeliveryTime, ZDateTimePickerFormat.Long));

			ZBindToChecker.CheckBindTo((CodeDescriptionPairList)((CommonPickupDeliveryConfirm)null).Lookups.DropModes);
			ZBindToChecker.CheckBindTo((ZString)((CommonPickupDeliveryConfirm)null).EU_DropMode);
			DeliveryGrid.Columns.Add(new ZDropDownListColumn(Res.GetString("203cd34f-6675-4169-8642-6c893ed6d68b", "Drop Mode"), CommonPickupDeliveryConfirm.Schema.EU_DropMode, "Lookups+DropModes"));

			ZBindToChecker.CheckBindTo((ZString)((CommonPickupDeliveryConfirm)null).EU_PickupDeliveryInstruction);
			DeliveryGrid.Columns.Add(new ZTextEditColumn(Res.GetString("0b9a4b00-b2d8-4b19-9439-8aeb5833a9e3", "Notes"), CommonPickupDeliveryConfirm.Schema.EU_PickupDeliveryInstruction));

			ZBindToChecker.CheckBindTo((ZDateTime)((CommonPickupDeliveryConfirm)null).EU_PickupDeliveryTime);
			DeliveryGrid.Columns.Add(new ZDateTimeColumn(Res.GetString("5d36bd63-bad2-4fcf-8e36-1f26cec9e8d6", "Actual Delivery"), CommonPickupDeliveryConfirm.Schema.EU_PickupDeliveryTime, ZDateTimePickerFormat.Long));

			ZBindToChecker.CheckBindTo((ZString)((CommonPickupDeliveryConfirm)null).EU_GoodsSignForBy);
			DeliveryGrid.Columns.Add(new ZTextEditColumn(Res.GetString("452b971c-a512-4d56-bce0-4153c09f2dfb", "Received By"), CommonPickupDeliveryConfirm.Schema.EU_GoodsSignForBy));

			ZBindToChecker.CheckBindTo((CommonPickupDeliveryConfirmDivotCollection)((CommonPickupDeliveryConfirm)null).Divots);
			ZNewRowColumn linesColumn = new ZNewRowColumn((NoResString)"Divots"); // Binding Name
			linesColumn.ItemTemplate = new ShipmentDeliveryDivotTemplate(linesColumn);
			DeliveryGrid.Columns.Add(linesColumn);
			DeliveryGrid.DataBinding += DeliveryGrid_DataBinding;
		}

		protected void DeliveryGrid_DataBinding(object sender, EventArgs e)
		{
			DeliveryGrid.Visible = Shipment != null && !Shipment.IsContainerised && Shipment.TotalOuterPacks > 0;
			if (!DeliveryGrid.Visible)
			{
				return;
			}

			bool gridReadOnly = false;
			bool gridAllowAdd = false;
			bool gridAllowEdit = false;
			bool gridAllowDelete = false;

			SaveConfirmations.Visible = false;

			if (!SiteUser.IsShipmentQuickViewUser && (SiteUser.CanAddDeliveryRequest || SiteUser.CanEditDeliveryRequest))
			{
				gridAllowEdit = SiteUser.CanEditDeliveryRequest;

				bool noLegsExistInDB = true;
				bool nothingToSave = true;
				foreach (CommonPickupDeliveryConfirm confirm in Shipment.DeliveryConfirms)
				{
					if (confirm.IsInDatabase)
					{
						noLegsExistInDB = false;
					}

					if (!confirm.ReadOnly)
					{
						nothingToSave = false;
					}
					confirm.Divots.SetReadOnlyIncludingChildren(confirm.DivotsAreReadOnlyOnWeb);
				}

				if (noLegsExistInDB)
				{
					if (Shipment.DeliveryConfirms.Count > 0)
					{
						SaveConfirmationsText = saveDeliveryRequests;
					}
					gridAllowAdd = gridAllowDelete = SiteUser.CanAddDeliveryRequest;
				}

				SaveConfirmationsText = string.IsNullOrEmpty(SaveConfirmationsText) ? editDeliveryRequests : SaveConfirmationsText;
				SaveConfirmations.Visible = !nothingToSave;

				if (nothingToSave || SaveConfirmationsText != saveDeliveryRequests)
				{
					gridAllowEdit = false;
				}

				if (Shipment.DeliveryConfirms.Count == 0)
				{
					gridAllowEdit = gridAllowAdd;
				}
			}

			if (!gridAllowEdit)
			{
				foreach (CommonPickupDeliveryConfirm confirm in Shipment.DeliveryConfirms)
				{
					confirm.Divots.SetReadOnlyIncludingChildren(true);
				}
			}

			DeliveryGrid.ReadOnly = gridReadOnly;
			DeliveryGrid.AllowAdd = gridAllowAdd;
			DeliveryGrid.AllowDelete = gridAllowDelete;
			DeliveryGrid.AllowEdit = gridAllowEdit;
		}

		string SaveConfirmationsText
		{
			get
			{
				return ViewState["SaveConfirmationsText"] == null ? null : ViewState["SaveConfirmationsText"].ToString();
			}
			set
			{
				ViewState["SaveConfirmationsText"] = value;
				SaveConfirmations.Text = value;
			}
		}

		protected static string saveDeliveryRequests
		{
			get { return Res.GetString("cd4ee14b-5548-408b-b6f8-f7b2ea10af4f", "Save Delivery Requests"); }
		}
		protected static string editDeliveryRequests
		{
			get { return Res.GetString("e926ea7d-422d-48a3-9392-06bd39b43e3c", "Edit Delivery Requests"); }
		}

		protected void SaveConfirmations_Click(object sender, EventArgs e)
		{
			if (SaveConfirmationsText == editDeliveryRequests)
			{
				SaveConfirmationsText = saveDeliveryRequests;
			}
			else
			{
				Shipment.EnableOnlyDeliveryConfirmationsValidationOnPreSave();
				NotificationFlags.DisplayAll = true;
				SuppressErrorDialog = false;
				SaveDataSourceFactory();

				if (!Shipment.HasErrors)
				{
					SaveConfirmationsText = editDeliveryRequests;
					NotificationFlags.DisplayAll = false;
				}
			}
			DeliveryGrid.DataBind();
		}

		#endregion Delivery Grid

		#region Charges Grid

		void SetupChargesGrid()
		{
			if (((TrackingSiteUser)SiteUser).CanViewAccounts || ((TrackingSiteUser)SiteUser).IsShipmentQuickViewUser)
			{
				ChargesGrid.Visible = true;
				InvoicePresenter presenter = new InvoicePresenter(SiteUser);
				presenter.SetupGrid(ChargesGrid);

				if (LocalChargesVisible)
				{
					presenter.SetupLocalChargesGrid(LocalChargesGrid);
				}
			}
			else
			{
				ChargesGrid.Visible = false;
			}
		}

		bool LocalChargesVisible
		{
			get
			{
				return SiteUser.IsShipmentQuickViewUser &&
					   WebDataRegistry.Instance.WebTrackerLocalChargesOnShipmentQuickView.Value &&
					   Shipment.InvoiceLoader.LocalChargesDetails != null &&
					   Shipment.InvoiceLoader.LocalChargesDetails.Count > 0;
			}
		}

		#endregion Charges Grid

		#region CustomsEntriesDataGrid

		public void SetupCustomsEntriesDataGrid()
		{
			CustomsEntriesDataGrid.ColumnProvider = new ForwardingShipmentCustomsEntriesDataColumnProvider(Shipment);
		}

		#endregion

		#endregion

		protected internal override IReadOnlyCollection<ILicenceCheckpoint> LicenceCheckPoints
		{
			get
			{
				List<ILicenceCheckpoint> result = new List<ILicenceCheckpoint>();
				result.Add(Environment.Env.Licence.WebTrackerForwarding);
				if (Shipment != null && Shipment.LastDeclaration != null)
				{
					result.Add(Shipment.LastDeclaration.IsImport ?
						Environment.Env.Licence.WebTrackerImportBrokerage :
						Environment.Env.Licence.WebTrackerExportBrokerage);
				}
				return result.ToArray();
			}
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.ShipmentDetailsPage;
		}

		protected void DuplicateShipment_Click(object sender, EventArgs e)
		{
			if (SiteUser?.CanEditBookings ?? false)
			{
				var newBooking = CopyShipment(Shipment);
				if (newBooking != null)
				{
					UpdateAutoCreatedLogReferenceAndSaveDataSource(newBooking.BookingPK, newBooking);
					HttpContext.Current.Response.Redirect(string.Format((NoResString)"{0}?Ref={1}&{2}={3}", AppInstance.EditBookingPage, newBooking.BookingPK, ZPage.DataSourceInSessionParameterName, ZPage.DataSourceInSessionParameterValue)); // Redirection path
				}
			}
		}

		protected void ReverseShipment_Click(object sender, EventArgs e)
		{
			if (SiteUser?.CanEditBookings ?? false)
			{
				var newBooking = CopyShipment(Shipment);
				if (newBooking != null)
				{
					((ITemplateReversible)newBooking).Reverse();
					UpdateAutoCreatedLogReferenceAndSaveDataSource(newBooking.BookingPK, newBooking);
					HttpContext.Current.Response.Redirect(string.Format((NoResString)"{0}?Ref={1}&{2}={3}", AppInstance.EditBookingPage, newBooking.BookingPK, ZPage.DataSourceInSessionParameterName, ZPage.DataSourceInSessionParameterValue)); // Redirection path
				}
			}
		}

		protected virtual TrackingBooking CopyShipment(TrackingShipment shipment)
		{
			if (shipment != null)
			{
				TrackingShipment newShipment = ((ITemplateCopyable)shipment).TemplateCopy() as TrackingShipment;
				if (newShipment != null)
				{
					return new TrackingBooking(newShipment.PK, newShipment.Factory, SiteUser);
				}
			}
			return null;
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
			ShipmentDetailsLabel.BindTo = null;
			Ztextlabel5.BindTo = "JS_UniqueConsignRef";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingShipment)(null)).JS_UniqueConsignRef)));
			HouseBill.BindTo = "JS_HouseBill";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingShipment)(null)).JS_HouseBill)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingShipment)(null)).JS_UniqueConsignRef)));
			ClientShipperRefData.BindTo = "BookingReference";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingShipment)(null)).BookingReference)));
			ClientOwnerRefData.BindTo = "OwnerReference";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingShipment)(null)).OwnerReference)));
			Ztextlabel1.BindTo = "OrderReference";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingShipment)(null)).OrderReference)));
			GoodsDescription.BindTo = "JS_GoodsDescription";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingShipment)(null)).JS_GoodsDescription)));
			ServiceLevelDescription.BindTo = "JS_RS_NKServiceLevel";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingShipment)(null)).JS_RS_NKServiceLevel)));
			PlannedVolumeLabel.BindTo = "VolumeWithUnits";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingShipment)(null)).VolumeWithUnits)));
			PlannedWeightLabel.BindTo = "WeightWithUnits";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingShipment)(null)).WeightWithUnits)));
			OuterPackCount.BindTo = "JS_OuterPacks";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((INumericZType)(((TrackingShipment)(null)).JS_OuterPacks)));
			PackType.BindTo = "JS_F3_NKPackType";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingShipment)(null)).JS_F3_NKPackType)));
			Origin.BindTo = "JS_RL_NKOrigin";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingShipment)(null)).JS_RL_NKOrigin)));
			Destination.BindTo = "JS_RL_NKDestination";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingShipment)(null)).JS_RL_NKDestination)));
			ETD.BindTo = "ETDWithSuppression";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingShipment)(null)).ETDWithSuppression)));
			ETA.BindTo = "ETAWithSuppression";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingShipment)(null)).ETAWithSuppression)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingShipment)(null)).JS_E_DEP)));
			Ztextlabel3.BindTo = "ConsignorPickupAddress.AddressAsASingleLine";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingShipment)(null)).ConsignorPickupAddress.AddressAsASingleLine)));
			Ztextlabel4.BindTo = "ConsigneeDeliveryAddress.AddressAsASingleLine";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((string)(((TrackingShipment)(null)).ConsigneeDeliveryAddress.AddressAsASingleLine)));
			Zdatetimelabel4.BindTo = "AvailableDate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingShipment)(null)).AvailableDate)));
			Zdatetimelabel5.BindTo = "StorageDate";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingShipment)(null)).StorageDate)));
			Zdatetimelabel1.BindTo = "DocsAndCartage.JP_EstimatedPickup";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingShipment)(null)).DocsAndCartage.JP_EstimatedPickup)));
			Zdatetimelabel6.BindTo = "DocsAndCartage.JP_EstimatedDelivery";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingShipment)(null)).DocsAndCartage.JP_EstimatedDelivery)));
			Zdatetimelabel2.BindTo = "DocsAndCartage.JP_PickupRequiredBy";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingShipment)(null)).DocsAndCartage.JP_PickupRequiredBy)));
			Zdatetimelabel7.BindTo = "DocsAndCartage.JP_DeliveryRequiredBy";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingShipment)(null)).DocsAndCartage.JP_DeliveryRequiredBy)));
			Zdatetimelabel9.BindTo = "DocsAndCartage.JP_PickupCartageAdvised";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingShipment)(null)).DocsAndCartage.JP_PickupCartageAdvised)));
			Zdatetimelabel8.BindTo = "DocsAndCartage.JP_DeliveryCartageAdvised";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingShipment)(null)).DocsAndCartage.JP_DeliveryCartageAdvised)));
			Zdatetimelabel3.BindTo = "DocsAndCartage.JP_PickupCartageCompleted";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingShipment)(null)).DocsAndCartage.JP_PickupCartageCompleted)));
			ReceivedDate.BindTo = "DocsAndCartage.JP_DeliveryCartageCompleted";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingShipment)(null)).DocsAndCartage.JP_DeliveryCartageCompleted)));
			ShipmentNotFoundLabel.BindTo = null;
			this.DataSourceAssemblyName = "Enterprise.Tracking.Business";
			this.DataSourceTypeName = "Enterprise.Tracking.Business.TrackingShipment";
		}
		#endregion

		#endregion
	}
}
