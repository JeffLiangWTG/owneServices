using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnIcon : CwnComponentBase
{
	protected string Classname =>
		new CssBuilder()
			.AddClass("cwn-icon")
			.AddClass($"cwn-icon--{Icon.GetDescription()}")
			.AddClass($"cwn-icon--{HoverIcon?.GetDescription()}__hover", HoverIcon.HasValue && !Disabled)
			.AddClass($"cwn-{Color?.GetDescription()}", Color.HasValue)
			.AddClass($"cwn-icon--{Size.GetDescription()}")
			.AddClass(Class)
			.Build();

	protected string? Stylename =>
		new StyleBuilder()
			.AddStyle(Style)
			.Build();

	[Parameter]
	[EditorRequired]
	public Icon Icon { get; set; }

	[Parameter]
	public Icon? HoverIcon { get; set; }

	[Parameter]
	public Size Size { get; set; } = Size.Medium;

	[Parameter]
	public bool Disabled { get; set; }

	[Parameter]
	public Color? Color { get; set; }

	[Parameter]
	public string? Title { get; set; }
}
