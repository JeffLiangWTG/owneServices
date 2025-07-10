using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnAppBar : CwnComponentBase
{
	protected string Classname => new CssBuilder()
			.AddClass("cwn-appbar")
			.AddClass("cwn-appbar--dense", Dense)
			.AddClass("cwn-appbar--fixed-top", Fixed && !Bottom)
			.AddClass("cwn-appbar--fixed-bottom", Fixed && Bottom)
			.AddClass($"cwn-{Color?.GetDescription()}", Color.HasValue)
			.AddClass($"cwn-elevation-{Elevation}")
			.AddClass(Class)
			.Build();

	protected string ToolBarClassname => new CssBuilder()
			.AddClass("cwn-toolbar-appbar")
			.AddClass(ToolBarClass)
			.Build();

	/// <summary>
	/// Places the appbar at the bottom of the screen instead of the top.
	/// </summary>
	[Parameter]
	public bool Bottom { get; set; }

	/// <summary>
	/// The size of the drop shadow.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>4</c>.  A higher number creates a heavier drop shadow.  Use a value of <c>0</c> for no shadow.
	/// </remarks>
	[Parameter]
	public int Elevation { set; get; } = 4;

	/// <summary>
	/// Uses compact padding.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>false</c>.
	/// </remarks>
	[Parameter]
	public bool Dense { get; set; }

	/// <summary>
	/// Adds left and right padding to this appbar.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>true</c>.
	/// </remarks>
	[Parameter]
	public bool Gutters { get; set; } = true;

	/// <summary>
	/// The color of this appbar.
	/// </summary>
	/// <remarks>
	/// Defaults to <see cref="Color.Default"/>.  Theme colors are supported.
	/// </remarks>
	[Parameter]
	public Color? Color { get; set; }

	/// <summary>
	/// Fixes this appbar in place as the page is scrolled.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>true</c>.  When <c>false</c>, the appbar will scroll with other page content.
	/// </remarks>
	[Parameter]
	public bool Fixed { get; set; } = true;

	/// <summary>
	/// Allows appbar content to wrap.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>false</c>.
	/// </remarks>
	[Parameter]
	public bool WrapContent { get; set; } = false;

	/// <summary>
	/// The CSS classes applied to the nested toolbar.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>null</c>.  You can use spaces to separate multiple classes.
	/// </remarks>
	[Parameter]
	public string? ToolBarClass { get; set; }

	/// <summary>
	/// The content within this component.
	/// </summary>
	[Parameter]
	public RenderFragment? ChildContent { get; set; }
}

