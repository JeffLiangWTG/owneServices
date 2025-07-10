using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	sealed partial class RoutingPluginControlTestForm
	{
		new void InitializeComponent()
		{
			base.InitializeComponent();
			control = new TestRoutingPluginControl();
			control.Dock = DockStyle.Fill;
			this.ClientSize = control.Size;
			this.Controls.Add(control);

			Grid = (ZGrid)GetControl("TransportsGrid");

			ReceivalAvailabilityPanel = (ZPanel)GetControl("ReceivalAvailabilityPanel");
			OriginDestinationPanel = (ZPanel)GetControl("OriginDestinationPanel");

			this.CaptionRenderingEnabled = true;
		}

	}
}
