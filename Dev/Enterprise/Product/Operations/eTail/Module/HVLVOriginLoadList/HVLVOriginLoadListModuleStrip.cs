using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Freight.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.eTail.Module
{
	class HVLVOriginLoadListModuleStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is VoyageVesselModuleFilter voyageVesselModuleFilter)
			{
				var control = new VoyageVesselModuleFilterControl(this, FilterControlBindingSource, true);
				control.SetMaxLength(voyageVesselModuleFilter);
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				return new Control[] { control };
			}

			return base.GetCurrentFilterControls(currentModuleFilter);
		}
	}
}
