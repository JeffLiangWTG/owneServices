using System.ComponentModel;

namespace CargoWiseNext.Blazor.Components;

public enum Variant
{
	[Description("text")]
	Text,

	[Description("filled")]
	Filled,

	[Description("outlined")]
	Outlined
}
