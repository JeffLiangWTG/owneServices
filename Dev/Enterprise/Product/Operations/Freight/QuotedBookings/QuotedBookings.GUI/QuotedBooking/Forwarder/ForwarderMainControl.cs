using System;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Freight.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class ForwarderMainControl : ZUserControl
	{
		#region Ctor

		public ForwarderMainControl()
		{
			InitializeComponent();
			CreateVisibilityDependencies();

			if (!Enterprise.ZArchitecture.Core.DesignModeFinder.IsDesigning)
			{
				DeliveryDocAddressControl.Enter += new EventHandler(DeliveryDocAddressControl_Enter);
				PickupDocAddressControl.Enter += new EventHandler(PickupDocAddressControl_Enter);
				ServiceLevelFindBox.PopupSelected += ServiceLevelFindBox_PopupSelected;
			}

			this.JS_GoodsDescriptionBoundTextBox.ButtonText = Res.GetString("a03a349f-8774-42fe-bfc8-309816d9813b", "Details");
			this.MarksAndNumbersNotePopupEdit.ButtonText = Res.GetString("24b0bfbe-e7a8-4371-a10c-6a070a5a8bd0", "More...");

#if DEBUG
			TypeDescriptor.AddAttributes(General1, new SuppressControlRequiresTextBasherAttribute());
			TypeDescriptor.AddAttributes(General2, new SuppressControlRequiresTextBasherAttribute());
			TypeDescriptor.AddAttributes(ValueOfGoodsCurrencyFindbox, new SuppressControlRequiresTextBasherAttribute());
			TypeDescriptor.AddAttributes(ValueOfInsuranceCurrencyFindbox, new SuppressControlRequiresTextBasherAttribute());
#endif
		}

		void CreateVisibilityDependencies()
		{
			this.VisibilityRelationshipProvider.SetDependency(this.CustomsEntryNumberBoundTextBox, this.CustomsEntryNumberTypeBoundDropEdit);
			this.VisibilityRelationshipProvider.SetDependency(this.ShipperCODTypeDropEdit, this.ShipperCODAmountCalcEdit);
			this.VisibilityRelationshipProvider.SetDependency(this.EntryRateFrequencyCalcDropEdit, this.EntryRateTransitTimeDropEdit);
			this.VisibilityRelationshipProvider.SetDependency(this.DeliveryCloseDateEdit, this.DeliveryOpenDateEdit);
			this.VisibilityRelationshipProvider.SetDependency(this.PickupCloseDateEdit, this.PickupReadyDateEdit);
			this.VisibilityRelationshipProvider.SetDependency(this.ChargeableUnitLabel, this.ChargeableDropEdit);
			this.VisibilityRelationshipProvider.SetDependency(this.TotalCO2eUnitLabel, this.TotalCO2eTextBox);
		}

		#endregion

		#region Properties

		QuotedBooking QuotedBooking
		{
			get { return DataSource as QuotedBooking; }
		}

		#endregion

		#region Implementation

		void ServiceLevelFindBox_PopupSelected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			var selectedObjects = e.SelectedBusinessObjects;
			if (selectedObjects.Length == 1 && QuotedBooking?.Booking != null)
			{
				QuotedBooking.Booking.OnServiceLevelOrTransitTimeSelected(selectedObjects[0]);
			}
		}

		async void ScreenButton_Click(object sender, EventArgs e)
		{
			IScreeningPartyProvider provider = QuotedBooking?.Booking;
			if (provider != null)
			{
				await new DeniedPartyScreeningPresentationManager().PerformScreening((ZForm)ParentForm, false, !DeniedPartyScreenerAsync.HasExcludedList(QuotedBooking.Booking.Factory), QuotedBooking.Booking, () => QuotedBooking.HasChanges);
			}
		}

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);

			var quotedBooking = QuotedBooking;
			if (ComplianceRiskHelper.CheckIfComplianceRiskEnabled(typeof(QuotedBooking), false))
			{
				ScreeningControlsGroupBox.Visible = false;
			}
			else
			{
				ScreeningControlsGroupBox.Visible = quotedBooking?.Booking != null && !quotedBooking.IsTemplate;
			}

			if (quotedBooking != null && quotedBooking.Quote != null && quotedBooking.Booking == null && quotedBooking.ClientDocAddress != null)
			{
				quotedBooking.ClientDocAddress.E2_AddressOverrideInfo.ValueChanged -= new EventHandler(E2_AddressOverrideInfo_ValueChanged);
			}

			if (quotedBooking != null)
			{
				quotedBooking.ModeInfo.ValueChanged -= new EventHandler(ModeInfo_ValueChanged);
				quotedBooking.TransportModeInfo.ValueChanged -= new EventHandler(ModeInfo_ValueChanged);
				quotedBooking.ContainerModeInfo.ValueChanged -= new EventHandler(ModeInfo_ValueChanged);
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var quotedBooking = QuotedBooking;

			if (quotedBooking != null && quotedBooking.Quote != null && quotedBooking.Booking == null && quotedBooking.ClientDocAddress != null)
			{
				quotedBooking.ClientDocAddress.E2_AddressOverrideInfo.ValueChanged += new EventHandler(E2_AddressOverrideInfo_ValueChanged);
			}

			if (quotedBooking != null)
			{
				quotedBooking.ModeInfo.ValueChanged += new EventHandler(ModeInfo_ValueChanged);
				quotedBooking.TransportModeInfo.ValueChanged += new EventHandler(ModeInfo_ValueChanged);
				quotedBooking.ContainerModeInfo.ValueChanged += new EventHandler(ModeInfo_ValueChanged);
			}

			SetupLayout();
		}

		void SetupLayout()
		{
			PickupDocAddressControl.Text = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;

			SetDataSourceBinding(PaymentTermDropEdit.GetExtension<LabelCaptionRenderer>(), "Caption", "PaymentTermLabel", true); // Binding constant

			var quotedBooking = QuotedBooking;

			if (quotedBooking == null)
			{
				return;
			}

			containersAndPackLineDynamicControl.UserControlType = quotedBooking.Booking != null ? typeof(BookingContainersAndPackLinesControl) : typeof(QuoteContainersAndPackLinesControl);

			StartDateDateEdit.Visible = EndDateDateEdit.Visible = OneOffQuoteStatisticsPanel.Visible = quotedBooking.Quote != null;

			SetCompTariffLevelVisibility();
			SetOneOffQuoteHBLDeliveryModeVisibility();
			SetApprovedShipperInspectionVisibility();
			SetCarrierContractControlsVisibility();
			SetOrgTypeVisibility();
		}

		void SetCompTariffLevelVisibility()
		{
			var quotedBooking = QuotedBooking;

			CompTariffLevelDropEdit.Visible = quotedBooking?.Quote != null && quotedBooking?.Booking == null;
			FreightRatesGroupBox.Visible = quotedBooking?.Booking != null;
		}

		void SetOrgTypeVisibility()
		{
			OrgRoleDropEdit.Visible = RatingDataRegistry.Instance.EnableOverseasAgentInOneOffQuote.Value &&
				QuotedBooking.QuotedBookingTypeCode != QuotedBooking.QuickBookingCode;
		}

		void SetOneOffQuoteHBLDeliveryModeVisibility()
		{
			var quotedBooking = QuotedBooking;

			OneOffQuoteHBLDeliveryModeDropEdit.Visible = quotedBooking?.Quote != null && quotedBooking?.Booking == null;
		}

		void SetApprovedShipperInspectionVisibility()
		{
			var quotedBooking = QuotedBooking;

			ApprovedShipperInspectionTypeDropEdit.Visible = quotedBooking != null && quotedBooking.Booking != null &&
				quotedBooking.Booking.IsAir;
		}

		void E2_AddressOverrideInfo_ValueChanged(object sender, EventArgs e)
		{
			SetCompTariffLevelVisibility();
		}

		void ModeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetApprovedShipperInspectionVisibility();
		}

		void DeliveryDocAddressControl_Enter(object sender, EventArgs e)
		{
			if (!QuotedBooking.ConsigneeDocumentaryAddress.ReadOnly && QuotedBooking.BuyerSupplierLinksHelper.ShouldShowRelatedConsignees)
			{
				DeliveryDocAddressControl.SelectFromPopupForm();
			}
		}

		void PickupDocAddressControl_Enter(object sender, EventArgs e)
		{
			if (!QuotedBooking.ConsignorDocumentaryAddress.ReadOnly && QuotedBooking.BuyerSupplierLinksHelper.ShouldShowRelatedConsignors)
			{
				PickupDocAddressControl.SelectFromPopupForm();
			}
		}

		#endregion

		#region Carrier Contracts

		void SetCarrierContractControlsVisibility()
		{
			if (QuotedBooking.Booking == null)
			{
				AllocationRouteCodeFindBox.Visible = false;
				CarrierContractNumberFindBox.Visible = false;
				CarrierContractNumberBoundTextEdit.Visible = false;

				return;
			}

			// For customers with both Contracts and Allocations enabled, we use the find box with the simulation GUI
			// For customers with Allocations disabled, only the textbox without the lookup button is used

			var allocationsAreEnabled = ContractsPermissions.IsAllocationsVisible();

			AllocationRouteCodeFindBox.Visible = allocationsAreEnabled;
			CarrierContractNumberFindBox.Visible = allocationsAreEnabled;

			CarrierContractNumberBoundTextEdit.Visible = !allocationsAreEnabled;
		}

		#endregion
	}
}
