using Blazor.Diagrams;
using CargoWise.NetworkVisualisation.GUI.Models;
using Microsoft.AspNetCore.Components;

namespace CargoWise.NetworkVisualisation.GUI.Components;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in NetworkPort.razor")]
/// <summary>
/// Represents a component for rendering a network port in the network visualization.
/// <see cref="NetworkPort"/> is used to create a link between nodes.
/// </summary>
public partial class NetworkPort : ComponentBase
{
	/// <summary>
	/// Gets or sets the model associated with the network port and used to display the port type.
	/// </summary>
	[Parameter]
	public NetworkPortModel Port { get; set; }

	[CascadingParameter]
	public BlazorDiagram Diagram { get; set; }

	bool? CompatibilityIndicator { get; set; }

	string CompatibilityIndicatorImage => CompatibilityIndicator switch
	{
		true => Resources.LinkOk,
		false => Resources.LinkBad,
		null => null,
	};

	void OnMouseEnter()
	{
		if (Diagram is NCNDiagramModel diagramModel)
		{
			CompatibilityIndicator = diagramModel.TryCanAttachToInProgressLink(Port);
			InvokeAsync(StateHasChanged);
		}
	}

	void OnMouseLeave()
	{
		if (CompatibilityIndicator is not null)
		{
			CompatibilityIndicator = null;
			InvokeAsync(StateHasChanged);
		}
	}
}
