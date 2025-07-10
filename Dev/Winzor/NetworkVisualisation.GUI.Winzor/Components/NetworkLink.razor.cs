using System.Threading.Tasks;
using Blazor.Diagrams;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWise.NetworkVisualisation.GUI.Components;

using DiagramPoint = global::Blazor.Diagrams.Core.Geometry.Point;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in NetworkLink.razor")]
/// <summary>
/// Represents the NetworkLink component, used for connecting nodes in a network diagram.
/// Provides style of the link displayed and behavior on user input
/// </summary>
public partial class NetworkLink
{
	/// <summary>
	/// Represents the network diagram inside which the network link would be created, passed as a cascading parameter from the parent component
	/// </summary>
	[CascadingParameter]
	public BlazorDiagram Diagram { get; set; }

	/// <summary>
	/// Represents the model for NetworkLink, contains properties and methods for link creation between two nodes
	/// </summary>
	[Parameter]
	public NetworkLinkModel Link { get; set; }

	/// <summary>
	/// Represents the position of delete icon, displayed on the link on hover and click
	/// </summary>
	public DiagramPoint DeleteIconPosition { get; set; } = DiagramPoint.Zero;

	public bool DeleteIconVisible { get; set; }

	/// <summary>
	/// Handles the event when mouse pointer hovers over the link
	/// </summary>
	public void OnMouseEnter(MouseEventArgs args)
	{
		if (Diagram.Container != null && Link.TargetName is not null)
		{
			DeleteIconPosition = Diagram.GetRelativeMousePoint(args.ClientX, args.ClientY);
			DeleteIconVisible = true;
			Link.QuickHideDeleteIcon = false;
		}
	}

	/// <summary>
	/// Handles the event when mouse pointer leaves the link
	/// </summary>
	public void OnMouseLeave(MouseEventArgs args)
	{
		DeleteIconVisible = false;
	}

	async Task DeleteLink()
	{
		await ((NCNDiagramModel)Diagram).DeleteLinkFromViewModelAsync(Link);
	}

	string linkColor => Link.BackColor?.ToLower() ?? (NoResString)"black";

	string linkAppearance => Link.Appearance switch
	{
		ArrowAppearance.Normal => string.Empty,
		ArrowAppearance.Dotted => "4 8",
		ArrowAppearance.Dashed => "10 4",
		_ => string.Empty
	};

	/// <summary>
	/// Calculates the vertices of the arrow shaped polygon shown at link end
	/// </summary>
	public string ArrowVertices
	{
		get
		{
			var start = Link.Source.GetPlainPosition();
			var end = Link.Target.GetPlainPosition();
			if (Link.Source.Model is NetworkPortModel port
				&& port.Type == NetworkPortModel.PortType.Input)
			{
				(start, end) = (end, start);
			}
			var dy2 = (end.Y - start.Y) * (end.Y - start.Y);
			var dx2 = (end.X - start.X) * (end.X - start.X);
			double v1x, v1y, v2x, v2y;
			if (dy2 > dx2)
			{
				v1x = end.X - 6;
				v2x = end.X + 6;
				if (end.Y > start.Y)
				{
					v1y = end.Y - 20;
				}
				else
				{
					v1y = end.Y + 20;
				}
				v2y = v1y;
			}
			else
			{
				v1y = end.Y - 6;
				v2y = end.Y + 6;
				if (end.X > start.X)
				{
					v1x = end.X - 20;
				}
				else
				{
					v1x = end.X + 20;
				}
				v2x = v1x;
			}
			return $"{end.X},{end.Y} {v1x},{v1y} {v2x},{v2y}";
		}
	}
}
