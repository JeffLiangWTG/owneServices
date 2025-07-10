using System;
using System.ComponentModel;
using System.Drawing;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Views;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public partial class BookingEngineRateCardControl : ItemTemplateControlBase
	{
		readonly SharedSizeGroupProvider SizeGroupProvider;

		public BookingEngineRateCardControl(SharedSizeGroupProvider sharedSizeGroupProvider)
		{
			InitializeComponent();

			SizeGroupProvider = sharedSizeGroupProvider;
			SizeGroupProvider.IncludeInSizeGroup(pnlRootLeft, "pnlRootLeft");

			lblRemarksLabel.Text = ResStrings.Remarks;
			lblRateLabel.Text = ResStrings.Rate;
			lblStatusLabel.Text = ResStrings.Status;

			expandCollapseToggle1.SetStates<BookingEngineRateViewModel>(vm => vm.IsExpanded, (vm, value) => vm.IsExpanded = value);

			SelectionChanged += UpdateSelectedCheckBox;

			defaultBackgroundColor = pnlRootLeft.BackColor;
			highlightedBackgroundColor = Color.FromArgb(208, 248, 163);

			BindingSource.DataSourceChanged += BindingSource_DataSourceChanged;

#if DEBUG
			var suppress = new SuppressFormsLocalizedTestAttribute();
			TypeDescriptor.AddAttributes(lblDepartureTimeDate, suppress);
			TypeDescriptor.AddAttributes(lblDepartureTimeTime, suppress);
			TypeDescriptor.AddAttributes(lblOrigin, suppress);
			TypeDescriptor.AddAttributes(lblArrivalTimeDate, suppress);
			TypeDescriptor.AddAttributes(lblArrivalTimeTime, suppress);
			TypeDescriptor.AddAttributes(lblDestination, suppress);
			TypeDescriptor.AddAttributes(lblMainChargeStatus, suppress);
			TypeDescriptor.AddAttributes(lblMainChargeRate, suppress);
			TypeDescriptor.AddAttributes(lblCarrierName, suppress);
			TypeDescriptor.AddAttributes(lblRemarks, suppress);
			TypeDescriptor.AddAttributes(lblRoutingLabel, suppress);
			TypeDescriptor.AddAttributes(lblAdditionalDetailsLabel, suppress);
			TypeDescriptor.AddAttributes(lblCostBreakdownLabel, suppress);
#endif
		}

		public BookingEngineRateCardControl(BookingEngineRateViewModel vm) : this(new SharedSizeGroupProvider())
		{
			DataBind(vm);
		}

		void BindingSource_DataSourceChanged(object sender, EventArgs e)
		{
			ApplyExtraStaticOneWayBindings();
		}

		protected override void ViewModelPropertiesChanged(object sender, PropertyChangedEventArgs e)
		{
			base.ViewModelPropertiesChanged(sender, e);

			RefreshBoundProperties();
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			base.ApplyExtraStaticOneWayBindings();

			RefreshBoundProperties();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Date format")]
		void RefreshBoundProperties()
		{
			if (CurrentViewModel == null)
			{
				return;
			}

			pnlBottomLeft.Visible = pnlBottomRight.Visible = CurrentViewModel.IsExpanded;
			pnlAdditionalDetailsContainer.Visible = CurrentViewModel.AdditionalDetails.Count > 0;

			lblMainChargeRate.Text = CurrentViewModel.MainCharge.ChargeCode;
			lblMainChargeStatus.Text = CurrentViewModel.MainCharge.ChargeCodeDescription;

			const string shortTimeFormat = "hh:mm tt"; // 12:34 PM
			const string shortDateFormat = "MMMM dd, yyyy"; // June 05, 2020
			lblArrivalTimeDate.Text = CurrentViewModel.ArrivalTime.ToString(shortDateFormat);
			lblArrivalTimeTime.Text = CurrentViewModel.ArrivalTime.ToString(shortTimeFormat);
			lblDepartureTimeDate.Text = CurrentViewModel.DepartureTime.ToString(shortDateFormat);
			lblDepartureTimeTime.Text = CurrentViewModel.DepartureTime.ToString(shortTimeFormat);
		}

		BookingEngineRateViewModel CurrentViewModel => DataSource as BookingEngineRateViewModel;

		void UpdateSelectedCheckBox(object sender, EventArgs e)
		{
			selectedCheckBox.Checked = IsSelected;
		}

		public override void ClearSelection()
		{
			IsSelected = false;
		}

		void selectedCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			IsSelected = selectedCheckBox.Checked;

			pnlRootLeft.BackColor = IsSelected ? highlightedBackgroundColor : defaultBackgroundColor;
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				SizeGroupProvider?.ExcludeFromSizeGroup(pnlRootLeft, "pnlRootLeft");

				if (disposing && (components != null))
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		readonly Color defaultBackgroundColor;
		readonly Color highlightedBackgroundColor;

#if DEBUG
		public ZPanel pnlAdditionalDetailsContainerForTest => pnlAdditionalDetailsContainer;
		public TotalPriceLargeDisplay totalPriceLargeDisplayForTest => totalPriceLargeDisplay1;
		public AirlineLogoControl airlineLogoForTest => airlineLogoControl1;
		public RouteViewIconsMode routeViewIconsModeForTest => routeViewIconsMode1;
		public ZCheckBox selectedCheckBoxForTest => selectedCheckBox;
#endif
	}
}
