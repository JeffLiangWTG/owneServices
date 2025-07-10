using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ContractManagement.Module
{
	public class AllocationRouteModuleStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			switch (currentModuleFilter)
			{
				case AllocationRouteCoveringLocationFilter locationFilter:
					var loadDischargeFilterControl = new LoadDischargeFilterControl();
					loadDischargeFilterControl.SetUpLocationDescriptions(locationFilter.ItemDescription1, locationFilter.ItemDescription2);
					ControlDpiScalingHelper.SetHeight(loadDischargeFilterControl, loadDischargeFilterControl.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
					PreferredHeight = loadDischargeFilterControl.Height;
					return [loadDischargeFilterControl];

				case AllocationContainerWeightLimitWithTypeFilter numberFilter:
					return numberFilter.GetFilterControl(CurrentDataItem, FilterControlBindingSource);

				default:
					return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}
	}
}
