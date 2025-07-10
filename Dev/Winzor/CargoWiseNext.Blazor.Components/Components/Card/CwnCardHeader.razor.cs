using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnCardHeader : CwnComponentBase
{
	protected string Classname => new CssBuilder()
		.AddClass("cwn-card__header")
		.AddClass(Class)
		.Build();

	[Parameter]
	public string? ClassHeaderContent { get; set; }

	protected string ClassnameHeaderContent => new CssBuilder()
		.AddClass("cwn-card__header__content")
		.AddClass(ClassHeaderContent)
		.Build();

	[Parameter]
	public string? ClassHeaderActions { get; set; }

	protected string ClassnameHeaderActions => new CssBuilder()
		.AddClass("cwn-card__header__actions")
		.AddClass(ClassHeaderActions)
		.Build();

	/// <summary>
	/// The main content of this header.
	/// </summary>
	[Parameter]
	public RenderFragment? CardHeaderContent { get; set; }

	/// <summary>
	/// The actions displayed within this header.
	/// </summary>
	[Parameter]
	public RenderFragment? CardHeaderActions { get; set; }

	/// <summary>
	/// The custom content within this header.
	/// </summary>
	[Parameter]
	public RenderFragment? ChildContent { get; set; }
}

