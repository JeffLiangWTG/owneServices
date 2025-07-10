using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnLabel
{
	[Parameter] public RenderFragment? ChildContent { get; set; }
	[Parameter] public string? Text { get; set; }
	[Parameter] public bool IsRequired { get; set; }

	string Classname => new CssBuilder()
		.AddClass("cwn-label")
		.AddClass(Class)
		.Build();

	string? Stylename => new StyleBuilder()
		.AddStyle(Style)
		.Build();

	MarkupString TextLabel => IsRequired
		? new MarkupString($"{Text} <span class=\"cwn-label--required\">*</span>")
		: new MarkupString(Text ?? string.Empty);
}
