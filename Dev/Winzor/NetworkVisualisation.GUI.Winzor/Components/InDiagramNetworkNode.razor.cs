#nullable enable
using System.ComponentModel;
using System.Threading.Tasks;
using Blazor.Diagrams;
using Blazor.Diagrams.Core.Controls;
using Blazor.Diagrams.Core.Models;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.NetworkVisualisation.GUI.Extensions;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.GUI.Services.NetworkNode;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CargoWise.NetworkVisualisation.GUI.Components;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in InDiagramNetworkNode.razor")]
/// <summary>
/// Represents a component that handles the rendering and behaviours of network nodes within a diagram.
/// </summary>
public partial class InDiagramNetworkNode<TModel, TComponent> : NetworkNodeBase<TModel> where TModel : NetworkNodeModel where TComponent : NetworkNode<TModel>
{
	[Inject]
	public IMenuDisplayer? MenuDisplayer { get; set; }

	[CascadingParameter]
	public BlazorDiagram? Diagram { get; set; }

	[CascadingParameter]
	public INetworkUserControl? NetworkUserControl { get; set; }

	[CascadingParameter]
	public IDiagramAreaUserControl? DiagramAreaUserControl { get; set; }

	[Inject]
	IJSRuntime? JSRuntime { get; set; }

	protected override void OnAfterRender(bool firstRender)
	{
		if (firstRender)
		{
			Node!.Moving += OnMoving;
		}
	}

	void OnMoving(NodeModel model)
	{
		QuickHideDeleteIcon = true;
	}

	protected override void OnPropertyChangedFromServer(object? sender, PropertyChangedEventArgs e)
	{
		switch (e.PropertyName)
		{
			case nameof(NetworkNodeData.IsSelected):
				HandleIsSelectedChangedFromServer();
				break;

			case nameof(NetworkNodeData.EntityState):
			case nameof(NetworkNodeData.IsLocked):
				UpdateResizerControls();
				break;

			default:
				base.OnPropertyChangedFromServer(sender, e);
				break;
		}
	}

	void HandleIsSelectedChangedFromServer()
	{
		if (Node is null)
		{
			return;
		}

		if (Node.IsSelected)
		{
			Diagram?.SelectModel(Node, false);
		}
		else
		{
			Diagram?.UnselectModel(Node);
		}
	}

	void UpdateResizerControls()
	{
		if (Node is null || Diagram is null)
		{
			return;
		}

		if (Node.Locked)
		{
			Diagram.Controls.RemoveFor(Node);
			return;
		}

		if (Node.BottomRightResizer is null || Node.TopLeftResizer is null || Node.TopRightResizer is null || Node.BottomLeftResizer is null)
		{
			return;
		}

		Diagram.Controls.AddFor(Node, ControlsType.AlwaysOn)
			.Add(Node.BottomRightResizer)
			.Add(Node.TopLeftResizer)
			.Add(Node.TopRightResizer)
			.Add(Node.BottomLeftResizer);
	}

	/// <summary>
	/// Toggles the display of <see cref="DeleteIcon"/> when mouse enters or leave the boundaries of a node.
	/// </summary>
	public bool DeleteIconVisible { get; set; }

	protected void OnMouseEnter(WebMouseEventArgs args)
	{
		DeleteIconVisible = true;
		QuickHideDeleteIcon = false;
	}

	protected void OnMouseLeave()
	{
		DeleteIconVisible = false;
		QuickHideDeleteIcon = false;
	}

	protected async Task OpenContextMenuAsync(WebMouseEventArgs args)
	{
		if (Node is not null)
		{
			await SafeClipboard.FetchClipboardDataAsync(JSRuntime);
			await MenuDisplayer.ShowContextMenuAsync(args, await Node.GetContextMenuItemsAsync(), ContextMenuActionAsync);
		}
	}

	async Task ContextMenuActionAsync(INetworkAction action, WebMouseEventArgs args)
	{
		if (DiagramAreaUserControl is not null)
		{
			await DiagramAreaUserControl.ExecuteContextMenuActionAsync(action, args);
		}
	}

	protected async Task OnDoubleClickAsync()
	{
		if (Node is not null)
		{
			await Node.View();
		}
	}

	async Task DeleteNode()
	{
		if (Diagram is not null && Node is not null)
		{
			await ((NCNDiagramModel)Diagram).DeleteNodeFromViewModelAsync(Node);
		}
	}

	double Height => (Node?.Size?.Height ?? 0);

	double Width => (Node?.Size?.Width ?? 0);

	int ZIndex => Node?.ZIndex ?? 0;

	string networkNodeStyle => $"z-index: {ZIndex}; cursor: {cursorStyle};";

	string networkNodeWrapperStyle => $"width: {Width}px;height: {Height}px;";

	string cursorStyle => (Node?.Locked ?? false) ? (NoResString)"default" : (NoResString)"move";

	string ClassName => Node switch
	{
		JobNodeModel => (NoResString)"jobnode",
		AnnotationNodeModel => (NoResString)"annotationnode",
		BufferNodeModel => (NoResString)"buffernode",
		_ => string.Empty
	};

	public bool QuickHideDeleteIcon { get; private set; }

	RenderFragment RenderNode() => builder =>
	{
		builder.OpenComponent<TComponent>(0);
		builder.AddAttribute(1, nameof(Node), Node);
		builder.CloseComponent();
	};
}
