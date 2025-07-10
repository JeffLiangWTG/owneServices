using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnText
{
	protected string Classname =>
	new CssBuilder()
		.AddClass("wtg-text")
		.AddClass($"wtg-text-{Typo.GetDescription()}")
		.AddClass($"wtg-text-{Color?.GetDescription()}", Color.HasValue)
		.AddClass($"wtg-text-align-{Align.GetDescription()}", Align != Align.Inherit)
		.AddClass(Class)
		.Build();

	[Parameter]
	public ElementType Typo { get; set; } = ElementType.body1;

	[Parameter]
	public Align Align { get; set; } = Align.Inherit;

	[Parameter]
	public Color? Color { get; set; }

	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	[Parameter]
	public string? HtmlTag { get; set; }

	string GetActualTag() => string.IsNullOrEmpty(HtmlTag) ? GetTagName(Typo) : HtmlTag;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html")]
	static string GetTagName(ElementType typo) => typo switch
	{
		ElementType.h1 => "h1",
		ElementType.h2 => "h2",
		ElementType.h3 => "h3",
		ElementType.h4 => "h4",
		ElementType.h5 => "h5",
		ElementType.h6 => "h6",
		ElementType.subtitle1 => "p",
		ElementType.subtitle2 => "p",
		ElementType.body1 => "p",
		ElementType.body2 => "p",
		_ => "span"
	};
}
