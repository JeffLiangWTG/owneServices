using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnFooter : CwnComponentBase
{
	protected string Classname => new CssBuilder()
		.AddClass("cwn-layout__footer")
		.AddClass(Class)
		.Build();

	protected string? Stylename => new StyleBuilder()
		.AddStyle(Style)
		.Build();

	[Parameter]
	public RenderFragment? ChildContent { get; set; }
}
