using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Core.PathGenerators;
using SvgPathProperties;

namespace CargoWise.NetworkVisualisation.GUI.Utils;

using Point = global::Blazor.Diagrams.Core.Geometry.Point;

/// <summary>
/// Contains methods for generating svg path for network link in a diagram.
/// Used in <see cref="Models.NetworkLinkModel"/> to set the value of <see cref="PathGenerator"/> for generating link path.
/// </summary>
public class NetworkLinkPathGenerator : PathGenerator
{
	/// <summary>
	/// Generates svg path to draw the network link between two nodes in a diagram.
	/// Used in <see cref="Components.NetworkLink"/> component to provide the coordinates of svg path.
	/// </summary>
	/// <param name="diagram">Network diagram containing nodes and link.</param>
	/// <param name="link">Model class for network link</param>
	/// <param name="route">Array of points for the route between nodes</param>
	/// <param name="source">Coordinate point of the source node</param>
	/// <param name="target">Coordinate point of the target node</param>
	public override PathGeneratorResult GetResult(Diagram diagram, BaseLinkModel link, Point[] route, Point source, Point target)
	{
		var path = new SvgPath();
		path.AddMoveTo(source.X, source.Y);
		var dx2 = (target.X - source.X) * (target.X - source.X);
		var dy2 = (target.Y - source.Y) * (target.Y - source.Y);
		if (dy2 > dx2)
		{
			var midY = (source.Y + target.Y) / 2;
			path.AddCubicBezierCurve(source.X, midY, target.X, midY, target.X, target.Y);
		}
		else
		{
			var midX = (source.X + target.X) / 2;
			path.AddCubicBezierCurve(midX, source.Y, midX, target.Y, target.X, target.Y);
		}

		return new PathGeneratorResult(path, new SvgPath[] { path });
	}
}
