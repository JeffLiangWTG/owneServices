using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ZA.Module
{
	public class JobDeclarationModuleStrip : Customs.Module.JobDeclarationModuleStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			var result = System.Array.Empty<Control>();
			if (currentModuleFilter is ProcedureCodesModuleFilter)
			{
				var control = new ProcedureCodesFilterStrip();
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
	}
}
