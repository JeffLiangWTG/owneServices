using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.Bookings
{
	public partial class EditBooking : BasePageWithAuthorisation
	{
		protected override void OnLoad(EventArgs e)
		{
			RedirectToTermsAndConditionsIfNecessary();
			EnsureChildControls();
			if (e == null)
			{
				throw new ArgumentNullException(nameof(e));
			}
			base.OnLoad(e);

			if (Booking == null)
			{
				NoBookingDiv.Visible = true;
				NoBookingLabel.Text = Res.GetString("B755A362-08AC-4A1C-A8EA-C81B5A35FFC1", "The booking does not exist.");
				AuthorisedContent.Visible = false;
				return;
			}

			ScheduleChooser.Visible = SiteUser != null && SiteUser.IsLoggedIn && SiteUser.CanBookSailings;

			NotificationFlags.DisplayAll = false;

			SetupTransportModeHandler();
			if (FCL == null)
			{
				FCL = new HtmlGenericControl();
			}
			bool prevFCLVisible = FCL.Visible;
			FCL.Visible = IsFCL && ShowContainersGrid;
			if (!prevFCLVisible && FCL.Visible)
			{
				ContainersDataGrid.ShowFooter = true;
			}
			notFCL.Visible = !IsFCL;
			PackLines.Visible = ShowGoodsPacksGrid;

			SetOriginDestination();

			WarehouseRecRow.Visible = !IsDomesticBooking;
			CustomsEntryRow.Visible = !IsDomesticBooking;
			InsuranceValueRow.Visible = !WebDataRegistry.Instance.DisableBookingInsuranceValue.Value;

			ThirdPartyAddress.ModuleID = WebModuleIDs.OrgReceivablesTracking;
			PanelThirdParyAddress.RenderCaption = !Booking.IsDomesticFreight && Booking.ThirdPartyAddressPK.IsEmpty;
			PanelThirdParyAddress.Visible =
							(Booking.IsDomesticFreight && Booking.INCO == Constants.DomesticPaymentTerms.CollectThirdParty ||
							!Booking.IsDomesticFreight);
			PanelThirdParyAddress.CollapsedButtonCaption = Res.GetString("ff13e5b5-3457-4031-83c6-4ad5244bd57b", "Enter Requested Billing Party");

			AdditionalTermsRow.Visible = !IsDomesticBooking;
			PayTermLabel.Text = IsDomesticBooking ? Res.GetString("41ad9052-81d4-4eca-931e-ce6267a72c3d", "Payment Term:") : Res.GetString("ebce9049-837a-abb0-4c5b-bb73eef92a17", "Incoterm:");

			AttachedOrdersGrid.Visible = SiteUser?.CanViewOrders ?? false;

			if (!Globals.IsTest)
			{
				LinkedOrders.Value = "";
				if (Booking.AttachedOrders.Count > 0)
				{
					foreach (Order order in Booking.AttachedOrders)
					{
						if (!LinkedOrders.Value.Contains(order.JD_OrderNumberAndSplit))
						{
							LinkedOrders.Value += "," + order.JD_OrderNumberAndSplit;// for test
						}
					}
				}
			}
		}

		bool IsFCL
		{
			get { return Booking.IsFCL; }
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
			get { return SiteUser.CanEditBookings; }
		}

		#region DataSource

		protected override bool IsPersistDataSourceBetweenPostbacks
		{
			get { return true; }
		}

		protected ZGuid BookingPK
		{
			get { return GetGuidFromParameter("Ref"); }
		}

		protected override string GetPageRelativePath()
		{
			return TrackingConstants.RelativePath.EditBookingPage;
		}

		protected override BusinessObject GetNewDataSource()
		{
			TrackingBooking result = null;

			if (BookingPK.IsValid)
			{
				ForwardingShipment booking = Factory.Load<ForwardingShipment>(BookingPK);
				if (booking != null)
				{
					result = new TrackingBooking(booking.PK, Factory, SiteUser);
					if (booking.ServiceLevel != null)
					{
						result.TemporaryServiceLevelPK = booking.ServiceLevel.PK;
					}
				}
			}
			else
			{
				result = new TrackingBooking(Factory, SiteUser);
				if (SiteUser.IsLoggedIn && result.BookingPartyDocumentaryAddress != null && SiteUser.LoggedInOrganisation != null)
				{
					result.BookingPartyDocumentaryAddress.OrganisationNameOrPK = SiteUser.LoggedInOrganisation.PK.ToString();
				}

				// Assigning Imperial Units for US Booking
				if (IsCurrentBranchLocatedInUS)
				{
					result.UnitOfVolume = "CF";
					result.UnitOfWeight = "LB";
				}

				SetDefaultMode(result);
				SetDefaultIsDomesticBooking(result);
			}

			if (result != null)
			{
				result.LoggedInContact = SiteUser.LoggedInUser;
				result.Logs.AutoCreatedLogDefaultSL_Reference = SiteUser.ContactAndCompanyReference;
			}
			return result;
		}

		protected override string GetSaveButtonClientID()
		{
			return MakeBooking.ClientID;
		}

		protected TrackingBooking Booking
		{
			get { return DataSource as TrackingBooking; }
		}

		protected override void OnDataSourceFactorySaved()
		{
			base.OnDataSourceFactorySaved();
			SaveDefaultMode();
			SaveDefaultIsDomesticBooking();
			SaveDefaultSettingsForDocAddress();
			RedirectToBookingDetailsPage();
		}

		bool IsCurrentBranchLocatedInUS
		{
			get
			{
				return !string.IsNullOrEmpty(Env.CurrentBranch.NKUNLOCO) &&
					Env.CurrentBranch.NKUNLOCO.ToUpper().StartsWith("US");
			}
		}

		bool IsDomesticBooking
		{
			get { return Booking.IsDomesticFreight; }
		}

		#endregion

		#region Page Confirmations

		protected override void AddPageConfirmations(List<ZPageConfirmation> confirmations)
		{
			base.AddPageConfirmations(confirmations);
			confirmations.Add(new LinkPackLinesConfirmations(this));
			confirmations.Add(new BookingTotalPacksConfirmation(this));
			confirmations.Add(new BookingNextSailingConfirmation(this) { IsSaveConfirmation = false });
		}

		#endregion

		#region Redirect to Terms and Conditions

		protected void RedirectToTermsAndConditionsIfNecessary()
		{
			if (!BookingPK.IsValid && !IsPostBack)
			{
				if (Session[Global.BookingTermsAndConditionsIndexer] == null &&
					!string.IsNullOrEmpty(WebDataRegistry.Instance.BookingTermsAndConditions.Value))
				{
					string redirectUrl = string.Format("{0}?{1}={2}", AppInstance.TermsAndConditionsPage, Global.TermsAndConditionsRedirectUrlTag, AppInstance.EditBookingPage);
					HttpContext.Current.Response.Redirect(redirectUrl);
				}
				else
				{
					Session[Global.BookingTermsAndConditionsIndexer] = null;
				}
			}
		}

		#endregion

		#region Default Mode

		protected void SetDefaultMode(TrackingBooking webBooking)
		{
			string transportMode = GetDefaultSetting(DefaultModeKey);

			if (webBooking != null && !string.IsNullOrEmpty(transportMode))
			{
				webBooking.Mode = transportMode;
			}
			else if (IsCurrentBranchLocatedInUS)
			{
				webBooking.Mode = Enterprise.Core.Constants.TransportModes.Road;
			}
		}

		protected void SaveDefaultMode()
		{
			SaveDefaultSetting(DefaultModeKey, Booking.Mode.ToString());
		}

		protected const string DefaultModeKey = "DefaultMode";

		protected GridLayoutRegistry DefaultSettingRegistry
		{
			get
			{
				if (fDefaultSettingRegistry == null)
				{
					fDefaultSettingRegistry = new GridLayoutRegistry();
				}

				return fDefaultSettingRegistry;
			}
		}
		GridLayoutRegistry fDefaultSettingRegistry;

		#endregion

		#region Default Is Domestic Booking

		protected void SetDefaultIsDomesticBooking(TrackingBooking booking)
		{
			string isDomesticBooking = GetDefaultSetting(DefaultIsDomesticBookingKey);

			if (booking != null && !string.IsNullOrEmpty(isDomesticBooking) && isDomesticBooking.ToUpper() == "Y")
			{
				booking.IsDomesticFreight = true;
			}
		}

		protected void SaveDefaultIsDomesticBooking()
		{
			SaveDefaultSetting(DefaultIsDomesticBookingKey, Booking.IsDomesticFreight ? "Y" : string.Empty);
		}

		protected const string DefaultIsDomesticBookingKey = "DefaultIsDomesticBooking";

		protected void SetOriginDestination()
		{
		}

		#endregion

		#region Default Setting's Implementation

		string GetDefaultSetting(string settingKey)
		{
			string result = string.Empty;

			if (Session != null && WebEnv.CurrentUser != null)
			{
				result = Session[settingKey] as string;

				if (result == null && DefaultSettingRegistry != null)
				{
					using (MemoryStream settingStream = DefaultSettingRegistry.GetGridLayout(settingKey, WebEnv.CurrentUser.PK.ToGuid()))
					{
						if (settingStream != null)
						{
							using (TextReader settingReader = new StreamReader(settingStream))
							{
								result = settingReader.ReadToEnd();
								Session[DefaultModeKey] = result;
							}
						}
						else
						{
							Session[settingKey] = string.Empty;
						}
					}
				}
			}

			return result;
		}

		void SaveDefaultSetting(string settingKey, string value)
		{
			if (Session != null && WebEnv.CurrentUser != null && Booking != null)
			{
				Session[settingKey] = value;

				using (MemoryStream settingStream = new MemoryStream())
				{
					using (TextWriter settingWriter = new StreamWriter(settingStream))
					{
						settingWriter.Write(value);
					}
					DefaultSettingRegistry.SetGridLayout(settingKey, WebEnv.CurrentUser.PK.ToGuid(), settingStream);
				}
			}
		}

		#endregion

		#region Web Form Designer generated code

		override protected void OnInit(EventArgs e)
		{
			base.OnInit(e);
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			AttachedOrdersGrid.DisplayAdditionalNewRow = WebDataRegistry.Instance.BookingOrderGridsEnableExtraRowMode.Value;
			SetupConsigneeAddressControl();
			SetupConsignorAddressControl();

			PacksCount = new HiddenField();
			PacksCount.ID = "_PacksCount";
			PackLines.Controls.Add(PacksCount);

			LinkedOrders = new HiddenField();
			LinkedOrders.ID = "_LinkedOrders";
			AttachedOrdersDiv.Controls.Add(LinkedOrders);
		}

		protected ZNumericTextBox FirstOuterPackLinePacks
		{
			get
			{
				return GetFirstOuterPackLineControl(JobPackLinesSchema.Constants.JL_PackageCount, PackLinesGrid.Controls) as ZNumericTextBox;
			}
		}

		protected ZDropDownList FirstOuterPackLinePacksUQ
		{
			get
			{
				return GetFirstOuterPackLineControl(JobPackLinesSchema.Constants.JL_F3_NKPackType, PackLinesGrid.Controls) as ZDropDownList;
			}
		}

		protected ZNumericTextBox FirstOuterPackLineWeight
		{
			get
			{
				return GetFirstOuterPackLineControl(JobPackLinesSchema.Constants.JL_ActualWeight, PackLinesGrid.Controls) as ZNumericTextBox;
			}
		}

		protected ZNumericTextBox FirstOuterPackLineVolume
		{
			get
			{
				return GetFirstOuterPackLineControl(JobPackLinesSchema.Constants.JL_ActualVolume, PackLinesGrid.Controls) as ZNumericTextBox;
			}
		}

		protected ZDropDownList FirstOuterPackLineWeightUQ
		{
			get
			{
				return GetFirstOuterPackLineControl(JobPackLinesSchema.Constants.JL_ActualWeightUQ, PackLinesGrid.Controls) as ZDropDownList;
			}
		}

		protected ZDropDownList FirstOuterPackLineVolumeUQ
		{
			get
			{
				return GetFirstOuterPackLineControl(JobPackLinesSchema.Constants.JL_ActualVolumeUQ, PackLinesGrid.Controls) as ZDropDownList;
			}
		}

		protected ISelfBindingWebControl GetFirstOuterPackLineControl(string bindTo, ControlCollection gridControls)
		{
			ISelfBindingWebControl selfBindingGridControl = null;
			foreach (Control gridControl in gridControls)
			{
				if (gridControl is ISelfBindingWebControl)
				{
					selfBindingGridControl = gridControl as ISelfBindingWebControl;
					if (selfBindingGridControl.BindTo == bindTo)
					{
						return selfBindingGridControl;
					}
				}
				else
				{
					selfBindingGridControl = GetFirstOuterPackLineControl(bindTo, gridControl.Controls);
					if (selfBindingGridControl != null)
					{
						return selfBindingGridControl;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>l
		void InitializeComponent()
		{
			TitleLabel.BindTo = null;
			UnauthorisedLabel.BindTo = null;

			ContainerMode.BindTo = TrackingBooking.Schema.Mode;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingBooking)(null)).ModeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingBooking)(null)).Mode)));

			ShippersRefTextBox.BindTo = TrackingBooking.Schema.BookingReference;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingBooking)(null)).BookingReference)));

			DescriptionTextBox.BindTo = TrackingBooking.Schema.GoodsDescription;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingBooking)(null)).GoodsDescription)));

			PacksEdit.BindTo = TrackingBooking.Schema.OuterPacks;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((INumericZType)(((TrackingBooking)(null)).OuterPacks)));

			PackTypeDropDown.BindTo = TrackingBooking.Schema.OuterPacksPackType;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingBooking)(null)).OuterPacksPackTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingBooking)(null)).OuterPacksPackType)));

			ActualWeightEdit.BindTo = TrackingBooking.Schema.ActualWeight;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((INumericZType)(((TrackingBooking)(null)).ActualWeight)));

			ActualWeightUQDropDown.BindTo = TrackingBooking.Schema.UnitOfWeight;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingBooking)(null)).UnitOfWeightInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingBooking)(null)).UnitOfWeight)));

			ActualVolumeEdit.BindTo = TrackingBooking.Schema.ActualVolume;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((INumericZType)(((TrackingBooking)(null)).ActualVolume)));

			ActualVolumeUQDropDown.BindTo = TrackingBooking.Schema.UnitOfVolume;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingBooking)(null)).UnitOfVolumeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingBooking)(null)).UnitOfVolume)));

			WarehouseRecDateEdit.BindTo = TrackingBooking.Schema.A_RCV;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingBooking)(null)).A_RCV)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingBooking)(null)).A_RCVInfo)));

			PickupFromDateEdit.BindTo = TrackingBooking.Schema.EstimatedPickup;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingBooking)(null)).EstimatedPickup)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingBooking)(null)).EstimatedPickupInfo)));

			PickupByDateEdit.BindTo = TrackingBooking.Schema.PickupRequiredBy;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingBooking)(null)).PickupRequiredBy)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingBooking)(null)).PickupRequiredByInfo)));

			PickupEquipmentNeededDropDownList.BindTo = TrackingBooking.Schema.FCLPickupEquipmentNeeded;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingBooking)(null)).FCLPickupEquipmentNeededInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingBooking)(null)).FCLPickupEquipmentNeeded)));

			DeliveryOnDateEdit.BindTo = TrackingBooking.Schema.EstimatedDelivery;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingBooking)(null)).EstimatedDelivery)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingBooking)(null)).EstimatedDeliveryInfo)));

			DeliveryByDateEdit.BindTo = TrackingBooking.Schema.DeliveryRequiredBy;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZDateTime)(((TrackingBooking)(null)).DeliveryRequiredBy)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingBooking)(null)).DeliveryRequiredByInfo)));

			CartageDropModeDropDownList.BindTo = TrackingBooking.Schema.FCLDeliveryEquipmentNeeded;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZPropertyInfo)(((TrackingBooking)(null)).FCLDeliveryEquipmentNeededInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingBooking)(null)).FCLDeliveryEquipmentNeeded)));
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.

			PayTermDropDownList.BindTo = TrackingBooking.Schema.INCO;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingBooking)(null)).INCO)));

			GoodsValueBox.BindTo = TrackingBooking.Schema.GoodsValue;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((INumericZType)(((TrackingBooking)(null)).GoodsValue)));

			ShipperCODAmountBox.BindTo = TrackingBooking.Schema.ShipperCODAmount;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((INumericZType)(((TrackingBooking)(null)).ShipperCODAmount)));

			ShipperCODTypeDropDownList.BindTo = TrackingBooking.Schema.ShipperCODPayMethod;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingBooking)(null)).ShipperCODPayMethod)));

			CustomsEntryNumber.BindTo = TrackingBooking.Schema.CustomsEntryNumber;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingBooking)(null)).CustomsEntryNumber)));

			Ztextbox2.BindTo = TrackingBooking.Schema.MarksAndNumbers;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingBooking)(null)).MarksAndNumbers)));

			OrderReferencesTextBox.BindTo = TrackingBooking.Schema.OrderItemsAsString;
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((ZString)(((TrackingBooking)(null)).OrderItemsAsString)));

			this.DataSourceAssemblyName = "Enterprise.Tracking.Business";
			this.DataSourceTypeName = "Enterprise.Tracking.Business.TrackingBooking";
		}
		#endregion

		#region Transport Handler & Script

		protected void SetupTransportModeHandler()
		{
			bool transportModeIsAir = ContainerMode.SelectedValue.Equals("AIR");
			ContainersDataGrid.Enabled = !transportModeIsAir;
		}

		#endregion Transport Handler & Script

		internal protected override ZTemplateColumn MultipleProductsColumn
		{
			get
			{
				ZNewRowColumn result = new ZNewRowColumn((NoResString)"Products"); // Binding name
				result.ItemTemplate = new MultipleProductsTemplate(result);
				result.HeaderText = Res.GetString("3442bae5-cff7-4ea7-9617-67894c03c6d3", "Multiple Products");
				result.Collapsable = true;
				return result;
			}
		}

		#region Data Grids Setup & Binding

		protected override void SetupGrids()
		{
			base.SetupGrids();

			SetupAttachedOrdersGrid();
			SetupPackLinesGrid();
			SetupContainersGrid();
			SetupReferenceDataGrid();
		}

		protected void SetupPackLinesGrid()
		{
			var packLineGridAddOn = GetNewPackLineGridOn();
			Controls.Add(packLineGridAddOn);
			packLineGridAddOn.Grid = PackLinesGrid;

			var pageSize = (int)WebDataRegistry.Instance.BookingPackLinesPageSize.Value;
			PackLinesGrid.AllowPaging = pageSize > 0;
			if (PackLinesGrid.AllowPaging)
			{
				PackLinesGrid.PageSize = pageSize;
			}
		}

		BookingPackLineGridAddOn GetNewPackLineGridOn() => new BookingPackLineGridAddOn()
		{
			QuantityBindTo = JobPackLinesSchema.Constants.JL_PackageCount,
			LengthBindTo = JobPackLinesSchema.Constants.JL_Length,
			WidthBindTo = JobPackLinesSchema.Constants.JL_Width,
			HeightBindTo = JobPackLinesSchema.Constants.JL_Height,
			PackUDBindTo = JobPackLinesSchema.Constants.JL_UnitOfDimension,
			VolumeBindTo = JobPackLinesSchema.Constants.JL_ActualVolume,
			VolumeUQBindTo = JobPackLinesSchema.Constants.JL_ActualVolumeUQ
		};

		protected void SetupAttachedOrdersGrid()
		{
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((OrderCollection)(((TrackingBookingOrderLink)(null)).AvailableOrders)));
			ZFindBoxColumn orderColumn = new ZFindBoxColumn(Res.GetString("f10153f6-7c21-44ad-b41a-96722d545d2d", "Order Number"), TrackingBookingOrderLink.Schema.OrderPK, "AvailableOrders");
			orderColumn.ModuleID = WebModuleIDs.TrackingOrders;
			orderColumn.AutoPostBack = true;
			orderColumn.DisplayNotifications = true;
			orderColumn.ValueFieldName = TrackingBookingOrderLink.Schema.OrderPK;
			orderColumn.TextFieldName = TrackingBookingOrderLink.Schema.OrderNumber;
			AttachedOrdersGrid.Columns.Add(orderColumn);

			ZDateTimeColumn dateColumn = new ZDateTimeColumn(Res.GetString("bcf25052-1ba8-421a-ade8-01a2965e69b6", "Date"), TrackingBookingOrderLink.Schema.OrderDate) { ReadOnly = true };
			AttachedOrdersGrid.Columns.Add(dateColumn);

			ZTextEditColumn goodsDescriptionColumn = new ZTextEditColumn(Res.GetString("b74c50cf-4ba3-4987-bf7f-857ce99c9eb8", "Goods Description"), TrackingBookingOrderLink.Schema.OrderGoodsDescription) { ReadOnly = true };
			AttachedOrdersGrid.Columns.Add(goodsDescriptionColumn);
		}

		protected void SetupContainersGrid()
		{
			if (ShowContainersGrid)
			{
				ZTextEditColumn numColumn = new ZTextEditColumn(Res.GetString("b9836bfd-ffec-4eaf-9b41-1c9836c17a61", "Container#"), JobContainerSchema.JC_ContainerNum.Name);
				if (SiteUser != null && !SiteUser.CanEditContainerNumbers)
				{
					numColumn.EditItemTemplate = numColumn.GetNewItemTemplate();
				}
				ContainersDataGrid.Columns.Add(numColumn);
				ZGuidDropDownListColumn typeColumn = new ZGuidDropDownListColumn(Res.GetString("e44c4b59-d3d3-407e-969a-3128e4966251", "Type"), JobContainerSchema.JC_RC.Name);
				CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((RefContainerCollection)(((TrackingContainer)(null)).Container_List)));
				typeColumn.BindToList = "Container_List";
				typeColumn.ValueFieldName = "PK";
				typeColumn.TextFieldName = MasterFiles.Business.RefContainer.Schema.RC_Code;
				ContainersDataGrid.Columns.Add(typeColumn);
				ZCalcEditColumn countColumn = new ZCalcEditColumn(Res.GetString("84aa0851-5c0f-49e0-939a-19e7b439c10b", "Count"), JobContainerSchema.JC_ContainerCount.Name);
				countColumn.Decimals = 0;
				ContainersDataGrid.Columns.Add(countColumn);
			}
		}

		protected void SetupReferenceDataGrid()
		{
			ReferenceGrid.AutoGenerateColumns = false;

			ReferenceGrid.Columns.Add(new ZCodeFindBoxColumn(Res.GetString("a342848e-540e-4fc5-a6e9-a47e42f0b924", "Country/Region"), CusEntryNumber.Schema.CE_RN_NKCountryCode)
			{
				ColumnKey = WebTracker.Grids.ReferenceNumbers.Country,
				ValueFieldName = RefCountry.Schema.RN_Code,
				BindToList = "Lookups.Countries",
				ModuleID = WebModuleIDs.RefCountry,
				AutoPostBack = true
			});

			ReferenceGrid.Columns.Add(new ZDropDownListColumn(Res.GetString("7450de51-6577-444b-a790-257dd526e8a8", "Number Type"), CusEntryNumber.Schema.CE_EntryType, "Lookups.AdditionalReferenceNumberTypes") { DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly });
			ReferenceGrid.Columns.Add(new ZTextEditColumn(Res.GetString("af0d229e-18a0-43dc-a4f2-b21bc2e96567", "Number"), CusEntryNumber.Schema.CE_EntryNum) { TextTransform = TextTransformOptions.UpperCase });
			ReferenceGrid.Columns.Add(new ZTextEditColumn(Res.GetString("98611047-b099-49ac-9fc4-0ff63b0d3b1e", "Type Description"), CusEntryNumber.Schema.AdditionalReferenceNumberTypeDescription) { ReadOnly = true });
			ReferenceGrid.Columns.Add(new ZDateTimeColumn(Res.GetString("85dbdb0a-6f94-4662-aeb0-e98e1548d390", "Issue Date"), CusEntryNumber.Schema.CE_IssueDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.ReferenceNumbers.IssueDate });
			ReferenceGrid.Columns.Add(new ZTextEditColumn(Res.GetString("4d9e162b-a423-416d-bf45-aa2f0fcb3d5b", "Information"), CusEntryNumber.Schema.CE_EntryLineReference));
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (Booking != null && Booking.AttachedOrders.Count > 0)
			{
				List<Order> nonLinkedOrders = GetNonLinkedAttachedOrders(Booking.AttachedOrders);
				if (nonLinkedOrders.Count > 0)
				{
					ZPageConfirmation linkPackLinesConfirmations = GetConfirmationByType(typeof(LinkPackLinesConfirmations));

					if (PageConfirmations.Count > 1 && linkPackLinesConfirmations != null && SiteUser.CanViewOrders)
					{
						((LinkPackLinesConfirmations)linkPackLinesConfirmations).Required = true;
						linkPackLinesConfirmations.ResponseHolder.Value = "";

						if (Session["nonLinkedOrders"] != null)
						{
							HttpContext.Current.Session["nonLinkedOrders"] = null;
						}

						HttpContext.Current.Session["nonLinkedOrders"] = nonLinkedOrders;
					}
				}
			}

			base.OnPreRender(e);

			if (Booking != null)
			{
				UnhookBookingEvents();

				OrderReferencesLabelDiv.Visible = (Booking.AttachedOrderLinks.Count == 0);
				OrderReferencesTextBox.Visible = (Booking.AttachedOrderLinks.Count == 0);

				PacksEdit.Attributes.Add((NoResString)"onchange", (NoResString)"UpdateFirstPacksLinePacks(this.value);"); // Javascript segment
				PackTypeDropDown.Attributes.Add((NoResString)"onchange", (NoResString)"UpdateFirstPacksLinePacksUQ(this.value);"); // Javascript segment
				ActualWeightEdit.Attributes.Add((NoResString)"onchange", (NoResString)"UpdateFirstPacksLineWeight(this.value);"); // Javascript segment
				ActualVolumeEdit.Attributes.Add((NoResString)"onchange", (NoResString)"UpdateFirstPacksLineVolume(this.value);"); // Javascript segment
				ActualWeightUQDropDown.Attributes.Add((NoResString)"onchange", (NoResString)"UpdateFirstPacksLineWeightUQ(this.value);"); // Javascript segment
				ActualVolumeUQDropDown.Attributes.Add((NoResString)"onchange", (NoResString)"UpdateFirstPacksLineVolumeUQ(this.value);"); // Javascript segment

				PacksCount.Value = Booking.QuotedBooking.Booking.OuterPackLines.Count.ToString();

				DisabledSubmitButtons.Add(MakeBooking.ClientID);
			}
		}

		List<Order> GetNonLinkedAttachedOrders(OrderCollection orderCollection)
		{
			List<Order> listOrders = new List<Order>();

			for (int i = 0; i < orderCollection.Count; i++)
			{
				if (!LinkedOrders.Value.Contains(orderCollection[i].JD_OrderNumberAndSplit))
				{
					listOrders.Add(orderCollection[i]);
				}
			}
			return listOrders;
		}

		HiddenField PacksCount;
		HiddenField LinkedOrders;

		protected override void OnPreBind()
		{
			LastTimeValueOfIsDomesticBookingUsedForBinding = IsDomesticBooking;
			HookBookingEvents();
			base.OnPreBind();
			SetupPackLinesGrid(PackLinesGrid, false, IsDomesticBooking, useVolumeCalculator: false);

			if (Booking != null && Booking.QuotedBooking != null && Booking.QuotedBooking.Booking != null && !Globals.IsTest)
			{
				ActualWeightEdit.Decimals = DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(Booking.QuotedBooking.Booking, Booking.QuotedBooking.Booking.JS_ActualWeightInfo.PropertyDescriptor);
				ActualVolumeEdit.Decimals = DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(Booking.QuotedBooking.Booking, Booking.QuotedBooking.Booking.JS_ActualVolumeInfo.PropertyDescriptor);
			}
		}
		bool LastTimeValueOfIsDomesticBookingUsedForBinding;

		void HookBookingEvents()
		{
			if (Booking != null)
			{
				UnhookBookingEvents();
				Booking.IsDomesticFreightValueChanged += IsDomesticFreight_Changed;
			}
		}

		void UnhookBookingEvents()
		{
			try
			{
				Booking.IsDomesticFreightValueChanged -= IsDomesticFreight_Changed;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}
		}

		void IsDomesticFreight_Changed(object sender, EventArgs e)
		{
			if (LastTimeValueOfIsDomesticBookingUsedForBinding != IsDomesticBooking)
			{
				Bind();
			}
		}

		#endregion Data Grid Setup

		#region Address Controls setup

		void SetupConsigneeAddressControl()
		{
			ConsigneeAddress.NewOrgRelationType = NewOrgRelationTypes.Buyer;
			//ConsigneeAddress.PostalCodeChanged += new EventHandler(ConsigneeAddress_PostalCodeChanged);
		}

		void SetupConsignorAddressControl()
		{
			ConsignorAddress.NewOrgRelationType = NewOrgRelationTypes.Supplier;
			//ConsignorAddress.PostalCodeChanged += new EventHandler(ConsignorAddress_PostalCodeChanged);
		}

		//        void ConsignorAddress_PostalCodeChanged(object sender, EventArgs e)
		//        {
		//            ZString NewOrigin = Booking.GetClosestPortOrCountryCode(Booking.ConsignorPickupAddress);
		//            if (!(NewOrigin.IsEmpty || Booking.Origin.IsEmpty) && NewOrigin != Booking.Origin)
		//            {
		//                Booking.OriginInfo.AddWarning("Origin has changed from " + Booking.Origin + " to " + NewOrigin);
		//            }
		//            Booking.Origin = NewOrigin;
		//            OriginPort.Bind(Booking);
		//        }
		//
		//        void ConsigneeAddress_PostalCodeChanged(object sender, EventArgs e)
		//        {
		//            ZString NewDestination = Booking.GetClosestPortOrCountryCode(Booking.ConsigneeDeliveryAddress);
		//            if (!(NewDestination.IsEmpty || Booking.Destination.IsEmpty) && NewDestination != Booking.Destination)
		//            {
		//                Booking.DestinationInfo.AddWarning("Destination has changed from " + Booking.Destination + " to " + NewDestination);
		//            }
		//            Booking.Destination = NewDestination;
		//            DestinationPort.Bind(Booking);
		//        }

		#endregion

		#region Controls With Postback Condition Setup

		protected override void SetupPostBackConditionControls()
		{
			base.SetupPostBackConditionControls();

			OriginPort.PostbackCondition = (NoResString)"return portChangeRequiresPostBack();"; // Javascript code
			DestinationPort.PostbackCondition = (NoResString)"return portChangeRequiresPostBack();"; // Javascript code
		}

		#endregion

		#region Scripts

		protected override void RenderPageSpecificScripts()
		{
			base.RenderPageSpecificScripts();

			RenderHelperFunctionsScript();

			RenderPacksGridScript();
		}

		#region PacksGridScript

		const string PacksGridScriptKey = "EditBooking_PacksGridScript";

		void RenderPacksGridScript()
		{
			AddPacksGridScript();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Javascript segment")]
		protected virtual void AddPacksGridScript()
		{
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), PacksGridScriptKey))
			{
				string script = @"<SCRIPT>

						function UpdateFirstPacksLinePacks(Value)
						{ " + (FirstOuterPackLinePacks != null ? @"
							if (AllowFirstPacksLineUpdate())
							{
								$('" + FirstOuterPackLinePacks.ClientID + @"').value=Value;
							}" : @"") + @"
						}

						function UpdateFirstPacksLinePacksUQ(Value)
						{ " + (FirstOuterPackLinePacksUQ != null ? @"
							if (AllowFirstPacksLineUpdate())
							{
								$('" + FirstOuterPackLinePacksUQ.ClientID + @"').value=Value;
							}" : @"") + @"
						}

						function UpdateFirstPacksLineWeight(Value)
						{ " + (FirstOuterPackLineWeight != null ? @"
							if (AllowFirstPacksLineUpdate())
							{
								$('" + FirstOuterPackLineWeight.ClientID + @"').value=Value;
							}" : @"") + @"
						}

						function UpdateFirstPacksLineVolume(Value)
						{ " + (FirstOuterPackLineVolume != null ? @"
							if (AllowFirstPacksLineUpdate())
							{
								$('" + FirstOuterPackLineVolume.ClientID + @"').value=Value;
							}" : @"") + @"
						}

						function UpdateFirstPacksLineWeightUQ(Value)
						{ " + (FirstOuterPackLineWeightUQ != null ? @"
							if (AllowFirstPacksLineUpdate())
							{
								$('" + FirstOuterPackLineWeightUQ.ClientID + @"').value=Value;
							}" : @"") + @"
						}

						function UpdateFirstPacksLineVolumeUQ(Value)
						{ " + (FirstOuterPackLineVolumeUQ != null ? @"
							if (AllowFirstPacksLineUpdate())
							{
								$('" + FirstOuterPackLineVolumeUQ.ClientID + @"').value=Value;
							}" : @"") + @"
						}

						function AllowFirstPacksLineUpdate()
						{
							var PacksCount = $('" + PacksCount.ClientID + @"');
							if (PacksCount)
							{
								try
								{
									if (parseInt(PacksCount.value)==1)
									{
										return true;
									}
								} catch(e) {}
							}
							return false;
						}                        

						</SCRIPT>";
				ZClientScript.RegisterClientScriptBlock(GetType(), PacksGridScriptKey, script);
			}
		}

		#endregion

		#region HelperFunctionsScript

		const string HelperFunctionsScriptKey = "EditBooking_HelperFunctionsScript";

		void RenderHelperFunctionsScript()
		{
			AddHelperFunctionsScript();
		}

		protected virtual void AddHelperFunctionsScript()
		{
			if (!Page.ZClientScript.IsClientScriptBlockRegistered(GetType(), HelperFunctionsScriptKey))
			{
				string script = @"<SCRIPT>

						function getOriginPortValue()
						{
							var originPort = getElement('" + OriginPort.TextBoxControl.ClientID + @"');
							if (originPort)
							{
								return originPort.get('value');
							}
							return '';
						}

						function getDestinationPortValue()
						{
							var destinationPort = getElement('" + DestinationPort.TextBoxControl.ClientID + @"');
							if (destinationPort)
							{
								return destinationPort.get('value');
							}
							return '';
						}

						function getIsDomesticValue()
						{
							var isDomesticCheckBox = getElement('" + IsDomesticCheckBox.ClientID + @"');
							if (isDomesticCheckBox)
							{
								return isDomesticCheckBox.get('checked');
							}
							return false;
						}            

						function calculateIsDomesticFreight()
						{
							return getOriginPortValue().substr(0,2)==getDestinationPortValue().substr(0,2);
						}

						function portChangeRequiresPostBack()
						{
							return calculateIsDomesticFreight()!=getIsDomesticValue();
						}

						</SCRIPT>";

				ZClientScript.RegisterClientScriptBlock(GetType(), HelperFunctionsScriptKey, script);
			}
		}

		#endregion

		#endregion

		#region Saving

		protected void DisableControl(Control parent)
		{
			foreach (Control control in parent.Controls)
			{
				DisableControl(control);
			}
			if (parent is HtmlControl)
			{
				((HtmlControl)parent).Disabled = true;
			}
			else if (parent is WebControl)
			{
				((WebControl)parent).Enabled = false;
			}
		}

#if DEBUG
		public void MakeBooking_Click_ExposedforTesting()
		{
			MakeBooking_Click(ClientID, EventArgs.Empty);
		}
#endif
		protected void MakeBooking_Click(object sender, EventArgs e)
		{
			List<Order> nonLinkedOrders = (List<Order>)Session["nonLinkedOrders"];

			if (DataSource != null && nonLinkedOrders == null)
			{
				SaveDataSourceFactory();
			}
			else
			{
				ZPageConfirmation linkPackLinesConfirmations = GetConfirmationByType(typeof(LinkPackLinesConfirmations));

				if (linkPackLinesConfirmations.HasResponse && linkPackLinesConfirmations.YesButtonClicked)
				{
					if (SiteUser.CanSplitOrders)
					{
						DataSourceIndexer = Booking.PK;
						SaveDataSource(DataSourceIndexer, DataSource);

						var popupUrl = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0}?Ref={1}&{2}={3}", AppInstance.LinesMappingPage, DataSourceIndexer, TrackingConstants.QueryStringKeys.PopupKey, "Y");// Redirect path
						var script = string.Format(CultureInfo.InvariantCulture, "<script type = 'text/javascript'>window.open('{0}', '{1}', '{2}');</script>", popupUrl, "_blank", Global.LinesMappingPopupWindowStyle);// JavaScript Code
						ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "OpenPopupScript", script, false);// Redirect path
					}
					else
					{
						var helper = new OrderLineToPackLineConversionHelper(Factory, Booking.QuotedBooking.Booking, nonLinkedOrders);
						helper.CreatePackLines();
						SuspendValidationErrorMessageForCurrentLoad();
						HttpContext.Current.Session["nonLinkedOrders"] = null;
						Bind();
					}
				}
				else if (linkPackLinesConfirmations.HasResponse && linkPackLinesConfirmations.NoButtonClicked)
				{
					HttpContext.Current.Session["nonLinkedOrders"] = null;
				}
			}
		}

		protected void RedirectToBookingDetailsPage()
		{
			HttpContext.Current.Response.Redirect(string.Format((NoResString)"{0}?Ref={1}", AppInstance.BookingDetailsPage, Booking.BookingPK)); // Redirect path
		}

		#endregion Saving

		#region Default Settings For DocAddress

		protected void SaveDefaultSettingsForDocAddress()
		{
			WebUserDefaultSettingsForDocAddress settings = null;

			if (Booking.ConsigneeDeliveryAddress.OrganisationPK == SiteUser.LoggedInOrganisation.PK)
			{
				settings = new WebUserDefaultSettingsForDocAddress(Booking.ConsigneeDeliveryAddress.OrganisationPK, Booking.ConsigneeDeliveryAddress.E2_OA_Address, Booking.ConsigneeDeliveryAddress.ContactPK, Booking.ConsigneeDeliveryAddress.DocAddressType.ToString());
			}
			else if (Booking.ConsignorPickupAddress.OrganisationPK == SiteUser.LoggedInOrganisation.PK)
			{
				settings = new WebUserDefaultSettingsForDocAddress(Booking.ConsignorPickupAddress.OrganisationPK, Booking.ConsignorPickupAddress.E2_OA_Address, Booking.ConsignorPickupAddress.ContactPK, Booking.ConsignorPickupAddress.DocAddressType.ToString());
			}

			if (settings != null)
			{
				Env.Registry.SetWebUserDefaultSettingsForDocAddress(SiteUser.LoggedInUser.PK.ToGuid(), settings);
			}
		}

		#endregion
		protected override string GetPageName()
		{
			return WebTracker.Pages.EditBooking;
		}

		protected void WeightVolumeDropDown_SelectedIndexChanged(object sender, EventArgs e)
		{
			Bind();
		}
	}
}
