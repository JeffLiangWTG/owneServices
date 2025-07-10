using System.Windows.Forms;
using Enterprise.Accounting.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Agency.Module
{
	public class SundryChargesModuleStrip : ZFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			var jobManagementAmountFilter = currentModuleFilter as IJobManagementAmountFilter;
			if (jobManagementAmountFilter != null)
			{
				result = new Control[] { (Control)jobManagementAmountFilter.GetFilterControl() };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}
	}
}
