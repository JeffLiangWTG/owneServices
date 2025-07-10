using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnToolBar : CwnComponentBase
{
	protected string Classname => new CssBuilder()
			.AddClass("cwn-toolbar")
			.AddClass("cwn-toolbar--dense", Dense)
			.AddClass("cwn-toolbar--gutters", Gutters)
			.AddClass("cwn-toolbar--wrap-content", WrapContent)
			.AddClass(Class)
			.Build();

	/// <summary>
	/// Uses compact padding.
	/// </summary>
	[Parameter]
	public bool Dense { get; set; }

	/// <summary>
	/// Adds left and right padding.
	/// </summary>
	[Parameter]
	public bool Gutters { get; set; } = true;

	/// <summary>
	/// Child content of component.
	/// </summary>
	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	/// <summary>
	/// Allows the toolbar's content to wrap.
	/// </summary>
	[Parameter]
	public bool WrapContent { get; set; }
}
