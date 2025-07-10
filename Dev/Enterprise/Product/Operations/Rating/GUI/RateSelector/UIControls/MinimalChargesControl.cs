using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public partial class MinimalChargesControl : TemplateBasedControl
	{
		public MinimalChargesControl()
		{
			InitializeComponent();
		}

		protected ChargesViewModel CurrentViewModel => CurrentDataItem as ChargesViewModel;

		protected override Panel GetContainerPanel()
		{
			return pnlItemsCountainer;
		}

		protected override IEnumerable<object> GetItemsData()
		{
			return CurrentViewModel?.Charges;
		}

		protected override IItemTemplateControl CreateNewItemControl(object data)
		{
			return new ChargeToggleButton();
		}

		protected override void ApplyExtraSettingsOnEachControl(Control control)
		{
			base.ApplyExtraSettingsOnEachControl(control);

			control.Dock = DockStyle.Left;
		}
	}
}
