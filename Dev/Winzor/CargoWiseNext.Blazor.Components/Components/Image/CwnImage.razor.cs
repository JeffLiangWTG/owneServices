using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnImage : CwnComponentBase
{
	protected string Classname => new CssBuilder()
		.AddClass("cwn-image")
		.AddClass($"cwn-image--{ObjectFit.GetDescription()}")
		.AddClass($"cwn-image--{ObjectPosition.GetDescription()}")
		.AddClass(Class)
		.Build();

	[Parameter]
	public string? Src { get; set; }

	[Parameter]
	public string? Alt { get; set; }

	[Parameter]
	public int? Height { get; set; }

	[Parameter]
	public int? Width { get; set; }

	[Parameter]
	public ObjectFit ObjectFit { set; get; } = ObjectFit.Fill;

	[Parameter]
	public ObjectPosition ObjectPosition { set; get; } = ObjectPosition.Center;
}
