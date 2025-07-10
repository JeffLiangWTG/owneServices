using Microsoft.AspNetCore.Components;

namespace CargoWise.NetworkVisualisation.GUI;

public partial class RibbonGroup
{
	[Parameter]
	public RibbonGroupViewModel ViewModel { get; set; }
}
