using System.Windows.Forms;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartageModuleStrip : WorkflowFilterStrip
	{
		protected override Control[] GetCurrentFilterControls(ModuleFilter currentModuleFilter)
		{
			Control[] result;
			IJobManagementAmountFilter otherModuleFilter;

			if ((otherModuleFilter = currentModuleFilter as IJobManagementAmountFilter) != null)
			{
				result = new Control[] { (Control)(otherModuleFilter.GetFilterControl()) };
			}
			else if (currentModuleFilter is ReferenceNumberFilter)
			{
				result = ReferenceNumberFilterGUIProvider.GetReferenceNumberFilterControls(this, FilterControlBindingSource);
			}
			else
			{
				result = base.GetCurrentFilterControls(currentModuleFilter);
			}

			return result;
		}
	}
}
