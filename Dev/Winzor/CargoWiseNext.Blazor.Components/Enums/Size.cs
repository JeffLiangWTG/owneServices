using System.ComponentModel;

namespace CargoWiseNext.Blazor.Components;

public enum Size
{
	[Description("small")]
	Small,

	[Description("medium")]
	Medium,

	[Description("large")]
	Large,

	[Description("xlarge")]
	XLarge,

	[Description("2xlarge")]
	XXLarge,

	[Description("3xlarge")]
	XXXLarge,

	[Description("fill-parent")]
	FillParent,
}
