using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnCardContent : CwnComponentBase
{
	protected string Classname => new CssBuilder()
		.AddClass("cwn-card__content")
		.AddClass("cwn-scrollbar")
		.AddClass(Class)
		.Build();

	/// <summary>
	/// The content within this component.
	/// </summary>
	[Parameter]
	public RenderFragment? ChildContent { get; set; }
}

