using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnLayout : CwnComponentBase
{
	protected string Classname => new CssBuilder()
		.AddClass("cwn-layout")
		.AddClass("cwn-theme--custom", Theme is not null)
		.AddClass(Class)
		.Build();

	protected string? Stylename => new StyleBuilder()
		.AddStyle(Style)
		.Build();

	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	[Parameter]
	public Theme? Theme { get; set; }
}
