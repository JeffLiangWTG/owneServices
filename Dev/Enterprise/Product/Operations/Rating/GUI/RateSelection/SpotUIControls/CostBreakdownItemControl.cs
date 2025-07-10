using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	public partial class CostBreakdownItemControl : ItemTemplateControlBase
	{
		public CostBreakdownItemControl()
		{
			InitializeComponent();

#if DEBUG
			var suppress = new SuppressFormsLocalizedTestAttribute();
			TypeDescriptor.AddAttributes(chargeCodeDescriptionLabel, suppress);
#endif
		}

		CostBreakdownChargeViewModel CurrentViewModel => (CostBreakdownChargeViewModel)CurrentDataItem;

		protected override void ApplyExtraStaticOneWayBindings()
		{
			chargeCodeDescriptionLabel.Text = $"{CurrentViewModel?.ChargeCodeDescription}:";
		}

		void EnsureDescriptionHasMaximumSize()
		{
			// Give the description label maximum possible space, but still
			// allow for wrapping if necessary
			var width = costAndDescriptionPanel.Size.Width;
			var costLabelWidth = costStringLabel.Size.Width;

			chargeCodeDescriptionLabel.MaximumSize = ControlDpiScalingHelper.NewScaledSize(width - costLabelWidth, 0, isInStandardDpi: false);
		}

		void costAndDescriptionPanel_SizeChanged(object sender, System.EventArgs e)
		{
			EnsureDescriptionHasMaximumSize();
		}

		void costStringLabel_SizeChanged(object sender, System.EventArgs e)
		{
			EnsureDescriptionHasMaximumSize();
		}
	}
}
