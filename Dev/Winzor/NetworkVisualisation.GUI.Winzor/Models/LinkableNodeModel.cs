using CargoWise.NetworkVisualisation.GUI.Services.LinkableNode;
using CargoWise.NetworkVisualisation.GUI.Services.Utils;
using DiagramPoint = Blazor.Diagrams.Core.Geometry.Point;
using Size = Blazor.Diagrams.Core.Geometry.Size;

namespace CargoWise.NetworkVisualisation.GUI.Models;

/// <summary>
/// Abstract base class for a node model that can be linked with other nodes.
/// </summary>
public abstract class LinkableNodeModel : NetworkNodeModel
{
	ILinkableNodeService Service { get; }
	LinkableNodeData Data { get; }

	protected LinkableNodeModel(ILinkableNodeService service, bool disableDragNewLink = false) : base(service)
	{
		Service = service;

		Input = new NetworkPortModel(this, NetworkPortModel.PortType.Input, GetPortPosition(NetworkPortModel.PortType.Input)) { Initialized = true, Locked = disableDragNewLink, Enabled = !disableDragNewLink };
		Output = new NetworkPortModel(this, NetworkPortModel.PortType.Output, GetPortPosition(NetworkPortModel.PortType.Output)) { Initialized = true, Locked = disableDragNewLink, Enabled = !disableDragNewLink };
		AddPort(Input);
		AddPort(Output);

		Data = Service.GetLinkableNodeData();
	}

	public NetworkPortModel Input { get; }
	public NetworkPortModel Output { get; }

	public bool IsOnCriticalPath => Data.IsOnCriticalPath;

	public override void SetPosition(double x, double y)
	{
		base.SetPosition(x, y);

		Input.SetPortPositionOnNodeSizeChanged(0, 0);
		Input.RefreshLinks();
		Output.SetPortPositionOnNodeSizeChanged(0, 0);
		Output.RefreshLinks();
		Refresh();
		RefreshLinks();
	}

	public override void SetSize(Size size)
	{
		var deltaWidth = size.Width - Size.Width;
		var deltaHeight = size.Height - Size.Height;

		base.SetSize(size);

		Input.SetPortPositionOnNodeSizeChanged(deltaWidth, deltaHeight);
		Input.RefreshLinks();
		Output.SetPortPositionOnNodeSizeChanged(deltaWidth, deltaHeight);
		Output.RefreshLinks();
		Refresh();
		RefreshLinks();
	}

	DiagramPoint GetPortPosition(NetworkPortModel.PortType portType)
	{
		return portType == NetworkPortModel.PortType.Input
			? new DiagramPoint(Position.X + NetworkNodeConstants.PORT_OFFSET_X, Position.Y + NetworkNodeConstants.PORT_OFFSET_Y)
			: new DiagramPoint(Position.X + Size!.Width - NetworkNodeConstants.PORT_OFFSET_X, Position.Y + NetworkNodeConstants.PORT_OFFSET_Y);
	}

	public virtual string GetName() => string.Empty;

	protected override void InitializePropertiesUpdater(PropertiesUpdater updater) => base.InitializePropertiesUpdater(
		updater
			.Add(nameof(LinkableNodeData.IsOnCriticalPath), (v) => Properties.TryUpdate((bool)v, Data.IsOnCriticalPath, (value) => Data.IsOnCriticalPath = value))
	);
}
