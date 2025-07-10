using Blazor.Diagrams;
using Blazor.Diagrams.Components;
using Blazor.Diagrams.Components.Widgets;
using CargoWise.NetworkVisualisation.GUI;
using CargoWise.NetworkVisualisation.GUI.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace NetworkVisualisation.GUI.Winzor.Test.Scaffolding;

public class NCNDiagramForTest : ComponentBase
{
	[Parameter]
	public NCNDiagramModel? DiagramModel { get; set; }

	[Parameter]
	public INetworkUserControl? NetworkUserControl { get; set; }

	protected override void BuildRenderTree(RenderTreeBuilder builder)
	{
		builder.OpenComponent<CascadingValue<INetworkUserControl>>(0);
		builder.AddAttribute(1, nameof(CascadingValue<INetworkUserControl>.Value), NetworkUserControl!);
		builder.AddAttribute(2, nameof(CascadingValue<INetworkUserControl>.ChildContent), (RenderFragment)((builder2) =>
		{
			builder2.OpenComponent<CascadingValue<BlazorDiagram>>(3);
			builder2.AddAttribute(4, nameof(CascadingValue<BlazorDiagram>.Value), DiagramModel!);
			builder2.AddAttribute(5, nameof(CascadingValue<BlazorDiagram>.ChildContent), (RenderFragment)((builder3) =>
			{
				builder3.OpenComponent<DiagramCanvas>(6);
				builder3.AddAttribute(7, nameof(DiagramCanvas.Widgets), (RenderFragment)((widgetsBuilder) =>
				{
					widgetsBuilder.OpenComponent<SelectionBoxWidget>(8);
					widgetsBuilder.CloseComponent();
				}));
				builder3.CloseComponent();
			}));
			builder2.CloseComponent();
		}));
		builder.CloseComponent();
	}
}
