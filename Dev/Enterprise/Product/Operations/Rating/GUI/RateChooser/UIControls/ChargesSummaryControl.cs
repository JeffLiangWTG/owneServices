using System;
using System.ComponentModel;
using Enterprise.Rating.GUI.RateChooser.ViewModel;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	public partial class ChargesSummaryControl : ItemTemplateControlBase
	{
		public ChargesSummaryControl()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				AutoSize = true;
				AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			}

#if DEBUG
			TypeDescriptor.AddAttributes(lblDSTChargeCodesString, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblDSTChargesLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblFRTChargeCodesString, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblFRTChargesLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblGroupShortName, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblORGChargeCodesString, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblORGChargesLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblTotalDSTChargesPriceString, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblTotalFRTChargesPriceString, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblTotalORGChargesPriceString, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblUnmappedChargeCodesString, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblUnmappedChargesLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			base.ApplyExtraStaticOneWayBindings();

			if (Data != null)
			{
				pbCalculationIcon.Visible = Data.CalculationIconVisibility;
				pbErrorIcon.Visible = Data.TotalPriceErrorVisibility;
				pnlOrgCharges.Visible = Data.ORGChargesVisibility;
				pnlFRTCharges.Visible = Data.FRTChargesVisibility;
				pnlDSTCharges.Visible = Data.DSTChargesVisibility;
				pnlUnmappedCharges.Visible = Data.UnmappedChargesVisibility;

				this.Visible = !string.IsNullOrEmpty(Data.ActiveChargeCodesString);

				// As PictureBox is not bindable, we have to set it's image here.
				pbErrorIcon.Image = RateChooserImageRepository.ErrorIcon;

				if (Data.CalculationIcon != null)
				{
					// As PictureBox is not bindable, we have to set it's image here.
					pbCalculationIcon.Image = Data.CalculationIcon;
				}
			}
		}

		protected override void ViewModelPropertiesChanged(object sender, PropertyChangedEventArgs e)
		{
			base.ViewModelPropertiesChanged(sender, e);
			switch (e.PropertyName)
			{
				case nameof(Data.ActiveChargeCodesString):
					this.Visible = !string.IsNullOrEmpty(Data.ActiveChargeCodesString);
					break;

				case nameof(Data.TotalPriceErrorVisibility):
					pbErrorIcon.Visible = Data.TotalPriceErrorVisibility;
					break;

				case nameof(Data.ORGChargesVisibility):
					pnlOrgCharges.Visible = Data.ORGChargesVisibility;
					break;

				case nameof(Data.FRTChargesVisibility):
					pnlFRTCharges.Visible = Data.FRTChargesVisibility;
					break;

				case nameof(Data.DSTChargesVisibility):
					pnlDSTCharges.Visible = Data.DSTChargesVisibility;
					break;

				case nameof(Data.UnmappedChargesVisibility):
					pnlUnmappedCharges.Visible = Data.UnmappedChargesVisibility;
					break;

				case nameof(ChargesViewModel.ORGChargeCodesString):
					lblORGChargeCodesString.Text = Data.ORGChargeCodesString;
					break;

				case nameof(ChargesViewModel.TotalORGChargesPriceString):
					lblTotalORGChargesPriceString.Text = Data.TotalORGChargesPriceString;
					break;

				case nameof(ChargesViewModel.FRTChargeCodesString):
					lblFRTChargeCodesString.Text = Data.FRTChargeCodesString;
					break;

				case nameof(ChargesViewModel.TotalFRTChargesPriceString):
					lblTotalFRTChargesPriceString.Text = Data.TotalFRTChargesPriceString;
					break;

				case nameof(ChargesViewModel.DSTChargeCodesString):
					lblDSTChargeCodesString.Text = Data.DSTChargeCodesString;
					break;

				case nameof(ChargesViewModel.TotalDSTChargesPriceString):
					lblTotalDSTChargesPriceString.Text = Data.TotalDSTChargesPriceString;
					break;

				case nameof(ChargesViewModel.UnmappedChargeCodesString):
					lblUnmappedChargeCodesString.Text = Data.UnmappedChargeCodesString;
					break;
			}
		}

		#region Tooltips
		void pbErrorIcon_MouseHover(object sender, EventArgs e)
		{
			pbErrorIcon.SetTooltip(Data?.TotalPriceErrorString);
		}

		void lblORGChargeCodesString_MouseHover(object sender, EventArgs e)
		{
			lblORGChargeCodesString.SetTooltip(Data?.ORGChargeCodesString);
		}

		void lblFRTChargeCodesString_MouseHover(object sender, EventArgs e)
		{
			lblFRTChargeCodesString.SetTooltip(Data?.FRTChargeCodesString);
		}

		void lblDSTChargeCodesString_MouseHover(object sender, EventArgs e)
		{
			lblDSTChargeCodesString.SetTooltip(Data?.DSTChargeCodesString);
		}

		void lblUnmappedChargeCodesString_MouseHover(object sender, EventArgs e)
		{
			lblUnmappedChargeCodesString.SetTooltip(Data?.UnmappedChargeCodesString);
		}
		#endregion

		ChargesViewModel Data => CurrentDataItem as ChargesViewModel;
	}
}
