using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI.Components;

/// <summary>
/// Represents a buffer node in a network, providing styles and status images based on the node's state.
/// </summary>
public partial class BufferNode : NetworkNode<BufferNodeModel>
{
	/// <summary>
	/// Gets the border style of the node. If the node is on the critical path, the border is red with solid 2px; otherwise, it is black with solid 1.5px.
	/// </summary>
	public string BorderStyle => Node.IsOnCriticalPath ? (NoResString)"border: 2px solid red;" : (NoResString)"border: 1.5px solid black;";

	/// <summary>
	/// Gets the notification warning style, which sets the border color based on the notification brush color.
	/// </summary>
	public string NotificationWarningStyle => $"border-color: {NotificationsBrushColor};";

	/// <summary>
	/// Gets the status image based on the current status of the node.
	/// </summary>
	public string StatusImage => Node.Status switch
	{
		WorkStatus.Startable => Resources.StatusStartable,
		WorkStatus.Suspended => Resources.StatusSuspended,
		WorkStatus.Working => Resources.StatusWorking,
		WorkStatus.Complete => Resources.StatusComplete,
		WorkStatus.Cancelled => Resources.StatusCancelled,
		_ => null
	};

	/// <summary>
	/// Gets the margin style for buffer details. Adds a top margin if the penetration percent label is not set.
	/// </summary>
	public string BufferDetailsMarginStyle => string.IsNullOrWhiteSpace(Node.PenetrationPercentLabel) ? (NoResString)"margin-top: 15px;" : "";
}
