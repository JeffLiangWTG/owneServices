using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnSearchShortcut : CwnComponentBase
{
	protected string Classname =>
new CssBuilder()
	.AddClass("cwn-search-shortcut")
	.AddClass(Class)
	.Build();

	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	[Parameter]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html")]
	public string? HtmlTag { get; set; } = "div";
}
