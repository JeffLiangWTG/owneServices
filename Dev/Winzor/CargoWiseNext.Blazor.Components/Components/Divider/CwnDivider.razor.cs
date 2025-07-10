using System.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnDivider : CwnComponentBase
{
	protected string Classname => new CssBuilder()
		.AddClass("cwn-divider")
		.AddClass("cwn-divider--vertical", Vertical)
		.AddClass($"cwn-divider--{DividerType.GetDescription()}", DividerType != DividerType.FullWidth || !Vertical)
		.AddClass(Class)
		.Build();

	[Parameter]
	public bool Vertical { get; set; }

	[Parameter]
	public DividerType DividerType { get; set; } = DividerType.FullWidth;
}
