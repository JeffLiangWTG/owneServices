using System.Windows.Forms;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WhsWorkflowFilterStrip : WorkflowFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;

			if (currentModuleFilter is IJobManagementAmountFilter jobManagementAmountFilter)
			{
				result = new Control[] { (Control)jobManagementAmountFilter.GetFilterControl() };
			}
			else if (currentModuleFilter is INumberRangeByUnitFilter numberRangeByUnitFilter)
			{
				var controlArray = GetTextWithListFilterControls(false, true);
				result = numberRangeByUnitFilter.GetFilterControl(CurrentDataItem, controlArray[0], FilterControlBindingSource);
			}
			else if (currentModuleFilter is ServiceTypeDateFilter)
			{
				var control = ServiceTypeDateFilter.GetServiceTypeDateFilterControl(this, FilterControlBindingSource);
				PreferredHeight = control.Height;
				result = new[] { control };
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}
	}
}
