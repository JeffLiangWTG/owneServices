using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.CFS.Module
{
	public class LoadListConsolFilterStrip : WorkflowFilterStripWithRoutingSupport
	{
		public LoadListConsolFilterStrip()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is LoadListModeFilter)
			{
				return new Control[] { new LoadListModeFilterControl() };
			}
			else if (currentModuleFilter is IJobManagementAmountFilter)
			{
				var filter = (IJobManagementAmountFilter)currentModuleFilter;
				return new Control[] { (Control)filter.GetFilterControl() };
			}
			else if (currentModuleFilter is ReferenceNumberFilter)
			{
				return ReferenceNumberFilterGUIProvider.GetReferenceNumberFilterControls(this, FilterControlBindingSource);
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
			else
			{
				return base.GetCurrentFilterControls(currentModuleFilter);
			}
		}
	}
}
