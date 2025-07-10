#nullable enable
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.NetworkVisualisation.GUI.Extensions;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components;

namespace CargoWise.NetworkVisualisation.GUI;

/// <summary>
/// Represents a button in a ribbon control with asynchronous action handling and support for child buttons.
/// </summary>
public partial class RibbonButton : IDisposable
{
	/// <summary>
	/// Gets or sets the view model associated with this ribbon button.
	/// </summary>
	[Parameter]
	public RibbonButtonViewModel? ViewModel { get; set; }
	RibbonButtonViewModel? viewModel;

	/// <summary>
	/// Gets or sets interface for network user control that handles network actions.
	/// </summary>
	[CascadingParameter]
	public INetworkUserControl? NetworkUserControl { get; set; }

	protected ElementReference ribbonButtonRef;

	protected bool HasChildButtons => ViewModel?.ChildButtons?.Count() > 0;

	protected virtual async Task OnClickAsync(WebMouseEventArgs args)
	{
		if (ViewModel is not null && !HasChildButtons && ViewModel.IsEnabled)
		{
			await ExecuteActionAsync(ViewModel.Action);
		}
	}

	protected async Task ExecuteActionAsync(INetworkAction action)
	{
		if (NetworkUserControl is not null)
		{
			await NetworkUserControl.InvokeWinzorDispatcherAsync(() => NetworkUserControl.ExecuteRibbonAction(action));
		}
	}

	protected virtual string ButtonType => (NoResString)"ribbonbutton";

	protected virtual bool Checked => false;

	string GetImageLayoutStyle()
	{
		return ViewModel?.ImageLayout switch
		{
			RibbonImageLayout.SmallImageOnly => "ribbonbutton--small",
			RibbonImageLayout.BothLargeAndSmallImages => "ribbonbutton--large",
			_ => string.Empty,
		};
	}

	protected override void OnParametersSet()
	{
		if (ViewModel != viewModel)
		{
			viewModel?.RemovePropertyChangedHandler(ViewModel_PropertyChanged);
			viewModel = ViewModel;
			viewModel?.AddPropertyChangedHandler(ViewModel_PropertyChanged);
		}
		base.OnParametersSet();
	}

	void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(RibbonButtonViewModel.IsEnabled))
		{
			NetworkUserControl?.InvokeRenderDispatcher(() => InvokeAsync(StateHasChanged));
		}
	}

	/// <summary>
	/// Disposes resources used by the <see cref="RibbonButton"/>.
	/// </summary>
	public void Dispose()
	{
		ViewModel?.RemovePropertyChangedHandler(ViewModel_PropertyChanged);
	}
}
