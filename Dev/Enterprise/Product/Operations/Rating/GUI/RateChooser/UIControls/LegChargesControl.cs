using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Rating.GUI.RateChooser.ViewModel;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	public partial class LegChargesControl : TemplateBasedControl
	{
		public LegChargesControl()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				pnlItemsContainer.AutoSize = true;
			}

#if DEBUG
			TypeDescriptor.AddAttributes(lblLeg, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override Panel GetContainerPanel()
		{
			return pnlItemsContainer;
		}

		protected override IItemTemplateControl CreateNewItemControl(object data)
		{
			return new ChargesControl();
		}

		protected override void ApplyExtraSettingsOnEachControl(Control control)
		{
			control.Dock = DockStyle.Left;
			control.MinimumSize = ControlDpiScalingHelper.NewScaledSize(350, 0);
			control.Margin = ControlDpiScalingHelper.NewScaledPadding(0);
		}

		protected override bool ShouldRenderItem(object data)
		{
			if (data is ChargesViewModel viewModel)
			{
				return viewModel.Charges != null;
			}

			return false;
		}

		protected override IEnumerable GetItemsDataCore()
		{
			return Data?.Charges;
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			base.ApplyExtraStaticOneWayBindings();

			if (Data != null)
			{
				pnlTop.Visible = Data.HeaderVisibility;
			}
		}

		LegChargesViewModel Data => CurrentDataItem as LegChargesViewModel;
	}
}
