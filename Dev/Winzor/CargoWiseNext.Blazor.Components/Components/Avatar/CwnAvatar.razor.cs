using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

partial class CwnAvatar : CwnComponentBase
{
	protected string Classname => new CssBuilder()
		.AddClass("cwn-avatar")
		.AddClass($"cwn-avatar--{Size.GetDescription()}")
		.AddClass(Class)
		.Build();

	[Parameter]
	public Size Size { get; set; } = Size.Medium;

	[Parameter]
	public RenderFragment? ChildContent { get; set; }
}
