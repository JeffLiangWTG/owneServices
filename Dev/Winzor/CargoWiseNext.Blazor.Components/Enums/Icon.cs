using System.ComponentModel;

namespace CargoWiseNext.Blazor.Components;

public enum Icon
{
	[Description("arrow-down")] ArrowDown,
	[Description("arrow-right")] ArrowRight,
	[Description("arrow-up")] ArrowUp,
	[Description("assigned")] Assigned,
	[Description("cancelled")] Cancelled,
	[Description("delete")] Delete,
	[Description("density-relaxed")] DensityRelaxed,
	[Description("density-tight")] DensityTight,
	[Description("drag")] Drag,
	[Description("enter")] Enter,
	[Description("esc")] Esc,
	[Description("help")] Help,
	[Description("history")] History,
	[Description("menu-meatballs")] MenuMeatballs,
	[Description("module")] Module,
	[Description("pause")] Pause,
	[Description("plus")] Plus,
	[Description("ready-to-start")] ReadyToStart,
	[Description("refresh")] Refresh,
	[Description("return")] Return,
	[Description("search")] Search,
	[Description("settings")] Settings,
	[Description("star-empty")] StarEmpty,
	[Description("star-filled")] StarFilled,
	[Description("status-critical")] Critical,
	[Description("status-warning")] Warning,
	[Description("status-info")] Info,
	[Description("switch")] Switch,
	[Description("vector")] Vector,
	[Description("suspended")] Suspended,
	[Description("working")] Working,
}
