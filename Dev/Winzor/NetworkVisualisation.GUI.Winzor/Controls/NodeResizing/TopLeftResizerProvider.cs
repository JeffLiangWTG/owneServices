using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.NetworkVisualisation.GUI.Controls.NodeResizing;
internal class TopLeftResizerProvider : NodeResizerProvider
{
	public override string Class => (NoResString)"networknode__resizer topleft";
	override public bool ShouldChangeXPositionOnResize => true;
	override public bool ShouldChangeYPositionOnResize => true;
	override public bool ShouldAddTotalMovedX => false;
	override public bool ShouldAddTotalMovedY => false;

	/// <summary>
	/// Gets the position of a model in the network.
	/// </summary>
	/// <param name="model">The model for which to get the position.</param>
	/// <returns>
	/// A <see cref="Point"/> representing the position of the model. If the model is a <see cref="NodeModel"/> and its size is not null.
	/// If the model is not a <see cref="NodeModel"/> or its size is null, this method returns null.
	/// </returns>
	public override Point GetPosition(Model model)
	{
		if (model is NodeModel nodeModel && nodeModel.Size is not null)
		{
			return new Point(nodeModel.Position.X - 5, nodeModel.Position.Y - 5);
		}
		return null;
	}
}
