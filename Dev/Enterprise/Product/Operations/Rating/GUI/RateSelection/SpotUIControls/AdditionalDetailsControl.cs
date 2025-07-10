using System;
using System.Collections;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	public partial class AdditionalDetailsControl : TemplateBasedControl
	{
		public AdditionalDetailsControl()
		{
			InitializeComponent();
		}

		protected override IItemTemplateControl CreateNewItemControl(object data)
		{
			return new AdditionalDetailItemControl();
		}

		protected override Type GetItemControlType(object data)
		{
			return typeof(AdditionalDetailItemControl);
		}

		protected override IEnumerable GetItemsDataCore()
		{
			return CurrentViewModel?.AdditionalDetails;
		}

		protected override void ApplyExtraSettingsOnEachControl(Control control)
		{
			base.ApplyExtraSettingsOnEachControl(control);

			control.TabStop = false;
		}

		protected override Panel GetContainerPanel()
		{
			var panel = base.GetContainerPanel();
			panel.AutoSize = true;
			panel.Dock = DockStyle.None;
			panel.Size = ControlDpiScalingHelper.NewScaledSize(500, 0);
			panel.MaximumSize = ControlDpiScalingHelper.NewScaledSize(500, 0);
			return panel;
		}

		BookingEngineRateViewModel CurrentViewModel => CurrentDataItem as BookingEngineRateViewModel;
	}
}
