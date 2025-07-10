using System.Collections;
using System.Windows.Forms;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	public partial class RoutesControl : TemplateBasedControl
	{
		public RoutesControl()
		{
			InitializeComponent();
		}

		protected override IItemTemplateControl CreateNewItemControl(object data)
		{
			return new RouteItemTemplate();
		}

		protected override void ApplyExtraSettingsOnEachControl(Control control)
		{
			base.ApplyExtraSettingsOnEachControl(control);

			control.TabStop = false;
		}

		protected override Panel GetContainerPanel()
		{
			return pnlItemsContainer;
		}

		protected override IEnumerable GetItemsDataCore()
		{
			return Data?.TransportLegs;
		}

		IHasTransportLegs Data => CurrentDataItem as IHasTransportLegs;

		public bool ContainerPanelAutoSize
		{
			get { return pnlItemsContainer.AutoSize; }
			set { pnlItemsContainer.AutoSize = value; }
		}
	}
}
