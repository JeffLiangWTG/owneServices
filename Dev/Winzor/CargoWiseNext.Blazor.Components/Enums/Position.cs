using System.ComponentModel;

namespace CargoWiseNext.Blazor.Components;

public enum Position
{
	[Description("bottom")]
	Bottom,
	[Description("center")]
	Center,
	[Description("top")]
	Top,
	[Description("left")]
	Left,
	[Description("right")]
	Right,
	[Description("start")]
	Start,
	[Description("end")]
	End
}

