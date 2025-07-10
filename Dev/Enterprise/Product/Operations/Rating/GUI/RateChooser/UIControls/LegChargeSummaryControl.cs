using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Rating.GUI.RateChooser.ViewModel;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateChooser.UIControls
{
	public partial class LegChargeSummaryControl : TemplateBasedControl
	{
		public LegChargeSummaryControl()
		{
			InitializeComponent();

			if (!DesignMode)
			{
				AutoSize = true;
				AutoSizeMode = AutoSizeMode.GrowAndShrink;
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
			return new ChargesSummaryControl();
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
				pnlLeg.Visible = Data.HeaderVisibility;
			}
		}

		LegChargesViewModel Data => CurrentDataItem as LegChargesViewModel;
	}
}
