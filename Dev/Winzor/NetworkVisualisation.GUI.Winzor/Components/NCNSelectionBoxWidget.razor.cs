using System;
using Blazor.Diagrams;
using Blazor.Diagrams.Core.Geometry;
using CargoWise.NetworkVisualisation.GUI.Controls;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components;

namespace CargoWise.NetworkVisualisation.GUI.Components;

#nullable enable
public partial class NCNSelectionBoxWidget : IDisposable
{
	Rectangle? _selectionBounds;
	NCNSelectionBoxBehavior? _selectionBoxBehavior;

	[CascadingParameter] public BlazorDiagram BlazorDiagram { get; set; } = null!;

	[Parameter] public string Background { get; set; } = (NoResString)"rgb(110 159 212 / 25%)";

	public void Dispose()
	{
		if (_selectionBoxBehavior is not null)
		{
			_selectionBoxBehavior.SelectionBoundsChanged -= SelectionBoundsChanged;
		}
	}

	protected override void OnInitialized()
	{
		_selectionBoxBehavior = BlazorDiagram.GetBehavior<NCNSelectionBoxBehavior>();
		if (_selectionBoxBehavior is not null)
		{
			_selectionBoxBehavior.SelectionBoundsChanged += SelectionBoundsChanged;
		}
	}

	void SelectionBoundsChanged(object? sender, Rectangle? bounds)
	{
		_selectionBounds = bounds;
		InvokeAsync(StateHasChanged);
	}

	string GenerateStyle()
	{
		return FormattableString.Invariant(
			$"position: absolute; background: {Background}; top: {_selectionBounds!.Top}px; left: {_selectionBounds.Left}px; width: {_selectionBounds.Width}px; height: {_selectionBounds.Height}px;");
	}
}
