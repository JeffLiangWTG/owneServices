using System.Collections.Generic;
using System.Linq;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Behaviors;
using Blazor.Diagrams.Core.Events;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using CargoWise.NetworkVisualisation.GUI.Models;

#nullable enable
namespace CargoWise.NetworkVisualisation.GUI;

/// <summary>
/// Represents  a class that handles dragging of nodes inside a network diagram.
/// This class inherits the class <see cref="Blazor.Diagrams.Core.Behaviors.DragMovablesBehavior"/>.
/// The default drag behavior of Blazor Diagrams is unregistered and `DragNodesBehaior` is registered in <see cref="NCNDiagramModel.SetupDragNodes"/>.
/// </summary>
public class DragNodesBehavior : DragMovablesBehavior
{
	record NodePositions(Point position)
	{
		public Dictionary<NetworkNodeModel, Point> ChildPositions { get; } = new Dictionary<NetworkNodeModel, Point>();
	}

	readonly Dictionary<NodeModel, NodePositions> ParentPositions;
	Dictionary<NodeModel, NodePositions> TempParentPositions { get; set; } 

	public DragNodesBehavior(Diagram diagram) : base(diagram)
	{
		ParentPositions = new ();
		TempParentPositions = new();
	}

	protected override void OnPointerDown(Model? model, PointerEventArgs e)
	{
		Reset();
		base.OnPointerDown(model, e);

		InitializeNodePositions();
	}

	void InitializeNodePositions()
	{
		foreach (var (movableModel, positions) in _initialPositions)
		{
			if (movableModel is NodeModel node)
			{
				if (!ParentPositions.ContainsKey(node))
				{
					ParentPositions.Add(node, new NodePositions(node.Position));
				}
			}
		}
	}

	void SelectChildShapes()
	{
		if (!_moved)
		{
			foreach (var node in ParentPositions.Keys.ToArray())
			{
				if (node is JobNodeModel jobNode)
				{
					var parent = jobNode.ParentNode;
					while (parent != null)
					{
						if (ParentPositions.ContainsKey(parent))
						{
							ParentPositions.Remove(jobNode);
							break;
						}

						parent = parent.ParentNode;
					}
				}
			}

			foreach (var (node, positions) in ParentPositions)
			{
				if (node is JobNodeModel jobNode)
				{
					foreach (var child in jobNode.GetAllChildNodes())
					{
						if (!child.Selected)
						{
							Diagram.SelectModel(child, false);
						}

						positions.ChildPositions[child] = child.Position;
					}
				}
			}
		}
	}

	protected override void MoveNodes(double deltaX, double deltaY)
	{
		SelectChildShapes();

		foreach (var (node, positions) in ParentPositions)
		{
			SetPosition(node, positions.position.X + deltaX, positions.position.Y + deltaY);
			deltaX = node.Position.X - positions.position.X;
			deltaY = node.Position.Y - positions.position.Y;
			foreach (var (childNode, childPosition) in positions.ChildPositions)
			{
				SetPosition(childNode, childPosition.X + deltaX, childPosition.Y + deltaY);
			}
			if (node is NetworkNodeModel networkNode)
			{
				networkNode.NodeMoving();
			}
		}
	}

	void SetPosition(NodeModel node, double x, double y)
	{
		if (node is NetworkNodeModel networkNode && networkNode.ParentNode != null)
		{
			x = Clamp(x, networkNode.Size?.Width, networkNode.ParentNode.Position.X, networkNode.ParentNode.Size?.Width);
			var parentY = networkNode.ParentNode.Position.Y + NetworkNodeConstants.CHILD_NODE_MIN_OFFSET_TOP;
			var parentH = networkNode.ParentNode.Size?.Height - NetworkNodeConstants.CHILD_NODE_MIN_OFFSET_TOP - NetworkNodeConstants.CHILD_NODE_MIN_OFFSET_BOTTOM;
			y = Clamp(y, networkNode.Size?.Height, parentY, parentH);
		}

		node.SetPosition(x, y);
	}

	double Clamp(double position, double? size, double? parentPosition, double? parentSize)
	{
		var clamped = position;

		if (size != null && parentPosition != null && parentSize != null)
		{
			if (position < parentPosition)
			{
				clamped = (double)parentPosition;
			}
			else if (position + size > parentPosition + parentSize)
			{
				clamped = (double)(parentPosition + parentSize - size);
			}
		}

		return clamped;
	}

	protected override void OnPointerUp(Model? model, PointerEventArgs e)
	{
		if (ParentPositions.Count == 0)
		{
			return;
		}

		if (_moved)
		{
			foreach (var (movable, childNodes) in ParentPositions)
			{
				movable.TriggerMoved();
				if (movable is JobNodeModel jobNode)
				{
					foreach (var child in jobNode.GetAllChildNodes())
					{
						if (childNodes.ChildPositions.ContainsKey(child))
						{
							child.TriggerMoved();
						}
					}
				}
			}
		}
		ParentPositions.Clear();
		base.OnPointerUp(model, e);
	}

	void Reset()
	{
		Diagram.SetPan(0, 0);
	}

	public void MoveSelectedNodes(double deltaX, double deltaY)
	{
		var selectedNode = Diagram.GetSelectedModels().FirstOrDefault();
		OnPointerDown(selectedNode, new PointerEventArgs(0, 0, 0, 0, false, false, false, 0, 0,0 ,0 ,0 ,0 ,"", false));
		TempParentPositions = new Dictionary<NodeModel, NodePositions>(ParentPositions);
		MoveNodes(deltaX, deltaY);
		OnPointerUp(selectedNode, new PointerEventArgs(0, 0, 0, 0, false, false, false, 0, 0, 0, 0, 0, 0, "", false));
	}

	public void UpdateMovedNodes()
	{
		foreach (var (movable, childNodes) in TempParentPositions)
		{
			movable.TriggerMoved();
			if (movable is JobNodeModel jobNode)
			{
				foreach (var child in jobNode.GetAllChildNodes())
				{
					if (childNodes.ChildPositions.ContainsKey(child))
					{
						child.TriggerMoved();
					}
				}
			}
		}
		TempParentPositions.Clear();
	}
	/// <summary>
	/// This will clear the ParentPositions dictionary.
	/// </summary>
	public override void Dispose()
	{
		base.Dispose();
		ParentPositions.Clear();
	}
}
#nullable disable
