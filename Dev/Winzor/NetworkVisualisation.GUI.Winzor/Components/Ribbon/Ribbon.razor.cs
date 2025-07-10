#nullable enable
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Extensions;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;

namespace CargoWise.NetworkVisualisation.GUI;

/// <summary>
/// Represents a ribbon control that displays a series of buttons across the top of the network diagram.
/// </summary>
public partial class Ribbon
{
	/// <summary>
	/// Gets or sets the viewmodel associated with the ribbon.
	/// </summary>
	[Parameter]
	public RibbonViewModel? ViewModel { get; set; }

	/// <summary>
	/// Gets or sets interface for network user control that handles network actions. 
	/// </summary>
	[CascadingParameter]
	public INetworkUserControl? NetworkUserControl { get; set; }

	/// <summary>
	/// Gets or sets the interface for displaying menus.
	/// </summary>
	[Inject]
	public IMenuDisplayer? MenuDisplayer { get; set; }

	/// <summary>
	/// Gets or sets the interface for interacting with clipboard functionality through JavaScript interop.
	/// </summary>
	[Inject]
	public IClipboardJSInterop? ClipboardInterop { get; set; }

	RibbonTabViewModel? SelectedTab => ViewModel?.Tabs.Skip(selectedTabIndex).FirstOrDefault() ?? ViewModel?.Tabs.FirstOrDefault();
	int selectedTabIndex;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Analyzer runner incorrectly detecting.")]
	string SearchText = string.Empty;
	readonly string SearchTextPlaceholder = Res.GetString("df1b702e-b74e-4a6a-9df9-4677d97749fa", "Search") + "...";
	internal bool isPerformingSearch;

	internal SearchFinderViewModel? SearchFinder;

	internal ElementReference SearchBoxElementReference;
	internal ElementReference RibbonElementReference;

	internal async Task UpdateSearchTermAsync()
	{
		if (ViewModel is not null && NetworkUserControl is not null)
		{
			await NetworkUserControl.InvokeWinzorDispatcherAsync(() =>
			{
				SearchFinder?.ResetSearchState();
			});
		}
	}

	async Task UpdateSearchAsync(WebKeyboardEventArgs args)
	{
		if (args.Key == "Enter" && !isPerformingSearch)
		{
			if (ViewModel is not null && (!string.IsNullOrEmpty(SearchText)) && NetworkUserControl is not null)
			{
				isPerformingSearch = true;

				await NetworkUserControl.InvokeWinzorDispatcherAsync(() =>
				{
					if (NetworkUserControl is not null)
					{
						if (SearchFinder == null || SearchFinder.NetworkViewModel != NetworkUserControl.NetworkViewModel)
						{
							SearchFinder = new SearchFinderViewModel(NetworkUserControl.NetworkViewModel);
						}

						if (!SearchFinder.HasPerformedSearch)
						{
							SearchFinder.PerformSearch(SearchText);
						}
						else
						{
							SearchFinder.ShowNextResult();
						}
					}

					isPerformingSearch = false;
				});
			}
		}
	}

	async Task OpenTextBoxContextMenuAsync(WebMouseEventArgs args)
	{
		await MenuDisplayer.ShowClipboardContextMenuAsync(args, ClipboardInterop, SearchBoxElementReference);
	}

	internal string ToggleRibbonStyle = $"display:inline;";
	readonly ResourceString ToggleRibbonText = ResString.GetMultilingualString("79289600-8103-4C2A-949A-83BCEB9D6377", "Minimize the Ribbon");

	async Task OpenContextMenuAsync(WebMouseEventArgs args)
	{
		var action = new StaticNetworkAction(() => { ToggleRibbonView(); }, ToggleRibbonText, ToggleRibbonText);
		var items = new[]
		{
			action.ToMenuItem()
		};

		await MenuDisplayer.ShowContextMenuAsync(args, items, ExecuteContextMenuActionAsync);
	}

	/// <summary>
	/// Executes a context menu action asynchronously.
	/// </summary>
	/// <param name="action">The network action to execute. This can be null.</param>
	/// <param name="args">The mouse event arguments associated with the action.</param>
	/// <remarks>
	/// If the action is not null, this method calls the Execute method on the action.
	/// After the action is executed, this method invokes an asynchronous operation to update the component's state.
	/// </remarks>
	public async Task ExecuteContextMenuActionAsync(INetworkAction? action, WebMouseEventArgs args)
	{
		action?.Execute();
		await InvokeAsync(StateHasChanged);
	}

	bool isRibbonTabChanged;

	internal void OnRibbonTabClick(int tabIndex, WebMouseEventArgs mouseEventArgs)
	{
		if (mouseEventArgs.GetConsecutiveClickCount() == 1)
		{
			isRibbonTabChanged = selectedTabIndex != tabIndex;
		}
		else if (mouseEventArgs.GetConsecutiveClickCount() == 2)
		{
			if (isRibbonTabChanged)
			{
				MaximizeRibbonView();
			}
			else
			{
				ToggleRibbonView();
			}
		}

		if (selectedTabIndex != tabIndex)
		{
			selectedTabIndex = tabIndex;
		}
	}

	internal void MaximizeRibbonView() => ToggleRibbonStyle = $"display:inline;";
	internal void ToggleRibbonView() => ToggleRibbonStyle = (ToggleRibbonStyle == "display:inline;") ? $"display:none;" : $"display:inline;";

	/// <summary>
	/// Sets the focus to the search box.
	/// </summary>
	/// <remarks>
	/// This method uses the FocusAsync method on the SearchBoxElementReference to set the focus.
	/// </remarks>
	public virtual void FocusOnSearchBox()
	{
		SearchBoxElementReference.FocusAsync();
	}
}
