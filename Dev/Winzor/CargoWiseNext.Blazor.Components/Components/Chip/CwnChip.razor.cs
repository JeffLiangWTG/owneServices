using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnChip : CwnComponentBase
{
	protected string Classname =>
		new CssBuilder()
			.AddClass("cwn-chip")
			.AddClass("cwn-chip--outlined")
			.AddClass($"cwn-{Color?.GetDescription()}", Color.HasValue)
			.AddClass(Class)
			.Build();

	[Parameter]
	public Icon? Icon { get; set; }

	[Parameter]
	public Color? Color { get; set; }

	[Parameter]
	public Color? IconColor { get; set; } = Blazor.Components.Color.Default;

	[Parameter]
	public Size IconSize { get; set; } = Size.Small;

	[Parameter]
	public Variant Variant { get; set; } = Variant.Text;

	[Parameter]
	public RenderFragment? ChildContent { get; set; }
}
