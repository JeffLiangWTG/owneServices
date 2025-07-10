using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.GUI
{
	public partial class RefTransitTimeForm : ZForm, IAllowTabBackwardBetweenSomeOfMyChildren
	{
		public RefTransitTimeForm(RefTransitTime transitTime)
			: base(transitTime)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
			ControllerID = ControllerID ?? ControllerIDs.RefTransitTime;
			originZoneGroupBox.AllowOutsideOfParent();
			destinationZoneGroupBox.AllowOutsideOfParent();

			if (!DesignModeFinder.IsDesigning)
			{
				SetDeliveryDueDateVisibility();
			}
		}

		void SetDeliveryDueDateVisibility()
		{
			panel1.Visible = FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive;

			if (!FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive)
			{
				var panelHeight = panel1.Height;
				this.Height -= panelHeight;
				this.postingButtonsUserControl.Height -= panelHeight;
				this.transitTimeTabPage.Height -= panelHeight;
				this.mainTabControl.Height -= panelHeight;
				this.transitTimeTabPage.Controls.Remove(this.panel1);
			}
			else
			{
				this.transitTimeTabPage.Controls.Remove(this.transitTimeGroupBox);
			}
		}

		protected override ZTabControl TopLevelTabControl
		{
			get { return mainTabControl; }
		}

		public bool AllowTabBackward(Control control, Control previousControl)
		{
			return control == modeDropEdit && previousControl == serviceLevelDropEdit;
		}
	}
}
