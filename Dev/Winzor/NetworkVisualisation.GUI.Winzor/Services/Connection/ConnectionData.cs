using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI.Services.Connection;

public class ConnectionData
{
	public string BackColor { get; internal set; }
	public string DisplayText { get; internal set; }
	public string DeleteTooltip { get; internal set; }
	public bool IsVisible { get; internal set; } = true;
	public bool IsResourceDependency { get; internal set; }
	public ArrowAppearance Appearance { get; internal set; }
}
