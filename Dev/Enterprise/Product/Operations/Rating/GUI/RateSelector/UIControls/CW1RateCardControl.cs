using System.ComponentModel;
using System.Drawing;
using System.Linq;
using CargoWise.BrandManager;
using CargoWise.Common;
using Enterprise.Rating.GUI.RateChooser;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Views;
using Enterprise.ZArchitecture.GUI;
using static CargoWise.Windows.UI.ControlDpiScalingHelper;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
#if DEBUG
	// Due to its WPF origins, this class uses an alternate means of
	// localisation, and is thus skipped from this part of testing
	[SuppressFormsLocalizedTest]
#endif
	public partial class CW1RateCardControl : ItemTemplateControlBase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Date format")]
		const string DateFormat = "MMMM dd, yyyy";
		public CW1RateCardControl()
		{
			InitializeComponent();

			lblTransitTimeLabel.Text = ResStrings.TransitTime;
			lblEffectiveFromLabel.Text = ResStrings.EffectiveFrom;
			lblEffectiveToLabel.Text = ResStrings.EffectiveTo;
			lblCarrierServiceLevelLabel.Text = ResStrings.CarrierServiceLevel;
			lblServiceLevelLabel.Text = ResStrings.ServiceLevel;
			lblConsignorLabel.Text = ResStrings.Consignor;
			lblConsigneeLabel.Text = ResStrings.Consignee;
			lblCarrierLabel.Text = ResStrings.Carrier;
			lblFreightChargesLabel.Text = ResStrings.FreightCharges;
			lblOtherChargesLabel.Text = ResStrings.OtherCharges;

			providerPictureBox.Image = BrandingFactory.Instance.ProductIcon.ToBitmap();

			totalPriceWarningIconSpacing = warningPictureBox.Location.X - lblTotalPriceString.Location.X - lblTotalPriceString.Size.Width;
			serviceProviderCodeVerticalSpacing = lblServiceProviderCode.Location.Y - lblServiceProviderName.Size.Height - lblServiceProviderName.Location.Y;

			BindingSource.DataSourceChanged += BindingSource_DataSourceChanged;

#if DEBUG
			TypeDescriptor.AddAttributes(lblTransitTimeLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblEffectiveFromLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblEffectiveToLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCarrierServiceLevelLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblServiceLevelLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblConsignorLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblConsigneeLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCarrierLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblFreightChargesLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblOtherChargesLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblServiceProviderName, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblServiceProviderCode, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblContractNumber, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblRouting, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCommodities, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblTransitTime, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblEffectiveFrom, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblEffectiveTo, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCarrierServiceLevel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblServiceLevel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblConsignor, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblConsignee, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCarrierCode, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblTotalPriceString, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		readonly SharedSizeGroupProvider SizeGroupProvider;
		public CW1RateCardControl(SharedSizeGroupProvider sharedSizeGroupProvider) : this()
		{
			Argument.NotNull(sharedSizeGroupProvider, nameof(sharedSizeGroupProvider));

			this.SizeGroupProvider = sharedSizeGroupProvider;
			SizeGroupProvider.IncludeInSizeGroup(pnlCol5, "Col5");
			SizeGroupProvider.IncludeInSizeGroup(pnlCol7, "Col7");
			SizeGroupProvider.IncludeInSizeGroup(pnlCol9, "Col9");
			SizeGroupProvider.IncludeInSizeGroup(pnlCol11, "Col11");
			SizeGroupProvider.IncludeInSizeGroup(pnlCol12, "Col12");
		}

		void BindingSource_DataSourceChanged(object sender, System.EventArgs e)
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

			if (CurrentViewModel == null)
			{
				return;
			}

			SuspendLayout();

			lblTotalPriceString.Text = CurrentViewModel.TotalPriceString;
			lblCarrierCode.Text = CurrentViewModel.CarrierCode;
			lblConsignee.Text = CurrentViewModel.Consignee;
			lblConsignor.Text = CurrentViewModel.Consignor;
			lblServiceLevel.Text = CurrentViewModel.ServiceLevel;
			lblCarrierServiceLevel.Text = CurrentViewModel.CarrierServiceLevel;
			lblCommodities.Text = CurrentViewModel.Commodities;
			lblTransitTime.Text = CurrentViewModel.TransitTime;
			lblRouting.Text = CurrentViewModel.Routing;
			lblContractNumber.Text = CurrentViewModel.ContractNumber;
			lblServiceProviderCode.Text = CurrentViewModel.ServiceProviderCode;
			lblServiceProviderName.Text = CurrentViewModel.ServiceProviderName;

			warningPictureBox.Visible = !string.IsNullOrWhiteSpace(CurrentViewModel.TotalPriceError);
			pnlBottom.Visible = CurrentViewModel.IsExpanded;
			pnlSideBottom.Visible = CurrentViewModel.IsExpanded;

			lblEffectiveFrom.Text = CurrentViewModel.StartDate.ToString(DateFormat);
			lblEffectiveTo.Text = CurrentViewModel.ExpiryDate?.ToString(DateFormat);

			var freightChargesVisibility = CurrentViewModel.FreightCharges?.Charges?.Any() ?? false;

			ccFreightCharges.Visible = freightChargesVisibility;
			lblFreightChargesLabel.Visible = freightChargesVisibility;
			freightChargesItemsControl.Visible = freightChargesVisibility;
			tbvFreightCharges.Visible = freightChargesVisibility;

			freightChargesItemsControl.SetDataBinding(CurrentViewModel, nameof(CurrentViewModel.FreightCharges));
			if (freightChargesVisibility)
			{
				tbvFreightCharges.Text = CurrentViewModel.FreightCharges?.TotalPriceString;
				tbvFreightCharges.Error = CurrentViewModel.FreightCharges?.TotalPriceError;
				tbvFreightCharges.ErrorLevel = CurrentViewModel.FreightCharges?.TotalPriceErrorLevel ?? ErrorLevel.None;
			}

			var otherChargesVisibility = CurrentViewModel.OtherCharges?.Charges?.Any() ?? false;
			ccOtherCharges.Visible = otherChargesVisibility;
			lblOtherChargesLabel.Visible = otherChargesVisibility;
			otherChargesItemsControl.Visible = otherChargesVisibility;
			tbvOtherCharges.Visible = otherChargesVisibility;

			otherChargesItemsControl.SetDataBinding(CurrentViewModel, nameof(CurrentViewModel.OtherCharges));
			if (otherChargesVisibility)
			{
				tbvOtherCharges.Text = CurrentViewModel.OtherCharges?.TotalPriceString;
				tbvOtherCharges.Error = CurrentViewModel.OtherCharges?.TotalPriceError;
				tbvOtherCharges.ErrorLevel = CurrentViewModel.OtherCharges?.TotalPriceErrorLevel ?? ErrorLevel.None;
			}

			SetWidth(pnlBottom, pnlMain.Width, false);

			ResumeLayout(false);
			PerformLayout();
		}

		CW1RateViewModel CurrentViewModel => DataSource as CW1RateViewModel;

		void totalPriceLabel_SizeChanged(object sender, System.EventArgs e) => RealignTotalPrice();

		void warningPictureBox_VisibleChanged(object sender, System.EventArgs e) => RealignTotalPrice();

		readonly int totalPriceWarningIconSpacing;
		void RealignTotalPrice()
		{
			var warningWidth = 0;
			if (warningPictureBox.Visible)
			{
				warningWidth = warningPictureBox.Size.Width;
			}

			var totalPriceLabelX = (lblTotalPriceString.Parent.Width - lblTotalPriceString.Width - warningWidth - totalPriceWarningIconSpacing) / 2;
			lblTotalPriceString.Location = NewScaledPoint(totalPriceLabelX, lblTotalPriceString.Location.Y, isInStandardDpi: false);

			var warningPictureBoxX = totalPriceLabelX + lblTotalPriceString.Width + totalPriceWarningIconSpacing;
			warningPictureBox.Location = NewScaledPoint(warningPictureBoxX, warningPictureBox.Location.Y, isInStandardDpi: false);
		}

		void lblServiceProviderName_SizeChanged(object sender, System.EventArgs e) => AlignServiceProviderLabels();

		readonly int serviceProviderCodeVerticalSpacing;
		void AlignServiceProviderLabels()
		{
			var serviceProviderCodeWidth = lblServiceProviderCode.Size.Width;
			var xAdjust = (serviceProviderCodeWidth - lblServiceProviderName.Size.Width) / 2;
			var newServiceProviderNameLocation = NewScaledPoint(lblServiceProviderCode.Location.X + xAdjust, lblServiceProviderName.Location.Y, isInStandardDpi: false);
			lblServiceProviderName.Location = newServiceProviderNameLocation;

			var newServiceProviderCodeLocation = NewScaledPoint(lblServiceProviderCode.Location.X, lblServiceProviderName.Location.Y + lblServiceProviderName.Size.Height + serviceProviderCodeVerticalSpacing, isInStandardDpi: false);
			lblServiceProviderCode.Location = newServiceProviderCodeLocation;
		}

		void pnlBottom_VisibleChanged(object sender, System.EventArgs e) => SizeToFitPanel();

		void pnlBottom_SizeChanged(object sender, System.EventArgs e) => SizeToFitPanel();

		void SizeToFitPanel()
		{
			var controlHeight = pnlMain.Size.Height + (pnlBottom.Visible ? pnlBottom.Size.Height : 0);
			Size = NewScaledSize(Size.Width, controlHeight, isInStandardDpi: false);
			if (pnlBottom.Visible)
			{
				picBottomPanelToggle.Image = Properties.Resources.CollapseDrawing;
			}
			else
			{
				picBottomPanelToggle.Image = Properties.Resources.ExpandDrawing;
			}
		}

		protected void picBottomPanelToggle_Click(object sender, System.EventArgs e)
		{
			CurrentViewModel.IsExpanded = !CurrentViewModel.IsExpanded;
		}

		#region Tooltips
		void lblServiceProviderName_MouseHover(object sender, System.EventArgs e)
		{
			lblServiceProviderName.SetTooltip(CurrentViewModel.ServiceProviderName);
		}
		#endregion

		public override bool IsSelected
		{
			get => base.IsSelected;
			set
			{
				base.IsSelected = value;
				if (!selectionIsDoneByUI)
				{
					cbIsSelected.Checked = value;
					pnlMain.BackColor = value ? Color.FromArgb(0xD0, 0xF8, 0xA3) : Color.White;
					selectionIsDoneByUI = false;
				}
			}
		}

		public override void ClearSelection()
		{
			base.ClearSelection();
			cbIsSelected.Checked = false;
			pnlMain.BackColor = Color.White;
		}

		void cbIsSelected_CheckedChanged(object sender, System.EventArgs e)
		{
			selectionIsDoneByUI = true;
			IsSelected = cbIsSelected.Checked;
			pnlMain.BackColor = cbIsSelected.Checked ? Color.FromArgb(0xD0, 0xF8, 0xA3) : Color.White;
		}
		bool selectionIsDoneByUI;

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

				components?.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
