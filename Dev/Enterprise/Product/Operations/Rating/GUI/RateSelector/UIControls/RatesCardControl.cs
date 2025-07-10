using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public partial class RatesCardControl : TemplateBasedControl
	{
		readonly SortableRatesViewModel ViewModel;
		readonly SharedSizeGroupProvider sharedSizeGroupProvider;

		public RatesCardControl()
		{
			InitializeComponent();
			sharedSizeGroupProvider = new SharedSizeGroupProvider();
		}

		public RatesCardControl(SortableRatesViewModel viewModel) : this()
		{
			SetDataBinding(viewModel, "");
			ViewModel = viewModel;
			pageNavigator.SetData(viewModel.SortedRates);
		}

		protected override Panel GetContainerPanel()
		{
			return pnlItemsContainer;
		}

		protected override void ApplyExtraSettingsOnEachControl(Control control)
		{
			pnlItemsContainer.RowCount += 1;
			pnlItemsContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			control.Anchor = AnchorStyles.Top;
			control.Margin = ControlDpiScalingHelper.NewScaledPadding(0, 10, 0, 0);
			pnlItemsContainer.SetRow(control, pnlItemsContainer.RowCount - 1);
		}

		protected override IItemTemplateControl CreateNewItemControl(object data)
		{
			if (data is CW1RateViewModel)
			{
				var cw1 = new CW1RateCardControl(sharedSizeGroupProvider);
				cw1.Dock = DockStyle.None;
				cw1.Anchor = AnchorStyles.Top; // centers horizontally

				return cw1;
			}

			if (data is CargoguideRateViewModel)
			{
				var cg = new CargoguideRateCardControl(sharedSizeGroupProvider);
				cg.Dock = DockStyle.None;
				cg.Anchor = AnchorStyles.Top; // centers horizontally

				return cg;
			}

			if (data is BookingEngineRateViewModel)
			{
				return new BookingEngineRateCardControl(sharedSizeGroupProvider);
			}

			return new ItemTemplateControlBase();
		}

		protected override IEnumerable<object> GetItemsData()
		{
			if (CurrentPageData != null)
			{
				return CurrentPageData;
			}

			return Enumerable.Empty<object>();
		}

		protected override bool RenderLayoutOnCurrentDataItemChanged => false;

		protected override Type GetItemControlType(object data)
		{
			if (data is CW1RateViewModel)
			{
				return typeof(CW1RateCardControl);
			}

			if (data is CargoguideRateViewModel)
			{
				return typeof(CargoguideRateCardControl);
			}

			return base.GetItemControlType(data);
		}

#if !WINZOR
		protected override bool ReuseExistingItemControls => true;
#endif

		protected override void RenderLayout()
		{
			base.RenderLayout();

			sharedSizeGroupProvider.AdjustAll();

			if (ViewModel is ContainerGroupViewModel containerGroupViewModel)
			{
				SelectedItem = containerGroupViewModel.SelectedRate;
			}

			if (ViewModel is NonContainerizedRatesViewModel ratesViewModel)
			{
				SelectedItem = ratesViewModel.SelectedRate;
			}

			if (ViewModel is BookingRatesViewModel bookingRatesViewModel)
			{
				SelectedItem = bookingRatesViewModel.SelectedRate;
			}
		}

		protected override void SetSelectedItemOnViewModel(object selectedData)
		{
			base.SetSelectedItemOnViewModel(selectedData);
			if (ViewModel is ContainerGroupViewModel containerGroupViewModel)
			{
				containerGroupViewModel.SelectedRate = (RateViewModel)selectedData;
			}

			if (ViewModel is NonContainerizedRatesViewModel ratesViewModel)
			{
				ratesViewModel.SelectedRate = (RateViewModel)selectedData;
			}

			if (ViewModel is BookingRatesViewModel bookingRatesViewModel)
			{
				bookingRatesViewModel.SelectedRate = (BookingEngineRateViewModel)selectedData;
			}
		}

		protected override void ViewModelPropertiesChanged(object sender, PropertyChangedEventArgs e)
		{
			base.ViewModelPropertiesChanged(sender, e);

			if (e.PropertyName == nameof(ViewModel.SortedRates))
			{
				pageNavigator.SetData(ViewModel.SortedRates);
			}
		}

		void pageNavigator_PageChanged(object sender, PageChangedEventArgs e)
		{
			SetSelectedItemOnViewModel(null);

			var pageData = new List<object>();
			foreach (var item in e.PageData)
			{
				pageData.Add(item);
			}

			CurrentPageData = pageData;

			pnlItemsContainer.VerticalScroll.Value = 0;

			RenderLayout();
		}

		IEnumerable<object> CurrentPageData;
	}
}
