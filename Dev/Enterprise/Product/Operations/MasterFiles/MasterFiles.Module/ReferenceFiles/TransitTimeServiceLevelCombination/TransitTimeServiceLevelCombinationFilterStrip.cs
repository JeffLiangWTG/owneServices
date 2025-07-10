using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.Module.ReferenceFiles.TransitTimeServiceLevelCombination
{
	class TransitTimeServiceLevelCombinationFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			if (currentModuleFilter is TransitTimeModuleFilter)
			{
				var control = new TransitTimeModuleFilterControl();
				control.comparisonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(219, 1, true);
				return new Control[] { control };
			}

			return base.GetCurrentFilterControls(currentModuleFilter);
		}
	}
}
