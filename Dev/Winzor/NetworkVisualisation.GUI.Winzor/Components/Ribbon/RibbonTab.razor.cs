using Microsoft.AspNetCore.Components;

namespace CargoWise.NetworkVisualisation.GUI;

public partial class RibbonTab
{
	[Parameter]
	public RibbonTabViewModel ViewModel { get; set; }
}
