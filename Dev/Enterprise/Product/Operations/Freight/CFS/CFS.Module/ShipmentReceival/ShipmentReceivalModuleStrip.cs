using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Module;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.CFS.Module
{
	public class ShipmentReceivalModuleStrip : WorkflowFilterStripWithRoutingSupport
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;

			var voyageVesselModuleFilter = currentModuleFilter as VoyageVesselModuleFilter;
			if (voyageVesselModuleFilter != null)
			{
				var control = new VoyageVesselModuleFilterControl(this, FilterControlBindingSource);
				control.SetMaxLength(voyageVesselModuleFilter);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				return new Control[] { control };
			}

			if (currentModuleFilter is ModuleWarehouseLocationFilter)
			{
				result = ModuleWarehouseLocationFilterControlHelper.GetNewWarehouseFilterControls(this, FilterControlBindingSource);
			}
			else if (currentModuleFilter is IJobManagementAmountFilter)
			{
				result = new Control[] { (Control)((IJobManagementAmountFilter)currentModuleFilter).GetFilterControl() };
			}
			else if (currentModuleFilter is ReferenceNumberFilter)
			{
				return ReferenceNumberFilterGUIProvider.GetReferenceNumberFilterControls(this, FilterControlBindingSource);
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}
	}
}
