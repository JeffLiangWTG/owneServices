using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnCardActions : CwnComponentBase
{
	protected string Classname => new CssBuilder()
		.AddClass("cwn-card__actions")
		.AddClass(Class)
		.Build();

	/// <summary>
	/// The content within this component.
	/// </summary>
	[Parameter]
	public RenderFragment? ChildContent { get; set; }
}

