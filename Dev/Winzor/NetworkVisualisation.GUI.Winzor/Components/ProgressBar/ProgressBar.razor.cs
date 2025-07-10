using System;
using System.Threading.Tasks;
using CargoWise.NetworkVisualisation.GUI.Models;
using Microsoft.AspNetCore.Components;

namespace CargoWise.NetworkVisualisation.GUI;

/// <summary>
/// Represents a component for rendering the Progress Bar.
/// This partial class inherits from IDisposable class.
/// </summary>
public partial class ProgressBar : IDisposable
{
	/// <summary>
	/// This is the Model of type ProgressBarModel to track properties
	/// </summary>
	[Parameter]
	public ProgressBarModel Model { get; set; }

	string GetTransform() => $"translateX({-(100 - Math.Min(Model.PercentProgress, 1.0) * 100)}%)";

	protected override Task OnInitializedAsync()
	{
		Model.Changed += ViewModel_PropertyChanged;
		return base.OnInitializedAsync();
	}

	void ViewModel_PropertyChanged()
	{
		InvokeAsync(StateHasChanged);
	}

	/// <summary>
	/// This method disposes the PropertyChanged event handler on the Model
	/// </summary>
	public void Dispose()
	{
		Model.Changed -= ViewModel_PropertyChanged;
	}
}
