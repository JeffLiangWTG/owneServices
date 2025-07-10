using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWise.Blazor.Controls;

public partial class Button
{
	[Parameter]
	public string Text { get; set; }

	[Parameter]
	public string Description { get; set; }

	[Parameter]
	public string CssClass { get; set; }

	[Parameter]
	public EventCallback<MouseEventArgs> OnClick { get; set; }

	[Parameter]
	public EventCallback OnFocusIn { get; set; }
}

