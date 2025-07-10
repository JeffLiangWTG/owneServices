using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class DiscrepancyForm : ZChildForm
	{
		public DiscrepancyForm(QuotedBooking quotedBooking, bool withSaveOptions)
			: base(quotedBooking)
		{
			WithSaveOptions = withSaveOptions;

			InitializeComponent();
			if (quotedBooking.ObjectState != QuotedBookingState.UnacceptedBookingWithQuote)
			{
				throw new NotSupportedException("should be in converted booking state");
			}

			if (!DesignModeFinder.IsDesigning)
			{
				HighlightDiscrepancyFields();
			}

			SetButtonLabels();
		}

		public DiscrepancyForm()
		{
			InitializeComponent();
		}

		public readonly bool WithSaveOptions;

		#region Overrides

		public override string FormHeading
		{
			get { return Res.GetString("c6534a64-cd36-499c-a314-d40d00aeca73", "Spot Quote / Booked Discrepancy"); }
		}

		#endregion

		#region HighlightDiscrepancyFields

		void HighlightDiscrepancyFields()
		{
			var discrepancyChecksAndRespectiveControls = new Dictionary<QuotedBooking.DiscrepancyCheck, Control>()
			{
				{ QuotedBooking.DiscrepancyCheck.Client,            QuoteClientFindBox },
				{ QuotedBooking.DiscrepancyCheck.TransportMode,     QuoteTransportModeTextBox },
				{ QuotedBooking.DiscrepancyCheck.ContainerMode,     QuoteContainerModeTextBox },
				{ QuotedBooking.DiscrepancyCheck.Inco,              QuoteIncoTermTextBox },
				{ QuotedBooking.DiscrepancyCheck.Pickup,            QuotePickupAddressControl },
				{ QuotedBooking.DiscrepancyCheck.Delivery,          QuoteDeliveryAddressControl },
				{ QuotedBooking.DiscrepancyCheck.Service,           QuoteServiceLevelFindBox },
				{ QuotedBooking.DiscrepancyCheck.Origin,            QuoteOriginCodeFindBox },
				{ QuotedBooking.DiscrepancyCheck.Destination,       QuoteDestinationCodeFindBox },
				{ QuotedBooking.DiscrepancyCheck.Carrier,           QuoteCarrierFindBox },
				{ QuotedBooking.DiscrepancyCheck.Weight,            QuoteActualWeightDropEdit },
				{ QuotedBooking.DiscrepancyCheck.WeightUnit,        QuoteActualWeightDropEdit },
				{ QuotedBooking.DiscrepancyCheck.Volume,            QuoteVolumeCalcDropEdit },
				{ QuotedBooking.DiscrepancyCheck.VolumeUnit,        QuoteVolumeCalcDropEdit },
				{ QuotedBooking.DiscrepancyCheck.Chargeable,        QuoteChargeableCalcDropEdit },
				{ QuotedBooking.DiscrepancyCheck.GoodsValue,        QuoteValueOfGoodsCalcEdit },
				{ QuotedBooking.DiscrepancyCheck.GoodsCurrency,     QuoteValueOfGoodsCalcEdit },
				{ QuotedBooking.DiscrepancyCheck.InsuranceValue,    QuoteInsuranceValueCalcEdit },
				{ QuotedBooking.DiscrepancyCheck.InsuranceCurrency, QuoteInsuranceValueCalcEdit },
			};

			foreach (var checkControlPair in discrepancyChecksAndRespectiveControls)
			{
				if (QuotedBooking.HasDiscrepancyFor(checkControlPair.Key))
				{
					checkControlPair.Value.GetExtension<ZLabelCaptionRenderer>().ForeColor = Color.Red;
				}
			}
		}

		#endregion

		#region Implementation

		QuotedBooking QuotedBooking
		{
			get { return (QuotedBooking)BusinessEntity; }
		}

		void SetButtonLabels()
		{
			if (WithSaveOptions)
			{
				UseQuotedPriceButton.CaptionResourceString = Res.GetData("DiscrepancyForm|e435baa1-d4a5-4a11-8edc-ab0d3c56b65e", "Use Quoted Price && Save");
				ReRateButton.CaptionResourceString = Res.GetData("DiscrepancyForm|ba0d1ca2-8316-43ac-b99b-4586c6ce742e", "Re-Rate && Save");
			}
			else
			{
				UseQuotedPriceButton.CaptionResourceString = Res.GetData("DiscrepancyForm|8eb68282-24d3-4227-a5dc-b1e144546ef9", "Use Quoted Price");
				ReRateButton.CaptionResourceString = Res.GetData("DiscrepancyForm|5b7ae87e-6ccd-4614-95a6-3cb4da456eaf", "Re-Rate");
			}
		}

		#endregion

		#region UseQuotedPrice

		void UseQuotedPriceButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		#endregion

		#region ReRateButton

		void ReRateButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Yes;
			Close();
		}

		#endregion

		#region HoldForLaterAnalysis

		void HoldForLaterAnalysisButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion
	}
}
