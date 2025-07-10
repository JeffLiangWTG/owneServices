#define CODE_ANALYSIS
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module
{
	public class CusPermitModuleStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			var result = System.Array.Empty<Control>();
			if (currentModuleFilter is PermitTypeModuleFilter)
			{
				var control = GetPermitTypeFilterStrip();
				ControlDpiScalingHelper.SetHeight(control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				result = new Control[] { control };
			}
			else if (currentModuleFilter is PermitRuleModuleFilter)
			{
				var control = new PermitRuleFilterStrip();
				ControlDpiScalingHelper.SetHeight(control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				PreferredHeight = control.Height;

				result = new Control[] { control };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}
			return result;
		}

		protected virtual Control GetPermitTypeFilterStrip()
		{
			return new PermitTypeFilterStrip();
		}
	}
}
