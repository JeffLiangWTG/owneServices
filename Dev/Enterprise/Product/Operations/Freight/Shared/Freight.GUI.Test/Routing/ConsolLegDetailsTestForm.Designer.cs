using System.Windows.Forms;

namespace Enterprise.Freight.GUI.Testing
{
	sealed partial class ConsolLegDetailsTestForm
	{
		new void InitializeComponent()
		{
			base.InitializeComponent();

			control = new ConsoLegDetailsControl();
			control.Dock = DockStyle.Fill;
			this.ClientSize = control.Size;
			this.Controls.Add(control);

			warningLabel = (Label)GetControl("WarningLabel");
			AirPanel = (Panel)GetControl("AirPanel");
			RoadPanel = (Panel)GetControl("RoadPanel");
			RailPanel = (Panel)GetControl("RailPanel");
			SeaPanel = (Panel)GetControl("SeaPanel");
			CargoOnlyControl = (CheckBox)GetControl("IsCargoOnlyCheckBox");

			this.CaptionRenderingEnabled = true;
		}
	}
}
