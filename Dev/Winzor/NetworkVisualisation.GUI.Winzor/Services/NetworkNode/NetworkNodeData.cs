using System;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;

namespace CargoWise.NetworkVisualisation.GUI.Services.NetworkNode;

public class NetworkNodeData
{
	public Guid EntityPK { get; internal set; }
	public string DiagramName { get; internal set; }
	public string Notes { get; internal set; }
	public string NotesPlaceholder { get; internal set; }
	public string ToolTip { get; internal set; }
	public string DeleteTooltip { get; internal set; }
	public string EntityStateTooltip { get; internal set; }
	public int ZIndex { get; internal set; }
	public bool IsSelected { get; internal set; }
	public bool HasNotifications { get; internal set; }
	public bool HasNotificationsForState { get; internal set; }
	public bool IsLocked { get; internal set; }
	public bool HasProgressBar { get; internal set; }
	public double X { get; internal set; }
	public double Y { get; internal set; }
	public double Width { get; internal set; }
	public double Height { get; internal set; }
	public double MinimunWidth { get; } = NodeViewModel.MinAllowedNodeWidth;
	public double MinimumHeight { get; } = NodeViewModel.MinAllowedNodeHeight;
	public NodeColors StatusColors { get; internal set; }
	public EntityState EntityState { get; internal set; }
	public INetworkEntity Entity { get; internal set; }
	public NodeFontWeight StatusTextWeight { get; internal set; }
	public System.Drawing.Color ForegroundColor { get; internal set; }
}
