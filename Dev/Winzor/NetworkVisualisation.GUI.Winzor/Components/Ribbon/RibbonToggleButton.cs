using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI;

public class RibbonToggleButton : RibbonButton
{
	protected override string ButtonType => (NoResString)"ribbontogglebutton " + base.ButtonType;

	protected override bool Checked => (bool)(ViewModel?.IsChecked);
}
