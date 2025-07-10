using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Module
{
	[SuppressFormDesignerAnalysis]
	public partial class CommercialInvoiceFilterStrip : MasterFiles.Module.WorkflowFilterStrip
	{
		public CommercialInvoiceFilterStrip()
		{
			InitializeComponent();
		}

		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			if (currentModuleFilter is ReferenceModuleFilter)
			{
				var refFilterControl = new ReferenceFilterControl();
				ControlDpiScalingHelper.SetHeight(ref refFilterControl, refFilterControl.Height + ControlDpiScalingHelper.ScaleToCurrentDpiY(1), false);
				var comparisonOperatorDropEdit = CreateComparisonOperatorDropList();
				refFilterControl.Controls.Add(comparisonOperatorDropEdit);

				result = new Control[] { refFilterControl };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}
			return result;
		}
	}
}
