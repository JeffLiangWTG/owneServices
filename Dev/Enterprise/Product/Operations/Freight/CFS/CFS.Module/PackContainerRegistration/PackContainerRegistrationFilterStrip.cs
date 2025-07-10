using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.CFS.Module
{
	public class PackContainerRegistrationFilterStrip : WorkflowFilterStripWithRoutingSupport
	{
		public PackContainerRegistrationFilterStrip()
		{
		}

		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			var voyageVesselModuleFilter = currentModuleFilter as VoyageVesselModuleFilter;
			if (voyageVesselModuleFilter != null)
			{
				var control = new VoyageVesselModuleFilterControl(this, FilterControlBindingSource);
				control.SetMaxLength(voyageVesselModuleFilter);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				return new Control[] { control };
			}
			else
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}
	}
}
