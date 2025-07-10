using System.Windows.Forms;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	[SuppressFormDesignerAnalysis]
	public class OnlineSailingSchedulesFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			var voyageVesselModuleFilter = currentModuleFilter as OnlineSchedulesVoyageVesselFilter;

			if (voyageVesselModuleFilter == null)
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}

			var filterControl = new OnlineSchedulesVoyageVesselFilterControl();
			FilterDescriptionDropEdit.AllowOverlap(filterControl);
			return new Control[] { filterControl };
		}
	}
}
