using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Views;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	public partial class RateChooserCard : ViewModelBasedControl
	{
		public RateChooserCard()
		{
			InitializeComponent();

			pnlRateDetails.Text = Res.GetString("FAFE66E9-9549-4EA7-B0B5-D9EAB66B0830", "More Details ...");

			if (!DesignMode)
			{
				this.AutoSizeMode = AutoSizeMode.GrowAndShrink;
				pnlTop.AutoSizeMode = AutoSizeMode.GrowAndShrink;

				lcInlandCharges.AutoSize = true;
				lcOutlandCharges.AutoSize = true;
				lcOceanCharges.AutoSize = true;

				pnlRateDetails.IsCollapsed = true;
				((ISplitterLayoutSaveProvider)pnlRateDetails).IsLayoutRestored = true;
				pnlRateDetails.AutoSizeMode = AutoSizeMode.GrowAndShrink;

				chargesCW1FreightCharges.AutoSize = true;
				chargesBOLCharges.AutoSize = true;
				chargesCW1BillOfLadingCharges.AutoSize = true;
				lcInlandCharges.AutoSize = true;
				lcOutlandCharges.AutoSize = true;
				lcOceanCharges.AutoSize = true;
				pnlChargeDetails.AutoSize = true;
				lblRoutingOrTransitTime.AutoSize = true;
			}

#if DEBUG
			TypeDescriptor.AddAttributes(lblSpotVoyageNumber, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblSpotVesselName, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblSpotTransitTime, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblSpotRateID, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblDestination, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblArrivalTime, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblOrigin, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblDepartureTime, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblRoutingOrTransitTime, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblExcludedLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblIncludedLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCommodityType, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCommodityName, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblContractNumberLink, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblContractNumber, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblArrivalDate, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblDepartureDate, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblLclUnit, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblTotalLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblAddOnOrCarrier, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblVesselOrConsigneeLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblRateType2OrConsignor, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblRateType2OrConsignorLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblAddonOrCarrierLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblVesselOrConsignee, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblRateType, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblRateTypeOrBlankLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblServiceStringOrLevel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblServiceStringOrLevelLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCarrierServiceLevel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCarrierServiceLevelLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblExpiryDate, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblExpiryDateLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblEffectiveDate, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblEffectiveDateLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblTransitTimeLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCommodities, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblAccount, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblTradeLane, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblServiceProvider, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblWarnings, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblTotalCalculatedCharges, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			if (Data == null)
			{
				return;
			}

			pnlNonSpotInfo.Visible = Data.NonVisibleOnSpotRates;

			lblLclUnit.Visible = !string.IsNullOrEmpty(Data?.LclUnit);

			if (Data.WiseRateRoutingVisibility)
			{
				pnlNonSpotInfo.SetColumn(lblRoutingOrTransitTime, 0);
				pnlNonSpotInfo.SetColumnSpan(lblRoutingOrTransitTime, 2);
				lblRoutingOrTransitTime.Dock = DockStyle.Fill;
			}
			else
			{
				pnlNonSpotInfo.SetColumn(lblRoutingOrTransitTime, 1);
				pnlNonSpotInfo.SetColumnSpan(lblRoutingOrTransitTime, 1);
				lblRoutingOrTransitTime.TextAlign = ContentAlignment.MiddleLeft;
			}

			lblRoutingOrTransitTime.Visible = Data.WiseRateRoutingVisibility;

			if (Data.ProviderIcon != null)
			{
				// As PictureBox is not bindable, we have to set it's image here.
				pbProviderIcon.Image = Data.ProviderIcon;
			}

			// As PictureBox is not bindable, we have to set it's image here.
			pbCarrierServiceLevelWarning.Image = RateChooserImageRepository.WarningIcon;
			pbTotalIcon.Image = Data.TotalIcon;

			pnlTotalIcon.Visible = Data.TotalIconVisibility;
			lblTotalCalculatedCharges.Text = Data.TotalCalculatedCharges;

			lblTransitTimeLabel.Visible = Data.CWTransitTimeVisibility;

			pbSpotRate.Visible = Data.VisibleOnSpotRates;
			lblContractNumber.Visible = Data.ContractNumberIsVisible;
			lblContractNumberLink.Visible = Data.ContractNumberAllocationHyperlinkVisibility;

			pbCarrierServiceLevelWarning.Visible = Data.CarrierServiceLevelErrorVisibility;

			pbCommodityInfo.Visible = Data.CommodityInfoVisibility;
			chargesSummaryBOLCharges.Visible = Data.BOLCharges != null;
			chargesSummaryCW1BillOfLadingCharges.Visible = Data.CW1BillOfLadingCharges != null;
			chargesSummaryCW1FreightCharges.Visible = Data.CW1FreightCharges != null;
			lcsInlandCharges.Visible = Data.InlandCharges != null;
			lcsOceanCharges.Visible = Data.OceanCharges != null;
			lcsOutlandCharges.Visible = Data.OutlandCharges != null;

			chargesCW1FreightCharges.Visible = Data.DisplayPrice && Data.CW1FreightCharges != null;
			chargesBOLCharges.Visible = Data.DisplayPrice && Data.BOLCharges != null;
			chargesCW1BillOfLadingCharges.Visible = Data.DisplayPrice && Data.CW1BillOfLadingCharges != null;
			lcInlandCharges.Visible = Data.DisplayPrice && Data.InlandCharges != null;
			lcOutlandCharges.Visible = Data.DisplayPrice && Data.OutlandCharges != null;
			lcOceanCharges.Visible = Data.DisplayPrice && Data.OceanCharges != null;

			AdjustChargeDetailsPanelHeight();

			#region Spot Information

			pnlRoute.Visible = Data.VisibleOnSpotRates;

			if (Data.VisibleOnSpotRates)
			{
				lblDepartureTime.Text = Data.BookingInfo.DepartureTime.ToString("hh:mm tt");
				lblDepartureDate.Text = Data.BookingInfo.DepartureTime.ToString("MMMM dd, yyyy");

				lblArrivalDate.Text = Data.BookingInfo.ArrivalTime.ToString("MMMM dd, yyyy");
				lblArrivalTime.Text = Data.BookingInfo.ArrivalTime.ToString("hh:mm tt");

				lblSpotTransitTime.Text = Data.BookingInfo.TransitTime.ToString("dd\\d\\ hh\\h");

				lblSpotVesselName.FontType = ZArchitecture.Core.OFontTypes.Small | ZArchitecture.Core.OFontTypes.Bold;
				lblSpotVoyageNumber.FontType = ZArchitecture.Core.OFontTypes.Small | ZArchitecture.Core.OFontTypes.Bold;
				lblSpotRateID.FontType = ZArchitecture.Core.OFontTypes.Small | ZArchitecture.Core.OFontTypes.Bold;
				lblSpotTransitTime.FontType = ZArchitecture.Core.OFontTypes.Small | ZArchitecture.Core.OFontTypes.Bold;
			}
			#endregion

			pbProviderIcon.Width = ControlDpiScalingHelper.MarkAsScaled(pbProviderIcon.Height);
			if (pbSpotRate.Visible)
			{
				pbSpotRate.Width = ControlDpiScalingHelper.MarkAsScaled(pbSpotRate.Height);
			}
		}

		void AdjustChargeDetailsPanelHeight()
		{
			var maxChargeCount = new[] {
				(Data?.CW1FreightCharges?.ChargeGroupsView.SelectMany(g => g.Charges)?.Count() ?? 0),
				(Data?.BOLCharges?.ChargeGroupsView?.SelectMany(g => g.Charges)?.Count() ?? 0),
				(Data?.CW1BillOfLadingCharges?.ChargeGroupsView?.SelectMany(g => g.Charges)?.Count() ?? 0),
				(Data?.InlandCharges?.Charges?.Where(c => c.ChargeGroupsView != null)?.SelectMany(c => c.ChargeGroupsView.SelectMany(g => g.Charges)).Count() ?? 0) + 1,
				(Data?.OutlandCharges?.Charges?.Where(c => c.ChargeGroupsView != null)?.SelectMany(c => c.ChargeGroupsView.SelectMany(g => g.Charges)).Count() ?? 0) + 1,
				(Data?.OceanCharges?.Charges?.Where(c => c.ChargeGroupsView != null)?.SelectMany(c => c.ChargeGroupsView.SelectMany(g => g.Charges)).Count() ?? 0) + 1 }.Max();

			var minHeight = 40 + maxChargeCount * 40;
			if (minHeight > 340)
			{
				minHeight = 340;
			}

			// isInStandardDpi = false because width and height have been scaled to user's DPI
			var minHeightScaled = ControlDpiScalingHelper.ScaleToCurrentDpiY(minHeight);
			chargesCW1FreightCharges.MinimumSize = ControlDpiScalingHelper.NewScaledSize(chargesCW1FreightCharges.MinimumSize.Width, minHeightScaled, isInStandardDpi: false);
			chargesCW1BillOfLadingCharges.MinimumSize = ControlDpiScalingHelper.NewScaledSize(chargesCW1FreightCharges.MinimumSize.Width, minHeightScaled, isInStandardDpi: false);
			chargesBOLCharges.MinimumSize = ControlDpiScalingHelper.NewScaledSize(chargesBOLCharges.MinimumSize.Width, minHeightScaled, isInStandardDpi: false);

			lcOceanCharges.MinimumSize = ControlDpiScalingHelper.NewScaledSize(lcOceanCharges.MinimumSize.Width, minHeightScaled, isInStandardDpi: false);
			lcInlandCharges.MinimumSize = ControlDpiScalingHelper.NewScaledSize(lcInlandCharges.MinimumSize.Width, minHeightScaled, isInStandardDpi: false);
			lcOutlandCharges.MinimumSize = ControlDpiScalingHelper.NewScaledSize(lcOutlandCharges.MinimumSize.Width, minHeightScaled, isInStandardDpi: false);
		}

		#region Tooltips
		void pbSpotRate_MouseHover(object sender, EventArgs e)
		{
			pbSpotRate.SetTooltip(Data?.SpotRateTooltip);
		}

		void lblServiceProvider_MouseHover(object sender, EventArgs e)
		{
			lblServiceProvider.SetTooltip(Data?.ServiceProviderToolTip);
		}

		void lblContractNumberLink_MouseHover(object sender, EventArgs e)
		{
			lblContractNumberLink.SetTooltip(Data?.ContractNumber);
		}

		void lblContractNumber_MouseHover(object sender, EventArgs e)
		{
			lblContractNumber.SetTooltip(Data?.ContractNumber);
		}

		void imgServiceLevelWarning_MouseHover(object sender, EventArgs e)
		{
			pbCarrierServiceLevelWarning.SetTooltip(Data?.CarrierServiceLevelError);
		}

		void lblTotalCalculatedCharges_MouseHover(object sender, EventArgs e)
		{
			lblTotalCalculatedCharges.SetTooltip(Data?.TotalCalculatedChargesDescription);
		}

		void pbShip_MouseHover(object sender, EventArgs e)
		{
			pbShip.SetTooltip(Data?.BookingInfo?.VesselName);
		}

		void lblSpotTransitTime_MouseHover(object sender, EventArgs e)
		{
			lblSpotTransitTime.SetTooltip(ResStrings.TransitTime);
		}

		void lblSpotRateID_MouseHover(object sender, EventArgs e)
		{
			lblSpotRateID.SetTooltip(ResStrings.RateId);
		}

		void lblSpotVoyageNumber_MouseHover(object sender, EventArgs e)
		{
			lblSpotVoyageNumber.SetTooltip(ResStrings.VoyageNumber);
		}

		void lblSpotVesselName_MouseHover(object sender, EventArgs e)
		{
			lblSpotVesselName.SetTooltip(ResStrings.VesselName);
		}

		#endregion

		ChooserRateRow Data => CurrentDataItem as ChooserRateRow;

		void pbCommodityInfo_MouseUp(object sender, MouseEventArgs e)
		{
			pnlCommodityInfo.Visible = !pnlCommodityInfo.Visible;
			pnlCommodityInfo.BringToFront();
		}

		void pbTotalIcon_MouseHover(object sender, EventArgs e)
		{
			pbTotalIcon.SetTooltip(Data?.TotalIconToolTip);
		}

		void lblContractNumberLink_Click(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (Data != null)
			{
				Data.PopupContractAndAllocationsAttachForm();
			}
		}

		void lblWarnings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (Data != null)
			{
				Globals.Message.ShowInformation(Data.PlainLogs);
			}
		}

		void pnlThirdRow_VisibleChanged(object sender, EventArgs e)
		{
			if (Data != null && pnlThirdRow.Visible && !pnlThirdRowVisibilityHandling)
			{
				pnlThirdRowVisibilityHandling = true;

				ucRoutes.Visible = Data.BookingInfo?.ShowTransportLegs ?? false;
				ucBookingTerms.Visible = Data.BookingInfo?.ShowBookingTerms ?? false;
				ucPenalties.Visible = Data.BookingInfo?.ShowPenalties ?? false;

				pnlThirdRow.Visible = ucRoutes.Visible || ucBookingTerms.Visible || ucPenalties.Visible;

				if (pnlThirdRow.Visible)
				{
					var requiredHeightForRoute = ucRoutes.Visible ? Data.BookingInfo.TransportLegs.Count * 81 : 0;
					var requiredHeightForBookingTerms = ucBookingTerms.Visible ? (22 + (Data.BookingInfo.BookingTerms.Count * 21)) : 0;
					var requiredHeightForPenalties = ucPenalties.Visible ? (44 + (Data.BookingInfo.Penalties.Count * 21)) : 0;

					pnlThirdRow.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(Math.Min(240, new[] { requiredHeightForRoute, requiredHeightForBookingTerms, requiredHeightForPenalties }.Max()));
				}

				pnlThirdRowVisibilityHandling = false;
			}
		}
		bool pnlThirdRowVisibilityHandling;

		protected override void ViewModelPropertiesChanged(object sender, PropertyChangedEventArgs e)
		{
			base.ViewModelPropertiesChanged(sender, e);

			if (Data != null)
			{
				if (e.PropertyName == nameof(Data.TotalIcon))
				{
					pbTotalIcon.Image = Data.TotalIcon;
				}

				if (e.PropertyName == nameof(Data.TotalIconVisibility))
				{
					pnlTotalIcon.Visible = Data.TotalIconVisibility;
				}

				if (e.PropertyName == nameof(Data.TotalCalculatedCharges))
				{
					lblTotalCalculatedCharges.Text = Data.TotalCalculatedCharges;
				}

#if DEBUG
				if (e.PropertyName == nameof(Data.ChargeDetailVisibility))
				{
					pnlRateDetails.IsCollapsed = !Data.ChargeDetailVisibility;
				}
#endif
			}
		}
	}
}
