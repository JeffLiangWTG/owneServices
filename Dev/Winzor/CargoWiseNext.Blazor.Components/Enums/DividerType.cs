using System.ComponentModel;

namespace CargoWiseNext.Blazor.Components;

public enum DividerType
{
	[Description("full-width")]
	FullWidth,

	[Description("inset")]
	Inset,

	[Description("middle")]
	Middle
}
