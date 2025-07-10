using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public class OrdersBaseModuleStrip : WorkflowFilterStripWithRoutingSupport
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is OrgClientAssignedStaffModuleFilter)
			{
				var control = new OrgClientAssignedStaffFilterStrip();
				ControlDpiScalingHelper.SetHeight(control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				return new Control[] { control };
			}

			var voyageVesselModuleFilter = currentModuleFilter as VoyageVesselModuleFilter;
			if (voyageVesselModuleFilter != null)
			{
				var control = new VoyageVesselModuleFilterControl(this, FilterControlBindingSource);
				control.SetMaxLength(voyageVesselModuleFilter);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				return new Control[] { control };
			}

			return base.GetCurrentFilterControls(currentModuleFilter);
		}
	}
}
