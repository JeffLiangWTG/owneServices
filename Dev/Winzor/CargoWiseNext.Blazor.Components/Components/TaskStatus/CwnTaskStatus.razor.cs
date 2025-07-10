using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components;
namespace CargoWiseNext.Blazor.Components;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in CwnTaskStatus.razor")]
public partial class CwnTaskStatus : CwnComponentBase
{
	[Parameter]
	public string Status { get; set; } = string.Empty;

	[Parameter]
	public string StatusClass { get; set; } = string.Empty;

	[Parameter]
	public Size StatusIconSize { get; set; } = Size.Small;

	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	Icon StatusIcon => Status switch
	{
		"WRK" => Icon.Working,
		"SUS" => Icon.Suspended,
		_ => Icon.Cancelled
	};

	Color StatusIconColor => Status switch
	{
		"WRK" => Color.Success,
		"SUS" => Color.Warning,
		_ => Color.Default
	};

	Color BackGroundColor => Status switch
	{
		"WRK" => Color.Success,
		"SUS" => Color.Warning,
		_ => Color.Default
	};
}

