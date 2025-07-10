using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.Module
{
	public class ContainerDetentionModuleStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;

			var jobManagementAmountFilter = currentModuleFilter as IJobManagementAmountFilter;

			if (jobManagementAmountFilter != null)
			{
				result = new Control[] { (Control)jobManagementAmountFilter.GetFilterControl() };
			}
			else if (currentModuleFilter is VoyageVesselModuleFilter)
			{
				var control = new VoyageVesselModuleFilterControl(this, FilterControlBindingSource);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				result = new Control[] { control };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}
	}
}
