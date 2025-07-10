using System.Drawing;

namespace CargoWise.NetworkVisualisation.GUI.Services.ProgressBarModel;

public class ProgressBarModelData
{
	public int LowerBarHeight { get; internal set; }
	public double Percent { get; internal set; }
	public Color Background { get; internal set; }
	public string LowerBarTooltip { get; internal set; }
}
