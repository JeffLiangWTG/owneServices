using System.Windows.Forms;
using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Invoicing.Module
{
	public class PeriodicInvoicingFilterStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;

			if (currentModuleFilter is IJobManagementAmountFilter jobManagementFilter)
			{
				result = new Control[] { (Control)jobManagementFilter.GetFilterControl() };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}
	}
}
