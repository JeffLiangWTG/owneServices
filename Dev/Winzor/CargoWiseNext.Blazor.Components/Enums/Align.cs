using System.ComponentModel;

namespace CargoWiseNext.Blazor.Components;

public enum Align
{
	[Description("inherit")]
	Inherit,
	[Description("left")]
	Left,
	[Description("center")]
	Center,
	[Description("right")]
	Right,
	[Description("justify")]
	Justify,
}
