using System;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class NVOCCMainControl : ZUserControl
	{
		public NVOCCMainControl()
		{
			InitializeComponent();
			this.VisibilityRelationshipProvider.SetDependency(this.ShipperCODTypeDropEdit, this.ShipperCODAmountCalcEdit);

			SetDataSourceBinding("TransportContainerMode", QuotedBooking.Schema.Mode);
			SetDataSourceBinding("IsDomesticFreight", QuotedBooking.Schema.IsDomesticFreight);
			SetDataSourceBinding(PaymentTermDropEdit.GetExtension<LabelCaptionRenderer>(), "Caption", "PaymentTermLabel", true); // Binding constant

#if DEBUG
			TypeDescriptor.AddAttributes(ChargeableUnitLabel, new SuppressFormsLocalizedTestAttribute());
#endif

			if (!Enterprise.ZArchitecture.Core.DesignModeFinder.IsDesigning)
			{
				PickupDocAddressControl.Enter += new EventHandler(PickupDocAddressControl_Enter);
				JS_ShipmentStatusDropEdit.Visible = true;
				SetTotalCO2eVisibility();
			}
			this.JS_GoodsDescriptionBoundTextBox.ButtonText = Res.GetString("a03a349f-8774-42fe-bfc8-309816d9813b", "Details");
		}

		#region Binding

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (BindingSource.DataSource != null)
			{
				AviationSecurityGroupBox.DataBindings.RemoveBinding("IsVisibleForBinding");
			}
			base.SetDataBinding(dataSource, dataMember);
			if (BindingSource.DataSource != null && QuotedBooking != null && QuotedBooking.Booking != null)
			{
				AviationSecurityGroupBox.DataBindings.Add(new KBinding("IsVisibleForBinding", QuotedBooking, "IsAviationSecurityApplicableForTransportMode"));
				PickupDocAddressControl.Text = FreightDataRegistry.Instance.ConsignorShipperTerminology.Value;
				SetupLayout();
			}
		}

		#region TransportContainerMode

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public ZString TransportContainerMode
		{
			get { return transportContainerMode; }
			set
			{
				if (transportContainerMode != value)
				{
					transportContainerMode = value;
					SetupLayout();
				}
			}
		}
		ZString transportContainerMode;

		#endregion

		#region IsDomesticFreight

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public ZString IsDomesticFreight
		{
			get { return isDomesticFreight; }
			set
			{
				if (isDomesticFreight != value)
				{
					isDomesticFreight = value;
					SetupLayout();
				}
			}
		}
		ZString isDomesticFreight;

		#endregion

		#endregion

		#region SetupLayout

		const int GroupBoxHeight_Containerised = 65;
		const int GroupBoxHeight_Loose = 150;

		void SetupLayout()
		{
			string containerMode = QuotedBooking.ContainerMode;
			bool showWeightVolumeDetails = containerMode != Constants.ContainerModes.FCL;
			WeightAndVolumePanel.Visible = showWeightVolumeDetails;
			ControlDpiScalingHelper.SetHeight(ref GoodsDetailGroupBox, (showWeightVolumeDetails) ? GroupBoxHeight_Loose : GroupBoxHeight_Containerised, true);

			bool isFCL = (containerMode == Constants.ContainerModes.FCL);
			CFSReferenceTextBox.Visible = !isFCL;

			EntryRateFrequencyCalcDropEdit.Visible = ParentQuotedBookingForm.ShowQuoteControls;
			EntryRateTransitTimeDropEdit.Visible = ParentQuotedBookingForm.ShowQuoteControls;

			ShipperCODAmountCalcEdit.Visible = QuotedBooking.IsDomesticFreight;
			ShipperCODTypeDropEdit.Visible = QuotedBooking.IsDomesticFreight;

			StartDateDateEdit.Visible = ParentQuotedBookingForm.ShowQuoteControls;
			EndDateDateEdit.Visible = ParentQuotedBookingForm.ShowQuoteControls;

			SetDeliveryDueDateVisibility();

			if (ComplianceRiskHelper.CheckIfComplianceRiskEnabled(typeof(QuotedBooking), false))
			{
				ScreeningSecurityGroupBox.Visible = false;
			}
			else
			{
				ScreeningSecurityGroupBox.Visible = !QuotedBooking.IsTemplate && QuotedBooking.Booking != null;
			}
		}

		#endregion
		#region Implementaion

		void SetDeliveryDueDateVisibility()
		{
			DeliveryDueDateDateEdit.Visible = FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsTransportModeActive(QuotedBooking?.TransportMode ?? ZString.Empty);
		}

		void SetTotalCO2eVisibility()
		{
			TotalCO2eTextBox.Visible = ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled;
		}

		async void ScreenButton_Click(object sender, EventArgs e)
		{
			IScreeningPartyProvider provider = QuotedBooking?.Booking;
			if (provider != null)
			{
				await new DeniedPartyScreeningPresentationManager().PerformScreening((ZForm)ParentForm, false, !DeniedPartyScreenerAsync.HasExcludedList(QuotedBooking.Booking.Factory), QuotedBooking.Booking, () => QuotedBooking.HasChanges);
			}
		}

		QuotedBookingForm ParentQuotedBookingForm
		{
			get { return (QuotedBookingForm)ParentForm; }
		}

		QuotedBooking QuotedBooking
		{
			get { return ParentQuotedBookingForm == null ? null : ParentQuotedBookingForm.QuotedBooking; }
		}

		void PickupDocAddressControl_Enter(object sender, EventArgs e)
		{
			if (!QuotedBooking.ConsignorDocumentaryAddress.ReadOnly && QuotedBooking.BuyerSupplierLinksHelper.ShouldShowRelatedConsignors)
			{
				PickupDocAddressControl.SelectFromPopupForm();
			}
		}

		#endregion

		#region Metadata

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<NVOCCMainControl>()
			.Property("IsDomesticFreight", ZString.Empty, false)
			.Property("TransportContainerMode", ZString.Empty, false)
			.Result;
		}

		#endregion
	}
}
