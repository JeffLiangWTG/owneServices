using System.Linq;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using DiagramPoint = Blazor.Diagrams.Core.Geometry.Point;

namespace CargoWise.NetworkVisualisation.GUI.Models;

/// <summary>
/// Represents the model for the NetworkPort Component.
/// It contains data and behaviors to render input and output ports in a node to enable node linking.
/// </summary>
/// <remarks>
/// Inherits PortModel of Blazor.Diagrams. It is used in <see cref="Components.NetworkPort"/>.
/// </remarks>
public class NetworkPortModel : PortModel
{
	/// <summary>
	/// Represents the list of port types available for a newtork port.
	/// </summary>
	public enum PortType { Input, Output }

	/// <summary>
	/// Gets the port type of the network port.
	/// </summary>
	public PortType Type { get; }

	/// <summary>
	/// Constructor for NetworkPortModel, it initializes the properties of the model and calls the base class PortModel's constructor.
	/// </summary>
	/// <param name="parent">model of the parent network node</param>
	/// <param name="type">type of the newtork port (input/output)</param>
	/// <param name="position">postion of the network port</param>
	public NetworkPortModel(NodeModel parent, PortType type, DiagramPoint position = null)
		: base(parent, type == PortType.Input ? PortAlignment.Left : PortAlignment.Right, position)
	{
		Type = type;
	}

	/// <summary>
	/// Determines whether a link can be attached to the port.
	/// </summary>
	/// <param name="link">network link to attach to the port</param>
	public override bool CanAttachTo(ILinkable link)
	{
		return link is NetworkPortModel port
			&& port.Parent != Parent
			&& port.Type != Type
			&& port.Enabled
			&& !Links.OfType<NetworkLinkModel>().Any(l => (l.Source.Model == port || l.Target.Model == port)
				&& !l.IsResourceDependency);
	}

	/// <summary>
	/// Calculates and sets the position of network port in the node after resizing the node.
	/// </summary>
	/// <param name="deltaWidth">change in node's width</param>
	/// <param name="deltaHeight">change in node's height</param>
	public override void SetPortPositionOnNodeSizeChanged(double deltaWidth, double deltaHeight)
	{
		switch (Alignment)
		{
			case PortAlignment.Right:
				Position = new DiagramPoint(Parent.Position.X + Parent.Size!.Width - NetworkNodeConstants.PORT_OFFSET_X, Parent.Position.Y + NetworkNodeConstants.PORT_OFFSET_Y);
				break;
			case PortAlignment.Left:
				Position = new DiagramPoint(Parent.Position.X + NetworkNodeConstants.PORT_OFFSET_X, Parent.Position.Y + NetworkNodeConstants.PORT_OFFSET_Y);
				break;
		}
	}
}
