using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models.Base;
using CargoWise.NetworkVisualisation.GUI.Models;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI.Controls.NodeResizing;
internal class BottomLeftResizerProvider : NodeResizerProvider
{
	public override string Class => (NoResString)"networknode__resizer bottomleft";
	override public bool ShouldChangeXPositionOnResize => true;
	override public bool ShouldChangeYPositionOnResize => false;
	override public bool ShouldAddTotalMovedX => false;
	override public bool ShouldAddTotalMovedY => true;

	/// <summary>
	/// Gets the position of a model in the network.
	/// </summary>
	/// <param name="model">The model for which to get the position.</param>
	/// <returns>
	/// A <see cref="Point"/> representing the position of the model. If the model is a <see cref="NetworkNodeModel"/> and its size is not null.
	/// If the model is not a <see cref="NetworkNodeModel"/> or its size is null, this method returns null.
	/// </returns>
	public override Point GetPosition(Model model)
	{
		if (model is NetworkNodeModel nodeModel && nodeModel.Size is not null)
		{
			return new Point(nodeModel.Position.X - 5, nodeModel.Position.Y + nodeModel.Size.Height + 5);
		}
		return null;
	}
}
