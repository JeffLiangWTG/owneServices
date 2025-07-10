using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnPaper : CwnComponentBase
{
	protected string Classname => new CssBuilder()
		.AddClass("cwn-paper")
		.AddClass(Class)
		.Build();

	[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard coded style names")]
	protected string? Stylename => new StyleBuilder()
		.AddStyle("height", $"{Height}", !string.IsNullOrEmpty(Height))
		.AddStyle("width", $"{Width}", !string.IsNullOrEmpty(Width))
		.AddStyle("max-height", $"{MaxHeight}", !string.IsNullOrEmpty(MaxHeight))
		.AddStyle("max-width", $"{MaxWidth}", !string.IsNullOrEmpty(MaxWidth))
		.AddStyle("min-height", $"{MinHeight}", !string.IsNullOrEmpty(MinHeight))
		.AddStyle("min-width", $"{MinWidth}", !string.IsNullOrEmpty(MinWidth))
		.AddStyle(Style)
		.Build();

	/// <summary>
	/// The height of this component.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>null</c>.  Can be a pixel height (<c>150px</c>), percentage (<c>30%</c>), or other CSS height value.
	/// </remarks>
	[Parameter]
	public string? Height { get; set; }

	/// <summary>
	/// The width of this component.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>null</c>.  Can be a pixel width (<c>150px</c>), percentage (<c>30%</c>), or other CSS width value.
	/// </remarks>
	[Parameter]
	public string? Width { get; set; }

	/// <summary>
	/// The maximum height of this component.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>null</c>.  Can be a pixel height (<c>150px</c>), percentage (<c>30%</c>), or other CSS height value.
	/// </remarks>
	[Parameter]
	public string? MaxHeight { get; set; }

	/// <summary>
	/// The maximum width of this component.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>null</c>.  Can be a pixel width (<c>150px</c>), percentage (<c>30%</c>), or other CSS width value.
	/// </remarks>
	[Parameter]
	public string? MaxWidth { get; set; }

	/// <summary>
	/// The minimum height of this component.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>null</c>.  Can be a pixel height (<c>150px</c>), percentage (<c>30%</c>), or other CSS height value.
	/// </remarks>
	[Parameter]
	public string? MinHeight { get; set; }

	/// <summary>
	/// The minimum width of this component.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>null</c>.  Can be a pixel width (<c>150px</c>), percentage (<c>30%</c>), or other CSS width value.
	/// </remarks>
	[Parameter]
	public string? MinWidth { get; set; }

	/// <summary>
	/// The content within this component.
	/// </summary>
	[Parameter]
	public RenderFragment? ChildContent { get; set; }
}

