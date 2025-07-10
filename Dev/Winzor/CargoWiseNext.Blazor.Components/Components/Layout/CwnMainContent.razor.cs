using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnMainContent : CwnComponentBase
{
	protected string Classname => new CssBuilder()
		.AddClass("cwn-layout__main-content")
		.AddClass(Class)
		.Build();

	protected string? Stylename => new StyleBuilder()
		.AddStyle(Style)
		.Build();

	[Parameter]
	public RenderFragment? ChildContent { get; set; }
}
