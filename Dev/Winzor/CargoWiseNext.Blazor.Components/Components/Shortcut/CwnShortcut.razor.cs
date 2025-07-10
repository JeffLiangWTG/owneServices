using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnShortcut : CwnComponentBase
{
	[Parameter]
	public List<string> Shortcuts { get; set; } = new List<string>();
}

