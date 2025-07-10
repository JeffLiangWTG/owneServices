using System.ComponentModel;

namespace CargoWiseNext.Blazor.Components;

/// <summary>
/// The color themes available, allowing components to adapt their visual style based on the selected color.
/// See CargoWise.Blazor.Components\Styles\themes\_default.scss
/// </summary>
public enum Color
{
	[Description("neutral")]
	Default,

	[Description("primary")]
	Primary,

	[Description("brand")]
	Brand,

	[Description("info")]
	Info,

	[Description("success")]
	Success,

	[Description("warning")]
	Warning,

	[Description("error")]
	Error,
}
