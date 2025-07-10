using System.Threading.Tasks;
using Blazor.Diagrams.Extensions;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.NetworkVisualisation.GUI.Extensions;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CargoWise.NetworkVisualisation.GUI;

public class RibbonMenuButton : RibbonButton
{
	[Inject]
	IMenuDisplayer MenuDisplayer { get; set; }

	[Inject]
	IJSRuntime JSRuntime { get; set; }

	protected override string ButtonType => (NoResString)"ribbonmenubutton " + base.ButtonType;

	protected override async Task OnClickAsync(WebMouseEventArgs args)
	{
		if (HasChildButtons)
		{
			var buttonBoundingClientRect = await JSRuntime.GetBoundingClientRect(ribbonButtonRef);
			await MenuDisplayer.ShowRibbonContextMenuAsync(buttonBoundingClientRect.Right, buttonBoundingClientRect.Bottom, ViewModel.ChildButtons, ExecuteActionAsync);
		}
	}
}
