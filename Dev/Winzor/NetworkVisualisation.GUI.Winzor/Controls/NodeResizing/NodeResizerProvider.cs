using System;
using Blazor.Diagrams.Core.Events;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Core.Positions.Resizing;
using CargoWise.NetworkVisualisation.GUI.Models;

namespace CargoWise.NetworkVisualisation.GUI.Controls.NodeResizing;

/// <summary>
/// Provides resizing behavior specific to node model.
/// </summary>
public abstract class NodeResizerProvider : ResizerProvider
{
	/// <summary>
	/// Gets or sets a value indicating whether the node resizer is visible.
	/// </summary>
	public bool IsVisible { get; internal set; }

	/// <summary>
	/// Handles pointer move event on a model.
	/// </summary>
	/// <param name="model">The model where the event occurred.</param>
	/// <param name="e">The event arguments associated with the pointer move event.</param>
	public override void OnPointerMove(Model model, PointerEventArgs e)
	{
		if (OriginalSize is null || OriginalPosition is null || NodeModel is null || Diagram is null)
		{
			return;
		}

		var deltaX = e.ClientX - LastClientX.Value;
		var deltaY = e.ClientY - LastClientY.Value;
		LastClientX = e.ClientX;
		LastClientY = e.ClientY;
		var result = base.CalculateNewSizeAndPosition(deltaX, deltaY);

		var adjustedResult = AdjustInCaseOfChildOrParentNodes(result.size, result.position);

		SetSizeAndPosition(adjustedResult.size, adjustedResult.position);
	}

	public override void OnPanChanged(double deltaX, double deltaY)
	{
		if (NodeModel != null)
		{
			var result = base.CalculateNewSizeAndPosition(deltaX, deltaY);
			var adjustedResult = AdjustInCaseOfChildOrParentNodes(result.size, result.position);

			SetSizeAndPosition(adjustedResult.size, adjustedResult.position);
		}
	}

	/// <summary>
	/// Handles the end of a resize event for a model.
	/// </summary>
	/// <param name="model">The model that was resized.</param>
	/// <param name="args">The event arguments associated with the resize event.</param>
	/// <remarks>
	/// If <see cref="ResizerProvider.NodeModel"/> is a NetworkNodeModel, this method triggers the Resized event for the model.
	/// </remarks>
	public override void OnResizeEnd(Model model, PointerEventArgs args)
	{
		if (NodeModel is NetworkNodeModel networkNodeModel)
		{
			networkNodeModel.TriggerResized();
		}
		base.OnResizeEnd(model, args);
	}

	protected (Size size, Point position) AdjustInCaseOfChildOrParentNodes(Size size, Point position)
	{
		var newHeight = size.Height;
		var newWidth = size.Width;
		var newPositionX = position.X;
		var newPositionY = position.Y;

		if (NodeModel is JobNodeModel jobNodeModel)
		{
			var parentNode = jobNodeModel.ParentNode;

			if (parentNode is not null)
			{
				if (newPositionX < parentNode.Position.X)
				{
					newPositionX = parentNode.Position.X;
					newWidth = OriginalSize.Width + (OriginalPosition.X - newPositionX);
				}
				else if (newPositionX + newWidth > parentNode.Position.X + parentNode.Size!.Width)
				{
					newWidth = parentNode.Size!.Width - (newPositionX - parentNode.Position.X);
				}

				if (newPositionY < parentNode.Position.Y + NetworkNodeConstants.CHILD_NODE_MIN_OFFSET_TOP)
				{
					newPositionY = parentNode.Position.Y + NetworkNodeConstants.CHILD_NODE_MIN_OFFSET_TOP;
					newHeight = OriginalSize.Height + (OriginalPosition.Y - newPositionY);
				}
				else if (newPositionY + newHeight > parentNode.Position.Y + parentNode.Size!.Height - NetworkNodeConstants.CHILD_NODE_MIN_OFFSET_BOTTOM)
				{
					newHeight = parentNode.Size!.Height - (newPositionY - parentNode.Position.Y + NetworkNodeConstants.CHILD_NODE_MIN_OFFSET_BOTTOM);
				}
			}

			var childNodes = jobNodeModel.GetAllChildNodes();

			var maxRightBound = double.MinValue;
			var maxBottomBound = double.MinValue;
			var minLeftBound = double.MaxValue;
			var minTopBound = double.MaxValue;

			foreach (var childNode in childNodes)
			{
				maxRightBound = Math.Max(maxRightBound, childNode.Position.X + childNode.Size!.Width);
				maxBottomBound = Math.Max(maxBottomBound, childNode.Position.Y + childNode.Size.Height + NetworkNodeConstants.CHILD_NODE_MIN_OFFSET_BOTTOM);
				minLeftBound = Math.Min(minLeftBound, childNode.Position.X);
				minTopBound = Math.Min(minTopBound, childNode.Position.Y - NetworkNodeConstants.CHILD_NODE_MIN_OFFSET_TOP);
			}

			if (newPositionX > minLeftBound)
			{
				newPositionX = minLeftBound;
				newWidth = OriginalSize.Width + (OriginalPosition.X - minLeftBound);
			}
			else if (newPositionX + newWidth < maxRightBound)
			{
				newWidth = maxRightBound - newPositionX;
			}

			if (newPositionY > minTopBound)
			{
				newPositionY = minTopBound;
				newHeight = OriginalSize.Height + (OriginalPosition.Y - minTopBound);
			}
			else if (newPositionY + newHeight < maxBottomBound)
			{
				newHeight = maxBottomBound - newPositionY;
			}
		}
		return (new Size(newWidth, newHeight), new Point(newPositionX, newPositionY));
	}
}
