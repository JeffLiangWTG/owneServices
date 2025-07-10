#nullable enable
using System.Threading.Tasks;
using CargoWise.NetworkVisualisation.Integration;
using Microsoft.AspNetCore.Components;

namespace CargoWise.NetworkVisualisation.GUI;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in ControlBar.razor")]
/// <summary>
/// Represents a component for rendering the Control Bar.
/// This partial class inherits from IDisposable class.
/// </summary>
public partial class ControlBar
{
	/// <summary>
	/// This is a reference to the network user control component
	/// </summary>
	[CascadingParameter]
	public INetworkUserControl? UserControl { get; set; }

	/// <summary>
	/// This is property gets and sets zoom to the UI
	/// </summary>
	[Parameter]
	public int Zoom { get; set; }

	/// <summary>
	/// This holds the event handler for zoom changed event
	/// </summary>
	[Parameter]
	public EventCallback<int> ZoomChanged { get; set; }

	/// <summary>
	/// This holds the event handler for saving previous zoom
	/// </summary>
	[Parameter]
	public EventCallback BeforeZoomChanged { get; set; }

	/// <summary>
	/// This contains the action to refresh the UI
	/// </summary>
	[Parameter]
	public INetworkAction? RefreshAction { get; set; }

	/// <summary>
	/// This contains the action to open the NCN in a new pop up window
	/// </summary>
	[Parameter]
	public INetworkAction? PopoutAction { get; set; }

	/// <summary>
	/// This contains the action to fit all nodes to the view port
	/// </summary>
	[Parameter]
	public INetworkAction? FitAction { get; set; }

	/// <summary>
	/// This contains the action to Fill the entire content area to the view port
	/// </summary>
	[Parameter]
	public INetworkAction? FillAction { get; set; }

	/// <summary>
	/// This contains the action to set the zoom level to 100%
	/// </summary>
	[Parameter]
	public INetworkAction? OneHundredPercentAction { get; set; }

	/// <summary>
	/// This contains the action to zoom out by 10%
	/// </summary>
	[Parameter]
	public INetworkAction? ZoomOutAction { get; set; }

	/// <summary>
	/// This contains the action to zoom in by 10%
	/// </summary>
	[Parameter]
	public INetworkAction? ZoomInAction { get; set; }

	async Task ZoomChangedAsync()
	{
		await ZoomChanged.InvokeAsync(Zoom);
	}

	async Task BeforeZoomChangedAsync()
	{
		await BeforeZoomChanged.InvokeAsync();
	}

	async Task PerformActionAsync(INetworkAction action)
	{
		if (UserControl is not null)
		{
			await UserControl.InvokeWinzorDispatcherAsync(() =>
			{
				action.Execute();
			});
		}
	}
}
