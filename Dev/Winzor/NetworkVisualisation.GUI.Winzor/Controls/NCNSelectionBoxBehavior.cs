using System;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Behaviors.Base;
using Blazor.Diagrams.Core.Events;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models.Base;
using CargoWise.NetworkVisualisation.GUI.Extensions;
using CargoWise.NetworkVisualisation.GUI.Models;

namespace CargoWise.NetworkVisualisation.GUI.Controls;
#nullable enable

class NCNSelectionBoxBehavior : DragBehavior
{
	Point? _initialPoint;
	Point? _currentPoint;
	Point? _currentPanPoint;

	public event EventHandler<Rectangle?>? SelectionBoundsChanged;

	public NCNSelectionBoxBehavior(Diagram diagram)
		: base(diagram)
	{
		Diagram.PointerDown += OnPointerDown;
		Diagram.PointerMove += OnPointerMove;
		Diagram.PointerUp += OnPointerUp;
		Diagram.PanChanged += OnPanChanged;
	}

	public override void Dispose()
	{
		Diagram.PointerDown -= OnPointerDown;
		Diagram.PointerMove -= OnPointerMove;
		Diagram.PointerUp -= OnPointerUp;
		Diagram.PanChanged -= OnPanChanged;
	}

	public override bool IsBehaviorEnabled(PointerEventArgs e)
	{
		if (e.IsMiddleButton())
		{
			return false;
		}

		return base.IsBehaviorEnabled(e);
	}

	protected override void OnPointerDown(Model? model, PointerEventArgs e)
	{
		if (SelectionBoundsChanged is null || model != null || !IsBehaviorEnabled(e))
		{
			return;
		}

		Diagram.SuspendRefresh = true;

		_initialPoint = new Point(e.ClientX, e.ClientY);
		_currentPoint = new Point(e.ClientX, e.ClientY);
		_currentPanPoint = Point.Zero;
	}

	protected override void OnPointerUp(Model? model, PointerEventArgs e)
	{
		SelectNodesInBounds();

		_initialPoint = null;
		_currentPoint = null;
		_currentPanPoint = null;

		SelectionBoundsChanged?.Invoke(this, null);

		Diagram.SuspendRefresh = false;
	}

	protected override void OnPointerMove(Model? model, PointerEventArgs e)
	{
		if (_initialPoint == null || _currentPanPoint == null)
		{
			return;
		}

		_currentPoint = new Point(e.ClientX, e.ClientY);

		var newPoint = CalculateNewPoint();

		var bounds = GetBounds(newPoint.X, newPoint.Y);
		UpdateSelectionBox(bounds);
		SelectNodeInClient(bounds);
	}

	public void OnPanChanged(double deltaX, double deltaY)
	{
		if (_initialPoint == null || _currentPoint == null || _currentPanPoint == null)
		{
			return;
		}

		_currentPanPoint = _currentPanPoint.Add(deltaX, deltaY);

		var newPoint = CalculateNewPoint();
		var bounds = GetBounds(newPoint.X, newPoint.Y);
		UpdateSelectionBox(bounds);
		SelectNodeInClient(bounds);
	}

	Point CalculateNewPoint()
	{
		if (_currentPoint == null || _currentPanPoint == null)
		{
			return _currentPoint ?? _currentPanPoint ?? new Point(0, 0);
		}
		return new Point(_currentPoint.X + _currentPanPoint.X, _currentPoint.Y + _currentPanPoint.Y);
	}

	void UpdateSelectionBox(Rectangle bounds)
	{
		if (_initialPoint == null)
		{
			return;
		}

		SelectionBoundsChanged?.Invoke(this, bounds);
	}

	Rectangle GetBounds(double clientX, double clientY)
	{
		var start = Diagram.GetRelativePoint(_initialPoint!.X, _initialPoint.Y);
		var end = Diagram.GetRelativePoint(clientX, clientY);
		var (sX, sY) = (Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
		var (eX, eY) = (Math.Max(start.X, end.X), Math.Max(start.Y, end.Y));
		return new Rectangle(sX, sY, eX, eY);
	}

	void SelectNodeInClient(Rectangle bounds)
	{
		if (_initialPoint == null)
		{
			return;
		}

		foreach (var node in Diagram.Nodes)
		{
			if (node is NetworkNodeModel nwnm)
			{
				var nodeBounds = node.GetBounds();
				if (nodeBounds == null)
				{
					continue;
				}
				nodeBounds = new Rectangle(nodeBounds!.Left * Diagram.Zoom, nodeBounds.Top * Diagram.Zoom, nodeBounds.Right * Diagram.Zoom, nodeBounds.Bottom * Diagram.Zoom);

				var isOverlapping = bounds.Overlap(nodeBounds);
				nwnm.ShowNodeAsSelected(isOverlapping);
			}
		}
	}

	void SelectNodesInBounds()
	{
		if (_initialPoint == null)
		{
			return;
		}

		foreach (var node in Diagram.Nodes)
		{
			if (node is NetworkNodeModel nwnm)
			{
				var isSelected = nwnm.IsSelected;
				nwnm.ShowNodeAsSelected(nwnm.Selected);

				if (isSelected)
				{
					Diagram.SelectModel(node, false);
				}
				else
				{
					Diagram.UnselectModel(node);
				}
			}
		}
	}
}
