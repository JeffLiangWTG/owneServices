using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	public partial class CostBreakdownControl : TemplateBasedControl
	{
		public CostBreakdownControl()
		{
			InitializeComponent();
		}

		protected override IItemTemplateControl CreateNewItemControl(object data)
		{
			return new CostBreakdownItemControl();
		}

		protected override Type GetItemControlType(object data)
		{
			return typeof(CostBreakdownItemControl);
		}

		protected override void ApplyExtraSettingsOnEachControl(Control control)
		{
			base.ApplyExtraSettingsOnEachControl(control);

			control.TabStop = false;
		}

		protected override IEnumerable GetItemsDataCore()
		{
			return CurrentViewModel?.CostBreakdownCharges;
		}

		protected override Panel GetContainerPanel()
		{
			var panel = base.GetContainerPanel();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			panel.Padding = ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 3, isInStandardDpi: true);

			return panel;
		}

		BookingEngineRateViewModel CurrentViewModel => CurrentDataItem as BookingEngineRateViewModel;
	}
}
