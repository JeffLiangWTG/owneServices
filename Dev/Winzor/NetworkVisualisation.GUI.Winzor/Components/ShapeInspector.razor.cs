using System.Threading.Tasks;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Models;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Microsoft.AspNetCore.Components;
using NetworkVisualisation.GUI.Winzor.Models.Factories;

namespace CargoWise.NetworkVisualisation.GUI.Components;

/// <summary>
/// The shape inspector is used to display details of any selected Shape, it allows the user to continue to view the
/// details of the shape regardless of zoom and scroll.
/// </summary>
public partial class ShapeInspector : ComponentBase
{
	/// <summary>
	/// The <see cref="JobNetworkNodeViewModel"/>, <see cref="BufferViewModel"/>
	/// or <see cref="AnnotationViewModel"/> provided to the shape inspector to be displayed.
	/// </summary>
	[Parameter] public NodeViewModel ViewModel { get; set; }
	[CascadingParameter] public INetworkUserControl UserControl { get; set; }
	NodeViewModel oldViewModel;

	/// <summary>
	/// The current <see cref="JobNodeModel"/>, <see cref="BufferNodeModel"/> or <see cref="AnnotationNodeModel"/> being displayed by the ShapeInspector component.
	/// </summary>
	public NetworkNodeModel Model { get; private set; }

	protected override async Task OnParametersSetAsync()
	{
		await base.OnParametersSetAsync();

		if (ReferenceEquals(ViewModel, oldViewModel))
		{
			return;
		}

		Model?.Dispose();
		Model = null;
		oldViewModel = ViewModel;

		if (ViewModel is null)
		{
			return;
		}

		var factory = new NodeModelFactory(UserControl);
		Model = await factory.GetNodeModelAsync(ViewModel);
	}

	/// <summary>
	/// These instructions are displayed when no shape is selected.
	/// </summary>
	public string Instructions => Res.GetString("ED8213BE-CE56-4C6F-97FD-E2F8FA10C162", "Choose a single shape or an annotation to appear here (multiple selected shapes are not supported)");
}
