#nullable enable
using System.Threading.Tasks;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Controls;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Extensions;
using Point = Blazor.Diagrams.Core.Geometry.Point;
using PointerEventArgs = Blazor.Diagrams.Core.Events.PointerEventArgs;

namespace CargoWise.NetworkVisualisation.GUI.Controls.NodeResizing;

/// <summary>
/// Represents a control that can resize nodes.
/// This class inherits from the ExecutableControl class.
/// </summary>
public class NodeResizeControl : ExecutableControl
{
	readonly NodeResizerProvider nodeResizerProvider;
	readonly IGlobalEventProvider globalEventProvider;

	/// <summary>
	/// Initializes a new instance of the NodeResizeControl class.
	/// </summary>
	/// <param name="resizerProvider">The provider that handles the resizing of nodes.</param>
	/// <param name="globalEventProvider">The provider for global events.</param>
	public NodeResizeControl(NodeResizerProvider nodeResizerProvider, IGlobalEventProvider globalEventProvider)
	{
		this.nodeResizerProvider = nodeResizerProvider;
		this.globalEventProvider = globalEventProvider;
	}

	/// <summary>
	/// Gets the position of a model in the network.
	/// </summary>
	/// <param name="model">The model for which to get the position.</param>
	/// <returns>A <see cref="Point"/> or null.</returns>
	public override Point? GetPosition(Model model) => nodeResizerProvider.GetPosition(model);

	/// <summary>
	/// The class of the resizer provider.
	/// </summary>
	public string? Class => nodeResizerProvider.Class;

	public bool IsVisible
	{
		get => nodeResizerProvider.IsVisible;
		set => nodeResizerProvider.IsVisible = value;
	}

	/// <summary>
	/// Handles the pointer down event on a diagram.
	/// </summary>
	/// <param name="diagram">The diagram where the event occurred.</param>
	/// <param name="model">The model associated with the event.</param>
	/// <param name="e">The event arguments associated with the pointer down event.</param>
	/// <remarks>
	/// This method registers global pointer events, starts the resize operation on the resizer provider, and adds event handlers for the PointerMove, PanChanged, and PointerUp events on the diagram.
	/// </remarks>
	public override async ValueTask OnPointerDown(Diagram diagram, Model model, PointerEventArgs e)
	{
		await globalEventProvider.RegisterGlobalPointerEvents((args) => OnPointerMove(args.ToCore()), (args) => OnResizeEnd(diagram, model, args.ToCore()));
		nodeResizerProvider.OnResizeStart(diagram, model, e);
		diagram.PointerMove += nodeResizerProvider.OnPointerMove;
		diagram.PanChanged += nodeResizerProvider.OnPanChanged;
		diagram.PointerUp += (nodeModel, args) => OnResizeEnd(diagram, nodeModel, args);
	}

	Task OnPointerMove(PointerEventArgs args)
	{
		nodeResizerProvider.OnPointerMove(null, args);
		return Task.CompletedTask;
	}

	Task OnResizeEnd(Diagram diagram, Model? model, PointerEventArgs args)
	{
		nodeResizerProvider.OnResizeEnd(model, args);
		diagram.PointerMove -= nodeResizerProvider.OnPointerMove;
		diagram.PointerUp -= nodeResizerProvider.OnResizeEnd;
		diagram.PanChanged -= nodeResizerProvider.OnPanChanged;
		return Task.CompletedTask;
	}
}
