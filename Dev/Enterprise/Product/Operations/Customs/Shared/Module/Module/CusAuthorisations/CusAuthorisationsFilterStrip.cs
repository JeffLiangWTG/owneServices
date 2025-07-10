using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Module
{
	public partial class CusAuthorisationsFilterStrip : ZFilterStrip
	{
		public CusAuthorisationsFilterStrip()
		{
			InitializeComponent();
		}

		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			var result = System.Array.Empty<Control>();
			if (currentModuleFilter is CusAuthorisationsRuleModuleFilter)
			{
				var control = new CusAuthorisationsRuleModuleFilterStrip();
				control.TabIndex = 1;
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
