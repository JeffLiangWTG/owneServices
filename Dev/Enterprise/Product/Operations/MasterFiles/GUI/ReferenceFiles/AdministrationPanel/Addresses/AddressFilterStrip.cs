
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	[SuppressFormDesignerAnalysis]
	public class AddressFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;

			if (currentModuleFilter is AddressSourceAndJobNumberModuleFilter)
			{
				var control = new AddressSourceAndJobNumberFilterControl();
				ControlDpiScalingHelper.SetHeight(ref control, control.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				var comparisonOperatorDropEdit = CreateComparisonOperatorDropList("ComparisonOperator", "ComparisonOperator_List");
				comparisonOperatorDropEdit.TabIndex = 2;
				control.Controls.Add(comparisonOperatorDropEdit);
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
