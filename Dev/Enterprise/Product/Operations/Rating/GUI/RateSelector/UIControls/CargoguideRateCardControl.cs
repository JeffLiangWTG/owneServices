using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using Enterprise.Rating.GUI.RateChooser;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Views;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public partial class CargoguideRateCardControl : ItemTemplateControlBase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Date format")]
		const string DateFormat = "MMMM dd, yyyy";

		public CargoguideRateCardControl()
		{
			InitializeComponent();

			lblIssueDateLabel.Text = ResStrings.IssueDate;
			lblEffectiveFromLabel.Text = ResStrings.EffectiveFrom;
			lblEffectiveToLabel.Text = ResStrings.EffectiveTo;
			lblCarrierServiceLevelLabel.Text = ResStrings.CarrierServiceLevel;
			lblCarrierProductNameLabel.Text = ResStrings.CarrierProductName;
			lblPaymentTermsLabel.Text = ResStrings.PaymentTerms;
			lblRatioLabel.Text = ResStrings.Ratio;
			lblRemarksLabel.Text = ResStrings.Remarks;
			lblDeckLabel.Text = ResStrings.Deck;
			lblFreightLabel.Text = ResStrings.FreightCharges;
			lblSubjectToLabel.Text = ResStrings.SubjectToCharges;
			lblOptionalLabel.Text = ResStrings.OptionalCharges;

			pbProviderIcon.Image = Properties.RatingResources.CargoguideIcon;
			pbBottomPanelToggle.Image = Properties.Resources.ExpandDrawing;

			lblMaxWeightLabel.Text = ResStrings.ContainerPayloadWeight;
			lblMaxVolumeLabel.Text = ResStrings.ContainerPayloadVolume;
			lblPivotLabel.Text = ResStrings.ContainerPivotWeight;

			BindingSource.DataSourceChanged += BindingSource_DataSourceChanged;

#if DEBUG
			TypeDescriptor.AddAttributes(lblMaxWeightLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblMaxVolumeLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblPivotLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblPayloadWeight, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblPayloadVolume, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblPivotWeight, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblIssueDateLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblEffectiveFromLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblEffectiveToLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCarrierServiceLevelLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCarrierProductNameLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblPaymentTermsLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblRatioLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblRemarksLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblDeckLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblFreightLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblSubjectToLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblOptionalLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCarrierName, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCarrierCommodities, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblRouting, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblEffectiveTo, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblEffectiveFrom, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblIssueDate, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblPaymentTerms, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCarrierProductName, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCarrierServiceLevel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblDeck, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblRemarks, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		readonly SharedSizeGroupProvider SizeGroupProvider;
		public CargoguideRateCardControl(SharedSizeGroupProvider sharedSizeGroupProvider) : this()
		{
			Argument.NotNull(sharedSizeGroupProvider, nameof(sharedSizeGroupProvider));

			this.SizeGroupProvider = sharedSizeGroupProvider;
			SizeGroupProvider.IncludeInSizeGroup(pnlCol5, "Col5");
			SizeGroupProvider.IncludeInSizeGroup(pnlCol7, "Col7");
			SizeGroupProvider.IncludeInSizeGroup(pnlCol9, "Col9");
			SizeGroupProvider.IncludeInSizeGroup(pnlCol11, "Col11");
			SizeGroupProvider.IncludeInSizeGroup(pnlCol12, "Col12");
		}

		void BindingSource_DataSourceChanged(object sender, EventArgs e)
		{
			ApplyExtraStaticOneWayBindings();
		}
		protected override void ViewModelPropertiesChanged(object sender, PropertyChangedEventArgs e)
		{
			base.ViewModelPropertiesChanged(sender, e);

			ApplyExtraStaticOneWayBindings();
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			base.ApplyExtraStaticOneWayBindings();

			if (CurrentViewModel is null)
			{
				return;
			}

			SuspendLayout();

			lblIssueDate.Text = CurrentViewModel.IssueDate.HasValue ? CurrentViewModel.IssueDate.Value.ToString(DateFormat) : string.Empty;
			lblEffectiveFrom.Text = CurrentViewModel.StartDate.ToString(DateFormat);
			lblEffectiveTo.Text = CurrentViewModel.ExpiryDate.HasValue ? CurrentViewModel.ExpiryDate.Value.ToString(DateFormat) : string.Empty;

			lblPayloadWeight.Text = CurrentViewModel.ContainerPayloadWeight;
			lblPayloadVolume.Text = CurrentViewModel.ContainerPayloadVolume;
			lblPivotWeight.Text = CurrentViewModel.ContainerPivotWeight;
			lblContractOrReference.Text = CurrentViewModel.ContractOrReference;
			lblCarrierName.Text = CurrentViewModel.CarrierName;
			lblCarrierCommodities.Text = CurrentViewModel.CarrierCommodities;
			lblRouting.Text = CurrentViewModel.Routing;
			lblPaymentTerms.Text = CurrentViewModel.PaymentTerms;
			lblCarrierProductName.Text = CurrentViewModel.Commodities;
			lblCarrierServiceLevel.Text = CurrentViewModel.CarrierServiceLevel;
			lblDeck.Text = CurrentViewModel.Deck;
			lblRemarks.Text = CurrentViewModel.Remarks;
			lblRatio.Text = CurrentViewModel.Ratio;

			tbvCommodityGroup.Text = CurrentViewModel.CommodityGroups.FirstOrDefault();
			tbvCommodityGroup.Error = CurrentViewModel.CommodityGroupError;
			tbvCommodityGroup.ErrorLevel = CurrentViewModel.CommodityGroupErrorLevel;
			tbvCommodityGroup.RealignCenter(pnlCol3);

			tbvCarrierCode.Text = CurrentViewModel.CarrierCode;
			tbvCarrierCode.Error = CurrentViewModel.CarrierError;
			tbvCarrierCode.ErrorLevel = CurrentViewModel.CarrierErrorLevel;
			tbvCarrierCode.RealignCenter(pnlCol2);

			tbvTotalPrice.Text = CurrentViewModel.TotalPriceString;
			tbvTotalPrice.Error = CurrentViewModel.TotalPriceError;
			tbvTotalPrice.ErrorLevel = !string.IsNullOrEmpty(CurrentViewModel.TotalPriceError) ? ErrorLevel.Warning : ErrorLevel.None;
			tbvTotalPrice.RealignCenter(pnlCol14);

			pnlBottom.Visible = CurrentViewModel.IsExpanded;
			pnlSideButtom.Visible = CurrentViewModel.IsExpanded;

			var freightChargesVisibility = CurrentViewModel.FreightCharges?.Charges?.Any() ?? false;
			ccFreightCharges.Visible = freightChargesVisibility;
			lblFreightLabel.Visible = freightChargesVisibility;
			mcFreightCharges.Visible = freightChargesVisibility;
			tbvFreightCharges.Visible = freightChargesVisibility;

			mcFreightCharges.SetDataBinding(CurrentViewModel, nameof(CurrentViewModel.FreightCharges));
			if (freightChargesVisibility)
			{
				tbvFreightCharges.Text = CurrentViewModel.FreightCharges?.TotalPriceString;
				tbvFreightCharges.Error = CurrentViewModel.FreightCharges?.TotalPriceError;
				tbvFreightCharges.ErrorLevel = CurrentViewModel.FreightCharges?.TotalPriceErrorLevel ?? ErrorLevel.None;
			}

			var subjectToChargesVisibility = CurrentViewModel.SubjectToCharges?.Charges?.Any() ?? false;
			ccSubjectToCharges.Visible = subjectToChargesVisibility;
			lblSubjectToLabel.Visible = subjectToChargesVisibility;
			mcSubjectToCharges.Visible = subjectToChargesVisibility;
			tbvSubjectToCharges.Visible = subjectToChargesVisibility;

			mcSubjectToCharges.SetDataBinding(CurrentViewModel, nameof(CurrentViewModel.SubjectToCharges));
			if (subjectToChargesVisibility)
			{
				tbvSubjectToCharges.Text = CurrentViewModel.SubjectToCharges?.TotalPriceString;
				tbvSubjectToCharges.Error = CurrentViewModel.SubjectToCharges?.TotalPriceError;
				tbvSubjectToCharges.ErrorLevel = CurrentViewModel.SubjectToCharges?.TotalPriceErrorLevel ?? ErrorLevel.None;
			}

			var optionalChargesVisibility = CurrentViewModel.OptionalCharges?.Charges?.Any() ?? false;
			ccOptionalCharges.Visible = optionalChargesVisibility;
			lblOptionalLabel.Visible = optionalChargesVisibility;
			mcOptionalCharges.Visible = optionalChargesVisibility;
			tbvOptionalCharges.Visible = optionalChargesVisibility;

			mcOptionalCharges.SetDataBinding(CurrentViewModel, nameof(CurrentViewModel.OptionalCharges));
			if (optionalChargesVisibility)
			{
				tbvOptionalCharges.Text = CurrentViewModel.OptionalCharges?.TotalPriceString;
				tbvOptionalCharges.Error = CurrentViewModel.OptionalCharges?.TotalPriceError;
				tbvOptionalCharges.ErrorLevel = CurrentViewModel.OptionalCharges?.TotalPriceErrorLevel ?? ErrorLevel.None;
			}
			pbShowRawRates.Image = Properties.Resources.Code;
			pbShowCargoguideRawRate.Image = Properties.RatingResources.CargoguideIcon;

			pbShowRawRates.Visible = !string.IsNullOrEmpty(CurrentViewModel.RawRateJson);
			pbShowCargoguideRawRate.Visible = !string.IsNullOrEmpty(CurrentViewModel.CargoguideRawRateJson);

			pbShowRawRates.SetTooltip(ResStrings.ShowRawRateTooltip);
			pbShowCargoguideRawRate.SetTooltip(ResStrings.ShowCargoguideRawRateTooltip);

			ResumeLayout(false);
			PerformLayout();
		}

		CargoguideRateViewModel CurrentViewModel => DataSource as CargoguideRateViewModel;

		public override bool IsSelected
		{
			get => base.IsSelected;
			set
			{
				base.IsSelected = value;
					cbIsSelected.Checked = value;
					pnlMain.BackColor = value ? Color.FromArgb(0xD0, 0xF8, 0xA3) : Color.White;
			}
		}

		public override void ClearSelection()
		{
			base.ClearSelection();
			cbIsSelected.Checked = false;
			pnlMain.BackColor = Color.White;
		}

		void cbIsSelected_CheckedChanged(object sender, EventArgs e)
		{
			IsSelected = cbIsSelected.Checked;
			pnlMain.BackColor = cbIsSelected.Checked ? Color.FromArgb(0xD0, 0xF8, 0xA3) : Color.White;
		}

		void pbBottomPanelToggle_Click(object sender, EventArgs e)
		{
			CurrentViewModel.IsExpanded = !CurrentViewModel.IsExpanded;

			if (CurrentViewModel.IsExpanded)
			{
				pbBottomPanelToggle.Image = Properties.Resources.CollapseDrawing;
			}
			else
			{
				pbBottomPanelToggle.Image = Properties.Resources.ExpandDrawing;
			}
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (SizeGroupProvider != null)
				{
					SizeGroupProvider.ExcludeFromSizeGroup(pnlCol5, "Col5");
					SizeGroupProvider.ExcludeFromSizeGroup(pnlCol7, "Col7");
					SizeGroupProvider.ExcludeFromSizeGroup(pnlCol9, "Col9");
					SizeGroupProvider.ExcludeFromSizeGroup(pnlCol11, "Col11");
					SizeGroupProvider.ExcludeFromSizeGroup(pnlCol12, "Col12");
				}

				if (disposing && (components != null))
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		void pbShowCargoguideRawRate_Click(object sender, EventArgs e)
		{
			CurrentViewModel?.ShowCargoguideRawRateCommand();
		}

		void pbShowRawRates_Click(object sender, EventArgs e)
		{
			CurrentViewModel?.ShowRawRateCommand();
		}
	}
}
