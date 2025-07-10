using System.ComponentModel;

namespace CargoWiseNext.Blazor.Components;

public enum CalloutType
{
	[Description("info")]
	Info,

	[Description("error")]
	Error,

	[Description("warning")]
	Warning
}
