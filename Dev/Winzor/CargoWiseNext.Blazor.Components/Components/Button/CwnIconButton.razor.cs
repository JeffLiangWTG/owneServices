using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnIconButton : CwnBaseButton
{
	[Parameter]
	public EventCallback<WebMouseEventArgs> OnSingleClick { get; set; } = default;

	protected string Classname => new CssBuilder()
		.AddClass("cwn-button")
		.AddClass("cwn-icon-button")
		.AddClass($"cwn-{Color?.GetDescription()}", Color.HasValue)
		.AddClass($"cwn-icon-button--{Size.GetDescription()}")
		.AddClass($"cwn-icon-button--{Variant.GetDescription()}")
		.AddClass("cwn-icon-button-disable-elevation", !DropShadow)
		.AddClass(Class)
		.Build();

	[Parameter]
	public string? ClassIcon { get; set; }

	[Parameter]
	public Icon? Icon { get; set; }

	[Parameter]
	public Icon? HoverIcon { get; set; }

	[Parameter]
	public Size Size { get; set; } = Size.Medium;

	[Parameter]
	public Color? Color { get; set; }

	[Parameter]
	public Variant Variant { get; set; } = Variant.Text;

	[Parameter]
	public RenderFragment? ChildContent { get; set; }
}
